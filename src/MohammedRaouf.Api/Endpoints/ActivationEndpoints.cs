using System.Security.Claims;
using FluentValidation;
using Microsoft.AspNetCore.RateLimiting;
using MohammedRaouf.Api.Security;
using MohammedRaouf.Application.Activation;
using MohammedRaouf.Application.Authorization;
using MohammedRaouf.Application.Common;
using MohammedRaouf.Contracts.Activation;

namespace MohammedRaouf.Api.Endpoints;

public static class ActivationEndpoints
{
    public static IEndpointRouteBuilder MapActivationEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/api/activation/redeem", RedeemAsync)
            .WithTags("Activation")
            .RequireAuthorization()
            .RequireRateLimiting(RateLimitingExtensions.ActivationRedeemPolicy);

        var adminPurchase = endpoints.MapGroup("/api/admin/purchase-requests")
            .WithTags("AdminActivation")
            .RequireAuthorization(AuthorizationPolicies.ManageActivations);
        adminPurchase.MapPost("/{id:guid}/issue-activation-code", IssueAsync);
        adminPurchase.MapPost("/{id:guid}/direct-activate", DirectActivateAsync);

        var adminCodes = endpoints.MapGroup("/api/admin/activation-codes")
            .WithTags("AdminActivation")
            .RequireAuthorization(AuthorizationPolicies.ManageActivations);
        adminCodes.MapGet("/", ListAsync);
        adminCodes.MapGet("/{id:guid}", GetAsync);
        adminCodes.MapPost("/{id:guid}/revoke", RevokeAsync);

        return endpoints;
    }

    private static async Task<IResult> RedeemAsync(
        RedeemActivationCodeRequest request,
        ClaimsPrincipal user,
        HttpContext httpContext,
        IValidator<RedeemActivationCodeRequest> validator,
        IActivationWorkflowService activations,
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
            : ToHttp(await activations.RedeemAsync(
                userId.Value,
                httpContext.Connection.RemoteIpAddress?.ToString(),
                request,
                cancellationToken));
    }

    private static async Task<IResult> IssueAsync(
        Guid id,
        ClaimsPrincipal user,
        HttpContext httpContext,
        IActivationWorkflowService activations,
        CancellationToken cancellationToken)
    {
        var userId = user.GetUserId();
        return userId is null
            ? Results.Unauthorized()
            : ToHttp(await activations.IssueAsync(
                id,
                userId.Value,
                httpContext.Connection.RemoteIpAddress?.ToString(),
                cancellationToken));
    }

    private static async Task<IResult> DirectActivateAsync(
        Guid id,
        ClaimsPrincipal user,
        HttpContext httpContext,
        IActivationWorkflowService activations,
        CancellationToken cancellationToken)
    {
        var userId = user.GetUserId();
        return userId is null
            ? Results.Unauthorized()
            : ToHttp(await activations.DirectActivateAsync(
                id,
                userId.Value,
                httpContext.Connection.RemoteIpAddress?.ToString(),
                cancellationToken));
    }

    private static Task<MohammedRaouf.Contracts.Public.PagedResponse<AdminActivationCodeSummaryResponse>> ListAsync(
        IActivationWorkflowService activations,
        int page = 1,
        int pageSize = 12,
        string? status = null,
        Guid? userId = null,
        Guid? courseId = null,
        CancellationToken cancellationToken = default) =>
        activations.ListAdminAsync(page, pageSize, status, userId, courseId, cancellationToken);

    private static async Task<IResult> GetAsync(
        Guid id,
        IActivationWorkflowService activations,
        CancellationToken cancellationToken)
    {
        var detail = await activations.GetAdminAsync(id, cancellationToken);
        return detail is null ? Results.NotFound() : Results.Ok(detail);
    }

    private static async Task<IResult> RevokeAsync(
        Guid id,
        ClaimsPrincipal user,
        HttpContext httpContext,
        IActivationWorkflowService activations,
        CancellationToken cancellationToken)
    {
        var userId = user.GetUserId();
        if (userId is null)
        {
            return Results.Unauthorized();
        }

        var result = await activations.RevokeAsync(
            id,
            userId.Value,
            httpContext.Connection.RemoteIpAddress?.ToString(),
            cancellationToken);
        return result.Succeeded
            ? Results.Ok()
            : Results.Problem(statusCode: result.StatusCode, title: result.Title, detail: result.Detail);
    }

    private static async Task<IResult?> ValidateAsync(IValidator<RedeemActivationCodeRequest> validator, RedeemActivationCodeRequest request)
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
