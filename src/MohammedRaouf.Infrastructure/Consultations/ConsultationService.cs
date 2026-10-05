using Microsoft.EntityFrameworkCore;
using MohammedRaouf.Application.Admin;
using MohammedRaouf.Application.Common;
using MohammedRaouf.Application.Consultations;
using MohammedRaouf.Contracts.Consultations;
using MohammedRaouf.Contracts.Public;
using MohammedRaouf.Domain.Entities;
using MohammedRaouf.Domain.Enums;
using MohammedRaouf.Infrastructure.Persistence;

namespace MohammedRaouf.Infrastructure.Consultations;

public sealed class ConsultationService(
    ApplicationDbContext dbContext,
    IConsultationNumberGenerator numbers,
    IConsultationStateMachine stateMachine,
    IAdminAuditService audit,
    TimeProvider timeProvider) : IConsultationService
{
    public async Task<ActionResult<CreateConsultationResponse>> CreateAsync(
        Guid? userId,
        CreateConsultationRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!string.IsNullOrWhiteSpace(request.Website))
        {
            return ActionResult<CreateConsultationResponse>.Ok(new CreateConsultationResponse
            {
                Id = Guid.NewGuid(),
                RequestNumber = "CONS-000000",
                Status = ConsultationStatus.New.ToString()
            });
        }

        var now = timeProvider.GetUtcNow();
        var entity = new ConsultationRequest
        {
            Id = Guid.NewGuid(),
            RequestNumber = await numbers.NextAsync(cancellationToken),
            UserId = userId,
            FullName = request.FullName.Trim(),
            PhoneNumber = request.PhoneNumber.Trim(),
            WhatsAppNumber = string.IsNullOrWhiteSpace(request.WhatsAppNumber) ? null : request.WhatsAppNumber.Trim(),
            Email = string.IsNullOrWhiteSpace(request.Email) ? null : request.Email.Trim(),
            ConsultationType = request.ConsultationType.Trim(),
            CompanyName = string.IsNullOrWhiteSpace(request.CompanyName) ? null : request.CompanyName.Trim(),
            PreferredDate = request.PreferredDate,
            PreferredTime = request.PreferredTime,
            Topic = request.Topic.Trim(),
            Message = request.Message.Trim(),
            PreferredCommunicationMethod = request.PreferredCommunicationMethod.Trim(),
            Status = ConsultationStatus.New,
            CreatedAt = now,
            UpdatedAt = now
        };
        dbContext.ConsultationRequests.Add(entity);
        await dbContext.SaveChangesAsync(cancellationToken);
        return ActionResult<CreateConsultationResponse>.Ok(new CreateConsultationResponse
        {
            Id = entity.Id,
            RequestNumber = entity.RequestNumber,
            Status = entity.Status.ToString()
        }, 201);
    }

    public async Task<PagedResponse<AdminConsultationSummaryResponse>> ListAdminAsync(
        int page,
        int pageSize,
        string? search,
        string? status,
        CancellationToken cancellationToken = default)
    {
        var (safePage, safeSize) = Paging.Normalize(page, pageSize);
        var query = dbContext.ConsultationRequests.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<ConsultationStatus>(status, true, out var parsed))
        {
            query = query.Where(item => item.Status == parsed);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLower();
            query = query.Where(item =>
                item.FullName.ToLower().Contains(term) ||
                item.PhoneNumber.Contains(term) ||
                item.RequestNumber.ToLower().Contains(term) ||
                item.Topic.ToLower().Contains(term));
        }

        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(item => item.CreatedAt)
            .Skip((safePage - 1) * safeSize)
            .Take(safeSize)
            .Select(item => new AdminConsultationSummaryResponse
            {
                Id = item.Id,
                RequestNumber = item.RequestNumber,
                FullName = item.FullName,
                PhoneNumber = item.PhoneNumber,
                ConsultationType = item.ConsultationType,
                PreferredDate = item.PreferredDate,
                Status = item.Status.ToString(),
                CreatedAt = item.CreatedAt
            })
            .ToListAsync(cancellationToken);

        return new PagedResponse<AdminConsultationSummaryResponse>
        {
            Items = items,
            Page = safePage,
            PageSize = safeSize,
            TotalCount = total
        };
    }

    public async Task<AdminConsultationDetailResponse?> GetAdminAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var item = await dbContext.ConsultationRequests.AsNoTracking().FirstOrDefaultAsync(row => row.Id == id, cancellationToken);
        return item is null ? null : MapDetail(item);
    }

    public Task<ActionResult<AdminConsultationDetailResponse>> MarkContactedAsync(
        Guid actorUserId, Guid id, ConsultationNoteRequest request, string? ip, CancellationToken cancellationToken = default) =>
        TransitionAsync(actorUserId, id, ConsultationStatus.Contacted, "ConsultationContacted", "تم التواصل بخصوص الاستشارة.", request.AdminNotes, ip, cancellationToken);

    public async Task<ActionResult<AdminConsultationDetailResponse>> ScheduleAsync(
        Guid actorUserId,
        Guid id,
        ScheduleConsultationRequest request,
        string? ip,
        CancellationToken cancellationToken = default)
    {
        var item = await dbContext.ConsultationRequests.FirstOrDefaultAsync(row => row.Id == id, cancellationToken);
        if (item is null)
        {
            return ActionResult<AdminConsultationDetailResponse>.Fail(404, "غير موجود", "طلب الاستشارة غير موجود.");
        }

        if (!stateMachine.CanTransition(item.Status, ConsultationStatus.Scheduled))
        {
            return ActionResult<AdminConsultationDetailResponse>.Fail(409, "تعارض", "لا يمكن جدولة الطلب في حالته الحالية.");
        }

        item.Status = ConsultationStatus.Scheduled;
        item.ScheduledAt = ToUtc(request.ScheduledDate, request.ScheduledTime);
        if (!string.IsNullOrWhiteSpace(request.AdminNotes))
        {
            item.AdminNotes = request.AdminNotes.Trim();
        }

        item.UpdatedAt = timeProvider.GetUtcNow();
        audit.Add(actorUserId, "ConsultationScheduled", "ConsultationRequest", item.Id, $"تم جدولة الاستشارة {item.RequestNumber}.", new { scheduledAt = item.ScheduledAt }, ip);
        await dbContext.SaveChangesAsync(cancellationToken);
        return ActionResult<AdminConsultationDetailResponse>.Ok(MapDetail(item));
    }

    public Task<ActionResult<AdminConsultationDetailResponse>> CompleteAsync(
        Guid actorUserId, Guid id, ConsultationNoteRequest request, string? ip, CancellationToken cancellationToken = default) =>
        TransitionAsync(actorUserId, id, ConsultationStatus.Completed, "ConsultationCompleted", "اكتملت الاستشارة.", request.AdminNotes, ip, cancellationToken);

    public Task<ActionResult<AdminConsultationDetailResponse>> CancelAsync(
        Guid actorUserId, Guid id, ConsultationNoteRequest request, string? ip, CancellationToken cancellationToken = default) =>
        TransitionAsync(actorUserId, id, ConsultationStatus.Cancelled, "ConsultationCancelled", "أُلغي طلب الاستشارة.", request.AdminNotes, ip, cancellationToken);

    public Task<ActionResult<AdminConsultationDetailResponse>> RejectAsync(
        Guid actorUserId, Guid id, ConsultationNoteRequest request, string? ip, CancellationToken cancellationToken = default) =>
        TransitionAsync(actorUserId, id, ConsultationStatus.Rejected, "ConsultationRejected", "رُفض طلب الاستشارة.", request.AdminNotes, ip, cancellationToken);

    private async Task<ActionResult<AdminConsultationDetailResponse>> TransitionAsync(
        Guid actorUserId,
        Guid id,
        ConsultationStatus to,
        string action,
        string description,
        string? notes,
        string? ip,
        CancellationToken cancellationToken)
    {
        var item = await dbContext.ConsultationRequests.FirstOrDefaultAsync(row => row.Id == id, cancellationToken);
        if (item is null)
        {
            return ActionResult<AdminConsultationDetailResponse>.Fail(404, "غير موجود", "طلب الاستشارة غير موجود.");
        }

        if (!stateMachine.CanTransition(item.Status, to))
        {
            return ActionResult<AdminConsultationDetailResponse>.Fail(409, "تعارض", "لا يمكن تغيير حالة الطلب بهذا الشكل.");
        }

        item.Status = to;
        if (!string.IsNullOrWhiteSpace(notes))
        {
            item.AdminNotes = notes.Trim();
        }

        item.UpdatedAt = timeProvider.GetUtcNow();
        audit.Add(actorUserId, action, "ConsultationRequest", item.Id, $"{description} ({item.RequestNumber}).", new { status = to.ToString() }, ip);
        await dbContext.SaveChangesAsync(cancellationToken);
        return ActionResult<AdminConsultationDetailResponse>.Ok(MapDetail(item));
    }

    private static DateTimeOffset ToUtc(DateOnly date, TimeOnly time)
    {
        TimeZoneInfo zone;
        try
        {
            zone = TimeZoneInfo.FindSystemTimeZoneById("Asia/Baghdad");
        }
        catch (TimeZoneNotFoundException)
        {
            zone = TimeZoneInfo.FindSystemTimeZoneById("Arabic Standard Time");
        }

        var local = DateTime.SpecifyKind(date.ToDateTime(time), DateTimeKind.Unspecified);
        return new DateTimeOffset(TimeZoneInfo.ConvertTimeToUtc(local, zone), TimeSpan.Zero);
    }

    private static AdminConsultationSummaryResponse MapSummary(ConsultationRequest item) =>
        new()
        {
            Id = item.Id,
            RequestNumber = item.RequestNumber,
            FullName = item.FullName,
            PhoneNumber = item.PhoneNumber,
            ConsultationType = item.ConsultationType,
            PreferredDate = item.PreferredDate,
            Status = item.Status.ToString(),
            CreatedAt = item.CreatedAt
        };

    private static AdminConsultationDetailResponse MapDetail(ConsultationRequest item) =>
        new()
        {
            Id = item.Id,
            RequestNumber = item.RequestNumber,
            UserId = item.UserId,
            FullName = item.FullName,
            PhoneNumber = item.PhoneNumber,
            WhatsAppNumber = item.WhatsAppNumber,
            Email = item.Email,
            ConsultationType = item.ConsultationType,
            CompanyName = item.CompanyName,
            PreferredDate = item.PreferredDate,
            PreferredTime = item.PreferredTime,
            Topic = item.Topic,
            Message = item.Message,
            PreferredCommunicationMethod = item.PreferredCommunicationMethod,
            Status = item.Status.ToString(),
            ScheduledAt = item.ScheduledAt,
            AdminNotes = item.AdminNotes,
            CreatedAt = item.CreatedAt,
            UpdatedAt = item.UpdatedAt
        };
}
