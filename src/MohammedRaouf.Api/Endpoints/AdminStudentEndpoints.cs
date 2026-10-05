using System.Security.Claims;
using MohammedRaouf.Api.Security;
using MohammedRaouf.Application.Admin;
using MohammedRaouf.Application.Authorization;
using MohammedRaouf.Contracts.Admin;
using MohammedRaouf.Contracts.Public;

namespace MohammedRaouf.Api.Endpoints;

public static class AdminStudentEndpoints
{
    public static IEndpointRouteBuilder MapAdminStudentEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var students = endpoints.MapGroup("/api/admin/students")
            .WithTags("AdminStudents")
            .RequireAuthorization(AuthorizationPolicies.ManageStudents);

        students.MapGet("/", ListAsync);
        students.MapGet("/{userId:guid}", GetAsync);
        students.MapPost("/{userId:guid}/courses/{courseId:guid}/suspend", SuspendCourseAsync);
        students.MapPost("/{userId:guid}/courses/{courseId:guid}/restore", RestoreCourseAsync);
        students.MapPost("/{userId:guid}/courses/{courseId:guid}/revoke", RevokeCourseAsync);
        students.MapPost("/{userId:guid}/suspend-account", SuspendAccountAsync);
        students.MapPost("/{userId:guid}/restore-account", RestoreAccountAsync);

        endpoints.MapGet("/api/admin/enrollments", ListEnrollmentsAsync)
            .WithTags("AdminStudents")
            .RequireAuthorization(AuthorizationPolicies.ManageStudents);

        return endpoints;
    }

    private static Task<PagedResponse<AdminStudentSummaryResponse>> ListAsync(
        IAdminStudentService students,
        int page = 1,
        int pageSize = 12,
        string? search = null,
        string? status = null,
        CancellationToken cancellationToken = default) =>
        students.ListAsync(page, pageSize, search, status, cancellationToken);

    private static async Task<IResult> GetAsync(
        Guid userId,
        IAdminStudentService students,
        CancellationToken cancellationToken)
    {
        var detail = await students.GetAsync(userId, cancellationToken);
        return detail is null ? Results.NotFound() : Results.Ok(detail);
    }

    private static Task<PagedResponse<AdminEnrollmentSummaryResponse>> ListEnrollmentsAsync(
        IAdminStudentService students,
        int page = 1,
        int pageSize = 12,
        string? status = null,
        Guid? courseId = null,
        Guid? userId = null,
        string? search = null,
        CancellationToken cancellationToken = default) =>
        students.ListEnrollmentsAsync(page, pageSize, status, courseId, userId, search, cancellationToken);

    private static Task<IResult> SuspendCourseAsync(
        Guid userId,
        Guid courseId,
        ClaimsPrincipal user,
        HttpContext httpContext,
        IAdminStudentService students,
        CancellationToken cancellationToken) =>
        RunAsync(user, httpContext, (actor, ip) => students.SuspendCourseAsync(actor, userId, courseId, ip, cancellationToken));

    private static Task<IResult> RestoreCourseAsync(
        Guid userId,
        Guid courseId,
        ClaimsPrincipal user,
        HttpContext httpContext,
        IAdminStudentService students,
        CancellationToken cancellationToken) =>
        RunAsync(user, httpContext, (actor, ip) => students.RestoreCourseAsync(actor, userId, courseId, ip, cancellationToken));

    private static Task<IResult> RevokeCourseAsync(
        Guid userId,
        Guid courseId,
        ClaimsPrincipal user,
        HttpContext httpContext,
        IAdminStudentService students,
        CancellationToken cancellationToken) =>
        RunAsync(user, httpContext, (actor, ip) => students.RevokeCourseAsync(actor, userId, courseId, ip, cancellationToken));

    private static Task<IResult> SuspendAccountAsync(
        Guid userId,
        ClaimsPrincipal user,
        HttpContext httpContext,
        IAdminStudentService students,
        CancellationToken cancellationToken) =>
        RunAsync(user, httpContext, (actor, ip) => students.SuspendAccountAsync(actor, userId, ip, cancellationToken));

    private static Task<IResult> RestoreAccountAsync(
        Guid userId,
        ClaimsPrincipal user,
        HttpContext httpContext,
        IAdminStudentService students,
        CancellationToken cancellationToken) =>
        RunAsync(user, httpContext, (actor, ip) => students.RestoreAccountAsync(actor, userId, ip, cancellationToken));

    private static async Task<IResult> RunAsync(
        ClaimsPrincipal user,
        HttpContext httpContext,
        Func<Guid, string?, Task<MohammedRaouf.Application.Common.ActionResult>> action)
    {
        var userId = user.GetUserId();
        if (userId is null)
        {
            return Results.Unauthorized();
        }

        return EndpointHttp.ToHttp(await action(userId.Value, httpContext.Connection.RemoteIpAddress?.ToString()));
    }
}
