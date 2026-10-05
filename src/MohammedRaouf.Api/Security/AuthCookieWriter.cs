using Microsoft.Extensions.Options;
using MohammedRaouf.Application.Auth;
using MohammedRaouf.Application.Security;

namespace MohammedRaouf.Api.Security;

public sealed class AuthCookieWriter(IOptions<AuthCookieOptions> cookieOptions, IHostEnvironment environment)
{
    public void Write(HttpResponse response, IssuedAuthCookies cookies)
    {
        var options = cookieOptions.Value;
        response.Cookies.Append(options.AccessCookieName, cookies.AccessToken, CreateCookieOptions(cookies.AccessExpiresAt));
        response.Cookies.Append(options.RefreshCookieName, cookies.RefreshToken, CreateCookieOptions(cookies.RefreshExpiresAt));
    }

    public void Clear(HttpResponse response)
    {
        var options = cookieOptions.Value;
        var expired = CreateCookieOptions(DateTimeOffset.UtcNow.AddDays(-1));
        response.Cookies.Delete(options.AccessCookieName, expired);
        response.Cookies.Delete(options.RefreshCookieName, expired);
    }

    public string? ReadRefreshToken(HttpRequest request) =>
        request.Cookies[cookieOptions.Value.RefreshCookieName];

    private CookieOptions CreateCookieOptions(DateTimeOffset expiresAt)
    {
        return new CookieOptions
        {
            HttpOnly = true,
            Secure = !environment.IsDevelopment(),
            SameSite = SameSiteMode.Lax,
            Path = cookieOptions.Value.Path,
            Expires = expiresAt,
            IsEssential = true
        };
    }
}
