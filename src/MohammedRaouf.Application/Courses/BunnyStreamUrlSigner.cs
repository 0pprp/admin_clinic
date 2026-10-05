using System.Security.Cryptography;
using System.Text;

namespace MohammedRaouf.Application.Courses;

public static class BunnyStreamUrlSigner
{
    public static string CreateEmbedUrl(
        string libraryId,
        string videoId,
        string tokenKey,
        DateTimeOffset expiresAt,
        string cdnHostname = "iframe.mediadelivery.net")
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(libraryId);
        ArgumentException.ThrowIfNullOrWhiteSpace(videoId);
        ArgumentException.ThrowIfNullOrWhiteSpace(tokenKey);

        var expires = expiresAt.ToUnixTimeSeconds();
        var token = ComputeToken(tokenKey, expires, videoId);
        var host = string.IsNullOrWhiteSpace(cdnHostname) ? "iframe.mediadelivery.net" : cdnHostname.Trim().TrimEnd('/');
        host = host.Replace("https://", string.Empty, StringComparison.OrdinalIgnoreCase)
            .Replace("http://", string.Empty, StringComparison.OrdinalIgnoreCase);
        return $"https://{host}/embed/{libraryId}/{videoId}?token={token}&expires={expires}";
    }

    public static string ComputeToken(string tokenKey, long expiresUnixSeconds, string videoId)
    {
        var payload = $"{tokenKey}{expiresUnixSeconds}{videoId}";
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(payload));
        return Convert.ToHexString(hash).ToLowerInvariant();
    }
}
