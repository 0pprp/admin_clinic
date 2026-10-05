using MohammedRaouf.Application.Courses;

namespace MohammedRaouf.UnitTests;

public class CourseSlugTests
{
    [Theory]
    [InlineData("My Course", "my-course")]
    [InlineData("  Hello---World  ", "hello-world")]
    [InlineData("دورة-التسويق", "دورة-التسويق")]
    public void Normalize_produces_url_safe_slug(string input, string expected)
    {
        Assert.Equal(expected, CourseSlug.Normalize(input));
        Assert.True(CourseSlug.IsValid(CourseSlug.Normalize(input)));
    }

    [Theory]
    [InlineData("")]
    [InlineData("***")]
    [InlineData("hello_world")]
    public void Invalid_slugs_are_rejected_after_normalize_or_directly(string input)
    {
        var normalized = CourseSlug.Normalize(input);
        if (string.IsNullOrWhiteSpace(normalized))
        {
            Assert.False(CourseSlug.IsValid(normalized));
        }
    }
}
