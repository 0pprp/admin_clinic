using System.Net;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MohammedRaouf.Infrastructure.Persistence;

namespace MohammedRaouf.IntegrationTests;

[Collection("Postgres")]
public class RefreshLogoutTests : IClassFixture<AuthApiFactory>
{
    private readonly AuthApiFactory _factory;

    public RefreshLogoutTests(AuthApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Refresh_rotates_token_and_revokes_the_old_one()
    {
        var client = _factory.CreateAuthClient();
        var request = AuthTestHelpers.NewRegisterRequest();
        await AuthTestHelpers.RegisterAsync(client, request);

        var login = await AuthTestHelpers.LoginAsync(client, request.Email, request.Password, rememberMe: true);
        var oldRefresh = AuthTestHelpers.ReadCookieValue(AuthTestHelpers.SetCookies(login), "mr_refresh");
        Assert.False(string.IsNullOrWhiteSpace(oldRefresh));

        var refresh = await client.PostAsync("/api/auth/refresh", null);
        var newRefresh = AuthTestHelpers.ReadCookieValue(AuthTestHelpers.SetCookies(refresh), "mr_refresh");

        Assert.Equal(HttpStatusCode.OK, refresh.StatusCode);
        Assert.False(string.IsNullOrWhiteSpace(newRefresh));
        Assert.NotEqual(oldRefresh, newRefresh);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var oldHash = Infrastructure.Auth.TokenHashing.Hash(oldRefresh!);
        var stored = await db.RefreshTokens.SingleAsync(token => token.TokenHash == oldHash);
        Assert.NotNull(stored.RevokedAt);
    }

    [Fact]
    public async Task Reused_refresh_token_revokes_the_family()
    {
        var client = _factory.CreateAuthClient();
        var request = AuthTestHelpers.NewRegisterRequest();
        await AuthTestHelpers.RegisterAsync(client, request);

        var login = await AuthTestHelpers.LoginAsync(client, request.Email, request.Password, rememberMe: true);
        var oldRefresh = AuthTestHelpers.ReadCookieValue(AuthTestHelpers.SetCookies(login), "mr_refresh")!;

        var firstRefresh = await client.PostAsync("/api/auth/refresh", null);
        var rotatedRefresh = AuthTestHelpers.ReadCookieValue(AuthTestHelpers.SetCookies(firstRefresh), "mr_refresh")!;
        Assert.Equal(HttpStatusCode.OK, firstRefresh.StatusCode);

        using var reuseClient = _factory.CreateAuthClient();
        using var reuseRequest = AuthTestHelpers.CreateRefreshRequest(oldRefresh);
        var reused = await reuseClient.SendAsync(reuseRequest);

        Assert.Equal(HttpStatusCode.Unauthorized, reused.StatusCode);

        using var rotatedClient = _factory.CreateAuthClient();
        using var rotatedRequest = AuthTestHelpers.CreateRefreshRequest(rotatedRefresh);
        var rotatedReuse = await rotatedClient.SendAsync(rotatedRequest);
        Assert.Equal(HttpStatusCode.Unauthorized, rotatedReuse.StatusCode);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var familyId = await db.RefreshTokens
            .Where(token => token.TokenHash == Infrastructure.Auth.TokenHashing.Hash(oldRefresh))
            .Select(token => token.FamilyId)
            .SingleAsync();
        var activeInFamily = await db.RefreshTokens.CountAsync(token =>
            token.FamilyId == familyId && token.RevokedAt == null);
        Assert.Equal(0, activeInFamily);
    }

    [Fact]
    public async Task Logout_clears_cookies_and_revokes_refresh_token()
    {
        var client = _factory.CreateAuthClient();
        var request = AuthTestHelpers.NewRegisterRequest();
        await AuthTestHelpers.RegisterAsync(client, request);

        var login = await AuthTestHelpers.LoginAsync(client, request.Email, request.Password);
        var refresh = AuthTestHelpers.ReadCookieValue(AuthTestHelpers.SetCookies(login), "mr_refresh")!;

        var logout = await client.PostAsync("/api/auth/logout", null);
        Assert.Equal(HttpStatusCode.NoContent, logout.StatusCode);

        var logoutCookies = AuthTestHelpers.SetCookies(logout);
        Assert.Contains(logoutCookies, cookie =>
            cookie.StartsWith("mr_access=", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(logoutCookies, cookie =>
            cookie.StartsWith("mr_refresh=", StringComparison.OrdinalIgnoreCase));

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var stored = await db.RefreshTokens.SingleAsync(token =>
            token.TokenHash == Infrastructure.Auth.TokenHashing.Hash(refresh));
        Assert.NotNull(stored.RevokedAt);

        using var anonymous = _factory.CreateAuthClient();
        var me = await anonymous.GetAsync("/api/auth/me");
        Assert.Equal(HttpStatusCode.Unauthorized, me.StatusCode);
    }
}
