using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using MohammedRaouf.Contracts.Auth;

namespace MohammedRaouf.IntegrationTests;

internal static class AuthTestHelpers
{
    public static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public static RegisterRequest NewRegisterRequest(
        string? email = null,
        string? phone = null,
        string password = "Passw0rd1")
    {
        return new RegisterRequest
        {
            FullName = "طالب اختبار",
            Email = email ?? UniqueEmail(),
            PhoneNumber = phone ?? UniquePhone(),
            WhatsAppNumber = phone,
            Governorate = "بغداد",
            Password = password,
            PasswordConfirmation = password,
            TermsAccepted = true
        };
    }

    public static string UniqueEmail() => $"auth.{Guid.NewGuid():N}@example.test";

    public static string UniquePhone()
    {
        var suffix = Math.Abs(Guid.NewGuid().GetHashCode()) % 1_000_000_000;
        return $"07{suffix:D9}";
    }

    public static string EncodeResetToken(string token)
    {
        return Convert.ToBase64String(Encoding.UTF8.GetBytes(token))
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
    }

    public static IReadOnlyList<string> SetCookies(HttpResponseMessage response) =>
        response.Headers.TryGetValues("Set-Cookie", out var values)
            ? values.ToArray()
            : [];

    public static string? ReadCookieValue(IEnumerable<string> setCookies, string name)
    {
        var prefix = name + "=";
        var header = setCookies.FirstOrDefault(value =>
            value.StartsWith(prefix, StringComparison.OrdinalIgnoreCase));
        if (header is null)
        {
            return null;
        }

        var token = header.Split(';', 2)[0];
        return token[prefix.Length..];
    }

    public static async Task<UserSummaryResponse> RegisterAsync(HttpClient client, RegisterRequest request)
    {
        var response = await client.PostAsJsonAsync("/api/auth/register", request);
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException(
                $"Register failed: {(int)response.StatusCode} {response.StatusCode}. Body: {body}");
        }

        var payload = await response.Content.ReadFromJsonAsync<UserSummaryResponse>(JsonOptions);
        return payload ?? throw new InvalidOperationException("Register response was empty.");
    }

    public static async Task<HttpResponseMessage> LoginAsync(
        HttpClient client,
        string email,
        string password,
        bool rememberMe = false)
    {
        return await client.PostAsJsonAsync("/api/auth/login", new LoginRequest
        {
            Email = email,
            Password = password,
            RememberMe = rememberMe
        });
    }

    public static HttpRequestMessage CreateRefreshRequest(string refreshToken)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/auth/refresh");
        request.Headers.Add("X-Requested-With", "XMLHttpRequest");
        request.Headers.Add("Cookie", $"mr_refresh={refreshToken}");
        return request;
    }
}

internal sealed class ProblemBody
{
    public string? Title { get; set; }

    public string? Detail { get; set; }

    public int? Status { get; set; }
}
