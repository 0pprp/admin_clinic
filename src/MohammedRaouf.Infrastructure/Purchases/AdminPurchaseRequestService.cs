using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using MohammedRaouf.Application.Common;
using MohammedRaouf.Application.Purchases;
using MohammedRaouf.Contracts.Public;
using MohammedRaouf.Contracts.Purchases;
using MohammedRaouf.Domain.Entities;
using MohammedRaouf.Domain.Enums;
using MohammedRaouf.Infrastructure.Persistence;

namespace MohammedRaouf.Infrastructure.Purchases;

public sealed class AdminPurchaseRequestService(
    ApplicationDbContext dbContext,
    IPurchaseRequestStateMachine stateMachine) : IAdminPurchaseRequestService
{
    public async Task<PagedResponse<AdminPurchaseRequestSummaryResponse>> ListAsync(
        int page,
        int pageSize,
        string? search,
        string? status,
        Guid? courseId,
        DateTimeOffset? fromDate,
        DateTimeOffset? toDate,
        CancellationToken cancellationToken = default)
    {
        var safePage = page < 1 ? 1 : page;
        var safeSize = pageSize < 1 ? 12 : Math.Min(pageSize, 100);
        var query = dbContext.PurchaseRequests.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<PurchaseRequestStatus>(status, true, out var parsed))
        {
            query = query.Where(item => item.Status == parsed);
        }

        if (courseId is Guid course)
        {
            query = query.Where(item => item.CourseId == course);
        }

        if (fromDate is DateTimeOffset from)
        {
            query = query.Where(item => item.CreatedAt >= from);
        }

        if (toDate is DateTimeOffset to)
        {
            query = query.Where(item => item.CreatedAt <= to);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = $"%{search.Trim()}%";
            query = query.Where(item =>
                EF.Functions.ILike(item.RequestNumber, term) ||
                EF.Functions.ILike(item.FullName, term) ||
                EF.Functions.ILike(item.PhoneNumber, term) ||
                (item.WhatsAppNumber != null && EF.Functions.ILike(item.WhatsAppNumber, term)) ||
                EF.Functions.ILike(item.Email, term) ||
                EF.Functions.ILike(item.Course.Title, term));
        }

        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(item => item.CreatedAt)
            .Skip((safePage - 1) * safeSize)
            .Take(safeSize)
            .Select(item => new AdminPurchaseRequestSummaryResponse
            {
                Id = item.Id,
                RequestNumber = item.RequestNumber,
                FullName = item.FullName,
                Email = item.Email,
                PhoneNumber = item.PhoneNumber,
                CourseTitle = item.Course.Title,
                AmountIQD = item.AmountIQD,
                Status = item.Status.ToString(),
                CreatedAt = item.CreatedAt
            })
            .ToListAsync(cancellationToken);

        return new PagedResponse<AdminPurchaseRequestSummaryResponse>
        {
            Items = items,
            Page = safePage,
            PageSize = safeSize,
            TotalCount = total
        };
    }

    public Task<AdminPurchaseRequestDetailResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        ProjectDetail(id).FirstOrDefaultAsync(cancellationToken);

    public Task<ActionResult<AdminPurchaseRequestDetailResponse>> MarkContactedAsync(
        Guid id,
        Guid actorUserId,
        AdminPurchaseNoteRequest request,
        CancellationToken cancellationToken = default) =>
        TransitionAsync(
            id,
            actorUserId,
            PurchaseRequestStatus.Contacted,
            request.Note,
            cancellationToken,
            after: purchase => purchase.ContactedAt ??= DateTimeOffset.UtcNow);

    public Task<ActionResult<AdminPurchaseRequestDetailResponse>> MarkAwaitingPaymentAsync(
        Guid id,
        Guid actorUserId,
        AdminAwaitingPaymentRequest request,
        CancellationToken cancellationToken = default) =>
        TransitionAsync(
            id,
            actorUserId,
            PurchaseRequestStatus.AwaitingPayment,
            request.AdminNote,
            cancellationToken,
            after: purchase =>
            {
                if (!string.IsNullOrWhiteSpace(request.PaymentMethod))
                {
                    purchase.PaymentMethod = request.PaymentMethod.Trim();
                }
            });

    public async Task<ActionResult<AdminPurchaseRequestDetailResponse>> ConfirmPaymentAsync(
        Guid id,
        Guid actorUserId,
        string? ipAddress,
        AdminConfirmPaymentRequest request,
        CancellationToken cancellationToken = default)
    {
        var result = await TransitionAsync(
            id,
            actorUserId,
            PurchaseRequestStatus.PaymentReceived,
            request.AdminNotes,
            cancellationToken,
            after: purchase =>
            {
                purchase.PaymentMethod = request.PaymentMethod.Trim();
                purchase.PaymentReference = string.IsNullOrWhiteSpace(request.PaymentReference)
                    ? purchase.PaymentReference
                    : request.PaymentReference.Trim();
                purchase.PaymentReceivedAt = DateTimeOffset.UtcNow;
                purchase.ConfirmedBy = actorUserId;
            });

        if (!result.Succeeded || result.Value is null)
        {
            return result;
        }

        dbContext.AdminAuditLogs.Add(new AdminAuditLog
        {
            Id = Guid.NewGuid(),
            AdminUserId = actorUserId,
            Action = "PurchasePaymentConfirmed",
            EntityType = "PurchaseRequest",
            EntityId = result.Value.Id,
            Description = $"تم تأكيد استلام الدفع للطلب {result.Value.RequestNumber}.",
            MetadataJson = JsonSerializer.Serialize(new
            {
                requestNumber = result.Value.RequestNumber,
                courseId = result.Value.CourseId,
                amountIQD = result.Value.AmountIQD,
                oldStatus = "AwaitingPayment",
                newStatus = "PaymentReceived"
            }),
            IpAddress = ipAddress,
            CreatedAt = DateTimeOffset.UtcNow
        });
        await dbContext.SaveChangesAsync(cancellationToken);
        return result;
    }

    public Task<ActionResult<AdminPurchaseRequestDetailResponse>> RejectAsync(
        Guid id,
        Guid actorUserId,
        AdminPurchaseReasonRequest request,
        CancellationToken cancellationToken = default) =>
        TransitionAsync(id, actorUserId, PurchaseRequestStatus.Rejected, request.Reason, cancellationToken);

    public Task<ActionResult<AdminPurchaseRequestDetailResponse>> CancelAsync(
        Guid id,
        Guid actorUserId,
        AdminPurchaseReasonRequest request,
        CancellationToken cancellationToken = default) =>
        TransitionAsync(id, actorUserId, PurchaseRequestStatus.Cancelled, request.Reason, cancellationToken);

    private async Task<ActionResult<AdminPurchaseRequestDetailResponse>> TransitionAsync(
        Guid id,
        Guid actorUserId,
        PurchaseRequestStatus target,
        string? note,
        CancellationToken cancellationToken,
        Action<PurchaseRequest>? after = null)
    {
        var purchase = await dbContext.PurchaseRequests
            .FirstOrDefaultAsync(item => item.Id == id, cancellationToken);
        if (purchase is null)
        {
            return ActionResult<AdminPurchaseRequestDetailResponse>.Fail(404, "غير موجود", "طلب الاشتراك غير موجود.");
        }

        if (!stateMachine.CanTransition(purchase.Status, target))
        {
            return ActionResult<AdminPurchaseRequestDetailResponse>.Fail(
                409,
                "تعارض",
                "لا يمكن نقل الطلب إلى هذه الحالة.");
        }

        var now = DateTimeOffset.UtcNow;
        var from = purchase.Status;
        purchase.Status = target;
        purchase.UpdatedAt = now;
        after?.Invoke(purchase);
        if (!string.IsNullOrWhiteSpace(note))
        {
            purchase.AdminNotes = note.Trim();
        }

        dbContext.PurchaseRequestEvents.Add(new PurchaseRequestEvent
        {
            Id = Guid.NewGuid(),
            PurchaseRequestId = purchase.Id,
            FromStatus = from,
            ToStatus = target,
            ActorUserId = actorUserId,
            Note = string.IsNullOrWhiteSpace(note) ? null : note.Trim(),
            CreatedAt = now
        });

        await dbContext.SaveChangesAsync(cancellationToken);
        var detail = await ProjectDetail(purchase.Id).FirstAsync(cancellationToken);
        return ActionResult<AdminPurchaseRequestDetailResponse>.Ok(detail);
    }

    private IQueryable<AdminPurchaseRequestDetailResponse> ProjectDetail(Guid id) =>
        dbContext.PurchaseRequests
            .AsNoTracking()
            .Where(item => item.Id == id)
            .Select(item => new AdminPurchaseRequestDetailResponse
            {
                Id = item.Id,
                RequestNumber = item.RequestNumber,
                UserId = item.UserId,
                FullName = item.FullName,
                Email = item.Email,
                PhoneNumber = item.PhoneNumber,
                WhatsAppNumber = item.WhatsAppNumber,
                Governorate = item.Governorate,
                CourseId = item.CourseId,
                CourseTitle = item.Course.Title,
                CourseSlug = item.Course.Slug,
                AmountIQD = item.AmountIQD,
                PaymentMethod = item.PaymentMethod,
                PaymentReference = item.PaymentReference,
                Status = item.Status.ToString(),
                CustomerNotes = item.CustomerNotes,
                AdminNotes = item.AdminNotes,
                CreatedAt = item.CreatedAt,
                ContactedAt = item.ContactedAt,
                PaymentReceivedAt = item.PaymentReceivedAt,
                ConfirmedBy = item.ConfirmedBy,
                CanActivate = item.Status == PurchaseRequestStatus.PaymentReceived,
                Timeline = item.Events
                    .OrderBy(evt => evt.CreatedAt)
                    .Select(evt => new AdminPurchaseEventResponse
                    {
                        Id = evt.Id,
                        FromStatus = evt.FromStatus == null ? null : evt.FromStatus.ToString(),
                        ToStatus = evt.ToStatus.ToString(),
                        ActorUserId = evt.ActorUserId,
                        Note = evt.Note,
                        CreatedAt = evt.CreatedAt
                    })
                    .ToList()
            });
}
