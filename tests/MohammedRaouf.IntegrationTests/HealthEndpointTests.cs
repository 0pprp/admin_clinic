using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using MohammedRaouf.Contracts.Health;

namespace MohammedRaouf.IntegrationTests;

public class HealthEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public HealthEndpointTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Health_returns_ok_and_healthy_status()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/health");
        var live = await client.GetAsync("/health/live");
        var payload = await response.Content.ReadFromJsonAsync<HealthResponse>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(HttpStatusCode.OK, live.StatusCode);
        Assert.NotNull(payload);
        Assert.Equal("Healthy", payload.Status);
        Assert.Equal("MohammedRaouf.Api", payload.Service);
    }
}
