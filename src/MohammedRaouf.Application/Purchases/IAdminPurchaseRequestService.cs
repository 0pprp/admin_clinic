using MohammedRaouf.Application.Common;
using MohammedRaouf.Contracts.Public;
using MohammedRaouf.Contracts.Purchases;

namespace MohammedRaouf.Application.Purchases;

public interface IAdminPurchaseRequestService
{
    Task<PagedResponse<AdminPurchaseRequestSummaryResponse>> ListAsync(
        int page,
        int pageSize,
        string? search,
        string? status,
        Guid? courseId,
        DateTimeOffset? fromDate,
        DateTimeOffset? toDate,
        CancellationToken cancellationToken = default);

    Task<AdminPurchaseRequestDetailResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<ActionResult<AdminPurchaseRequestDetailResponse>> MarkContactedAsync(
        Guid id,
        Guid actorUserId,
        AdminPurchaseNoteRequest request,
        CancellationToken cancellationToken = default);

    Task<ActionResult<AdminPurchaseRequestDetailResponse>> MarkAwaitingPaymentAsync(
        Guid id,
        Guid actorUserId,
        AdminAwaitingPaymentRequest request,
        CancellationToken cancellationToken = default);

    Task<ActionResult<AdminPurchaseRequestDetailResponse>> ConfirmPaymentAsync(
        Guid id,
        Guid actorUserId,
        string? ipAddress,
        AdminConfirmPaymentRequest request,
        CancellationToken cancellationToken = default);

    Task<ActionResult<AdminPurchaseRequestDetailResponse>> RejectAsync(
        Guid id,
        Guid actorUserId,
        AdminPurchaseReasonRequest request,
        CancellationToken cancellationToken = default);

    Task<ActionResult<AdminPurchaseRequestDetailResponse>> CancelAsync(
        Guid id,
        Guid actorUserId,
        AdminPurchaseReasonRequest request,
        CancellationToken cancellationToken = default);
}
