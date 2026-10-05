namespace MohammedRaouf.Application.Learning;

public static class CourseProgressCalculator
{
    public static int Percent(int completedPublishedLessons, int totalPublishedLessons)
    {
        if (totalPublishedLessons <= 0)
        {
            return 0;
        }

        var percent = (int)Math.Floor(completedPublishedLessons * 100d / totalPublishedLessons);
        return Math.Clamp(percent, 0, 100);
    }

    public static int ClampWatchedSeconds(int watchedSeconds, int durationSeconds)
    {
        var value = Math.Max(0, watchedSeconds);
        if (durationSeconds > 0)
        {
            return Math.Min(value, durationSeconds);
        }

        return Math.Min(value, 86_400);
    }
}
