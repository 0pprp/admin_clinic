using MohammedRaouf.Application.Common;
using MohammedRaouf.Contracts.Activation;
using MohammedRaouf.Contracts.Public;

namespace MohammedRaouf.Application.Activation;

public interface IActivationWorkflowService
{
    Task<ActionResult<IssuedActivationCodeResponse>> IssueAsync(
        Guid purchaseRequestId,
        Guid actorUserId,
        string? ipAddress,
        CancellationToken cancellationToken = default);

    Task<ActionResult> RevokeAsync(
        Guid activationCodeId,
        Guid actorUserId,
        string? ipAddress,
        CancellationToken cancellationToken = default);

    Task<ActionResult<RedeemActivationCodeResponse>> RedeemAsync(
        Guid userId,
        string? ipAddress,
        RedeemActivationCodeRequest request,
        CancellationToken cancellationToken = default);

    Task<ActionResult<DirectActivationResponse>> DirectActivateAsync(
        Guid purchaseRequestId,
        Guid actorUserId,
        string? ipAddress,
        CancellationToken cancellationToken = default);

    Task<PagedResponse<AdminActivationCodeSummaryResponse>> ListAdminAsync(
        int page,
        int pageSize,
        string? status,
        Guid? userId,
        Guid? courseId,
        CancellationToken cancellationToken = default);

    Task<AdminActivationCodeSummaryResponse?> GetAdminAsync(
        Guid id,
        CancellationToken cancellationToken = default);
}
