namespace MohammedRaouf.Application.Courses;

public enum LessonAccessStatus
{
    NotFound = 0,
    Allowed = 1,
    Unauthorized = 2,
    Forbidden = 3
}

public sealed record LessonAccessDecision(LessonAccessStatus Status);
