using MohammedRaouf.Domain.Enums;
using MohammedRaouf.Domain.Identity;

namespace MohammedRaouf.Domain.Entities;

public class ContactMessage
{
    public Guid Id { get; set; }

    public Guid? UserId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Phone { get; set; }

    public string Email { get; set; } = string.Empty;

    public string Subject { get; set; } = string.Empty;

    public string Message { get; set; } = string.Empty;

    public ContactMessageStatus Status { get; set; } = ContactMessageStatus.New;

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

    public ApplicationUser? User { get; set; }
}
