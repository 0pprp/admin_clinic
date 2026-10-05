using System.Security.Claims;
using FluentValidation;
using MohammedRaouf.Api.Security;
using MohammedRaouf.Application.Admin;
using MohammedRaouf.Application.Authorization;
using MohammedRaouf.Contracts.Admin;
using MohammedRaouf.Contracts.Public;

namespace MohammedRaouf.Api.Endpoints;

public static class AdminUserEndpoints
{
    public static IEndpointRouteBuilder MapAdminUserEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/admin/users")
            .WithTags("AdminUsers")
            .RequireAuthorization(AuthorizationPolicies.ManageUsers);

        group.MapGet("/", ListAsync);
        group.MapPost("/{userId:guid}/roles", UpdateRolesAsync);
        return endpoints;
    }

    private static Task<PagedResponse<AdminStudentSummaryResponse>> ListAsync(
        IAdminUserService users,
        int page = 1,
        int pageSize = 12,
        string? search = null,
        string? status = null,
        CancellationToken cancellationToken = default) =>
        users.ListAsync(page, pageSize, search, status, cancellationToken);

    private static async Task<IResult> UpdateRolesAsync(
        Guid userId,
        UpdateRolesRequest request,
        ClaimsPrincipal user,
        HttpContext httpContext,
        IValidator<UpdateRolesRequest> validator,
        IAdminUserService users,
        CancellationToken cancellationToken)
    {
        var invalid = await EndpointHttp.ValidateAsync(validator, request);
        if (invalid is not null)
        {
            return invalid;
        }

        var actor = user.GetUserId();
        return actor is null
            ? Results.Unauthorized()
            : EndpointHttp.ToHttp(await users.UpdateRolesAsync(
                actor.Value,
                userId,
                request.Roles,
                httpContext.Connection.RemoteIpAddress?.ToString(),
                cancellationToken));
    }
}
