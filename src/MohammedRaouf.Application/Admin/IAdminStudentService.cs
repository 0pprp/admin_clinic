using MohammedRaouf.Application.Common;
using MohammedRaouf.Contracts.Admin;
using MohammedRaouf.Contracts.Public;

namespace MohammedRaouf.Application.Admin;

public interface IAdminStudentService
{
    Task<PagedResponse<AdminStudentSummaryResponse>> ListAsync(
        int page,
        int pageSize,
        string? search,
        string? status,
        CancellationToken cancellationToken = default);

    Task<AdminStudentDetailResponse?> GetAsync(Guid userId, CancellationToken cancellationToken = default);

    Task<ActionResult> SuspendCourseAsync(Guid actorUserId, Guid userId, Guid courseId, string? ip, CancellationToken cancellationToken = default);

    Task<ActionResult> RestoreCourseAsync(Guid actorUserId, Guid userId, Guid courseId, string? ip, CancellationToken cancellationToken = default);

    Task<ActionResult> RevokeCourseAsync(Guid actorUserId, Guid userId, Guid courseId, string? ip, CancellationToken cancellationToken = default);

    Task<ActionResult> SuspendAccountAsync(Guid actorUserId, Guid userId, string? ip, CancellationToken cancellationToken = default);

    Task<ActionResult> RestoreAccountAsync(Guid actorUserId, Guid userId, string? ip, CancellationToken cancellationToken = default);

    Task<PagedResponse<AdminEnrollmentSummaryResponse>> ListEnrollmentsAsync(
        int page,
        int pageSize,
        string? status,
        Guid? courseId,
        Guid? userId,
        string? search,
        CancellationToken cancellationToken = default);
}
