using MohammedRaouf.Application.Courses;
using MohammedRaouf.Application.Security;

namespace MohammedRaouf.UnitTests;

public class ProductionConfigurationRulesTests
{
    [Fact]
    public void Jwt_key_must_be_at_least_32_bytes()
    {
        Assert.False(ProductionConfigurationRules.IsJwtSigningKeyAcceptable(null, production: true));
        Assert.False(ProductionConfigurationRules.IsJwtSigningKeyAcceptable("short", production: true));
        Assert.True(ProductionConfigurationRules.IsJwtSigningKeyAcceptable("prod-test-signing-key-do-not-ship!!", production: true));
    }

    [Fact]
    public void Production_rejects_known_insecure_jwt_keys()
    {
        foreach (var key in ProductionConfigurationRules.KnownInsecureJwtKeys)
        {
            Assert.False(ProductionConfigurationRules.IsJwtSigningKeyAcceptable(key, production: true));
            Assert.True(ProductionConfigurationRules.IsJwtSigningKeyAcceptable(key, production: false));
        }
    }

    [Fact]
    public void Production_public_url_must_be_https_and_not_localhost()
    {
        Assert.False(ProductionConfigurationRules.TryValidatePublicUrl("http://example.com", production: true, out _));
        Assert.False(ProductionConfigurationRules.TryValidatePublicUrl("https://localhost", production: true, out _));
        Assert.True(ProductionConfigurationRules.TryValidatePublicUrl("https://example.com", production: true, out _));
        Assert.True(ProductionConfigurationRules.TryValidatePublicUrl("http://localhost:3000", production: false, out _));
    }

    [Fact]
    public void Production_cors_origin_must_be_https()
    {
        Assert.False(ProductionConfigurationRules.IsValidCorsOrigin("http://example.com", production: true));
        Assert.False(ProductionConfigurationRules.IsValidCorsOrigin("https://localhost", production: true));
        Assert.True(ProductionConfigurationRules.IsValidCorsOrigin("https://example.com", production: true));
        Assert.True(ProductionConfigurationRules.IsValidCorsOrigin("http://localhost:3000", production: false));
    }
}

public class BunnyStreamUrlSignerTests
{
    [Fact]
    public void Embed_url_contains_token_and_expiry_without_logging_requirements()
    {
        var expires = DateTimeOffset.FromUnixTimeSeconds(1_700_000_000);
        var url = BunnyStreamUrlSigner.CreateEmbedUrl("lib", "video-1", "token-key", expires);

        Assert.StartsWith("https://iframe.mediadelivery.net/embed/lib/video-1?", url);
        Assert.Contains("expires=1700000000", url);
        Assert.Equal(
            BunnyStreamUrlSigner.ComputeToken("token-key", 1_700_000_000, "video-1"),
            url.Split("token=")[1].Split('&')[0]);
    }
}
