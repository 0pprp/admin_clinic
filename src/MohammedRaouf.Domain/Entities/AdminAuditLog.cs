using MohammedRaouf.Domain.Identity;

namespace MohammedRaouf.Domain.Entities;

public class AdminAuditLog
{
    public Guid Id { get; set; }

    public Guid AdminUserId { get; set; }

    public string Action { get; set; } = string.Empty;

    public string EntityType { get; set; } = string.Empty;

    public Guid? EntityId { get; set; }

    public string Description { get; set; } = string.Empty;

    public string? MetadataJson { get; set; }

    public string? IpAddress { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public ApplicationUser AdminUser { get; set; } = null!;
}
