using System.Text.Json;
using MohammedRaouf.Application.Admin;
using MohammedRaouf.Domain.Entities;
using MohammedRaouf.Infrastructure.Persistence;

namespace MohammedRaouf.Infrastructure.Admin;

public sealed class AdminAuditService(ApplicationDbContext dbContext, TimeProvider timeProvider) : IAdminAuditService
{
    public void Add(
        Guid adminUserId,
        string action,
        string entityType,
        Guid? entityId,
        string description,
        object? metadata = null,
        string? ipAddress = null)
    {
        dbContext.AdminAuditLogs.Add(new AdminAuditLog
        {
            Id = Guid.NewGuid(),
            AdminUserId = adminUserId,
            Action = action,
            EntityType = entityType,
            EntityId = entityId,
            Description = description,
            MetadataJson = metadata is null ? null : JsonSerializer.Serialize(metadata),
            IpAddress = ipAddress,
            CreatedAt = timeProvider.GetUtcNow()
        });
    }
}
