namespace MohammedRaouf.Application.Activation;

public sealed class ActivationCodeOptions
{
    public const string SectionName = "ActivationCodes";

    public int ExpirationDays { get; set; } = 30;
}
