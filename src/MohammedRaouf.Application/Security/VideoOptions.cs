namespace MohammedRaouf.Application.Security;

public sealed class VideoOptions
{
    public const string SectionName = "Video";

    public string Provider { get; set; } = "Unavailable";

    public int TokenLifetimeMinutes { get; set; } = 10;

    public BunnyStreamOptions Bunny { get; set; } = new();

    public SelfHostedVideoOptions SelfHosted { get; set; } = new();

    public bool IsBunnyStream =>
        string.Equals(Provider, "BunnyStream", StringComparison.OrdinalIgnoreCase);

    public bool IsSelfHostedHls =>
        string.Equals(Provider, "SelfHostedHls", StringComparison.OrdinalIgnoreCase);

    public bool IsUnavailable =>
        string.IsNullOrWhiteSpace(Provider) ||
        string.Equals(Provider, "Unavailable", StringComparison.OrdinalIgnoreCase) ||
        string.Equals(Provider, "None", StringComparison.OrdinalIgnoreCase);
}

public sealed class BunnyStreamOptions
{
    public string LibraryId { get; set; } = string.Empty;

    public string CdnHostname { get; set; } = "iframe.mediadelivery.net";

    public string TokenKey { get; set; } = string.Empty;
}

public sealed class SelfHostedVideoOptions
{
    public string RootPath { get; set; } = "/data/media";

    public string SigningKey { get; set; } = string.Empty;

    public int MaxUploadMegabytes { get; set; } = 2048;
}
