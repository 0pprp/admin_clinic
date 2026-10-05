using Microsoft.AspNetCore.Identity;
using MohammedRaouf.Domain.Enums;

namespace MohammedRaouf.Domain.Identity;

public class ApplicationUser : IdentityUser<Guid>
{
    public string FullName { get; set; } = string.Empty;

    public string? WhatsAppNumber { get; set; }

    public string? Governorate { get; set; }

    public string? AvatarUrl { get; set; }

    public AccountStatus AccountStatus { get; set; } = AccountStatus.Active;

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

    public DateTimeOffset? LastLoginAt { get; set; }
}
