using MohammedRaouf.Application.Common;
using MohammedRaouf.Contracts.Admin;
using MohammedRaouf.Contracts.Public;

namespace MohammedRaouf.Application.Courses;

public interface ICourseManagementService
{
    Task<PagedResponse<AdminCourseSummaryResponse>> ListAsync(
        int page,
        int pageSize,
        string? search,
        string? status,
        CancellationToken cancellationToken = default);

    Task<AdminCourseDetailResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<ActionResult<AdminCourseDetailResponse>> CreateAsync(
        SaveCourseRequest request,
        CancellationToken cancellationToken = default);

    Task<ActionResult<AdminCourseDetailResponse>> UpdateAsync(
        Guid id,
        SaveCourseRequest request,
        CancellationToken cancellationToken = default);

    Task<ActionResult<AdminCourseDetailResponse>> PublishAsync(
        Guid id,
        Guid actorUserId,
        string? ip,
        CancellationToken cancellationToken = default);

    Task<ActionResult<AdminCourseDetailResponse>> UnpublishAsync(
        Guid id,
        Guid actorUserId,
        string? ip,
        CancellationToken cancellationToken = default);

    Task<ActionResult<AdminCourseDetailResponse>> ArchiveAsync(
        Guid id,
        Guid actorUserId,
        string? ip,
        CancellationToken cancellationToken = default);

    Task<ActionResult<AdminSectionResponse>> CreateSectionAsync(
        Guid courseId,
        SaveSectionRequest request,
        CancellationToken cancellationToken = default);

    Task<ActionResult<AdminSectionResponse>> UpdateSectionAsync(
        Guid sectionId,
        SaveSectionRequest request,
        CancellationToken cancellationToken = default);

    Task<ActionResult> DeleteSectionAsync(Guid sectionId, CancellationToken cancellationToken = default);

    Task<ActionResult> ReorderSectionsAsync(
        Guid courseId,
        ReorderItemsRequest request,
        CancellationToken cancellationToken = default);

    Task<ActionResult<AdminLessonResponse>> CreateLessonAsync(
        Guid sectionId,
        SaveLessonRequest request,
        CancellationToken cancellationToken = default);

    Task<ActionResult<AdminLessonResponse>> UpdateLessonAsync(
        Guid lessonId,
        SaveLessonRequest request,
        CancellationToken cancellationToken = default);

    Task<ActionResult<AdminLessonResponse>> PublishLessonAsync(Guid lessonId, CancellationToken cancellationToken = default);

    Task<ActionResult<AdminLessonResponse>> ArchiveLessonAsync(Guid lessonId, CancellationToken cancellationToken = default);

    Task<ActionResult> ReorderLessonsAsync(
        Guid sectionId,
        ReorderItemsRequest request,
        CancellationToken cancellationToken = default);
}
