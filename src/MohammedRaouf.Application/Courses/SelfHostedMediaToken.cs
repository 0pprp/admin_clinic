using System.Security.Cryptography;
using System.Text;

namespace MohammedRaouf.Application.Courses;

public static class SelfHostedMediaToken
{
    public static string Create(Guid lessonId, DateTimeOffset expiresAt, string signingKey)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(signingKey);
        var exp = expiresAt.ToUnixTimeSeconds();
        var payload = $"{lessonId:N}.{exp}";
        var sig = Convert.ToHexString(HMACSHA256.HashData(Encoding.UTF8.GetBytes(signingKey), Encoding.UTF8.GetBytes(payload))).ToLowerInvariant();
        return $"{payload}.{sig}";
    }

    public static bool TryValidate(string? token, string signingKey, out Guid lessonId, out DateTimeOffset expiresAt)
    {
        lessonId = Guid.Empty;
        expiresAt = default;
        if (string.IsNullOrWhiteSpace(token) || string.IsNullOrWhiteSpace(signingKey))
        {
            return false;
        }

        var parts = token.Split('.', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (parts.Length != 3)
        {
            return false;
        }

        if (!Guid.TryParseExact(parts[0], "N", out lessonId) ||
            !long.TryParse(parts[1], out var expUnix))
        {
            return false;
        }

        var payload = $"{parts[0]}.{parts[1]}";
        var expected = Convert.ToHexString(HMACSHA256.HashData(Encoding.UTF8.GetBytes(signingKey), Encoding.UTF8.GetBytes(payload))).ToLowerInvariant();
        if (!CryptographicOperations.FixedTimeEquals(
                Encoding.UTF8.GetBytes(expected),
                Encoding.UTF8.GetBytes(parts[2].ToLowerInvariant())))
        {
            return false;
        }

        expiresAt = DateTimeOffset.FromUnixTimeSeconds(expUnix);
        return expiresAt > DateTimeOffset.UtcNow;
    }
}
