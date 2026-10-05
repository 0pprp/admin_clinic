using MohammedRaouf.Application.Common;
using MohammedRaouf.Contracts.Learning;

namespace MohammedRaouf.Application.Learning;

public interface ILessonProgressService
{
    Task<ActionResult<LessonProgressResponse>> GetAsync(
        Guid userId,
        IReadOnlyCollection<string> roles,
        Guid lessonId,
        CancellationToken cancellationToken = default);

    Task<ActionResult<LessonProgressResponse>> UpdateWatchedSecondsAsync(
        Guid userId,
        IReadOnlyCollection<string> roles,
        Guid lessonId,
        int watchedSeconds,
        CancellationToken cancellationToken = default);

    Task<ActionResult<LessonProgressResponse>> CompleteAsync(
        Guid userId,
        IReadOnlyCollection<string> roles,
        Guid lessonId,
        CancellationToken cancellationToken = default);
}
