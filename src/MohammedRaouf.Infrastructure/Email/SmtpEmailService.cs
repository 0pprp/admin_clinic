using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using MohammedRaouf.Application.Notifications;
using MohammedRaouf.Application.Security;

namespace MohammedRaouf.Infrastructure.Email;

public sealed class SmtpEmailService(
    IOptions<EmailOptions> options,
    ILogger<SmtpEmailService> logger) : IEmailService
{
    public Task SendPasswordResetAsync(string email, string resetLink, CancellationToken cancellationToken = default) =>
        SendAsync(
            email,
            "استعادة كلمة المرور — العيادة الإدارية",
            $"""
            <div dir="rtl" style="font-family:Tahoma,Arial,sans-serif;line-height:1.7;color:#0f172a">
              <h2 style="margin:0 0 12px">استعادة كلمة المرور</h2>
              <p>اضغط الرابط التالي لتعيين كلمة مرور جديدة. الرابط صالح لفترة محدودة.</p>
              <p><a href="{resetLink}" style="color:#f97316;font-weight:700">تعيين كلمة مرور جديدة</a></p>
              <p style="color:#64748b;font-size:13px">إذا لم تطلب ذلك، تجاهل هذه الرسالة.</p>
            </div>
            """,
            cancellationToken);

    public Task SendEmailConfirmationAsync(string email, string confirmationLink, CancellationToken cancellationToken = default) =>
        SendAsync(
            email,
            "تأكيد البريد الإلكتروني — العيادة الإدارية",
            $"""
            <div dir="rtl" style="font-family:Tahoma,Arial,sans-serif;line-height:1.7;color:#0f172a">
              <h2 style="margin:0 0 12px">تأكيد البريد</h2>
              <p>أكمل تأكيد بريدك من خلال الرابط التالي:</p>
              <p><a href="{confirmationLink}" style="color:#f97316;font-weight:700">تأكيد البريد الإلكتروني</a></p>
            </div>
            """,
            cancellationToken);

    public Task SendOtpAsync(string email, string code, string purpose, CancellationToken cancellationToken = default)
    {
        var (title, intro) = purpose switch
        {
            "PasswordReset" => ("رمز استعادة كلمة المرور", "استخدم الرمز التالي لإعادة تعيين كلمة المرور:"),
            "GoogleLogin" => ("رمز تحقق تسجيل الدخول عبر Google", "أكمل تسجيل الدخول بإدخال الرمز المرسل إلى بريدك:"),
            _ => ("رمز تحقق البريد الإلكتروني", "أدخل الرمز التالي لتأكيد بريدك الإلكتروني:")
        };

        return SendAsync(
            email,
            $"{title} — العيادة الإدارية",
            $"""
            <div dir="rtl" style="font-family:Tahoma,Arial,sans-serif;line-height:1.7;color:#0f172a">
              <h2 style="margin:0 0 12px">{title}</h2>
              <p>{intro}</p>
              <p style="font-size:28px;letter-spacing:8px;font-weight:800;margin:20px 0;color:#0f172a">{code}</p>
              <p style="color:#64748b;font-size:13px">الرمز صالح لمدة محدودة. لا تشاركه مع أحد.</p>
            </div>
            """,
            cancellationToken);
    }

    private async Task SendAsync(string email, string subject, string htmlBody, CancellationToken cancellationToken)
    {
        var settings = options.Value;
        if (string.IsNullOrWhiteSpace(settings.Smtp.Host) ||
            string.IsNullOrWhiteSpace(settings.Smtp.Username) ||
            string.IsNullOrWhiteSpace(settings.Smtp.Password))
        {
            throw new InvalidOperationException("Email SMTP is not fully configured.");
        }

        var fromAddress = string.IsNullOrWhiteSpace(settings.FromAddress)
            ? settings.Smtp.Username
            : settings.FromAddress;

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(settings.FromName, fromAddress));
        message.To.Add(MailboxAddress.Parse(email));
        message.Subject = subject;
        message.Body = new BodyBuilder { HtmlBody = htmlBody, TextBody = StripHtml(htmlBody) }.ToMessageBody();

        using var client = new SmtpClient();
        var secureSocket = settings.Smtp.UseStartTls
            ? SecureSocketOptions.StartTls
            : SecureSocketOptions.Auto;

        await client.ConnectAsync(settings.Smtp.Host, settings.Smtp.Port, secureSocket, cancellationToken);
        await client.AuthenticateAsync(settings.Smtp.Username, settings.Smtp.Password, cancellationToken);
        await client.SendAsync(message, cancellationToken);
        await client.DisconnectAsync(true, cancellationToken);

        logger.LogInformation("SMTP email sent to {EmailDomain}", email.Contains('@') ? email[(email.IndexOf('@') + 1)..] : "unknown");
    }

    private static string StripHtml(string html) =>
        System.Text.RegularExpressions.Regex.Replace(html, "<[^>]+>", " ")
            .Replace("&nbsp;", " ", StringComparison.Ordinal);
}
