using MohammedRaouf.Application.Common;
using MohammedRaouf.Contracts.Public;
using MohammedRaouf.Contracts.Purchases;

namespace MohammedRaouf.Application.Purchases;

public interface IPurchaseRequestService
{
    Task<ActionResult<PurchaseRequestCreatedResponse>> CreateAsync(
        Guid userId,
        CreatePurchaseRequestRequest request,
        CancellationToken cancellationToken = default);

    Task<PagedResponse<StudentPurchaseRequestSummaryResponse>> ListMineAsync(
        Guid userId,
        int page,
        int pageSize,
        string? status,
        CancellationToken cancellationToken = default);

    Task<StudentPurchaseRequestDetailResponse?> GetMineAsync(
        Guid userId,
        Guid id,
        CancellationToken cancellationToken = default);

    Task<PaymentInstructionsResponse> GetPaymentInstructionsAsync(CancellationToken cancellationToken = default);
}
