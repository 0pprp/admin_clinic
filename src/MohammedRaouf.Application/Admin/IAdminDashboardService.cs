using MohammedRaouf.Application.Common;
using MohammedRaouf.Contracts.Admin;

namespace MohammedRaouf.Application.Admin;

public interface IAdminDashboardService
{
    Task<AdminDashboardSummaryResponse> GetSummaryAsync(
        IReadOnlyCollection<string> roles,
        CancellationToken cancellationToken = default);
}
