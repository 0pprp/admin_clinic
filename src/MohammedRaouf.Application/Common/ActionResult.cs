namespace MohammedRaouf.Application.Common;

public record ActionResult
{
    public bool Succeeded { get; init; }

    public int StatusCode { get; init; } = 400;

    public string? Title { get; init; }

    public string? Detail { get; init; }

    public static ActionResult Success() => new() { Succeeded = true, StatusCode = 200 };

    public static ActionResult Fail(int statusCode, string title, string detail) =>
        new() { Succeeded = false, StatusCode = statusCode, Title = title, Detail = detail };
}

public sealed record ActionResult<T> : ActionResult
{
    public T? Value { get; init; }

    public static ActionResult<T> Ok(T value, int statusCode = 200) =>
        new() { Succeeded = true, StatusCode = statusCode, Value = value };

    public static new ActionResult<T> Fail(int statusCode, string title, string detail) =>
        new() { Succeeded = false, StatusCode = statusCode, Title = title, Detail = detail };
}
