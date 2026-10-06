using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MohammedRaouf.Application.Auth;
using MohammedRaouf.Application.Security;
using MohammedRaouf.Domain.Entities;
using MohammedRaouf.Domain.Identity;
using MohammedRaouf.Infrastructure.Persistence;

namespace MohammedRaouf.Infrastructure.Auth;

public sealed class EmailOtpService(
    ApplicationDbContext dbContext,
    IOptions<EmailOptions> options) : IEmailOtpService
{
    public async Task<string> IssueAsync(ApplicationUser user, string purpose, CancellationToken cancellationToken = default)
    {
        var settings = options.Value;
        var length = Math.Clamp(settings.OtpLength, 4, 8);
        var code = RandomNumberGenerator.GetInt32(0, (int)Math.Pow(10, length))
            .ToString($"D{length}");

        var active = await dbContext.EmailOtpCodes
            .Where(entry =>
                entry.UserId == user.Id &&
                entry.Purpose == purpose &&
                entry.ConsumedAt == null)
            .ToListAsync(cancellationToken);

        foreach (var entry in active)
        {
            entry.ConsumedAt = DateTimeOffset.UtcNow;
        }

        dbContext.EmailOtpCodes.Add(new EmailOtpCode
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            Purpose = purpose,
            CodeHash = Hash(code),
            CreatedAt = DateTimeOffset.UtcNow,
            ExpiresAt = DateTimeOffset.UtcNow.AddMinutes(Math.Clamp(settings.OtpLifetimeMinutes, 2, 60)),
            AttemptCount = 0
        });

        await dbContext.SaveChangesAsync(cancellationToken);
        return code;
    }

    public async Task<bool> VerifyAsync(
        ApplicationUser user,
        string purpose,
        string code,
        CancellationToken cancellationToken = default)
    {
        var normalized = (code ?? string.Empty).Trim();
        if (normalized.Length == 0)
        {
            return false;
        }

        var entry = await dbContext.EmailOtpCodes
            .Where(item =>
                item.UserId == user.Id &&
                item.Purpose == purpose &&
                item.ConsumedAt == null)
            .OrderByDescending(item => item.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);

        if (entry is null || entry.ExpiresAt <= DateTimeOffset.UtcNow)
        {
            return false;
        }

        var maxAttempts = Math.Clamp(options.Value.OtpMaxAttempts, 3, 20);
        if (entry.AttemptCount >= maxAttempts)
        {
            entry.ConsumedAt = DateTimeOffset.UtcNow;
            await dbContext.SaveChangesAsync(cancellationToken);
            return false;
        }

        entry.AttemptCount += 1;
        var matched = CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(entry.CodeHash),
            Encoding.UTF8.GetBytes(Hash(normalized)));

        if (!matched)
        {
            await dbContext.SaveChangesAsync(cancellationToken);
            return false;
        }

        entry.ConsumedAt = DateTimeOffset.UtcNow;
        await dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    private static string Hash(string code)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(code.Trim()));
        return Convert.ToHexString(bytes);
    }
}
