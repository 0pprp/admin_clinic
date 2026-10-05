using System.Net;
using System.Net.Http.Json;
using MohammedRaouf.Contracts.Auth;

namespace MohammedRaouf.IntegrationTests;

[Collection("Postgres")]
public class RateLimitingTests : IClassFixture<RateLimitedAuthApiFactory>
{
    private readonly RateLimitedAuthApiFactory _factory;

    public RateLimitingTests(RateLimitedAuthApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Login_returns_429_after_the_configured_limit()
    {
        var client = _factory.CreateAuthClient();
        var email = AuthTestHelpers.UniqueEmail();

        HttpResponseMessage? last = null;
        for (var attempt = 0; attempt < 3; attempt++)
        {
            last = await client.PostAsJsonAsync("/api/auth/login", new LoginRequest
            {
                Email = email,
                Password = "WrongPass1"
            });
        }

        Assert.NotNull(last);
        Assert.Equal(HttpStatusCode.TooManyRequests, last.StatusCode);
        var body = await last.Content.ReadAsStringAsync();
        Assert.Contains("429", body);
    }
}

[Collection("Postgres")]
public class ActivationRateLimitingTests : IClassFixture<ActivationRateLimitedAuthApiFactory>
{
    private readonly ActivationRateLimitedAuthApiFactory _factory;

    public ActivationRateLimitingTests(ActivationRateLimitedAuthApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Redeem_returns_429_after_the_configured_limit()
    {
        var (client, _) = await CourseTestHelpers.LoginAsRoleAsync(_factory, MohammedRaouf.Domain.Identity.RoleNames.Student);
        HttpResponseMessage? last = null;
        for (var attempt = 0; attempt < 3; attempt++)
        {
            last = await client.PostAsJsonAsync("/api/activation/redeem", new MohammedRaouf.Contracts.Activation.RedeemActivationCodeRequest
            {
                Code = "MR-AAAA-BBBB"
            });
        }

        Assert.NotNull(last);
        Assert.Equal(HttpStatusCode.TooManyRequests, last!.StatusCode);
    }
}
