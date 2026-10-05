namespace MohammedRaouf.Api.Security;

public sealed class CsrfProtectionMiddleware(
    RequestDelegate next,
    IHostEnvironment environment,
    IConfiguration configuration)
{
    private static readonly string[] SafeMethods = ["GET", "HEAD", "OPTIONS"];

    public async Task InvokeAsync(HttpContext context)
    {
        if (SafeMethods.Contains(context.Request.Method, StringComparer.OrdinalIgnoreCase) ||
            !context.Request.Path.StartsWithSegments("/api"))
        {
            await next(context);
            return;
        }

        var requestedWith = context.Request.Headers["X-Requested-With"].ToString();
        if (string.Equals(requestedWith, "XMLHttpRequest", StringComparison.OrdinalIgnoreCase))
        {
            await next(context);
            return;
        }

        var origin = context.Request.Headers.Origin.ToString();
        if (!string.IsNullOrWhiteSpace(origin) && IsAllowedOrigin(origin, context.Request))
        {
            await next(context);
            return;
        }

        context.Response.StatusCode = StatusCodes.Status403Forbidden;
        await context.Response.WriteAsJsonAsync(new
        {
            type = "https://tools.ietf.org/html/rfc9110#section-15.5.4",
            title = "ممنوع",
            status = 403,
            detail = "طلب غير مسموح.",
            correlationId = Middleware.CorrelationIdMiddleware.Read(context)
        });
    }

    private bool IsAllowedOrigin(string origin, HttpRequest request)
    {
        if (!Uri.TryCreate(origin, UriKind.Absolute, out var originUri))
        {
            return false;
        }

        var current = $"{request.Scheme}://{request.Host}".TrimEnd('/');
        if (string.Equals(origin.TrimEnd('/'), current, StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        if (PlatformCorsExtensions.ResolveAllowedOrigins(configuration, environment).Any(allowed =>
                string.Equals(allowed.TrimEnd('/'), origin.TrimEnd('/'), StringComparison.OrdinalIgnoreCase)))
        {
            return true;
        }

        return environment.IsDevelopment() && originUri.Host is "localhost" or "127.0.0.1";
    }
}
