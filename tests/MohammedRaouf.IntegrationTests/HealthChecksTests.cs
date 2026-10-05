using System.Net;
using MohammedRaouf.Contracts.Health;

namespace MohammedRaouf.IntegrationTests;

[Collection("Postgres")]
public class HealthChecksTests : IClassFixture<AuthApiFactory>
{
    private readonly AuthApiFactory _factory;

    public HealthChecksTests(AuthApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Live_returns_healthy_without_database_dependency_in_payload()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync("/health/live");
        var body = await response.Content.ReadAsStringAsync();
        var payload = System.Text.Json.JsonSerializer.Deserialize<HealthResponse>(body, new System.Text.Json.JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("Healthy", payload?.Status);
        Assert.DoesNotContain("Password", body, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("5433", body, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Ready_returns_ok_when_postgres_is_reachable()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync("/health/ready");
        var body = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.DoesNotContain("Password", body, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("ConnectionString", body, StringComparison.OrdinalIgnoreCase);
    }
}
