namespace MohammedRaouf.Domain.Entities;

public class Testimonial
{
    public Guid Id { get; set; }

    public string AuthorDisplayName { get; set; } = string.Empty;

    public string? AuthorTitle { get; set; }

    public string Body { get; set; } = string.Empty;

    public bool IsPublished { get; set; }

    public int SortOrder { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }
}
