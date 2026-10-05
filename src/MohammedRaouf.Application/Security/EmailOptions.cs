namespace MohammedRaouf.Application.Security;

public sealed class EmailOptions
{
    public const string SectionName = "Email";

    public string Provider { get; set; } = "Logging";
}
