namespace MohammedRaouf.Application.Admin;

public interface IAdminAuditService
{
    void Add(
        Guid adminUserId,
        string action,
        string entityType,
        Guid? entityId,
        string description,
        object? metadata = null,
        string? ipAddress = null);
}
