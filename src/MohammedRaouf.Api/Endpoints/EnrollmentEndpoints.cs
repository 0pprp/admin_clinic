using System.Security.Claims;
using MohammedRaouf.Api.Security;
using MohammedRaouf.Application.Activation;

namespace MohammedRaouf.Api.Endpoints;

public static class EnrollmentEndpoints
{
    public static IEndpointRouteBuilder MapEnrollmentEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/api/enrollments", ListAsync)
            .WithTags("Enrollments")
            .RequireAuthorization();

        return endpoints;
    }

    private static async Task<IResult> ListAsync(
        ClaimsPrincipal user,
        IEnrollmentQueryService enrollments,
        CancellationToken cancellationToken)
    {
        var userId = user.GetUserId();
        return userId is null
            ? Results.Unauthorized()
            : Results.Ok(await enrollments.ListMineAsync(userId.Value, cancellationToken));
    }
}
