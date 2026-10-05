using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MohammedRaouf.Application.Activation;
using MohammedRaouf.Application.Common;
using MohammedRaouf.Application.Purchases;
using MohammedRaouf.Contracts.Activation;
using MohammedRaouf.Contracts.Public;
using MohammedRaouf.Domain.Entities;
using MohammedRaouf.Domain.Enums;
using MohammedRaouf.Infrastructure.Persistence;
using Npgsql;

namespace MohammedRaouf.Infrastructure.Activation;

public sealed class ActivationWorkflowService(
    ApplicationDbContext dbContext,
    IActivationCodeGenerator generator,
    IActivationCodeHasher hasher,
    IPurchaseRequestStateMachine stateMachine,
    IOptions<ActivationCodeOptions> options,
    TimeProvider timeProvider,
    ILogger<ActivationWorkflowService> logger) : IActivationWorkflowService
{
    private const string GenericRedeemMessage = "كود التفعيل غير صحيح أو غير متاح للاستخدام.";
    private const string ExpiredOwnedMessage = "انتهت صلاحية كود التفعيل، يرجى التواصل مع الدعم.";
    private const string AlreadyEnrolledMessage = "هذه الدورة مفعّلة مسبقاً على حسابك.";
    private const string ActiveCodeExistsMessage = "يوجد كود تفعيل نشط لهذا الطلب. قم بإلغائه أولاً قبل إصدار كود جديد.";

    public async Task<ActionResult<IssuedActivationCodeResponse>> IssueAsync(
        Guid purchaseRequestId,
        Guid actorUserId,
        string? ipAddress,
        CancellationToken cancellationToken = default)
    {
        var now = timeProvider.GetUtcNow();
        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            var purchase = await LoadPurchaseForUpdateAsync(purchaseRequestId, cancellationToken);
            if (purchase is null)
            {
                return ActionResult<IssuedActivationCodeResponse>.Fail(404, "غير موجود", "طلب الاشتراك غير موجود.");
            }

            if (purchase.Status != PurchaseRequestStatus.PaymentReceived &&
                purchase.Status != PurchaseRequestStatus.ActivationCodeIssued)
            {
                return ActionResult<IssuedActivationCodeResponse>.Fail(
                    409,
                    "تعارض",
                    "لا يمكن إصدار كود تفعيل إلا بعد تأكيد استلام الدفع.");
            }

            var codes = await dbContext.ActivationCodes
                .Where(code => code.PurchaseRequestId == purchase.Id)
                .ToListAsync(cancellationToken);

            foreach (var stale in codes.Where(code =>
                         code.Status == ActivationCodeStatus.Active &&
                         code.UsedAt is null &&
                         code.ExpiresAt is DateTimeOffset expires &&
                         expires <= now))
            {
                stale.Status = ActivationCodeStatus.Expired;
            }

            if (codes.Any(code =>
                    code.Status == ActivationCodeStatus.Active &&
                    code.UsedAt is null &&
                    (code.ExpiresAt is null || code.ExpiresAt > now)))
            {
                return ActionResult<IssuedActivationCodeResponse>.Fail(409, "تعارض", ActiveCodeExistsMessage);
            }

            string plain;
            string hash;
            var attempts = 0;
            do
            {
                plain = generator.GeneratePlainCode();
                hash = hasher.Hash(plain);
                attempts++;
            } while (await dbContext.ActivationCodes.AnyAsync(code => code.CodeHash == hash, cancellationToken) &&
                     attempts < 8);

            var expirationDays = Math.Max(1, options.Value.ExpirationDays);
            var expiresAt = now.AddDays(expirationDays);
            var activation = new ActivationCode
            {
                Id = Guid.NewGuid(),
                CodeHash = hash,
                UserId = purchase.UserId,
                CourseId = purchase.CourseId,
                PurchaseRequestId = purchase.Id,
                Status = ActivationCodeStatus.Active,
                ExpiresAt = expiresAt,
                CreatedAt = now,
                CreatedBy = actorUserId
            };
            dbContext.ActivationCodes.Add(activation);

            if (purchase.Status == PurchaseRequestStatus.PaymentReceived)
            {
                ApplyTransition(purchase, PurchaseRequestStatus.ActivationCodeIssued, actorUserId, now, note: null);
            }

            AddAudit(
                actorUserId,
                "ActivationCodeIssued",
                "ActivationCode",
                activation.Id,
                $"تم إصدار كود تفعيل للطلب {purchase.RequestNumber}.",
                new
                {
                    activationCodeId = activation.Id,
                    courseId = purchase.CourseId,
                    purchaseRequestId = purchase.Id,
                    expiresAt
                },
                ipAddress,
                now);

            await dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            return ActionResult<IssuedActivationCodeResponse>.Ok(new IssuedActivationCodeResponse
            {
                ActivationCode = plain,
                ExpiresAt = expiresAt,
                RequestNumber = purchase.RequestNumber
            });
        }
        catch (DbUpdateException exception) when (IsUniqueViolation(exception))
        {
            await transaction.RollbackAsync(cancellationToken);
            return ActionResult<IssuedActivationCodeResponse>.Fail(409, "تعارض", ActiveCodeExistsMessage);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    public async Task<ActionResult> RevokeAsync(
        Guid activationCodeId,
        Guid actorUserId,
        string? ipAddress,
        CancellationToken cancellationToken = default)
    {
        var now = timeProvider.GetUtcNow();
        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            await LockActivationCodeAsync(activationCodeId, cancellationToken);
            var code = await dbContext.ActivationCodes
                .FirstOrDefaultAsync(item => item.Id == activationCodeId, cancellationToken);
            if (code is null)
            {
                return ActionResult.Fail(404, "غير موجود", "كود التفعيل غير موجود.");
            }

            if (code.Status != ActivationCodeStatus.Active || code.UsedAt is not null)
            {
                return ActionResult.Fail(409, "تعارض", "لا يمكن إلغاء هذا الكود.");
            }

            var purchase = await LoadPurchaseForUpdateAsync(code.PurchaseRequestId, cancellationToken);
            if (purchase is null)
            {
                return ActionResult.Fail(404, "غير موجود", "طلب الاشتراك غير موجود.");
            }

            code.Status = ActivationCodeStatus.Revoked;

            if (purchase.Status == PurchaseRequestStatus.ActivationCodeIssued)
            {
                ApplyTransition(purchase, PurchaseRequestStatus.PaymentReceived, actorUserId, now, "تم إلغاء كود التفعيل.");
            }

            AddAudit(
                actorUserId,
                "ActivationCodeRevoked",
                "ActivationCode",
                code.Id,
                $"تم إلغاء كود التفعيل للطلب {purchase.RequestNumber}.",
                new
                {
                    activationCodeId = code.Id,
                    courseId = code.CourseId,
                    purchaseRequestId = purchase.Id
                },
                ipAddress,
                now);

            await dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return ActionResult.Success();
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    public async Task<ActionResult<RedeemActivationCodeResponse>> RedeemAsync(
        Guid userId,
        string? ipAddress,
        RedeemActivationCodeRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!ActivationCodeFormat.TryNormalize(request.Code, out var canonical))
        {
            LogRedeem(userId, ipAddress, succeeded: false);
            return RedeemGenericFailure();
        }

        var hash = hasher.Hash(canonical);
        var now = timeProvider.GetUtcNow();

        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            var codeId = await dbContext.ActivationCodes
                .AsNoTracking()
                .Where(item => item.CodeHash == hash)
                .Select(item => (Guid?)item.Id)
                .FirstOrDefaultAsync(cancellationToken);

            if (codeId is null)
            {
                await transaction.RollbackAsync(cancellationToken);
                LogRedeem(userId, ipAddress, succeeded: false);
                return RedeemGenericFailure();
            }

            await LockActivationCodeAsync(codeId.Value, cancellationToken);
            var code = await dbContext.ActivationCodes
                .FirstAsync(item => item.Id == codeId.Value, cancellationToken);
            var purchase = await LoadPurchaseForUpdateAsync(code.PurchaseRequestId, cancellationToken);
            if (purchase is null)
            {
                await transaction.RollbackAsync(cancellationToken);
                LogRedeem(userId, ipAddress, succeeded: false);
                return RedeemGenericFailure();
            }

            if (code.UserId == userId &&
                code.Status == ActivationCodeStatus.Active &&
                code.UsedAt is null &&
                code.ExpiresAt is DateTimeOffset expiresAt &&
                expiresAt <= now)
            {
                await transaction.RollbackAsync(cancellationToken);
                LogRedeem(userId, ipAddress, succeeded: false);
                return ActionResult<RedeemActivationCodeResponse>.Fail(400, "غير متاح", ExpiredOwnedMessage);
            }

            if (code.Status != ActivationCodeStatus.Active ||
                code.UsedAt is not null ||
                code.UserId != userId ||
                code.ExpiresAt is DateTimeOffset expired && expired <= now ||
                purchase.Status != PurchaseRequestStatus.ActivationCodeIssued ||
                purchase.UserId != userId ||
                purchase.CourseId != code.CourseId)
            {
                await transaction.RollbackAsync(cancellationToken);
                LogRedeem(userId, ipAddress, succeeded: false);
                return RedeemGenericFailure();
            }

            var course = await dbContext.Courses.FirstAsync(item => item.Id == purchase.CourseId, cancellationToken);
            var existing = await dbContext.CourseEnrollments
                .FirstOrDefaultAsync(
                    item => item.UserId == userId && item.CourseId == purchase.CourseId,
                    cancellationToken);

            if (IsUsableActiveEnrollment(existing, now))
            {
                await transaction.RollbackAsync(cancellationToken);
                LogRedeem(userId, ipAddress, succeeded: false);
                return ActionResult<RedeemActivationCodeResponse>.Fail(409, "تعارض", AlreadyEnrolledMessage);
            }

            var enrollment = ApplyEnrollment(existing, course, purchase, code.Id, userId, now);
            code.Status = ActivationCodeStatus.Used;
            code.UsedAt = now;
            ApplyTransition(purchase, PurchaseRequestStatus.Completed, userId, now, "تم تفعيل الدورة بكود التفعيل.");
            AddAudit(
                userId,
                "CourseActivatedByCode",
                "CourseEnrollment",
                enrollment.Id,
                $"تم تفعيل الدورة عبر كود للطلب {purchase.RequestNumber}.",
                new
                {
                    activationCodeId = code.Id,
                    courseId = course.Id,
                    purchaseRequestId = purchase.Id,
                    enrollmentId = enrollment.Id
                },
                ipAddress,
                now);

            await dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            LogRedeem(userId, ipAddress, succeeded: true);

            return ActionResult<RedeemActivationCodeResponse>.Ok(new RedeemActivationCodeResponse
            {
                EnrollmentId = enrollment.Id,
                CourseId = course.Id,
                CourseTitle = course.Title,
                CourseSlug = course.Slug,
                Status = enrollment.Status.ToString(),
                ExpiresAt = enrollment.ExpiresAt
            });
        }
        catch (DbUpdateException exception) when (IsUniqueViolation(exception))
        {
            await transaction.RollbackAsync(cancellationToken);
            LogRedeem(userId, ipAddress, succeeded: false);
            return RedeemGenericFailure();
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    public async Task<ActionResult<DirectActivationResponse>> DirectActivateAsync(
        Guid purchaseRequestId,
        Guid actorUserId,
        string? ipAddress,
        CancellationToken cancellationToken = default)
    {
        var now = timeProvider.GetUtcNow();
        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            var purchase = await LoadPurchaseForUpdateAsync(purchaseRequestId, cancellationToken);
            if (purchase is null)
            {
                return ActionResult<DirectActivationResponse>.Fail(404, "غير موجود", "طلب الاشتراك غير موجود.");
            }

            if (purchase.Status != PurchaseRequestStatus.PaymentReceived)
            {
                return ActionResult<DirectActivationResponse>.Fail(
                    409,
                    "تعارض",
                    "التفعيل المباشر متاح فقط بعد تأكيد استلام الدفع.");
            }

            var userExists = await dbContext.Users.AnyAsync(user => user.Id == purchase.UserId, cancellationToken);
            var course = await dbContext.Courses.FirstOrDefaultAsync(item => item.Id == purchase.CourseId, cancellationToken);
            if (!userExists || course is null)
            {
                return ActionResult<DirectActivationResponse>.Fail(400, "غير متاح", "لا يمكن تفعيل هذا الطلب.");
            }

            var existing = await dbContext.CourseEnrollments
                .FirstOrDefaultAsync(
                    item => item.UserId == purchase.UserId && item.CourseId == purchase.CourseId,
                    cancellationToken);

            if (IsUsableActiveEnrollment(existing, now))
            {
                return ActionResult<DirectActivationResponse>.Fail(409, "تعارض", AlreadyEnrolledMessage);
            }

            var enrollment = ApplyEnrollment(existing, course, purchase, activationCodeId: null, actorUserId, now);
            ApplyTransition(purchase, PurchaseRequestStatus.Completed, actorUserId, now, "تم التفعيل المباشر بواسطة الإدارة.");
            AddAudit(
                actorUserId,
                "CourseDirectlyActivated",
                "CourseEnrollment",
                enrollment.Id,
                $"تم التفعيل المباشر للطلب {purchase.RequestNumber}.",
                new
                {
                    purchaseRequestId = purchase.Id,
                    courseId = course.Id,
                    userId = purchase.UserId,
                    enrollmentId = enrollment.Id
                },
                ipAddress,
                now);

            await dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            return ActionResult<DirectActivationResponse>.Ok(new DirectActivationResponse
            {
                EnrollmentId = enrollment.Id,
                CourseId = course.Id,
                CourseTitle = course.Title,
                CourseSlug = course.Slug,
                Status = enrollment.Status.ToString(),
                RequestNumber = purchase.RequestNumber
            });
        }
        catch (DbUpdateException exception) when (IsUniqueViolation(exception))
        {
            await transaction.RollbackAsync(cancellationToken);
            return ActionResult<DirectActivationResponse>.Fail(409, "تعارض", AlreadyEnrolledMessage);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    public async Task<PagedResponse<AdminActivationCodeSummaryResponse>> ListAdminAsync(
        int page,
        int pageSize,
        string? status,
        Guid? userId,
        Guid? courseId,
        CancellationToken cancellationToken = default)
    {
        var safePage = page < 1 ? 1 : page;
        var safeSize = pageSize < 1 ? 12 : Math.Min(pageSize, 100);
        var now = timeProvider.GetUtcNow();
        var query = dbContext.ActivationCodes.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<ActivationCodeStatus>(status, true, out var parsed))
        {
            query = parsed == ActivationCodeStatus.Expired
                ? query.Where(item =>
                    item.Status == ActivationCodeStatus.Expired ||
                    (item.Status == ActivationCodeStatus.Active && item.ExpiresAt != null && item.ExpiresAt <= now))
                : query.Where(item => item.Status == parsed);
        }

        if (userId is Guid user)
        {
            query = query.Where(item => item.UserId == user);
        }

        if (courseId is Guid course)
        {
            query = query.Where(item => item.CourseId == course);
        }

        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(item => item.CreatedAt)
            .Skip((safePage - 1) * safeSize)
            .Take(safeSize)
            .Select(item => new AdminActivationCodeSummaryResponse
            {
                Id = item.Id,
                UserId = item.UserId,
                UserEmail = item.User.Email ?? string.Empty,
                UserFullName = item.User.FullName,
                CourseId = item.CourseId,
                CourseTitle = item.Course.Title,
                PurchaseRequestId = item.PurchaseRequestId,
                RequestNumber = item.PurchaseRequest.RequestNumber,
                Status = item.Status == ActivationCodeStatus.Active && item.ExpiresAt != null && item.ExpiresAt <= now
                    ? nameof(ActivationCodeStatus.Expired)
                    : item.Status.ToString(),
                CreatedAt = item.CreatedAt,
                ExpiresAt = item.ExpiresAt,
                UsedAt = item.UsedAt,
                CreatedBy = item.CreatedBy
            })
            .ToListAsync(cancellationToken);

        return new PagedResponse<AdminActivationCodeSummaryResponse>
        {
            Items = items,
            Page = safePage,
            PageSize = safeSize,
            TotalCount = total
        };
    }

    public Task<AdminActivationCodeSummaryResponse?> GetAdminAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var now = timeProvider.GetUtcNow();
        return dbContext.ActivationCodes
            .AsNoTracking()
            .Where(item => item.Id == id)
            .Select(item => new AdminActivationCodeSummaryResponse
            {
                Id = item.Id,
                UserId = item.UserId,
                UserEmail = item.User.Email ?? string.Empty,
                UserFullName = item.User.FullName,
                CourseId = item.CourseId,
                CourseTitle = item.Course.Title,
                PurchaseRequestId = item.PurchaseRequestId,
                RequestNumber = item.PurchaseRequest.RequestNumber,
                Status = item.Status == ActivationCodeStatus.Active && item.ExpiresAt != null && item.ExpiresAt <= now
                    ? nameof(ActivationCodeStatus.Expired)
                    : item.Status.ToString(),
                CreatedAt = item.CreatedAt,
                ExpiresAt = item.ExpiresAt,
                UsedAt = item.UsedAt,
                CreatedBy = item.CreatedBy
            })
            .FirstOrDefaultAsync(cancellationToken);
    }

    private async Task<PurchaseRequest?> LoadPurchaseForUpdateAsync(Guid id, CancellationToken cancellationToken)
    {
        await dbContext.Database.ExecuteSqlAsync(
            $"""SELECT 1 FROM "PurchaseRequests" WHERE "Id" = {id} FOR UPDATE""",
            cancellationToken);
        return await dbContext.PurchaseRequests.FirstOrDefaultAsync(item => item.Id == id, cancellationToken);
    }

    private Task<int> LockActivationCodeAsync(Guid id, CancellationToken cancellationToken) =>
        dbContext.Database.ExecuteSqlAsync(
            $"""SELECT 1 FROM "ActivationCodes" WHERE "Id" = {id} FOR UPDATE""",
            cancellationToken);

    private void ApplyTransition(
        PurchaseRequest purchase,
        PurchaseRequestStatus target,
        Guid actorUserId,
        DateTimeOffset now,
        string? note)
    {
        if (!stateMachine.CanTransition(purchase.Status, target))
        {
            throw new InvalidOperationException($"Illegal purchase transition {purchase.Status} → {target}.");
        }

        var from = purchase.Status;
        purchase.Status = target;
        purchase.UpdatedAt = now;
        dbContext.PurchaseRequestEvents.Add(new PurchaseRequestEvent
        {
            Id = Guid.NewGuid(),
            PurchaseRequestId = purchase.Id,
            FromStatus = from,
            ToStatus = target,
            ActorUserId = actorUserId,
            Note = note,
            CreatedAt = now
        });
    }

    private CourseEnrollment ApplyEnrollment(
        CourseEnrollment? existing,
        Course course,
        PurchaseRequest purchase,
        Guid? activationCodeId,
        Guid actorUserId,
        DateTimeOffset now)
    {
        var expiresAt = course.AccessType == CourseAccessType.LimitedDuration && course.AccessDurationDays is int days and > 0
            ? now.AddDays(days)
            : (DateTimeOffset?)null;

        if (existing is null)
        {
            var created = new CourseEnrollment
            {
                Id = Guid.NewGuid(),
                UserId = purchase.UserId,
                CourseId = course.Id,
                PurchaseRequestId = purchase.Id,
                ActivationCodeId = activationCodeId,
                Status = EnrollmentStatus.Active,
                StartedAt = now,
                ExpiresAt = expiresAt,
                ActivatedBy = actorUserId,
                CreatedAt = now,
                UpdatedAt = now
            };
            dbContext.CourseEnrollments.Add(created);
            return created;
        }

        existing.Status = EnrollmentStatus.Active;
        existing.StartedAt = now;
        existing.ExpiresAt = expiresAt;
        existing.PurchaseRequestId = purchase.Id;
        existing.ActivationCodeId = activationCodeId;
        existing.ActivatedBy = actorUserId;
        existing.UpdatedAt = now;
        return existing;
    }

    private void AddAudit(
        Guid actorUserId,
        string action,
        string entityType,
        Guid entityId,
        string description,
        object metadata,
        string? ipAddress,
        DateTimeOffset now)
    {
        dbContext.AdminAuditLogs.Add(new AdminAuditLog
        {
            Id = Guid.NewGuid(),
            AdminUserId = actorUserId,
            Action = action,
            EntityType = entityType,
            EntityId = entityId,
            Description = description,
            MetadataJson = JsonSerializer.Serialize(metadata),
            IpAddress = ipAddress,
            CreatedAt = now
        });
    }

    private void LogRedeem(Guid userId, string? ipAddress, bool succeeded) =>
        logger.LogInformation(
            "Activation redeem attempt. UserId={UserId} Ip={Ip} Success={Success}",
            userId,
            ipAddress,
            succeeded);

    private static bool IsUsableActiveEnrollment(CourseEnrollment? enrollment, DateTimeOffset now) =>
        enrollment is { Status: EnrollmentStatus.Active } &&
        (enrollment.ExpiresAt is null || enrollment.ExpiresAt > now);

    private static ActionResult<RedeemActivationCodeResponse> RedeemGenericFailure() =>
        ActionResult<RedeemActivationCodeResponse>.Fail(400, "غير متاح", GenericRedeemMessage);

    private static bool IsUniqueViolation(DbUpdateException exception) =>
        exception.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation };
}
