using MohammedRaouf.Domain.Enums;
using MohammedRaouf.Domain.Identity;

namespace MohammedRaouf.Domain.Entities;

public class Article
{
    public Guid Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Slug { get; set; } = string.Empty;

    public string Excerpt { get; set; } = string.Empty;

    public string Content { get; set; } = string.Empty;

    public string? CoverImage { get; set; }

    public ContentStatus Status { get; set; } = ContentStatus.Draft;

    public DateTimeOffset? PublishedAt { get; set; }

    public Guid AuthorId { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

    public ApplicationUser Author { get; set; } = null!;
}
