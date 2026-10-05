using MohammedRaouf.Application.Learning;

namespace MohammedRaouf.UnitTests;

public class CourseProgressCalculatorTests
{
    [Fact]
    public void Percent_is_zero_when_there_are_no_published_lessons()
    {
        Assert.Equal(0, CourseProgressCalculator.Percent(0, 0));
        Assert.Equal(0, CourseProgressCalculator.Percent(3, 0));
    }

    [Fact]
    public void Percent_uses_completed_published_lessons_only()
    {
        Assert.Equal(50, CourseProgressCalculator.Percent(2, 4));
        Assert.Equal(20, CourseProgressCalculator.Percent(1, 5));
        Assert.Equal(100, CourseProgressCalculator.Percent(4, 4));
    }

    [Fact]
    public void Percent_floors_partial_values()
    {
        Assert.Equal(33, CourseProgressCalculator.Percent(1, 3));
    }

    [Fact]
    public void Clamp_watched_seconds_stays_within_duration()
    {
        Assert.Equal(0, CourseProgressCalculator.ClampWatchedSeconds(-12, 300));
        Assert.Equal(120, CourseProgressCalculator.ClampWatchedSeconds(120, 300));
        Assert.Equal(300, CourseProgressCalculator.ClampWatchedSeconds(999_999, 300));
        Assert.Equal(86_400, CourseProgressCalculator.ClampWatchedSeconds(999_999, 0));
    }
}
