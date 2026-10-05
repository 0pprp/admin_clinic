namespace MohammedRaouf.Domain.Enums;

public enum VideoProvider
{
    None = 0,
    LocalPlaceholder = 1,
    CloudflareStream = 2,
    CloudflareR2 = 3,
    BunnyStream = 4,
    Vimeo = 5,
    SelfHostedHls = 6
}
