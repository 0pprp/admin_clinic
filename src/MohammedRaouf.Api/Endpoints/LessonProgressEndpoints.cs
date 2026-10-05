using System.Security.Claims;
using FluentValidation;
using Microsoft.AspNetCore.RateLimiting;
using MohammedRaouf.Api.Security;
using MohammedRaouf.Application.Common;
using MohammedRaouf.Application.Learning;
using MohammedRaouf.Contracts.Learning;

namespace MohammedRaouf.Api.Endpoints;

public static class LessonProgressEndpoints
{
    public static IEndpointRouteBuilder MapLessonProgressEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/progress")
            .WithTags("LessonProgress")
            .RequireAuthorization()
            .RequireRateLimiting(RateLimitingExtensions.ProgressPolicy);

        group.MapGet("/{lessonId:guid}", GetAsync);
        group.MapPut("/{lessonId:guid}", UpdateAsync);
        group.MapPost("/{lessonId:guid}/complete", CompleteAsync);

        return endpoints;
    }

    private static async Task<IResult> GetAsync(
        Guid lessonId,
        ClaimsPrincipal user,
        ILessonProgressService progress,
        CancellationToken cancellationToken)
    {
        var userId = user.GetUserId();
        return userId is null
            ? Results.Unauthorized()
            : ToHttp(await progress.GetAsync(userId.Value, user.GetRoles(), lessonId, cancellationToken));
    }

    private static async Task<IResult> UpdateAsync(
        Guid lessonId,
        UpdateLessonProgressRequest request,
        ClaimsPrincipal user,
        IValidator<UpdateLessonProgressRequest> validator,
        ILessonProgressService progress,
        CancellationToken cancellationToken)
    {
        var validation = await validator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
        {
            var errors = validation.Errors
                .GroupBy(error => error.PropertyName)
                .ToDictionary(group => group.Key, group => group.Select(error => error.ErrorMessage).ToArray());
            return Results.ValidationProblem(errors);
        }

        var userId = user.GetUserId();
        return userId is null
            ? Results.Unauthorized()
            : ToHttp(await progress.UpdateWatchedSecondsAsync(
                userId.Value,
                user.GetRoles(),
                lessonId,
                request.WatchedSeconds,
                cancellationToken));
    }

    private static async Task<IResult> CompleteAsync(
        Guid lessonId,
        ClaimsPrincipal user,
        ILessonProgressService progress,
        CancellationToken cancellationToken)
    {
        var userId = user.GetUserId();
        return userId is null
            ? Results.Unauthorized()
            : ToHttp(await progress.CompleteAsync(userId.Value, user.GetRoles(), lessonId, cancellationToken));
    }

    private static IResult ToHttp<T>(ActionResult<T> result) =>
        result.Succeeded
            ? Results.Ok(result.Value)
            : Results.Problem(statusCode: result.StatusCode, title: result.Title, detail: result.Detail);
}
