using System.Security.Claims;
using FluentValidation;
using MohammedRaouf.Api.Security;
using MohammedRaouf.Application.Common;
using MohammedRaouf.Application.Purchases;
using MohammedRaouf.Contracts.Purchases;

namespace MohammedRaouf.Api.Endpoints;

public static class PurchaseRequestEndpoints
{
    public static IEndpointRouteBuilder MapPurchaseRequestEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/purchase-requests")
            .WithTags("PurchaseRequests")
            .RequireAuthorization();

        group.MapPost("/", CreateAsync);
        group.MapGet("/", ListAsync);
        group.MapGet("/payment-instructions", GetPaymentInstructionsAsync);
        group.MapGet("/{id:guid}", GetAsync);

        return endpoints;
    }

    private static async Task<IResult> CreateAsync(
        CreatePurchaseRequestRequest request,
        ClaimsPrincipal user,
        IValidator<CreatePurchaseRequestRequest> validator,
        IPurchaseRequestService purchases,
        CancellationToken cancellationToken)
    {
        var invalid = await ValidateAsync(validator, request);
        if (invalid is not null)
        {
            return invalid;
        }

        var userId = user.GetUserId();
        if (userId is null)
        {
            return Results.Unauthorized();
        }

        return ToHttp(await purchases.CreateAsync(userId.Value, request, cancellationToken));
    }

    private static async Task<IResult> ListAsync(
        ClaimsPrincipal user,
        IPurchaseRequestService purchases,
        int page = 1,
        int pageSize = 10,
        string? status = null,
        CancellationToken cancellationToken = default)
    {
        var userId = user.GetUserId();
        if (userId is null)
        {
            return Results.Unauthorized();
        }

        return Results.Ok(await purchases.ListMineAsync(userId.Value, page, pageSize, status, cancellationToken));
    }

    private static Task<PaymentInstructionsResponse> GetPaymentInstructionsAsync(
        IPurchaseRequestService purchases,
        CancellationToken cancellationToken) =>
        purchases.GetPaymentInstructionsAsync(cancellationToken);

    private static async Task<IResult> GetAsync(
        Guid id,
        ClaimsPrincipal user,
        IPurchaseRequestService purchases,
        CancellationToken cancellationToken)
    {
        var userId = user.GetUserId();
        if (userId is null)
        {
            return Results.Unauthorized();
        }

        var detail = await purchases.GetMineAsync(userId.Value, id, cancellationToken);
        return detail is null ? Results.NotFound() : Results.Ok(detail);
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

    private static IResult ToHttp<T>(ActionResult<T> result)
    {
        if (!result.Succeeded)
        {
            return Results.Problem(statusCode: result.StatusCode, title: result.Title, detail: result.Detail);
        }

        return result.StatusCode == 201
            ? Results.Created($"/api/purchase-requests", result.Value)
            : Results.Ok(result.Value);
    }
}
