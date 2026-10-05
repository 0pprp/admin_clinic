using Microsoft.EntityFrameworkCore;
using MohammedRaouf.Application.Admin;
using MohammedRaouf.Application.Common;
using MohammedRaouf.Contracts.Admin;
using MohammedRaouf.Contracts.Public;
using MohammedRaouf.Infrastructure.Persistence;

namespace MohammedRaouf.Infrastructure.Admin;

public sealed class AuditLogQueryService(ApplicationDbContext dbContext) : IAuditLogQueryService
{
    public async Task<PagedResponse<AdminAuditLogSummaryResponse>> ListAsync(
        int page,
        int pageSize,
        string? action,
        string? entityType,
        Guid? adminUserId,
        DateTimeOffset? fromDate,
        DateTimeOffset? toDate,
        string? search,
        CancellationToken cancellationToken = default)
    {
        var (safePage, safeSize) = Paging.Normalize(page, pageSize);
        var query = dbContext.AdminAuditLogs.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(action))
        {
            query = query.Where(item => item.Action == action);
        }

        if (!string.IsNullOrWhiteSpace(entityType))
        {
            query = query.Where(item => item.EntityType == entityType);
        }

        if (adminUserId is Guid actor)
        {
            query = query.Where(item => item.AdminUserId == actor);
        }

        if (fromDate is DateTimeOffset from)
        {
            query = query.Where(item => item.CreatedAt >= from);
        }

        if (toDate is DateTimeOffset to)
        {
            query = query.Where(item => item.CreatedAt <= to);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLower();
            query = query.Where(item =>
                item.Description.ToLower().Contains(term) ||
                item.Action.ToLower().Contains(term) ||
                item.AdminUser.FullName.ToLower().Contains(term));
        }

        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(item => item.CreatedAt)
            .Skip((safePage - 1) * safeSize)
            .Take(safeSize)
            .Select(item => new AdminAuditLogSummaryResponse
            {
                Id = item.Id,
                CreatedAt = item.CreatedAt,
                AdminUserId = item.AdminUserId,
                ActorName = item.AdminUser.FullName,
                Action = item.Action,
                EntityType = item.EntityType,
                EntityId = item.EntityId,
                Description = item.Description
            })
            .ToListAsync(cancellationToken);

        return new PagedResponse<AdminAuditLogSummaryResponse>
        {
            Items = items,
            Page = safePage,
            PageSize = safeSize,
            TotalCount = total
        };
    }

    public async Task<AdminAuditLogDetailResponse?> GetAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var item = await dbContext.AdminAuditLogs.AsNoTracking()
            .Where(row => row.Id == id)
            .Select(row => new AdminAuditLogDetailResponse
            {
                Id = row.Id,
                CreatedAt = row.CreatedAt,
                AdminUserId = row.AdminUserId,
                ActorName = row.AdminUser.FullName,
                Action = row.Action,
                EntityType = row.EntityType,
                EntityId = row.EntityId,
                Description = row.Description,
                MetadataJson = row.MetadataJson,
                IpAddress = row.IpAddress
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (item is null)
        {
            return null;
        }

        return new AdminAuditLogDetailResponse
        {
            Id = item.Id,
            CreatedAt = item.CreatedAt,
            AdminUserId = item.AdminUserId,
            ActorName = item.ActorName,
            Action = item.Action,
            EntityType = item.EntityType,
            EntityId = item.EntityId,
            Description = item.Description,
            MetadataJson = SanitizeMetadata(item.MetadataJson),
            IpAddress = item.IpAddress
        };
    }

    private static string? SanitizeMetadata(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return json;
        }

        var blocked = new[] { "password", "token", "secret", "codeHash", "activationCode", "signingKey" };
        return blocked.Any(key => json.Contains(key, StringComparison.OrdinalIgnoreCase))
            ? "{\"redacted\":true}"
            : json;
    }
}
