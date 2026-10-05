using MohammedRaouf.Application.Activation;

namespace MohammedRaouf.UnitTests;

public class ActivationCodeFormatTests
{
    private readonly ActivationCodeGenerator _generator = new();
    private readonly ActivationCodeHasher _hasher = new();

    [Fact]
    public void Generator_returns_display_format_without_confusing_characters()
    {
        var code = _generator.GeneratePlainCode();
        Assert.Matches(@"^MR-[23456789ABCDEFGHJKMNPQRSTUVWXYZ]{4}-[23456789ABCDEFGHJKMNPQRSTUVWXYZ]{4}$", code);
        Assert.DoesNotContain("0", code);
        Assert.DoesNotContain("O", code);
        Assert.DoesNotContain("1", code);
        Assert.DoesNotContain("I", code);
        Assert.DoesNotContain("L", code);
    }

    [Theory]
    [InlineData("MR-8K2P-7X4M", "MR-8K2P-7X4M")]
    [InlineData("mr-8k2p-7x4m", "MR-8K2P-7X4M")]
    [InlineData("MR8K2P7X4M", "MR-8K2P-7X4M")]
    [InlineData(" mr 8k2p 7x4m ", "MR-8K2P-7X4M")]
    public void Normalize_accepts_equivalent_user_input(string input, string expected)
    {
        Assert.True(ActivationCodeFormat.TryNormalize(input, out var canonical));
        Assert.Equal(expected, canonical);
    }

    [Fact]
    public void Hash_is_deterministic_sha256_hex_of_canonical_form()
    {
        Assert.True(ActivationCodeFormat.TryNormalize("mr8k2p7x4m", out var canonical));
        var first = _hasher.Hash(canonical);
        var second = _hasher.Hash("MR-8K2P-7X4M");
        Assert.Equal(64, first.Length);
        Assert.Equal(first, second);
        Assert.Equal(first, first.ToUpperInvariant());
    }
}
