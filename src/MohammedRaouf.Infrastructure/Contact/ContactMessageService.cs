using Microsoft.EntityFrameworkCore;
using MohammedRaouf.Application.Admin;
using MohammedRaouf.Application.Common;
using MohammedRaouf.Application.Contact;
using MohammedRaouf.Contracts.Contact;
using MohammedRaouf.Contracts.Public;
using MohammedRaouf.Domain.Entities;
using MohammedRaouf.Domain.Enums;
using MohammedRaouf.Infrastructure.Persistence;

namespace MohammedRaouf.Infrastructure.Contact;

public sealed class ContactMessageService(
    ApplicationDbContext dbContext,
    IAdminAuditService audit,
    TimeProvider timeProvider) : IContactMessageService
{
    public async Task<ActionResult<CreateContactMessageResponse>> CreateAsync(
        Guid? userId,
        CreateContactMessageRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!string.IsNullOrWhiteSpace(request.Website))
        {
            return ActionResult<CreateContactMessageResponse>.Ok(new CreateContactMessageResponse
            {
                Id = Guid.NewGuid(),
                Status = ContactMessageStatus.New.ToString()
            });
        }

        var now = timeProvider.GetUtcNow();
        var entity = new ContactMessage
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Name = request.Name.Trim(),
            Phone = string.IsNullOrWhiteSpace(request.Phone) ? null : request.Phone.Trim(),
            Email = request.Email.Trim(),
            Subject = request.Subject.Trim(),
            Message = request.Message.Trim(),
            Status = ContactMessageStatus.New,
            CreatedAt = now,
            UpdatedAt = now
        };
        dbContext.ContactMessages.Add(entity);
        await dbContext.SaveChangesAsync(cancellationToken);
        return ActionResult<CreateContactMessageResponse>.Ok(new CreateContactMessageResponse
        {
            Id = entity.Id,
            Status = entity.Status.ToString()
        }, 201);
    }

    public async Task<PagedResponse<AdminContactMessageSummaryResponse>> ListAdminAsync(
        int page,
        int pageSize,
        string? search,
        string? status,
        CancellationToken cancellationToken = default)
    {
        var (safePage, safeSize) = Paging.Normalize(page, pageSize);
        var query = dbContext.ContactMessages.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<ContactMessageStatus>(status, true, out var parsed))
        {
            query = query.Where(item => item.Status == parsed);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLower();
            query = query.Where(item =>
                item.Name.ToLower().Contains(term) ||
                item.Email.ToLower().Contains(term) ||
                item.Subject.ToLower().Contains(term));
        }

        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(item => item.CreatedAt)
            .Skip((safePage - 1) * safeSize)
            .Take(safeSize)
            .Select(item => new AdminContactMessageSummaryResponse
            {
                Id = item.Id,
                Name = item.Name,
                Email = item.Email,
                Subject = item.Subject,
                Status = item.Status.ToString(),
                CreatedAt = item.CreatedAt
            })
            .ToListAsync(cancellationToken);

        return new PagedResponse<AdminContactMessageSummaryResponse>
        {
            Items = items,
            Page = safePage,
            PageSize = safeSize,
            TotalCount = total
        };
    }

    public async Task<AdminContactMessageDetailResponse?> GetAdminAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var item = await dbContext.ContactMessages.AsNoTracking().FirstOrDefaultAsync(row => row.Id == id, cancellationToken);
        return item is null ? null : Map(item);
    }

    public Task<ActionResult<AdminContactMessageDetailResponse>> MarkReadAsync(Guid actorUserId, Guid id, CancellationToken cancellationToken = default) =>
        SetStatusAsync(actorUserId, id, ContactMessageStatus.Read, cancellationToken);

    public Task<ActionResult<AdminContactMessageDetailResponse>> MarkRepliedAsync(Guid actorUserId, Guid id, CancellationToken cancellationToken = default) =>
        SetStatusAsync(actorUserId, id, ContactMessageStatus.Replied, cancellationToken);

    public Task<ActionResult<AdminContactMessageDetailResponse>> ArchiveAsync(Guid actorUserId, Guid id, CancellationToken cancellationToken = default) =>
        SetStatusAsync(actorUserId, id, ContactMessageStatus.Archived, cancellationToken);

    private async Task<ActionResult<AdminContactMessageDetailResponse>> SetStatusAsync(
        Guid actorUserId,
        Guid id,
        ContactMessageStatus status,
        CancellationToken cancellationToken)
    {
        var item = await dbContext.ContactMessages.FirstOrDefaultAsync(row => row.Id == id, cancellationToken);
        if (item is null)
        {
            return ActionResult<AdminContactMessageDetailResponse>.Fail(404, "غير موجود", "الرسالة غير موجودة.");
        }

        item.Status = status;
        item.UpdatedAt = timeProvider.GetUtcNow();
        audit.Add(actorUserId, "ContactMessageUpdated", "ContactMessage", item.Id, $"تم تحديث رسالة التواصل إلى {status}.");
        await dbContext.SaveChangesAsync(cancellationToken);
        return ActionResult<AdminContactMessageDetailResponse>.Ok(Map(item));
    }

    private static AdminContactMessageDetailResponse Map(ContactMessage item) =>
        new()
        {
            Id = item.Id,
            UserId = item.UserId,
            Name = item.Name,
            Phone = item.Phone,
            Email = item.Email,
            Subject = item.Subject,
            Message = item.Message,
            Status = item.Status.ToString(),
            CreatedAt = item.CreatedAt,
            UpdatedAt = item.UpdatedAt
        };
}
