using MohammedRaouf.Application.Admin;
using MohammedRaouf.Application.Authorization;
using MohammedRaouf.Contracts.Admin;
using MohammedRaouf.Contracts.Public;

namespace MohammedRaouf.Api.Endpoints;

public static class AdminAuditLogEndpoints
{
    public static IEndpointRouteBuilder MapAdminAuditLogEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/admin/audit-logs")
            .WithTags("AdminAuditLogs")
            .RequireAuthorization(AuthorizationPolicies.ViewAuditLogs);

        group.MapGet("/", ListAsync);
        group.MapGet("/{id:guid}", GetAsync);
        return endpoints;
    }

    private static Task<PagedResponse<AdminAuditLogSummaryResponse>> ListAsync(
        IAuditLogQueryService audit,
        int page = 1,
        int pageSize = 12,
        string? action = null,
        string? entityType = null,
        Guid? adminUserId = null,
        DateTimeOffset? fromDate = null,
        DateTimeOffset? toDate = null,
        string? search = null,
        CancellationToken cancellationToken = default) =>
        audit.ListAsync(page, pageSize, action, entityType, adminUserId, fromDate, toDate, search, cancellationToken);

    private static async Task<IResult> GetAsync(
        Guid id,
        IAuditLogQueryService audit,
        CancellationToken cancellationToken)
    {
        var item = await audit.GetAsync(id, cancellationToken);
        return item is null ? Results.NotFound() : Results.Ok(item);
    }
}
