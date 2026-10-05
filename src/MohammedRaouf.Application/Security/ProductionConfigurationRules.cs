using System.Text;

namespace MohammedRaouf.Application.Security;

public static class ProductionConfigurationRules
{
    public const int MinimumJwtSigningKeyBytes = 32;

    public static readonly string[] KnownInsecureJwtKeys =
    [
        "dev-only-signing-key-change-in-user-secrets!",
        "test-only-signing-key-must-be-32chars!"
    ];

    public static bool IsJwtSigningKeyAcceptable(string? signingKey, bool production)
    {
        if (string.IsNullOrWhiteSpace(signingKey))
        {
            return false;
        }

        if (Encoding.UTF8.GetByteCount(signingKey) < MinimumJwtSigningKeyBytes)
        {
            return false;
        }

        if (production && KnownInsecureJwtKeys.Contains(signingKey, StringComparer.Ordinal))
        {
            return false;
        }

        return true;
    }

    public static bool TryValidatePublicUrl(string? publicUrl, bool production, out string error)
    {
        error = string.Empty;
        if (string.IsNullOrWhiteSpace(publicUrl))
        {
            error = "App:PublicUrl is required.";
            return false;
        }

        if (!Uri.TryCreate(publicUrl, UriKind.Absolute, out var uri) ||
            (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
        {
            error = "App:PublicUrl must be an absolute http or https URL.";
            return false;
        }

        if (production)
        {
            if (!string.Equals(uri.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase))
            {
                error = "App:PublicUrl must use https in Production.";
                return false;
            }

            if (uri.Host is "localhost" or "127.0.0.1")
            {
                error = "App:PublicUrl cannot use localhost in Production.";
                return false;
            }
        }

        return true;
    }

    public static bool IsLocalDatabaseConnection(string? connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return false;
        }

        return connectionString.Contains("localhost", StringComparison.OrdinalIgnoreCase) ||
               connectionString.Contains("127.0.0.1", StringComparison.OrdinalIgnoreCase);
    }

    public static bool IsValidCorsOrigin(string origin, bool production)
    {
        if (!Uri.TryCreate(origin, UriKind.Absolute, out var uri))
        {
            return false;
        }

        if (production)
        {
            return string.Equals(uri.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase) &&
                   uri.Host is not "localhost" and not "127.0.0.1";
        }

        return uri.Scheme is "http" or "https";
    }

    public static bool IsDevelopmentEmailSink(string? provider) =>
        string.IsNullOrWhiteSpace(provider) ||
        string.Equals(provider, "Logging", StringComparison.OrdinalIgnoreCase);
}
