using MohammedRaouf.Application.Common;
using MohammedRaouf.Contracts.Consultations;
using MohammedRaouf.Contracts.Public;

namespace MohammedRaouf.Application.Consultations;

public interface IConsultationService
{
    Task<ActionResult<CreateConsultationResponse>> CreateAsync(
        Guid? userId,
        CreateConsultationRequest request,
        CancellationToken cancellationToken = default);

    Task<PagedResponse<AdminConsultationSummaryResponse>> ListAdminAsync(
        int page,
        int pageSize,
        string? search,
        string? status,
        CancellationToken cancellationToken = default);

    Task<AdminConsultationDetailResponse?> GetAdminAsync(Guid id, CancellationToken cancellationToken = default);

    Task<ActionResult<AdminConsultationDetailResponse>> MarkContactedAsync(
        Guid actorUserId,
        Guid id,
        ConsultationNoteRequest request,
        string? ip,
        CancellationToken cancellationToken = default);

    Task<ActionResult<AdminConsultationDetailResponse>> ScheduleAsync(
        Guid actorUserId,
        Guid id,
        ScheduleConsultationRequest request,
        string? ip,
        CancellationToken cancellationToken = default);

    Task<ActionResult<AdminConsultationDetailResponse>> CompleteAsync(
        Guid actorUserId,
        Guid id,
        ConsultationNoteRequest request,
        string? ip,
        CancellationToken cancellationToken = default);

    Task<ActionResult<AdminConsultationDetailResponse>> CancelAsync(
        Guid actorUserId,
        Guid id,
        ConsultationNoteRequest request,
        string? ip,
        CancellationToken cancellationToken = default);

    Task<ActionResult<AdminConsultationDetailResponse>> RejectAsync(
        Guid actorUserId,
        Guid id,
        ConsultationNoteRequest request,
        string? ip,
        CancellationToken cancellationToken = default);
}

public interface IConsultationNumberGenerator
{
    Task<string> NextAsync(CancellationToken cancellationToken = default);
}
