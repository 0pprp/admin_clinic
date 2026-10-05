using System.Security.Claims;
using MohammedRaouf.Api.Security;
using MohammedRaouf.Application.Admin;
using MohammedRaouf.Application.Authorization;

namespace MohammedRaouf.Api.Endpoints;

public static class AdminDashboardEndpoints
{
    public static IEndpointRouteBuilder MapAdminDashboardEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/api/admin/dashboard/summary", SummaryAsync)
            .WithTags("AdminDashboard")
            .RequireAuthorization(AuthorizationPolicies.AccessAdminPanel);
        return endpoints;
    }

    private static Task<MohammedRaouf.Contracts.Admin.AdminDashboardSummaryResponse> SummaryAsync(
        ClaimsPrincipal user,
        IAdminDashboardService dashboard,
        CancellationToken cancellationToken) =>
        dashboard.GetSummaryAsync(user.GetRoles(), cancellationToken);
}
