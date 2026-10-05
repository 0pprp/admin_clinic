using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using MohammedRaouf.Application.Authorization;
using MohammedRaouf.Application.Security;
using MohammedRaouf.Domain.Enums;
using MohammedRaouf.Domain.Identity;
using MohammedRaouf.Infrastructure.Auth;

namespace MohammedRaouf.Api.Security;

public static class AuthenticationExtensions
{
    public static IServiceCollection AddPlatformAuthentication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddOptions<JwtOptions>()
            .Bind(configuration.GetSection(JwtOptions.SectionName))
            .Validate(
                options => ProductionConfigurationRules.IsJwtSigningKeyAcceptable(options.SigningKey, production: false),
                "Jwt:SigningKey must be configured with at least 32 bytes.")
            .ValidateOnStart();

        services.Configure<AuthCookieOptions>(configuration.GetSection(AuthCookieOptions.SectionName));
        services.Configure<RateLimitingOptions>(configuration.GetSection(RateLimitingOptions.SectionName));

        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer();

        services.AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
            .Configure<IOptions<JwtOptions>, IOptions<AuthCookieOptions>>(ConfigureJwtBearer);

        services.AddAuthorization(options =>
        {
            foreach (var pair in AuthorizationPolicies.RoleMap)
            {
                options.AddPolicy(pair.Key, policy =>
                    policy.RequireAuthenticatedUser().RequireRole(pair.Value));
            }
        });

        return services;
    }

    private static void ConfigureJwtBearer(
        JwtBearerOptions options,
        IOptions<JwtOptions> jwtAccessor,
        IOptions<AuthCookieOptions> cookieAccessor)
    {
        var jwt = jwtAccessor.Value;
        var cookieNames = cookieAccessor.Value;
        var signingKey = JwtSigning.CreateKey(jwt.SigningKey);

        options.MapInboundClaims = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwt.Issuer,
            ValidAudience = jwt.Audience,
            IssuerSigningKey = signingKey,
            ClockSkew = TimeSpan.FromSeconds(30),
            RoleClaimType = ClaimTypes.Role,
            NameClaimType = ClaimTypes.NameIdentifier
        };

        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                if (context.Request.Cookies.TryGetValue(cookieNames.AccessCookieName, out var token))
                {
                    context.Token = token;
                }

                return Task.CompletedTask;
            },
            OnTokenValidated = async context =>
            {
                var userManager = context.HttpContext.RequestServices.GetRequiredService<UserManager<ApplicationUser>>();
                var userId = context.Principal?.FindFirstValue(ClaimTypes.NameIdentifier)
                    ?? context.Principal?.FindFirstValue("sub");
                if (userId is null)
                {
                    context.Fail("Missing subject.");
                    return;
                }

                var user = await userManager.FindByIdAsync(userId);
                if (user is null || user.AccountStatus != AccountStatus.Active)
                {
                    context.Fail("Account is not active.");
                    return;
                }

                var stamp = context.Principal?.FindFirstValue("sid");
                if (!string.IsNullOrWhiteSpace(user.SecurityStamp) && stamp != user.SecurityStamp)
                {
                    context.Fail("Security stamp mismatch.");
                }
            },
            OnChallenge = async context =>
            {
                context.HandleResponse();
                if (context.Response.HasStarted)
                {
                    return;
                }

                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsJsonAsync(new ProblemDetails
                {
                    Status = StatusCodes.Status401Unauthorized,
                    Title = "غير مصرح",
                    Detail = "يجب تسجيل الدخول.",
                    Type = "https://tools.ietf.org/html/rfc9110#section-15.5.2",
                    Extensions =
                    {
                        ["correlationId"] = Middleware.CorrelationIdMiddleware.Read(context.HttpContext)
                    }
                });
            },
            OnForbidden = async context =>
            {
                if (context.Response.HasStarted)
                {
                    return;
                }

                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                await context.Response.WriteAsJsonAsync(new ProblemDetails
                {
                    Status = StatusCodes.Status403Forbidden,
                    Title = "ممنوع",
                    Detail = "ليست لديك صلاحية لتنفيذ هذا الإجراء.",
                    Type = "https://tools.ietf.org/html/rfc9110#section-15.5.4",
                    Extensions =
                    {
                        ["correlationId"] = Middleware.CorrelationIdMiddleware.Read(context.HttpContext)
                    }
                });
            }
        };
    }
}
