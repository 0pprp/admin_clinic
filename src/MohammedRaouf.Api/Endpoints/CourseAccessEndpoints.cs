using System.Security.Claims;
using MohammedRaouf.Api.Security;
using MohammedRaouf.Application.Courses;
using MohammedRaouf.Contracts.Courses;

namespace MohammedRaouf.Api.Endpoints;

public static class CourseAccessEndpoints
{
    public static IEndpointRouteBuilder MapCourseAccessEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var lessons = endpoints.MapGroup("/api/lessons").WithTags("Lessons");
        lessons.MapGet("/{lessonId:guid}", GetLessonAsync).AllowAnonymous();
        lessons.MapGet("/{lessonId:guid}/playback", GetPlaybackAsync).AllowAnonymous();

        endpoints.MapGet("/api/courses/{courseId:guid}/outline", GetOutlineAsync)
            .WithTags("Courses")
            .RequireAuthorization();

        return endpoints;
    }

    private static async Task<IResult> GetLessonAsync(
        Guid lessonId,
        ClaimsPrincipal user,
        ICourseAccessService access,
        ICourseQueryService queries,
        CancellationToken cancellationToken)
    {
        var decision = await access.EvaluateLessonAccessAsync(user.GetUserId(), lessonId, user.GetRoles(), cancellationToken);
        var denied = ToAccessResult(decision);
        if (denied is not null)
        {
            return denied;
        }

        var lesson = await queries.GetLessonMetadataAsync(lessonId, user.GetUserId(), cancellationToken);
        return lesson is null ? Results.NotFound() : Results.Ok(lesson);
    }

    private static async Task<IResult> GetPlaybackAsync(
        Guid lessonId,
        ClaimsPrincipal user,
        ICourseAccessService access,
        IVideoPlaybackService playback,
        CancellationToken cancellationToken)
    {
        var decision = await access.EvaluateLessonAccessAsync(user.GetUserId(), lessonId, user.GetRoles(), cancellationToken);
        var denied = ToAccessResult(decision);
        if (denied is not null)
        {
            return denied;
        }

        var result = await playback.GetPlaybackAsync(lessonId, cancellationToken);
        if (result is null)
        {
            return Results.NotFound();
        }

        return Results.Ok(new LessonPlaybackResponse
        {
            LessonId = lessonId,
            PlaybackUnavailable = result.PlaybackUnavailable,
            Message = result.Message,
            PlaybackUrl = result.PlaybackUrl,
            ExpiresAt = result.ExpiresAt,
            Provider = result.Provider,
            Kind = result.Kind
        });
    }

    private static async Task<IResult> GetOutlineAsync(
        Guid courseId,
        ClaimsPrincipal user,
        ICourseAccessService access,
        ICourseQueryService queries,
        CancellationToken cancellationToken)
    {
        var userId = user.GetUserId();
        if (userId is null)
        {
            return Results.Unauthorized();
        }

        var outline = await queries.GetStudentOutlineAsync(courseId, cancellationToken);
        if (outline is null)
        {
            return Results.NotFound();
        }

        if (!await access.CanUserAccessCourseAsync(userId.Value, courseId, user.GetRoles(), cancellationToken))
        {
            return Results.Forbid();
        }

        return Results.Ok(outline);
    }

    private static IResult? ToAccessResult(LessonAccessDecision decision) =>
        decision.Status switch
        {
            LessonAccessStatus.Allowed => null,
            LessonAccessStatus.NotFound => Results.NotFound(),
            LessonAccessStatus.Unauthorized => Results.Unauthorized(),
            LessonAccessStatus.Forbidden => Results.Forbid(),
            _ => Results.Forbid()
        };
}
