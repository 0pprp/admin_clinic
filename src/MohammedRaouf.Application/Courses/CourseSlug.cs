using System.Text.RegularExpressions;

namespace MohammedRaouf.Application.Courses;

public static partial class CourseSlug
{
    [GeneratedRegex(@"\s+", RegexOptions.CultureInvariant)]
    private static partial Regex WhitespaceRegex();

    [GeneratedRegex(@"[^\p{L}\p{N}-]+", RegexOptions.CultureInvariant)]
    private static partial Regex DisallowedRegex();

    [GeneratedRegex("-{2,}", RegexOptions.CultureInvariant)]
    private static partial Regex RepeatedHyphenRegex();

    [GeneratedRegex(@"^[\p{L}\p{N}]+(?:-[\p{L}\p{N}]+)*$", RegexOptions.CultureInvariant)]
    private static partial Regex ValidSlugRegex();

    public static string Normalize(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        var collapsed = WhitespaceRegex().Replace(value.Trim().ToLowerInvariant(), "-");
        collapsed = DisallowedRegex().Replace(collapsed, string.Empty);
        collapsed = RepeatedHyphenRegex().Replace(collapsed, "-").Trim('-');
        return collapsed;
    }

    public static bool IsValid(string? value)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Length > 220)
        {
            return false;
        }

        return ValidSlugRegex().IsMatch(value);
    }
}
