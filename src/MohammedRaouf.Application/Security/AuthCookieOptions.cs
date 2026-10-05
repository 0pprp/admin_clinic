namespace MohammedRaouf.Application.Security;

public sealed class AuthCookieOptions
{
    public const string SectionName = "AuthCookies";

    public string AccessCookieName { get; set; } = "mr_access";

    public string RefreshCookieName { get; set; } = "mr_refresh";

    public string Path { get; set; } = "/";
}
