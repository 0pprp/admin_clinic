namespace MohammedRaouf.Application.Security;

public sealed class AppOptions
{
    public const string SectionName = "App";

    public string PublicUrl { get; set; } = string.Empty;

    public bool AllowLocalDatabase { get; set; }

    public bool DisableHttpsRedirection { get; set; }
}
