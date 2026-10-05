namespace MohammedRaouf.Contracts.Admin;

public sealed class AdminLessonVideoStatusResponse
{
    public required Guid LessonId { get; init; }

    public required string VideoProvider { get; init; }

    public string? VideoKey { get; init; }

    public required string ProcessingStatus { get; init; }

    public string? Message { get; init; }

    public required int DurationSeconds { get; init; }
}
