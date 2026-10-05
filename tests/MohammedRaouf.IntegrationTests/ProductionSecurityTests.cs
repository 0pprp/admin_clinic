using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using MohammedRaouf.Contracts.Health;

namespace MohammedRaouf.IntegrationTests;

[Collection("Postgres")]
public class ProductionSecurityTests : IClassFixture<ProductionApiFactory>, IClassFixture<HttpsRedirectProductionApiFactory>
{
    private readonly ProductionApiFactory _factory;
    private readonly HttpsRedirectProductionApiFactory _httpsFactory;

    public ProductionSecurityTests(ProductionApiFactory factory, HttpsRedirectProductionApiFactory httpsFactory)
    {
        _factory = factory;
        _httpsFactory = httpsFactory;
    }

    [Fact]
    public async Task Production_login_cookies_are_secure_http_only_and_same_site_lax()
    {
        var client = _factory.CreateAuthClient();
        var request = AuthTestHelpers.NewRegisterRequest();
        await AuthTestHelpers.RegisterAsync(client, request);
        var response = await AuthTestHelpers.LoginAsync(client, request.Email, request.Password);
        var cookies = AuthTestHelpers.SetCookies(response);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains(cookies, cookie =>
            cookie.StartsWith("mr_access=", StringComparison.OrdinalIgnoreCase) &&
            cookie.Contains("httponly", StringComparison.OrdinalIgnoreCase) &&
            cookie.Contains("samesite=lax", StringComparison.OrdinalIgnoreCase) &&
            cookie.Contains("secure", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(cookies, cookie =>
            cookie.StartsWith("mr_refresh=", StringComparison.OrdinalIgnoreCase) &&
            cookie.Contains("secure", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task Production_cors_does_not_allow_arbitrary_origin()
    {
        var client = _factory.CreateClient();
        using var allowed = new HttpRequestMessage(HttpMethod.Get, "/health");
        allowed.Headers.Add("Origin", "https://example.com");
        var allowedResponse = await client.SendAsync(allowed);

        using var denied = new HttpRequestMessage(HttpMethod.Get, "/health");
        denied.Headers.Add("Origin", "https://attacker.example");
        var deniedResponse = await client.SendAsync(denied);

        Assert.Equal(HttpStatusCode.OK, allowedResponse.StatusCode);
        Assert.Equal("https://example.com", allowedResponse.Headers.GetValues("Access-Control-Allow-Origin").Single());
        Assert.False(deniedResponse.Headers.Contains("Access-Control-Allow-Origin"));
        Assert.DoesNotContain(
            deniedResponse.Headers.SelectMany(header => header.Value),
            value => value.Contains('*', StringComparison.Ordinal));
    }

    [Fact]
    public async Task Production_security_headers_are_present()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync("/health");

        Assert.Equal("nosniff", response.Headers.GetValues("X-Content-Type-Options").Single());
        Assert.Equal("strict-origin-when-cross-origin", response.Headers.GetValues("Referrer-Policy").Single());
        Assert.Contains("DENY", response.Headers.GetValues("X-Frame-Options").Single());
        Assert.False(string.IsNullOrWhiteSpace(response.Headers.GetValues("Content-Security-Policy").Single()));
        Assert.DoesNotContain("*", response.Headers.GetValues("Content-Security-Policy").Single());
    }

    [Fact]
    public async Task Production_echoes_correlation_id()
    {
        var client = _factory.CreateClient();
        using var request = new HttpRequestMessage(HttpMethod.Get, "/health");
        request.Headers.Add("X-Correlation-ID", "qa-correlation-id-1");
        var response = await client.SendAsync(request);

        Assert.Equal("qa-correlation-id-1", response.Headers.GetValues("X-Correlation-ID").Single());
    }

    [Fact]
    public async Task Forwarded_proto_https_prevents_https_redirect()
    {
        var client = _httpsFactory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

        var redirected = await client.GetAsync("/health");
        Assert.Equal(HttpStatusCode.TemporaryRedirect, redirected.StatusCode);
        Assert.Contains("https://", redirected.Headers.Location?.ToString() ?? string.Empty, StringComparison.OrdinalIgnoreCase);

        using var forwarded = new HttpRequestMessage(HttpMethod.Get, "/health");
        forwarded.Headers.Host = "example.com";
        forwarded.Headers.Add("X-Forwarded-Proto", "https");
        forwarded.Headers.Add("X-Forwarded-For", "203.0.113.10");
        var ok = await client.SendAsync(forwarded);

        Assert.Equal(HttpStatusCode.OK, ok.StatusCode);
        var payload = await ok.Content.ReadFromJsonAsync<HealthResponse>();
        Assert.Equal("Healthy", payload?.Status);
        Assert.True(ok.Headers.TryGetValues("Strict-Transport-Security", out var hsts));
        Assert.Contains("max-age", hsts.Single(), StringComparison.OrdinalIgnoreCase);
    }
}
