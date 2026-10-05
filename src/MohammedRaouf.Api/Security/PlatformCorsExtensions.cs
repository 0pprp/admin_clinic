using MohammedRaouf.Application.Security;

namespace MohammedRaouf.Api.Security;

public static class PlatformCorsExtensions
{
    public const string PolicyName = "Platform";

    public static IReadOnlyList<string> ResolveAllowedOrigins(IConfiguration configuration, IHostEnvironment environment)
    {
        var configured = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
        var origins = configured
            .Where(origin => !string.IsNullOrWhiteSpace(origin))
            .Select(origin => origin.Trim().TrimEnd('/'))
            .Where(origin => ProductionConfigurationRules.IsValidCorsOrigin(origin, environment.IsProduction()))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (origins.Count == 0 && environment.IsDevelopment())
        {
            origins.AddRange(
            [
                "http://localhost:3000",
                "http://localhost:3001",
                "http://127.0.0.1:3000",
                "http://127.0.0.1:3001"
            ]);
        }

        return origins;
    }

    public static IServiceCollection AddPlatformCors(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        var origins = ResolveAllowedOrigins(configuration, environment);
        services.Configure<PlatformCorsOptions>(options =>
        {
            options.AllowedOrigins = origins.ToList();
        });
        services.AddCors(options =>
        {
            options.AddPolicy(PolicyName, policy =>
            {
                policy.SetIsOriginAllowed(origin =>
                    {
                        if (string.IsNullOrWhiteSpace(origin))
                        {
                            return false;
                        }

                        var allowed = ResolveAllowedOrigins(configuration, environment);
                        var normalized = origin.Trim().TrimEnd('/');
                        return allowed.Any(item =>
                            string.Equals(item, normalized, StringComparison.OrdinalIgnoreCase));
                    })
                    .WithHeaders("Content-Type", "X-Requested-With", "X-Correlation-ID")
                    .WithMethods("GET", "POST", "PUT", "PATCH", "DELETE", "OPTIONS")
                    .AllowCredentials();
            });
        });
        return services;
    }
}
