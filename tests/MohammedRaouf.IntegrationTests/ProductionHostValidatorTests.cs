using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using MohammedRaouf.Api.Security;
using MohammedRaouf.Application.Security;

namespace MohammedRaouf.IntegrationTests;

public class ProductionHostValidatorTests
{
    [Fact]
    public void Production_fails_without_jwt_key()
    {
        var config = CreateConfig(("Jwt:SigningKey", null));
        var error = Assert.Throws<InvalidOperationException>(() =>
            ProductionHostValidator.Validate(config, new TestHostEnvironment("Production")));
        Assert.Contains("SigningKey", error.Message, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("prod-test-signing-key", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Production_rejects_weak_jwt_key()
    {
        var config = CreateConfig(("Jwt:SigningKey", "too-short"));
        var error = Assert.Throws<InvalidOperationException>(() =>
            ProductionHostValidator.Validate(config, new TestHostEnvironment("Production")));
        Assert.Contains("32 bytes", error.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Production_rejects_development_placeholder_jwt_key()
    {
        var config = CreateConfig(("Jwt:SigningKey", ProductionConfigurationRules.KnownInsecureJwtKeys[0]));
        Assert.Throws<InvalidOperationException>(() =>
            ProductionHostValidator.Validate(config, new TestHostEnvironment("Production")));
    }

    [Fact]
    public void Production_requires_https_public_app_url()
    {
        var config = CreateConfig(("App:PublicUrl", "http://example.com"));
        var error = Assert.Throws<InvalidOperationException>(() =>
            ProductionHostValidator.Validate(config, new TestHostEnvironment("Production")));
        Assert.Contains("https", error.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Production_rejects_localhost_public_app_url()
    {
        var config = CreateConfig(("App:PublicUrl", "https://localhost:3000"));
        var error = Assert.Throws<InvalidOperationException>(() =>
            ProductionHostValidator.Validate(config, new TestHostEnvironment("Production")));
        Assert.Contains("localhost", error.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Production_seed_admin_blocked_by_default()
    {
        var config = CreateConfig(("SEED_ADMIN_PASSWORD", "TemporaryAdminPass1"));
        var error = Assert.Throws<InvalidOperationException>(() =>
            ProductionHostValidator.Validate(config, new TestHostEnvironment("Production")));
        Assert.Contains("SEED_ADMIN_PASSWORD", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Production_dev_email_sink_blocked()
    {
        var config = CreateConfig(("Email:Provider", "Logging"));
        var error = Assert.Throws<InvalidOperationException>(() =>
            ProductionHostValidator.Validate(config, new TestHostEnvironment("Production")));
        Assert.Contains("email", error.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Production_requires_https_cors_origin()
    {
        var config = CreateConfig(("Cors:AllowedOrigins:0", "http://example.com"));
        Assert.Throws<InvalidOperationException>(() =>
            ProductionHostValidator.Validate(config, new TestHostEnvironment("Production")));
    }

    [Fact]
    public void Production_rejects_local_database_connection()
    {
        var config = CreateConfig(
            ("ConnectionStrings:DefaultConnection", "Host=localhost;Port=5433;Database=mohammed_raouf_dev"),
            ("App:AllowLocalDatabase", "false"));
        var error = Assert.Throws<InvalidOperationException>(() =>
            ProductionHostValidator.Validate(config, new TestHostEnvironment("Production")));
        Assert.Contains("localhost", error.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Production_bunny_stream_requires_credentials()
    {
        var config = CreateConfig(
            ("Video:Provider", "BunnyStream"),
            ("Video:Bunny:LibraryId", ""),
            ("Video:Bunny:TokenKey", ""));
        var error = Assert.Throws<InvalidOperationException>(() =>
            ProductionHostValidator.Validate(config, new TestHostEnvironment("Production")));
        Assert.Contains("BunnyStream", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Production_accepts_complete_configuration()
    {
        ProductionHostValidator.Validate(CreateConfig(), new TestHostEnvironment("Production"));
    }

    [Fact]
    public void Development_does_not_require_production_public_url()
    {
        var config = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>()).Build();
        ProductionHostValidator.Validate(config, new TestHostEnvironment("Development"));
    }

    private static IConfiguration CreateConfig(params (string Key, string? Value)[] overrides)
    {
        var values = new Dictionary<string, string?>
        {
            ["Jwt:SigningKey"] = "prod-test-signing-key-do-not-ship!!",
            ["App:PublicUrl"] = "https://example.com",
            ["ConnectionStrings:DefaultConnection"] = "Host=db.example.com;Port=5432;Database=mohammed_raouf;Username=app;Password=placeholder;Ssl Mode=Require",
            ["Cors:AllowedOrigins:0"] = "https://example.com",
            ["Email:Provider"] = "Disabled",
            ["Video:Provider"] = "Unavailable",
            ["App:AllowLocalDatabase"] = "false"
        };

        foreach (var (key, value) in overrides)
        {
            values[key] = value;
        }

        return new ConfigurationBuilder().AddInMemoryCollection(values).Build();
    }

    private sealed class TestHostEnvironment(string environmentName) : IHostEnvironment
    {
        public string EnvironmentName { get; set; } = environmentName;

        public string ApplicationName { get; set; } = "Tests";

        public string ContentRootPath { get; set; } = AppContext.BaseDirectory;

        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    }
}
