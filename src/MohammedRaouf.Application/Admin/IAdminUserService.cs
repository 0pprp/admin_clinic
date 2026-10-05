using MohammedRaouf.Application.Common;
using MohammedRaouf.Contracts.Admin;
using MohammedRaouf.Contracts.Public;

namespace MohammedRaouf.Application.Admin;

public interface IAdminUserService
{
    Task<PagedResponse<AdminStudentSummaryResponse>> ListAsync(
        int page,
        int pageSize,
        string? search,
        string? status,
        CancellationToken cancellationToken = default);

    Task<ActionResult<AdminStudentSummaryResponse>> UpdateRolesAsync(
        Guid actorUserId,
        Guid userId,
        IReadOnlyList<string> roles,
        string? ip,
        CancellationToken cancellationToken = default);
}
