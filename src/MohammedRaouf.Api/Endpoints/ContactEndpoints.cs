using System.Security.Claims;
using FluentValidation;
using Microsoft.AspNetCore.RateLimiting;
using MohammedRaouf.Api.Security;
using MohammedRaouf.Application.Authorization;
using MohammedRaouf.Application.Contact;
using MohammedRaouf.Contracts.Contact;
using MohammedRaouf.Contracts.Public;

namespace MohammedRaouf.Api.Endpoints;

public static class ContactEndpoints
{
    public static IEndpointRouteBuilder MapContactEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/api/contact", CreateAsync)
            .WithTags("Contact")
            .AllowAnonymous()
            .RequireRateLimiting(RateLimitingExtensions.ContactPolicy);

        var admin = endpoints.MapGroup("/api/admin/contact-messages")
            .WithTags("AdminContact")
            .RequireAuthorization(AuthorizationPolicies.ManageConsultations);

        admin.MapGet("/", ListAsync);
        admin.MapGet("/{id:guid}", GetAsync);
        admin.MapPost("/{id:guid}/mark-read", MarkReadAsync);
        admin.MapPost("/{id:guid}/mark-replied", MarkRepliedAsync);
        admin.MapPost("/{id:guid}/archive", ArchiveAsync);
        return endpoints;
    }

    private static async Task<IResult> CreateAsync(
        CreateContactMessageRequest request,
        ClaimsPrincipal user,
        IValidator<CreateContactMessageRequest> validator,
        IContactMessageService messages,
        CancellationToken cancellationToken)
    {
        var invalid = await EndpointHttp.ValidateAsync(validator, request);
        if (invalid is not null)
        {
            return invalid;
        }

        return EndpointHttp.ToHttp(await messages.CreateAsync(user.GetUserId(), request, cancellationToken));
    }

    private static Task<PagedResponse<AdminContactMessageSummaryResponse>> ListAsync(
        IContactMessageService messages,
        int page = 1,
        int pageSize = 12,
        string? search = null,
        string? status = null,
        CancellationToken cancellationToken = default) =>
        messages.ListAdminAsync(page, pageSize, search, status, cancellationToken);

    private static async Task<IResult> GetAsync(
        Guid id,
        IContactMessageService messages,
        CancellationToken cancellationToken)
    {
        var item = await messages.GetAdminAsync(id, cancellationToken);
        return item is null ? Results.NotFound() : Results.Ok(item);
    }

    private static Task<IResult> MarkReadAsync(
        Guid id, ClaimsPrincipal user, IContactMessageService messages, CancellationToken cancellationToken) =>
        RunAsync(user, actor => messages.MarkReadAsync(actor, id, cancellationToken));

    private static Task<IResult> MarkRepliedAsync(
        Guid id, ClaimsPrincipal user, IContactMessageService messages, CancellationToken cancellationToken) =>
        RunAsync(user, actor => messages.MarkRepliedAsync(actor, id, cancellationToken));

    private static Task<IResult> ArchiveAsync(
        Guid id, ClaimsPrincipal user, IContactMessageService messages, CancellationToken cancellationToken) =>
        RunAsync(user, actor => messages.ArchiveAsync(actor, id, cancellationToken));

    private static async Task<IResult> RunAsync(
        ClaimsPrincipal user,
        Func<Guid, Task<MohammedRaouf.Application.Common.ActionResult<AdminContactMessageDetailResponse>>> action)
    {
        var userId = user.GetUserId();
        return userId is null ? Results.Unauthorized() : EndpointHttp.ToHttp(await action(userId.Value));
    }
}
