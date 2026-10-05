namespace MohammedRaouf.Application.Auth;

public record AuthCommandResult
{
    public bool Succeeded { get; init; }

    public int StatusCode { get; init; } = 400;

    public string? Title { get; init; }

    public string? Detail { get; init; }

    public IssuedAuthCookies? Cookies { get; init; }

    public bool ClearCookies { get; init; }

    public static AuthCommandResult Success(IssuedAuthCookies? cookies = null) =>
        new() { Succeeded = true, Cookies = cookies };

    public static AuthCommandResult Fail(int statusCode, string title, string detail) =>
        new() { Succeeded = false, StatusCode = statusCode, Title = title, Detail = detail };
}

public sealed record AuthCommandResult<T> : AuthCommandResult
{
    public T? Value { get; init; }

    public static AuthCommandResult<T> Ok(T value, IssuedAuthCookies? cookies = null) =>
        new() { Succeeded = true, Value = value, Cookies = cookies };

    public static new AuthCommandResult<T> Fail(int statusCode, string title, string detail) =>
        new() { Succeeded = false, StatusCode = statusCode, Title = title, Detail = detail };
}

public sealed class IssuedAuthCookies
{
    public required string AccessToken { get; init; }

    public required string RefreshToken { get; init; }

    public required DateTimeOffset AccessExpiresAt { get; init; }

    public required DateTimeOffset RefreshExpiresAt { get; init; }
}
