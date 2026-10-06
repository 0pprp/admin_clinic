using System.Security.Claims;
using FluentValidation;
using Microsoft.AspNetCore.RateLimiting;
using MohammedRaouf.Application.Auth;
using MohammedRaouf.Api.Security;
using MohammedRaouf.Contracts.Auth;

namespace MohammedRaouf.Api.Endpoints;

public static class AuthEndpoints
{
    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/auth").WithTags("Auth");

        group.MapPost("/register", RegisterAsync)
            .RequireRateLimiting(RateLimitingExtensions.RegisterPolicy)
            .AllowAnonymous();

        group.MapPost("/login", LoginAsync)
            .RequireRateLimiting(RateLimitingExtensions.LoginPolicy)
            .AllowAnonymous();

        group.MapPost("/google", GoogleLoginAsync)
            .RequireRateLimiting(RateLimitingExtensions.LoginPolicy)
            .AllowAnonymous();

        group.MapPost("/logout", LogoutAsync)
            .AllowAnonymous();

        group.MapPost("/logout-all", LogoutAllAsync)
            .RequireAuthorization();

        group.MapPost("/refresh", RefreshAsync)
            .RequireRateLimiting(RateLimitingExtensions.RefreshPolicy)
            .AllowAnonymous();

        group.MapGet("/me", MeAsync)
            .RequireAuthorization();

        group.MapPost("/forgot-password", ForgotPasswordAsync)
            .RequireRateLimiting(RateLimitingExtensions.ForgotPasswordPolicy)
            .AllowAnonymous();

        group.MapPost("/reset-password", ResetPasswordAsync)
            .RequireRateLimiting(RateLimitingExtensions.ResetPasswordPolicy)
            .AllowAnonymous();

        group.MapPost("/resend-verification", ResendVerificationAsync)
            .RequireRateLimiting(RateLimitingExtensions.ForgotPasswordPolicy)
            .AllowAnonymous();

        group.MapPost("/verify-email", VerifyEmailAsync)
            .RequireRateLimiting(RateLimitingExtensions.ResetPasswordPolicy)
            .AllowAnonymous();

        return endpoints;
    }

    private static async Task<IResult> RegisterAsync(
        RegisterRequest request,
        IValidator<RegisterRequest> validator,
        IAuthService authService)
    {
        var validation = await validator.ValidateAsync(request);
        if (!validation.IsValid)
        {
            return ValidationProblem(validation);
        }

        var result = await authService.RegisterAsync(request);
        return result.Succeeded
            ? Results.Created("/api/auth/me", result.Value)
            : ToProblem(result);
    }

    private static async Task<IResult> LoginAsync(
        LoginRequest request,
        IValidator<LoginRequest> validator,
        IAuthService authService,
        AuthCookieWriter cookies,
        HttpContext httpContext)
    {
        var validation = await validator.ValidateAsync(request);
        if (!validation.IsValid)
        {
            return ValidationProblem(validation);
        }

        var result = await authService.LoginAsync(request, httpContext.Connection.RemoteIpAddress?.ToString());
        if (!result.Succeeded)
        {
            return ToProblem(result);
        }

        cookies.Write(httpContext.Response, result.Cookies!);
        return Results.Ok(result.Value);
    }

    private static async Task<IResult> GoogleLoginAsync(
        GoogleLoginRequest request,
        IAuthService authService,
        HttpContext httpContext)
    {
        _ = httpContext;
        var result = await authService.GoogleLoginAsync(request, httpContext.Connection.RemoteIpAddress?.ToString());
        return result.Succeeded
            ? Results.Ok(result.Value)
            : ToProblem(result);
    }

    private static async Task<IResult> LogoutAsync(
        IAuthService authService,
        AuthCookieWriter cookies,
        HttpContext httpContext)
    {
        await authService.LogoutAsync(cookies.ReadRefreshToken(httpContext.Request), httpContext.Connection.RemoteIpAddress?.ToString());
        cookies.Clear(httpContext.Response);
        return Results.NoContent();
    }

    private static async Task<IResult> LogoutAllAsync(
        ClaimsPrincipal user,
        IAuthService authService,
        AuthCookieWriter cookies,
        HttpContext httpContext)
    {
        var userId = user.GetUserId();
        if (userId is null)
        {
            return Results.Unauthorized();
        }

        await authService.LogoutAllAsync(userId.Value, httpContext.Connection.RemoteIpAddress?.ToString());
        cookies.Clear(httpContext.Response);
        return Results.NoContent();
    }

    private static async Task<IResult> RefreshAsync(
        IAuthService authService,
        AuthCookieWriter cookies,
        HttpContext httpContext)
    {
        var result = await authService.RefreshAsync(
            cookies.ReadRefreshToken(httpContext.Request),
            httpContext.Connection.RemoteIpAddress?.ToString());

        if (!result.Succeeded)
        {
            cookies.Clear(httpContext.Response);
            return ToProblem(result);
        }

        cookies.Write(httpContext.Response, result.Cookies!);
        return Results.Ok(result.Value);
    }

    private static async Task<IResult> MeAsync(ClaimsPrincipal user, IAuthService authService)
    {
        var userId = user.GetUserId();
        if (userId is null)
        {
            return Results.Unauthorized();
        }

        return Results.Ok(await authService.GetCurrentUserAsync(userId.Value));
    }

    private static async Task<IResult> ForgotPasswordAsync(
        ForgotPasswordRequest request,
        IValidator<ForgotPasswordRequest> validator,
        IAuthService authService)
    {
        var validation = await validator.ValidateAsync(request);
        if (!validation.IsValid)
        {
            return ValidationProblem(validation);
        }

        await authService.ForgotPasswordAsync(request.Email);
        return Results.Ok(new MessageResponse
        {
            Message = "إذا كان البريد مسجلاً لدينا، فسيتم إرسال رمز استعادة كلمة المرور."
        });
    }

    private static async Task<IResult> ResetPasswordAsync(
        ResetPasswordRequest request,
        IValidator<ResetPasswordRequest> validator,
        IAuthService authService,
        AuthCookieWriter cookies,
        HttpContext httpContext)
    {
        var validation = await validator.ValidateAsync(request);
        if (!validation.IsValid)
        {
            return ValidationProblem(validation);
        }

        var result = await authService.ResetPasswordAsync(request, httpContext.Connection.RemoteIpAddress?.ToString());
        if (!result.Succeeded)
        {
            return ToProblem(result);
        }

        cookies.Clear(httpContext.Response);
        return Results.Ok(new MessageResponse { Message = "تم تعيين كلمة المرور. يمكنك تسجيل الدخول الآن." });
    }

    private static async Task<IResult> ResendVerificationAsync(
        ResendVerificationRequest request,
        IAuthService authService)
    {
        if (string.IsNullOrWhiteSpace(request.Email))
        {
            return Results.Ok(new MessageResponse { Message = "إذا كان البريد مسجلاً لدينا، فسيتم إرسال رمز التأكيد." });
        }

        await authService.ResendVerificationAsync(request.Email, request.Purpose);
        return Results.Ok(new MessageResponse { Message = "إذا كان البريد مسجلاً لدينا، فسيتم إرسال رمز التأكيد." });
    }

    private static async Task<IResult> VerifyEmailAsync(
        VerifyEmailRequest request,
        IAuthService authService,
        AuthCookieWriter cookies,
        HttpContext httpContext)
    {
        var result = await authService.VerifyEmailAsync(
            request,
            httpContext.Connection.RemoteIpAddress?.ToString());

        if (!result.Succeeded)
        {
            return ToProblem(result);
        }

        if (result.Cookies is not null)
        {
            cookies.Write(httpContext.Response, result.Cookies);
        }

        return Results.Ok(result.Value);
    }

    private static IResult ValidationProblem(FluentValidation.Results.ValidationResult validation)
    {
        var errors = validation.Errors
            .GroupBy(error => error.PropertyName)
            .ToDictionary(group => group.Key, group => group.Select(error => error.ErrorMessage).ToArray());

        return Results.ValidationProblem(errors);
    }

    private static IResult ToProblem(AuthCommandResult result) =>
        Results.Problem(statusCode: result.StatusCode, title: result.Title, detail: result.Detail);
}
