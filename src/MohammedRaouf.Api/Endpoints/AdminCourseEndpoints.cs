using System.Security.Claims;
using FluentValidation;
using MohammedRaouf.Api.Security;
using MohammedRaouf.Application.Authorization;
using MohammedRaouf.Application.Common;
using MohammedRaouf.Application.Courses;
using MohammedRaouf.Contracts.Admin;

namespace MohammedRaouf.Api.Endpoints;

public static class AdminCourseEndpoints
{
    public static IEndpointRouteBuilder MapAdminCourseEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/admin")
            .WithTags("AdminCourses")
            .RequireAuthorization(AuthorizationPolicies.ManageCourses);

        group.MapGet("/courses", ListAsync);
        group.MapPost("/courses", CreateAsync);
        group.MapGet("/courses/{id:guid}", GetAsync);
        group.MapPut("/courses/{id:guid}", UpdateAsync);
        group.MapPost("/courses/{id:guid}/publish", PublishAsync);
        group.MapPost("/courses/{id:guid}/unpublish", UnpublishAsync);
        group.MapPost("/courses/{id:guid}/archive", ArchiveAsync);

        group.MapPost("/courses/{courseId:guid}/sections", CreateSectionAsync);
        group.MapPut("/sections/{sectionId:guid}", UpdateSectionAsync);
        group.MapDelete("/sections/{sectionId:guid}", DeleteSectionAsync);
        group.MapPost("/courses/{courseId:guid}/sections/reorder", ReorderSectionsAsync);

        group.MapPost("/sections/{sectionId:guid}/lessons", CreateLessonAsync);
        group.MapPut("/lessons/{lessonId:guid}", UpdateLessonAsync);
        group.MapPost("/lessons/{lessonId:guid}/publish", PublishLessonAsync);
        group.MapPost("/lessons/{lessonId:guid}/archive", ArchiveLessonAsync);
        group.MapPost("/sections/{sectionId:guid}/lessons/reorder", ReorderLessonsAsync);

        return endpoints;
    }

    private static Task<MohammedRaouf.Contracts.Public.PagedResponse<AdminCourseSummaryResponse>> ListAsync(
        ICourseManagementService courses,
        int page = 1,
        int pageSize = 12,
        string? search = null,
        string? status = null,
        CancellationToken cancellationToken = default) =>
        courses.ListAsync(page, pageSize, search, status, cancellationToken);

    private static async Task<IResult> CreateAsync(
        SaveCourseRequest request,
        IValidator<SaveCourseRequest> validator,
        ICourseManagementService courses,
        CancellationToken cancellationToken)
    {
        var invalid = await ValidateAsync(validator, request);
        if (invalid is not null)
        {
            return invalid;
        }

        return ToHttp(await courses.CreateAsync(request, cancellationToken));
    }

    private static async Task<IResult> GetAsync(
        Guid id,
        ICourseManagementService courses,
        CancellationToken cancellationToken)
    {
        var course = await courses.GetByIdAsync(id, cancellationToken);
        return course is null ? Results.NotFound() : Results.Ok(course);
    }

    private static async Task<IResult> UpdateAsync(
        Guid id,
        SaveCourseRequest request,
        IValidator<SaveCourseRequest> validator,
        ICourseManagementService courses,
        CancellationToken cancellationToken)
    {
        var invalid = await ValidateAsync(validator, request);
        if (invalid is not null)
        {
            return invalid;
        }

        return ToHttp(await courses.UpdateAsync(id, request, cancellationToken));
    }

    private static async Task<IResult> PublishAsync(
        Guid id,
        ClaimsPrincipal user,
        HttpContext httpContext,
        ICourseManagementService courses,
        CancellationToken cancellationToken)
    {
        var userId = user.GetUserId();
        return userId is null
            ? Results.Unauthorized()
            : ToHttp(await courses.PublishAsync(id, userId.Value, httpContext.Connection.RemoteIpAddress?.ToString(), cancellationToken));
    }

    private static async Task<IResult> UnpublishAsync(
        Guid id,
        ClaimsPrincipal user,
        HttpContext httpContext,
        ICourseManagementService courses,
        CancellationToken cancellationToken)
    {
        var userId = user.GetUserId();
        return userId is null
            ? Results.Unauthorized()
            : ToHttp(await courses.UnpublishAsync(id, userId.Value, httpContext.Connection.RemoteIpAddress?.ToString(), cancellationToken));
    }

    private static async Task<IResult> ArchiveAsync(
        Guid id,
        ClaimsPrincipal user,
        HttpContext httpContext,
        ICourseManagementService courses,
        CancellationToken cancellationToken)
    {
        var userId = user.GetUserId();
        return userId is null
            ? Results.Unauthorized()
            : ToHttp(await courses.ArchiveAsync(id, userId.Value, httpContext.Connection.RemoteIpAddress?.ToString(), cancellationToken));
    }

    private static async Task<IResult> CreateSectionAsync(
        Guid courseId,
        SaveSectionRequest request,
        IValidator<SaveSectionRequest> validator,
        ICourseManagementService courses,
        CancellationToken cancellationToken)
    {
        var invalid = await ValidateAsync(validator, request);
        return invalid ?? ToHttp(await courses.CreateSectionAsync(courseId, request, cancellationToken));
    }

    private static async Task<IResult> UpdateSectionAsync(
        Guid sectionId,
        SaveSectionRequest request,
        IValidator<SaveSectionRequest> validator,
        ICourseManagementService courses,
        CancellationToken cancellationToken)
    {
        var invalid = await ValidateAsync(validator, request);
        return invalid ?? ToHttp(await courses.UpdateSectionAsync(sectionId, request, cancellationToken));
    }

    private static async Task<IResult> DeleteSectionAsync(
        Guid sectionId,
        ICourseManagementService courses,
        CancellationToken cancellationToken)
    {
        var result = await courses.DeleteSectionAsync(sectionId, cancellationToken);
        return result.Succeeded ? Results.NoContent() : ToHttp(result);
    }

    private static async Task<IResult> ReorderSectionsAsync(
        Guid courseId,
        ReorderItemsRequest request,
        IValidator<ReorderItemsRequest> validator,
        ICourseManagementService courses,
        CancellationToken cancellationToken)
    {
        var invalid = await ValidateAsync(validator, request);
        return invalid ?? ToHttp(await courses.ReorderSectionsAsync(courseId, request, cancellationToken));
    }

    private static async Task<IResult> CreateLessonAsync(
        Guid sectionId,
        SaveLessonRequest request,
        IValidator<SaveLessonRequest> validator,
        ICourseManagementService courses,
        CancellationToken cancellationToken)
    {
        var invalid = await ValidateAsync(validator, request);
        return invalid ?? ToHttp(await courses.CreateLessonAsync(sectionId, request, cancellationToken));
    }

    private static async Task<IResult> UpdateLessonAsync(
        Guid lessonId,
        SaveLessonRequest request,
        IValidator<SaveLessonRequest> validator,
        ICourseManagementService courses,
        CancellationToken cancellationToken)
    {
        var invalid = await ValidateAsync(validator, request);
        return invalid ?? ToHttp(await courses.UpdateLessonAsync(lessonId, request, cancellationToken));
    }

    private static async Task<IResult> PublishLessonAsync(Guid lessonId, ICourseManagementService courses, CancellationToken cancellationToken) =>
        ToHttp(await courses.PublishLessonAsync(lessonId, cancellationToken));

    private static async Task<IResult> ArchiveLessonAsync(Guid lessonId, ICourseManagementService courses, CancellationToken cancellationToken) =>
        ToHttp(await courses.ArchiveLessonAsync(lessonId, cancellationToken));

    private static async Task<IResult> ReorderLessonsAsync(
        Guid sectionId,
        ReorderItemsRequest request,
        IValidator<ReorderItemsRequest> validator,
        ICourseManagementService courses,
        CancellationToken cancellationToken)
    {
        var invalid = await ValidateAsync(validator, request);
        return invalid ?? ToHttp(await courses.ReorderLessonsAsync(sectionId, request, cancellationToken));
    }

    private static async Task<IResult?> ValidateAsync<T>(IValidator<T> validator, T request)
    {
        var validation = await validator.ValidateAsync(request);
        if (validation.IsValid)
        {
            return null;
        }

        var errors = validation.Errors
            .GroupBy(error => error.PropertyName)
            .ToDictionary(group => group.Key, group => group.Select(error => error.ErrorMessage).ToArray());
        return Results.ValidationProblem(errors);
    }

    private static IResult ToHttp(ActionResult result) =>
        result.Succeeded
            ? Results.Ok()
            : Results.Problem(statusCode: result.StatusCode, title: result.Title, detail: result.Detail);

    private static IResult ToHttp<T>(ActionResult<T> result)
    {
        if (!result.Succeeded)
        {
            return Results.Problem(statusCode: result.StatusCode, title: result.Title, detail: result.Detail);
        }

        return result.StatusCode switch
        {
            201 => Results.Created(string.Empty, result.Value),
            204 => Results.NoContent(),
            _ => Results.Ok(result.Value)
        };
    }
}
