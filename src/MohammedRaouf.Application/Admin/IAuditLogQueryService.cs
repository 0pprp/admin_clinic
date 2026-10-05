using MohammedRaouf.Contracts.Admin;
using MohammedRaouf.Contracts.Public;

namespace MohammedRaouf.Application.Admin;

public interface IAuditLogQueryService
{
    Task<PagedResponse<AdminAuditLogSummaryResponse>> ListAsync(
        int page,
        int pageSize,
        string? action,
        string? entityType,
        Guid? adminUserId,
        DateTimeOffset? fromDate,
        DateTimeOffset? toDate,
        string? search,
        CancellationToken cancellationToken = default);

    Task<AdminAuditLogDetailResponse?> GetAsync(Guid id, CancellationToken cancellationToken = default);
}
