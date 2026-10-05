using MohammedRaouf.Application.Security;

namespace MohammedRaouf.Api.Security;

public static class ProductionHostValidator
{
    public static void Validate(IConfiguration configuration, IHostEnvironment environment)
    {
        if (!environment.IsProduction())
        {
            return;
        }

        ValidateJwt(configuration, production: true);
        ValidatePublicUrl(configuration);
        ValidateConnectionString(configuration);
        ValidateCors(configuration);
        ValidateEmail(configuration);
        ValidateVideo(configuration);
        ValidateAdminSeed(configuration);
    }

    private static void ValidateJwt(IConfiguration configuration, bool production)
    {
        var signingKey = configuration["Jwt:SigningKey"];
        if (ProductionConfigurationRules.IsJwtSigningKeyAcceptable(signingKey, production))
        {
            return;
        }

        throw new InvalidOperationException(
            production
                ? "Production requires Jwt:SigningKey of at least 32 bytes. Do not reuse the Development placeholder."
                : "Jwt:SigningKey must be configured with at least 32 bytes.");
    }

    private static void ValidatePublicUrl(IConfiguration configuration)
    {
        var publicUrl = ResolvePublicUrl(configuration);
        if (!ProductionConfigurationRules.TryValidatePublicUrl(publicUrl, production: true, out var error))
        {
            throw new InvalidOperationException(error);
        }
    }

    private static void ValidateConnectionString(IConfiguration configuration)
    {
        var connection = configuration.GetConnectionString("DefaultConnection");
        if (string.IsNullOrWhiteSpace(connection))
        {
            throw new InvalidOperationException("Production requires ConnectionStrings:DefaultConnection from the environment.");
        }

        var allowLocal = configuration.GetValue("App:AllowLocalDatabase", false);
        if (!allowLocal && ProductionConfigurationRules.IsLocalDatabaseConnection(connection))
        {
            throw new InvalidOperationException("Production cannot use a localhost database connection.");
        }
    }

    private static void ValidateCors(IConfiguration configuration)
    {
        var origins = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
        var valid = origins
            .Where(origin => !string.IsNullOrWhiteSpace(origin))
            .Select(origin => origin.Trim().TrimEnd('/'))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
        if (valid.Length == 0 || valid.Any(origin => !ProductionConfigurationRules.IsValidCorsOrigin(origin, production: true)))
        {
            throw new InvalidOperationException("Production requires Cors:AllowedOrigins with one or more https origins.");
        }
    }

    private static void ValidateEmail(IConfiguration configuration)
    {
        var provider = configuration["Email:Provider"];
        if (ProductionConfigurationRules.IsDevelopmentEmailSink(provider))
        {
            throw new InvalidOperationException("Production cannot use the development email sink. Set Email:Provider to Disabled until SMTP is configured.");
        }
    }

    private static void ValidateVideo(IConfiguration configuration)
    {
        var provider = configuration["Video:Provider"];
        if (string.IsNullOrWhiteSpace(provider))
        {
            throw new InvalidOperationException("Production requires Video:Provider (Unavailable or BunnyStream).");
        }

        if (string.Equals(provider, "BunnyStream", StringComparison.OrdinalIgnoreCase))
        {
            if (string.IsNullOrWhiteSpace(configuration["Video:Bunny:LibraryId"]) ||
                string.IsNullOrWhiteSpace(configuration["Video:Bunny:TokenKey"]))
            {
                throw new InvalidOperationException("Video:Provider=BunnyStream requires Video:Bunny:LibraryId and Video:Bunny:TokenKey.");
            }
        }
        else if (string.Equals(provider, "SelfHostedHls", StringComparison.OrdinalIgnoreCase))
        {
            if (string.IsNullOrWhiteSpace(configuration["Video:SelfHosted:SigningKey"]))
            {
                throw new InvalidOperationException("Video:Provider=SelfHostedHls requires Video:SelfHosted:SigningKey.");
            }
        }
        else if (!string.Equals(provider, "Unavailable", StringComparison.OrdinalIgnoreCase) &&
                 !string.Equals(provider, "None", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException($"Unsupported Video:Provider '{provider}'.");
        }
    }

    private static void ValidateAdminSeed(IConfiguration configuration)
    {
        var password = configuration["SEED_ADMIN_PASSWORD"];
        var allow = configuration.GetValue("SEED_ADMIN_ALLOW_NON_DEVELOPMENT", false);
        if (!string.IsNullOrWhiteSpace(password) && !allow)
        {
            throw new InvalidOperationException(
                "SEED_ADMIN_PASSWORD is set in Production without SEED_ADMIN_ALLOW_NON_DEVELOPMENT=true. Remove the seed password after bootstrap.");
        }
    }

    public static string ResolvePublicUrl(IConfiguration configuration) =>
        (configuration["App:PublicUrl"] ?? configuration["PublicAppUrl"] ?? string.Empty).Trim();
}
