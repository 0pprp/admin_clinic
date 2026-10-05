using System.Security.Claims;
using MohammedRaouf.Api.Security;
using MohammedRaouf.Application.Common;
using MohammedRaouf.Application.Learning;

namespace MohammedRaouf.Api.Endpoints;

public static class StudentLearningEndpoints
{
    public static IEndpointRouteBuilder MapStudentLearningEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api")
            .WithTags("StudentLearning")
            .RequireAuthorization();

        group.MapGet("/dashboard/summary", SummaryAsync);
        group.MapGet("/student/courses/{courseSlug}", GetCourseAsync);
        group.MapGet("/student/courses/{courseSlug}/lessons/{lessonId:guid}", GetLessonAsync);

        return endpoints;
    }

    private static async Task<IResult> SummaryAsync(
        ClaimsPrincipal user,
        IStudentLearningService learning,
        CancellationToken cancellationToken)
    {
        var userId = user.GetUserId();
        return userId is null
            ? Results.Unauthorized()
            : Results.Ok(await learning.GetSummaryAsync(userId.Value, cancellationToken));
    }

    private static async Task<IResult> GetCourseAsync(
        string courseSlug,
        ClaimsPrincipal user,
        IStudentLearningService learning,
        CancellationToken cancellationToken)
    {
        var userId = user.GetUserId();
        return userId is null
            ? Results.Unauthorized()
            : ToHttp(await learning.GetCourseAsync(userId.Value, user.GetRoles(), courseSlug, cancellationToken));
    }

    private static async Task<IResult> GetLessonAsync(
        string courseSlug,
        Guid lessonId,
        ClaimsPrincipal user,
        IStudentLearningService learning,
        CancellationToken cancellationToken)
    {
        var userId = user.GetUserId();
        return userId is null
            ? Results.Unauthorized()
            : ToHttp(await learning.GetLessonAsync(userId.Value, user.GetRoles(), courseSlug, lessonId, cancellationToken));
    }

    private static IResult ToHttp<T>(ActionResult<T> result) =>
        result.Succeeded
            ? Results.Ok(result.Value)
            : Results.Problem(statusCode: result.StatusCode, title: result.Title, detail: result.Detail);
}
