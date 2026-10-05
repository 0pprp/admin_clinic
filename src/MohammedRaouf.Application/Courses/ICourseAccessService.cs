namespace MohammedRaouf.Application.Courses;

public interface ICourseAccessService
{
    Task<bool> CanUserAccessCourseAsync(
        Guid userId,
        Guid courseId,
        IReadOnlyCollection<string> roles,
        CancellationToken cancellationToken = default);

    Task<bool> CanUserAccessLessonAsync(
        Guid? userId,
        Guid lessonId,
        IReadOnlyCollection<string> roles,
        CancellationToken cancellationToken = default);

    Task<LessonAccessDecision> EvaluateLessonAccessAsync(
        Guid? userId,
        Guid lessonId,
        IReadOnlyCollection<string> roles,
        CancellationToken cancellationToken = default);
}
