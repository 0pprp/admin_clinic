using MohammedRaouf.Application.Common;
using MohammedRaouf.Contracts.Activation;
using MohammedRaouf.Contracts.Learning;

namespace MohammedRaouf.Application.Learning;

public interface IStudentLearningService
{
    Task<DashboardSummaryResponse> GetSummaryAsync(Guid userId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<StudentEnrollmentResponse>> ListMyCoursesAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    Task<ActionResult<StudentCourseLearningResponse>> GetCourseAsync(
        Guid userId,
        IReadOnlyCollection<string> roles,
        string courseSlug,
        CancellationToken cancellationToken = default);

    Task<ActionResult<StudentLessonLearningResponse>> GetLessonAsync(
        Guid userId,
        IReadOnlyCollection<string> roles,
        string courseSlug,
        Guid lessonId,
        CancellationToken cancellationToken = default);
}
