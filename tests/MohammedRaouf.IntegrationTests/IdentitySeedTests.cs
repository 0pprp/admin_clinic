using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using MohammedRaouf.Application.Identity;
using MohammedRaouf.Domain.Identity;
using MohammedRaouf.Infrastructure;

namespace MohammedRaouf.IntegrationTests;

[Collection("Postgres")]
public class IdentitySeedTests
{
    private readonly PostgresFixture _fixture;

    public IdentitySeedTests(PostgresFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task Seed_creates_the_four_platform_roles()
    {
        await using var provider = BuildProvider(new Dictionary<string, string?>
        {
            ["ConnectionStrings:DefaultConnection"] = _fixture.ConnectionString
        });
        var seeder = provider.GetRequiredService<IIdentityDataSeeder>();
        await seeder.SeedAsync();

        await using var dbContext = _fixture.CreateContext();
        var roleNames = await dbContext.Roles
            .Select(role => role.Name)
            .OrderBy(name => name)
            .ToListAsync();

        Assert.Equal(RoleNames.All.OrderBy(name => name), roleNames);
    }

    [Fact]
    public async Task Seed_creates_admin_once_when_credentials_are_set_in_development()
    {
        var email = $"seed-admin-{Guid.NewGuid():N}@localhost.test";
        const string password = "SeedAdmin9local";
        var configuration = new Dictionary<string, string?>
        {
            ["ConnectionStrings:DefaultConnection"] = _fixture.ConnectionString,
            ["SEED_ADMIN_EMAIL"] = email,
            ["SEED_ADMIN_PASSWORD"] = password
        };

        await using (var provider = BuildProvider(configuration, Environments.Development))
        {
            var seeder = provider.GetRequiredService<IIdentityDataSeeder>();
            await seeder.SeedAsync();
            await seeder.SeedAsync();
        }

        await using var dbContext = _fixture.CreateContext();
        var admins = await dbContext.Users.Where(user => user.Email == email).ToListAsync();
        Assert.Single(admins);

        var adminRoleId = await dbContext.Roles
            .Where(role => role.Name == RoleNames.Admin)
            .Select(role => role.Id)
            .SingleAsync();
        Assert.True(await dbContext.UserRoles.AnyAsync(item => item.UserId == admins[0].Id && item.RoleId == adminRoleId));
    }

    [Fact]
    public async Task Seed_skips_admin_outside_development_unless_explicitly_allowed()
    {
        var email = $"prod-admin-{Guid.NewGuid():N}@localhost.test";
        var configuration = new Dictionary<string, string?>
        {
            ["ConnectionStrings:DefaultConnection"] = _fixture.ConnectionString,
            ["SEED_ADMIN_EMAIL"] = email,
            ["SEED_ADMIN_PASSWORD"] = "SeedAdmin9local"
        };

        await using (var provider = BuildProvider(configuration, Environments.Production))
        {
            await provider.GetRequiredService<IIdentityDataSeeder>().SeedAsync();
        }

        await using var dbContext = _fixture.CreateContext();
        Assert.False(await dbContext.Users.AnyAsync(user => user.Email == email));
    }

    private ServiceProvider BuildProvider(
        Dictionary<string, string?> values,
        string environmentName = "Development")
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(values)
            .Build();

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddDataProtection();
        services.AddSingleton<IConfiguration>(configuration);
        services.AddSingleton<IHostEnvironment>(new TestHostEnvironment { EnvironmentName = environmentName });
        services.AddInfrastructure(configuration);
        return services.BuildServiceProvider();
    }

    private sealed class TestHostEnvironment : IHostEnvironment
    {
        public string EnvironmentName { get; set; } = Environments.Development;

        public string ApplicationName { get; set; } = "IdentitySeedTests";

        public string ContentRootPath { get; set; } = AppContext.BaseDirectory;

        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    }
}
