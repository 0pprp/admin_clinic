using MohammedRaouf.Contracts.Courses;

namespace MohammedRaouf.Application.Courses;

public interface ICourseQueryService
{
    Task<LessonPreviewResponse?> GetPublicPreviewAsync(Guid lessonId, CancellationToken cancellationToken = default);

    Task<LessonMetadataResponse?> GetLessonMetadataAsync(
        Guid lessonId,
        Guid? userId,
        CancellationToken cancellationToken = default);

    Task<CourseOutlineResponse?> GetStudentOutlineAsync(Guid courseId, CancellationToken cancellationToken = default);
}
