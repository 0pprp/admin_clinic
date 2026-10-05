using MohammedRaouf.Contracts.Public;

namespace MohammedRaouf.Contracts.Courses;

public sealed class CourseOutlineResponse
{
    public required Guid CourseId { get; init; }

    public required string Title { get; init; }

    public required string Slug { get; init; }

    public required string Status { get; init; }

    public required IReadOnlyList<CourseSectionPublicResponse> Sections { get; init; }
}
