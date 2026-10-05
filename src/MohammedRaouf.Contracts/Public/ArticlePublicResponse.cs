namespace MohammedRaouf.Contracts.Public;

public sealed class ArticleSummaryResponse
{
    public required Guid Id { get; init; }

    public required string Title { get; init; }

    public required string Slug { get; init; }

    public required string Excerpt { get; init; }

    public string? CoverImage { get; init; }

    public DateTimeOffset? PublishedAt { get; init; }
}

public sealed class ArticleDetailResponse
{
    public required Guid Id { get; init; }

    public required string Title { get; init; }

    public required string Slug { get; init; }

    public required string Excerpt { get; init; }

    public required string Content { get; init; }

    public string? CoverImage { get; init; }

    public DateTimeOffset? PublishedAt { get; init; }
}
