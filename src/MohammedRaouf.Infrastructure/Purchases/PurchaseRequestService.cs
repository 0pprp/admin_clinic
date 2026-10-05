using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MohammedRaouf.Application.Common;
using MohammedRaouf.Application.Purchases;
using MohammedRaouf.Contracts.Public;
using MohammedRaouf.Contracts.Purchases;
using MohammedRaouf.Domain.Entities;
using MohammedRaouf.Domain.Enums;
using MohammedRaouf.Domain.Identity;
using MohammedRaouf.Infrastructure.Persistence;
using Npgsql;

namespace MohammedRaouf.Infrastructure.Purchases;

public sealed class PurchaseRequestService(
    ApplicationDbContext dbContext,
    UserManager<ApplicationUser> userManager,
    IRequestNumberGenerator numbers) : IPurchaseRequestService
{
    public async Task<ActionResult<PurchaseRequestCreatedResponse>> CreateAsync(
        Guid userId,
        CreatePurchaseRequestRequest request,
        CancellationToken cancellationToken = default)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user is null || user.AccountStatus != AccountStatus.Active)
        {
            return ActionResult<PurchaseRequestCreatedResponse>.Fail(401, "غير مصرح", "يجب تسجيل الدخول بحساب نشط.");
        }

        if (string.IsNullOrWhiteSpace(user.FullName) ||
            string.IsNullOrWhiteSpace(user.PhoneNumber) ||
            string.IsNullOrWhiteSpace(user.WhatsAppNumber) ||
            string.IsNullOrWhiteSpace(user.Email) ||
            string.IsNullOrWhiteSpace(user.Governorate))
        {
            return ActionResult<PurchaseRequestCreatedResponse>.Fail(
                400,
                "بيانات ناقصة",
                "يرجى إكمال بيانات حسابك قبل إرسال طلب الاشتراك.");
        }

        var course = await dbContext.Courses
            .AsNoTracking()
            .FirstOrDefaultAsync(item => item.Id == request.CourseId, cancellationToken);
        if (course is null || course.Status != CourseStatus.Published)
        {
            return ActionResult<PurchaseRequestCreatedResponse>.Fail(
                400,
                "غير متاح",
                "لا يمكن إرسال طلب اشتراك لهذه الدورة.");
        }

        if (await HasBlockingRequestAsync(userId, course.Id, cancellationToken))
        {
            return DuplicateResult();
        }

        var now = DateTimeOffset.UtcNow;
        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            var purchase = new PurchaseRequest
            {
                Id = Guid.NewGuid(),
                RequestNumber = await numbers.NextAsync(cancellationToken),
                UserId = user.Id,
                CourseId = course.Id,
                FullName = user.FullName.Trim(),
                PhoneNumber = user.PhoneNumber!.Trim(),
                WhatsAppNumber = user.WhatsAppNumber.Trim(),
                Email = user.Email!.Trim(),
                Governorate = user.Governorate.Trim(),
                AmountIQD = course.PriceIQD,
                Status = PurchaseRequestStatus.Pending,
                CustomerNotes = string.IsNullOrWhiteSpace(request.CustomerNotes) ? null : request.CustomerNotes.Trim(),
                CreatedAt = now,
                UpdatedAt = now
            };

            dbContext.PurchaseRequests.Add(purchase);
            dbContext.PurchaseRequestEvents.Add(new PurchaseRequestEvent
            {
                Id = Guid.NewGuid(),
                PurchaseRequestId = purchase.Id,
                FromStatus = null,
                ToStatus = PurchaseRequestStatus.Pending,
                ActorUserId = user.Id,
                Note = "تم إنشاء طلب الاشتراك.",
                CreatedAt = now
            });

            await dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            return ActionResult<PurchaseRequestCreatedResponse>.Ok(
                new PurchaseRequestCreatedResponse
                {
                    Id = purchase.Id,
                    RequestNumber = purchase.RequestNumber,
                    CourseId = course.Id,
                    CourseTitle = course.Title,
                    AmountIQD = purchase.AmountIQD,
                    Status = purchase.Status.ToString(),
                    CreatedAt = purchase.CreatedAt
                },
                201);
        }
        catch (DbUpdateException exception) when (IsUniqueViolation(exception))
        {
            await transaction.RollbackAsync(cancellationToken);
            return DuplicateResult();
        }
    }

    public async Task<PagedResponse<StudentPurchaseRequestSummaryResponse>> ListMineAsync(
        Guid userId,
        int page,
        int pageSize,
        string? status,
        CancellationToken cancellationToken = default)
    {
        var safePage = page < 1 ? 1 : page;
        var safeSize = pageSize < 1 ? 10 : Math.Min(pageSize, 100);
        var query = dbContext.PurchaseRequests.AsNoTracking().Where(item => item.UserId == userId);

        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<PurchaseRequestStatus>(status, true, out var parsed))
        {
            query = query.Where(item => item.Status == parsed);
        }

        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(item => item.CreatedAt)
            .Skip((safePage - 1) * safeSize)
            .Take(safeSize)
            .Select(item => new StudentPurchaseRequestSummaryResponse
            {
                Id = item.Id,
                RequestNumber = item.RequestNumber,
                CourseId = item.CourseId,
                CourseTitle = item.Course.Title,
                CourseSlug = item.Course.Slug,
                AmountIQD = item.AmountIQD,
                Status = item.Status.ToString(),
                CreatedAt = item.CreatedAt
            })
            .ToListAsync(cancellationToken);

        return new PagedResponse<StudentPurchaseRequestSummaryResponse>
        {
            Items = items,
            Page = safePage,
            PageSize = safeSize,
            TotalCount = total
        };
    }

    public async Task<StudentPurchaseRequestDetailResponse?> GetMineAsync(
        Guid userId,
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await dbContext.PurchaseRequests
            .AsNoTracking()
            .Where(item => item.Id == id && item.UserId == userId)
            .Select(item => new StudentPurchaseRequestDetailResponse
            {
                Id = item.Id,
                RequestNumber = item.RequestNumber,
                CourseId = item.CourseId,
                CourseTitle = item.Course.Title,
                CourseSlug = item.Course.Slug,
                AmountIQD = item.AmountIQD,
                Status = item.Status.ToString(),
                CreatedAt = item.CreatedAt,
                ContactedAt = item.ContactedAt,
                PaymentReceivedAt = item.PaymentReceivedAt,
                CustomerNotes = item.CustomerNotes,
                HasActiveEnrollment = item.Course.Enrollments.Any(enrollment =>
                    enrollment.UserId == userId &&
                    enrollment.Status == EnrollmentStatus.Active &&
                    (enrollment.ExpiresAt == null || enrollment.ExpiresAt > DateTimeOffset.UtcNow)),
                Timeline = item.Events
                    .OrderBy(evt => evt.CreatedAt)
                    .Select(evt => new StudentPurchaseEventResponse
                    {
                        Status = evt.ToStatus.ToString(),
                        CreatedAt = evt.CreatedAt
                    })
                    .ToList()
            })
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<PaymentInstructionsResponse> GetPaymentInstructionsAsync(CancellationToken cancellationToken = default)
    {
        var keys = new[] { "PaymentMethods", "TransferInstructions", "SupportPhone", "SupportWhatsApp" };
        var rows = await dbContext.SiteSettings
            .AsNoTracking()
            .Where(setting => keys.Contains(setting.Key))
            .Select(setting => new { setting.Key, setting.ValueJson })
            .ToListAsync(cancellationToken);

        string? Read(string key)
        {
            var row = rows.FirstOrDefault(item => item.Key == key);
            if (row is null)
            {
                return null;
            }

            var json = row.ValueJson.Trim();
            if (json.Length >= 2 && json.StartsWith('"') && json.EndsWith('"'))
            {
                json = json[1..^1];
            }

            return string.IsNullOrWhiteSpace(json) || json == "{}" ? null : json;
        }

        return new PaymentInstructionsResponse
        {
            PaymentMethods = Read("PaymentMethods"),
            TransferInstructions = Read("TransferInstructions"),
            SupportPhone = Read("SupportPhone"),
            SupportWhatsApp = Read("SupportWhatsApp")
        };
    }

    private Task<bool> HasBlockingRequestAsync(Guid userId, Guid courseId, CancellationToken cancellationToken) =>
        dbContext.PurchaseRequests.AsNoTracking().AnyAsync(
            item =>
                item.UserId == userId &&
                item.CourseId == courseId &&
                PurchaseWorkflowStatuses.BlockingNewRequest.Contains(item.Status),
            cancellationToken);

    private static ActionResult<PurchaseRequestCreatedResponse> DuplicateResult() =>
        ActionResult<PurchaseRequestCreatedResponse>.Fail(
            409,
            "تعارض",
            "لديك طلب اشتراك قائم لهذه الدورة بالفعل.");

    private static bool IsUniqueViolation(DbUpdateException exception) =>
        exception.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation };
}
