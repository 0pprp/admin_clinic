using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MohammedRaouf.Application.Security;
using MohammedRaouf.Infrastructure.Persistence;

namespace MohammedRaouf.IntegrationTests;

public class AuthApiFactory : WebApplicationFactory<Program>
{
    protected virtual string EnvironmentName => "Development";

    protected virtual int LoginPermitLimit => 1000;

    protected virtual int ActivationRedeemPermitLimit => 1000;

    protected virtual int ContactPermitLimit => 1000;

    protected virtual int ConsultationPermitLimit => 1000;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment(EnvironmentName);
        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(BuildTestSettings());
        });
        builder.ConfigureServices(services =>
        {
            RemoveDbContext(services);
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseNpgsql(PostgresFixture.BuildConnectionString()));
            services.PostConfigure<RateLimitingOptions>(options =>
            {
                options.LoginPermitLimit = LoginPermitLimit;
                options.LoginWindowSeconds = 60;
                options.RegisterPermitLimit = 1000;
                options.RegisterWindowSeconds = 60;
                options.ForgotPasswordPermitLimit = 1000;
                options.ForgotPasswordWindowSeconds = 60;
                options.ResetPasswordPermitLimit = 1000;
                options.ResetPasswordWindowSeconds = 60;
                options.RefreshPermitLimit = 1000;
                options.RefreshWindowSeconds = 60;
                options.ActivationRedeemPermitLimit = ActivationRedeemPermitLimit;
                options.ActivationRedeemWindowSeconds = 60;
                options.ProgressPermitLimit = 1000;
                options.ProgressWindowSeconds = 60;
                options.ConsultationPermitLimit = ConsultationPermitLimit;
                options.ConsultationWindowSeconds = 60;
                options.ContactPermitLimit = ContactPermitLimit;
                options.ContactWindowSeconds = 60;
            });
        });
    }

    public HttpClient CreateAuthClient()
    {
        var client = CreateClient();
        client.DefaultRequestHeaders.Add("X-Requested-With", "XMLHttpRequest");
        return client;
    }

    private Dictionary<string, string?> BuildTestSettings()
    {
        var settings = new Dictionary<string, string?>
        {
            ["ConnectionStrings:DefaultConnection"] = PostgresFixture.BuildConnectionString(),
            ["Jwt:Issuer"] = "MohammedRaouf",
            ["Jwt:Audience"] = "MohammedRaouf.Web"
        };
        foreach (var pair in ExtraSettings())
        {
            settings[pair.Key] = pair.Value;
        }

        return settings;
    }

    protected virtual Dictionary<string, string?> ExtraSettings() => new()
    {
        ["Jwt:SigningKey"] = "test-only-signing-key-must-be-32chars!",
        ["PublicAppUrl"] = "http://localhost:3000",
        ["App:PublicUrl"] = "http://localhost:3000",
        ["Email:Provider"] = "Logging",
        ["Video:Provider"] = "Unavailable"
    };

    private static void RemoveDbContext(IServiceCollection services)
    {
        var descriptors = services
            .Where(descriptor =>
                descriptor.ServiceType == typeof(ApplicationDbContext) ||
                descriptor.ServiceType == typeof(DbContextOptions) ||
                descriptor.ServiceType == typeof(DbContextOptions<ApplicationDbContext>))
            .ToList();

        foreach (var descriptor in descriptors)
        {
            services.Remove(descriptor);
        }
    }
}

public sealed class RateLimitedAuthApiFactory : AuthApiFactory
{
    protected override int LoginPermitLimit => 2;
}

public sealed class ActivationRateLimitedAuthApiFactory : AuthApiFactory
{
    protected override int ActivationRedeemPermitLimit => 2;
}

public sealed class ContactRateLimitedAuthApiFactory : AuthApiFactory
{
    protected override int ContactPermitLimit => 2;
}

public sealed class ConsultationRateLimitedAuthApiFactory : AuthApiFactory
{
    protected override int ConsultationPermitLimit => 2;
}
