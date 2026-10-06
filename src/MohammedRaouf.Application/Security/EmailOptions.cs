namespace MohammedRaouf.Application.Security;

public sealed class EmailOptions
{
    public const string SectionName = "Email";

    public string Provider { get; set; } = "Logging";

    public string FromName { get; set; } = "العيادة الإدارية";

    public string FromAddress { get; set; } = string.Empty;

    public SmtpOptions Smtp { get; set; } = new();

    public GoogleAuthOptions Google { get; set; } = new();

    public int OtpLength { get; set; } = 6;

    public int OtpLifetimeMinutes { get; set; } = 10;

    public int OtpMaxAttempts { get; set; } = 5;
}

public sealed class SmtpOptions
{
    public string Host { get; set; } = "smtp.gmail.com";

    public int Port { get; set; } = 587;

    public bool UseStartTls { get; set; } = true;

    public string Username { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;
}

public sealed class GoogleAuthOptions
{
    public string ClientId { get; set; } = string.Empty;
}
