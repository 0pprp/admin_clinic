using System.Security.Claims;
using FluentValidation;
using MohammedRaouf.Api.Security;
using MohammedRaouf.Application.Authorization;
using MohammedRaouf.Application.Common;
using MohammedRaouf.Application.Purchases;
using MohammedRaouf.Contracts.Purchases;

namespace MohammedRaouf.Api.Endpoints;

public static class AdminPurchaseRequestEndpoints
{
    public static IEndpointRouteBuilder MapAdminPurchaseRequestEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/admin/purchase-requests")
            .WithTags("AdminPurchaseRequests")
            .RequireAuthorization(AuthorizationPolicies.ManagePayments);

        group.MapGet("/", ListAsync);
        group.MapGet("/{id:guid}", GetAsync);
        group.MapPost("/{id:guid}/contacted", ContactedAsync);
        group.MapPost("/{id:guid}/awaiting-payment", AwaitingPaymentAsync);
        group.MapPost("/{id:guid}/confirm-payment", ConfirmPaymentAsync);
        group.MapPost("/{id:guid}/reject", RejectAsync);
        group.MapPost("/{id:guid}/cancel", CancelAsync);

        return endpoints;
    }

    private static Task<MohammedRaouf.Contracts.Public.PagedResponse<AdminPurchaseRequestSummaryResponse>> ListAsync(
        IAdminPurchaseRequestService purchases,
        int page = 1,
        int pageSize = 12,
        string? search = null,
        string? status = null,
        Guid? courseId = null,
        DateTimeOffset? fromDate = null,
        DateTimeOffset? toDate = null,
        CancellationToken cancellationToken = default) =>
        purchases.ListAsync(page, pageSize, search, status, courseId, fromDate, toDate, cancellationToken);

    private static async Task<IResult> GetAsync(
        Guid id,
        IAdminPurchaseRequestService purchases,
        CancellationToken cancellationToken)
    {
        var detail = await purchases.GetByIdAsync(id, cancellationToken);
        return detail is null ? Results.NotFound() : Results.Ok(detail);
    }

    private static async Task<IResult> ContactedAsync(
        Guid id,
        AdminPurchaseNoteRequest request,
        ClaimsPrincipal user,
        IValidator<AdminPurchaseNoteRequest> validator,
        IAdminPurchaseRequestService purchases,
        CancellationToken cancellationToken)
    {
        var invalid = await ValidateAsync(validator, request);
        if (invalid is not null)
        {
            return invalid;
        }

        var userId = user.GetUserId();
        return userId is null
            ? Results.Unauthorized()
            : ToHttp(await purchases.MarkContactedAsync(id, userId.Value, request, cancellationToken));
    }

    private static async Task<IResult> AwaitingPaymentAsync(
        Guid id,
        AdminAwaitingPaymentRequest request,
        ClaimsPrincipal user,
        IValidator<AdminAwaitingPaymentRequest> validator,
        IAdminPurchaseRequestService purchases,
        CancellationToken cancellationToken)
    {
        var invalid = await ValidateAsync(validator, request);
        if (invalid is not null)
        {
            return invalid;
        }

        var userId = user.GetUserId();
        return userId is null
            ? Results.Unauthorized()
            : ToHttp(await purchases.MarkAwaitingPaymentAsync(id, userId.Value, request, cancellationToken));
    }

    private static async Task<IResult> ConfirmPaymentAsync(
        Guid id,
        AdminConfirmPaymentRequest request,
        ClaimsPrincipal user,
        HttpContext httpContext,
        IValidator<AdminConfirmPaymentRequest> validator,
        IAdminPurchaseRequestService purchases,
        CancellationToken cancellationToken)
    {
        var invalid = await ValidateAsync(validator, request);
        if (invalid is not null)
        {
            return invalid;
        }

        var userId = user.GetUserId();
        return userId is null
            ? Results.Unauthorized()
            : ToHttp(await purchases.ConfirmPaymentAsync(
                id,
                userId.Value,
                httpContext.Connection.RemoteIpAddress?.ToString(),
                request,
                cancellationToken));
    }

    private static async Task<IResult> RejectAsync(
        Guid id,
        AdminPurchaseReasonRequest request,
        ClaimsPrincipal user,
        IValidator<AdminPurchaseReasonRequest> validator,
        IAdminPurchaseRequestService purchases,
        CancellationToken cancellationToken)
    {
        var invalid = await ValidateAsync(validator, request);
        if (invalid is not null)
        {
            return invalid;
        }

        var userId = user.GetUserId();
        return userId is null
            ? Results.Unauthorized()
            : ToHttp(await purchases.RejectAsync(id, userId.Value, request, cancellationToken));
    }

    private static async Task<IResult> CancelAsync(
        Guid id,
        AdminPurchaseReasonRequest request,
        ClaimsPrincipal user,
        IValidator<AdminPurchaseReasonRequest> validator,
        IAdminPurchaseRequestService purchases,
        CancellationToken cancellationToken)
    {
        var invalid = await ValidateAsync(validator, request);
        if (invalid is not null)
        {
            return invalid;
        }

        var userId = user.GetUserId();
        return userId is null
            ? Results.Unauthorized()
            : ToHttp(await purchases.CancelAsync(id, userId.Value, request, cancellationToken));
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

    private static IResult ToHttp<T>(ActionResult<T> result) =>
        result.Succeeded
            ? Results.Ok(result.Value)
            : Results.Problem(statusCode: result.StatusCode, title: result.Title, detail: result.Detail);
}
