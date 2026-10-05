namespace MohammedRaouf.Api.Middleware;

public sealed class SecurityHeadersMiddleware(RequestDelegate next, IHostEnvironment environment)
{
    public async Task InvokeAsync(HttpContext context)
    {
        context.Response.OnStarting(() =>
        {
            var headers = context.Response.Headers;
            headers["X-Content-Type-Options"] = "nosniff";
            headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
            headers["X-Frame-Options"] = "DENY";
            headers["Permissions-Policy"] = "camera=(), microphone=(), geolocation=(), payment=()";
            headers["Content-Security-Policy"] = environment.IsDevelopment()
                ? "default-src 'none'; frame-ancestors 'none'; base-uri 'none'"
                : "default-src 'none'; frame-ancestors 'none'; base-uri 'none'; form-action 'none'";
            return Task.CompletedTask;
        });

        await next(context);
    }
}
