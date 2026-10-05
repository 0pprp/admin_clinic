namespace MohammedRaouf.Application.Security;

public sealed class JwtOptions
{
    public const string SectionName = "Jwt";

    public string Issuer { get; set; } = "MohammedRaouf";

    public string Audience { get; set; } = "MohammedRaouf.Web";

    public string SigningKey { get; set; } = string.Empty;

    public int AccessTokenMinutes { get; set; } = 15;

    public int RefreshTokenDays { get; set; } = 14;

    public int SessionRefreshTokenDays { get; set; } = 1;
}
