using System.Security.Claims;
using FluentValidation;
using Microsoft.AspNetCore.RateLimiting;
using MohammedRaouf.Api.Security;
using MohammedRaouf.Application.Authorization;
using MohammedRaouf.Application.Consultations;
using MohammedRaouf.Contracts.Consultations;
using MohammedRaouf.Contracts.Public;

namespace MohammedRaouf.Api.Endpoints;

public static class ConsultationEndpoints
{
    public static IEndpointRouteBuilder MapConsultationEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/api/consultations", CreateAsync)
            .WithTags("Consultations")
            .AllowAnonymous()
            .RequireRateLimiting(RateLimitingExtensions.ConsultationPolicy);

        var admin = endpoints.MapGroup("/api/admin/consultations")
            .WithTags("AdminConsultations")
            .RequireAuthorization(AuthorizationPolicies.ManageConsultations);

        admin.MapGet("/", ListAsync);
        admin.MapGet("/{id:guid}", GetAsync);
        admin.MapPost("/{id:guid}/contacted", ContactedAsync);
        admin.MapPost("/{id:guid}/schedule", ScheduleAsync);
        admin.MapPost("/{id:guid}/complete", CompleteAsync);
        admin.MapPost("/{id:guid}/cancel", CancelAsync);
        admin.MapPost("/{id:guid}/reject", RejectAsync);
        return endpoints;
    }

    private static async Task<IResult> CreateAsync(
        CreateConsultationRequest request,
        ClaimsPrincipal user,
        IValidator<CreateConsultationRequest> validator,
        IConsultationService consultations,
        CancellationToken cancellationToken)
    {
        var invalid = await EndpointHttp.ValidateAsync(validator, request);
        if (invalid is not null)
        {
            return invalid;
        }

        return EndpointHttp.ToHttp(await consultations.CreateAsync(user.GetUserId(), request, cancellationToken));
    }

    private static Task<PagedResponse<AdminConsultationSummaryResponse>> ListAsync(
        IConsultationService consultations,
        int page = 1,
        int pageSize = 12,
        string? search = null,
        string? status = null,
        CancellationToken cancellationToken = default) =>
        consultations.ListAdminAsync(page, pageSize, search, status, cancellationToken);

    private static async Task<IResult> GetAsync(
        Guid id,
        IConsultationService consultations,
        CancellationToken cancellationToken)
    {
        var item = await consultations.GetAdminAsync(id, cancellationToken);
        return item is null ? Results.NotFound() : Results.Ok(item);
    }

    private static Task<IResult> ContactedAsync(
        Guid id,
        ConsultationNoteRequest? request,
        ClaimsPrincipal user,
        HttpContext httpContext,
        IValidator<ConsultationNoteRequest> validator,
        IConsultationService consultations,
        CancellationToken cancellationToken) =>
        NoteAsync(id, request ?? new ConsultationNoteRequest(), user, httpContext, validator,
            consultations.MarkContactedAsync, cancellationToken);

    private static async Task<IResult> ScheduleAsync(
        Guid id,
        ScheduleConsultationRequest request,
        ClaimsPrincipal user,
        HttpContext httpContext,
        IValidator<ScheduleConsultationRequest> validator,
        IConsultationService consultations,
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
            : EndpointHttp.ToHttp(await consultations.ScheduleAsync(
                actor.Value,
                id,
                request,
                httpContext.Connection.RemoteIpAddress?.ToString(),
                cancellationToken));
    }

    private static Task<IResult> CompleteAsync(
        Guid id,
        ConsultationNoteRequest? request,
        ClaimsPrincipal user,
        HttpContext httpContext,
        IValidator<ConsultationNoteRequest> validator,
        IConsultationService consultations,
        CancellationToken cancellationToken) =>
        NoteAsync(id, request ?? new ConsultationNoteRequest(), user, httpContext, validator,
            consultations.CompleteAsync, cancellationToken);

    private static Task<IResult> CancelAsync(
        Guid id,
        ConsultationNoteRequest? request,
        ClaimsPrincipal user,
        HttpContext httpContext,
        IValidator<ConsultationNoteRequest> validator,
        IConsultationService consultations,
        CancellationToken cancellationToken) =>
        NoteAsync(id, request ?? new ConsultationNoteRequest(), user, httpContext, validator,
            consultations.CancelAsync, cancellationToken);

    private static Task<IResult> RejectAsync(
        Guid id,
        ConsultationNoteRequest? request,
        ClaimsPrincipal user,
        HttpContext httpContext,
        IValidator<ConsultationNoteRequest> validator,
        IConsultationService consultations,
        CancellationToken cancellationToken) =>
        NoteAsync(id, request ?? new ConsultationNoteRequest(), user, httpContext, validator,
            consultations.RejectAsync, cancellationToken);

    private static async Task<IResult> NoteAsync(
        Guid id,
        ConsultationNoteRequest request,
        ClaimsPrincipal user,
        HttpContext httpContext,
        IValidator<ConsultationNoteRequest> validator,
        Func<Guid, Guid, ConsultationNoteRequest, string?, CancellationToken, Task<MohammedRaouf.Application.Common.ActionResult<AdminConsultationDetailResponse>>> action,
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
            : EndpointHttp.ToHttp(await action(
                actor.Value,
                id,
                request,
                httpContext.Connection.RemoteIpAddress?.ToString(),
                cancellationToken));
    }
}
