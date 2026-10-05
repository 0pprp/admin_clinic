using System.Security.Claims;
using FluentValidation;
using MohammedRaouf.Application.Auth;
using MohammedRaouf.Api.Security;
using MohammedRaouf.Contracts.Auth;
using MohammedRaouf.Contracts.Profile;

namespace MohammedRaouf.Api.Endpoints;

public static class ProfileEndpoints
{
    public static IEndpointRouteBuilder MapProfileEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/profile").WithTags("Profile").RequireAuthorization();

        group.MapGet("/", GetAsync);
        group.MapPut("/", UpdateAsync);
        group.MapPut("/password", ChangePasswordAsync);

        return endpoints;
    }

    private static async Task<IResult> GetAsync(ClaimsPrincipal user, IAuthService authService)
    {
        var userId = user.GetUserId();
        if (userId is null)
        {
            return Results.Unauthorized();
        }

        return Results.Ok(await authService.GetCurrentUserAsync(userId.Value));
    }

    private static async Task<IResult> UpdateAsync(
        UpdateProfileRequest request,
        ClaimsPrincipal user,
        IValidator<UpdateProfileRequest> validator,
        IAuthService authService)
    {
        var validation = await validator.ValidateAsync(request);
        if (!validation.IsValid)
        {
            var errors = validation.Errors
                .GroupBy(error => error.PropertyName)
                .ToDictionary(group => group.Key, group => group.Select(error => error.ErrorMessage).ToArray());
            return Results.ValidationProblem(errors);
        }

        var userId = user.GetUserId();
        if (userId is null)
        {
            return Results.Unauthorized();
        }

        try
        {
            return Results.Ok(await authService.UpdateProfileAsync(userId.Value, request));
        }
        catch (InvalidOperationException exception) when (exception.Message == "PHONE_IN_USE")
        {
            return Results.Problem(statusCode: 409, title: "تعارض", detail: "رقم الهاتف مستخدم مسبقاً.");
        }
    }

    private static async Task<IResult> ChangePasswordAsync(
        ChangePasswordRequest request,
        ClaimsPrincipal user,
        IValidator<ChangePasswordRequest> validator,
        IAuthService authService,
        AuthCookieWriter cookies,
        HttpContext httpContext)
    {
        var validation = await validator.ValidateAsync(request);
        if (!validation.IsValid)
        {
            var errors = validation.Errors
                .GroupBy(error => error.PropertyName)
                .ToDictionary(group => group.Key, group => group.Select(error => error.ErrorMessage).ToArray());
            return Results.ValidationProblem(errors);
        }

        var userId = user.GetUserId();
        if (userId is null)
        {
            return Results.Unauthorized();
        }

        var result = await authService.ChangePasswordAsync(
            userId.Value,
            request,
            httpContext.Connection.RemoteIpAddress?.ToString());

        if (!result.Succeeded)
        {
            return Results.Problem(statusCode: result.StatusCode, title: result.Title, detail: result.Detail);
        }

        cookies.Clear(httpContext.Response);
        return Results.Ok(new MessageResponse { Message = "تم تغيير كلمة المرور. يرجى تسجيل الدخول مرة أخرى." });
    }
}
