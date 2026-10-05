namespace MohammedRaouf.Application.Security;

public sealed class PlatformCorsOptions
{
    public const string SectionName = "Cors";

    public List<string> AllowedOrigins { get; set; } = [];
}
