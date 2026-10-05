using System.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using MohammedRaouf.Api.Security;
using MohammedRaouf.Application.Auth;
using MohammedRaouf.Application.Security;

namespace MohammedRaouf.IntegrationTests;

[Collection("Postgres")]
public class CookieSecurityTests : IClassFixture<AuthApiFactory>
{
    private readonly AuthApiFactory _factory;

    public CookieSecurityTests(AuthApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Login_cookies_are_http_only_and_same_site_lax()
    {
        var client = _factory.CreateAuthClient();
        var request = AuthTestHelpers.NewRegisterRequest();
        await AuthTestHelpers.RegisterAsync(client, request);

        var response = await AuthTestHelpers.LoginAsync(client, request.Email, request.Password);
        var cookies = AuthTestHelpers.SetCookies(response);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains(cookies, cookie => IsProtectedAuthCookie(cookie, "mr_access"));
        Assert.Contains(cookies, cookie => IsProtectedAuthCookie(cookie, "mr_refresh"));
        Assert.DoesNotContain(cookies, cookie =>
            cookie.StartsWith("mr_access=", StringComparison.OrdinalIgnoreCase) &&
            cookie.Contains("secure", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Production_cookie_options_require_secure()
    {
        var writer = new AuthCookieWriter(
            Options.Create(new AuthCookieOptions()),
            new FakeHostEnvironment(Environments.Production));
        var context = new DefaultHttpContext();
        writer.Write(context.Response, new IssuedAuthCookies
        {
            AccessToken = "access",
            RefreshToken = "refresh",
            AccessExpiresAt = DateTimeOffset.UtcNow.AddMinutes(15),
            RefreshExpiresAt = DateTimeOffset.UtcNow.AddDays(14)
        });

        var cookies = context.Response.Headers.SetCookie.ToString();
        Assert.Contains("httponly", cookies, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("samesite=lax", cookies, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("secure", cookies, StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsProtectedAuthCookie(string cookie, string name) =>
        cookie.StartsWith(name + "=", StringComparison.OrdinalIgnoreCase) &&
        cookie.Contains("httponly", StringComparison.OrdinalIgnoreCase) &&
        cookie.Contains("samesite=lax", StringComparison.OrdinalIgnoreCase);

    private sealed class FakeHostEnvironment(string environmentName) : IHostEnvironment
    {
        public string EnvironmentName { get; set; } = environmentName;

        public string ApplicationName { get; set; } = "Tests";

        public string ContentRootPath { get; set; } = AppContext.BaseDirectory;

        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    }
}
