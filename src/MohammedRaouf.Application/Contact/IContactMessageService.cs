using MohammedRaouf.Application.Common;
using MohammedRaouf.Contracts.Contact;
using MohammedRaouf.Contracts.Public;

namespace MohammedRaouf.Application.Contact;

public interface IContactMessageService
{
    Task<ActionResult<CreateContactMessageResponse>> CreateAsync(
        Guid? userId,
        CreateContactMessageRequest request,
        CancellationToken cancellationToken = default);

    Task<PagedResponse<AdminContactMessageSummaryResponse>> ListAdminAsync(
        int page,
        int pageSize,
        string? search,
        string? status,
        CancellationToken cancellationToken = default);

    Task<AdminContactMessageDetailResponse?> GetAdminAsync(Guid id, CancellationToken cancellationToken = default);

    Task<ActionResult<AdminContactMessageDetailResponse>> MarkReadAsync(Guid actorUserId, Guid id, CancellationToken cancellationToken = default);

    Task<ActionResult<AdminContactMessageDetailResponse>> MarkRepliedAsync(Guid actorUserId, Guid id, CancellationToken cancellationToken = default);

    Task<ActionResult<AdminContactMessageDetailResponse>> ArchiveAsync(Guid actorUserId, Guid id, CancellationToken cancellationToken = default);
}
