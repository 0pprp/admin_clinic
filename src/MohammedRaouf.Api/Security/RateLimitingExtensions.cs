using System.Security.Claims;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Options;
using MohammedRaouf.Application.Security;

namespace MohammedRaouf.Api.Security;

public static class RateLimitingExtensions
{
    public const string LoginPolicy = "login";
    public const string RegisterPolicy = "register";
    public const string ForgotPasswordPolicy = "forgot-password";
    public const string ResetPasswordPolicy = "reset-password";
    public const string RefreshPolicy = "refresh";
    public const string ActivationRedeemPolicy = "activation-redeem";
    public const string ProgressPolicy = "lesson-progress";
    public const string ConsultationPolicy = "consultation";
    public const string ContactPolicy = "contact";

    public static IServiceCollection AddPlatformRateLimiting(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<RateLimitingOptions>(configuration.GetSection(RateLimitingOptions.SectionName));

        services.AddRateLimiter(limiter =>
        {
            limiter.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            limiter.OnRejected = async (context, cancellationToken) =>
            {
                context.HttpContext.Response.ContentType = "application/problem+json";
                await context.HttpContext.Response.WriteAsJsonAsync(new
                {
                    type = "https://tools.ietf.org/html/rfc6585#section-4",
                    title = "تم تجاوز الحد المسموح",
                    status = 429,
                    detail = "محاولات كثيرة. حاول مرة أخرى بعد قليل."
                }, cancellationToken);
            };

            limiter.AddPolicy(LoginPolicy, context => Partition(context, options => (options.LoginPermitLimit, options.LoginWindowSeconds)));
            limiter.AddPolicy(RegisterPolicy, context => Partition(context, options => (options.RegisterPermitLimit, options.RegisterWindowSeconds)));
            limiter.AddPolicy(ForgotPasswordPolicy, context => Partition(context, options => (options.ForgotPasswordPermitLimit, options.ForgotPasswordWindowSeconds)));
            limiter.AddPolicy(ResetPasswordPolicy, context => Partition(context, options => (options.ResetPasswordPermitLimit, options.ResetPasswordWindowSeconds)));
            limiter.AddPolicy(RefreshPolicy, context => Partition(context, options => (options.RefreshPermitLimit, options.RefreshWindowSeconds)));
            limiter.AddPolicy(ActivationRedeemPolicy, context => Partition(
                context,
                options => (options.ActivationRedeemPermitLimit, options.ActivationRedeemWindowSeconds),
                includeUser: true));
            limiter.AddPolicy(ProgressPolicy, context => Partition(
                context,
                options => (options.ProgressPermitLimit, options.ProgressWindowSeconds),
                includeUser: true));
            limiter.AddPolicy(ConsultationPolicy, context => Partition(
                context,
                options => (options.ConsultationPermitLimit, options.ConsultationWindowSeconds)));
            limiter.AddPolicy(ContactPolicy, context => Partition(
                context,
                options => (options.ContactPermitLimit, options.ContactWindowSeconds)));
        });

        return services;
    }

    private static RateLimitPartition<string> Partition(
        HttpContext context,
        Func<RateLimitingOptions, (int PermitLimit, int WindowSeconds)> selector,
        bool includeUser = false)
    {
        var options = context.RequestServices.GetRequiredService<IOptions<RateLimitingOptions>>().Value;
        var (permitLimit, windowSeconds) = selector(options);
        var ip = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var user = includeUser
            ? context.User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "anon"
            : null;
        var key = user is null ? ip : $"{user}:{ip}";
        return RateLimitPartition.GetFixedWindowLimiter(key, _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = permitLimit,
            Window = TimeSpan.FromSeconds(windowSeconds),
            QueueLimit = 0,
            AutoReplenishment = true
        });
    }
}
