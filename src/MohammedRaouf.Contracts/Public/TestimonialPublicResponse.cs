namespace MohammedRaouf.Contracts.Public;

public sealed class TestimonialPublicResponse
{
    public required Guid Id { get; init; }

    public required string AuthorDisplayName { get; init; }

    public string? AuthorTitle { get; init; }

    public required string Body { get; init; }

    public required int SortOrder { get; init; }
}
