using MohammedRaouf.Application.Common;
using MohammedRaouf.Contracts.Admin;

namespace MohammedRaouf.Application.Courses;

public interface ISelfHostedVideoService
{
    Task<ActionResult<AdminLessonVideoStatusResponse>> GetStatusAsync(Guid lessonId, CancellationToken cancellationToken = default);

    Task<ActionResult<AdminLessonVideoStatusResponse>> UploadAsync(
        Guid lessonId,
        Stream content,
        string fileName,
        long contentLength,
        Guid actorUserId,
        CancellationToken cancellationToken = default);
}
