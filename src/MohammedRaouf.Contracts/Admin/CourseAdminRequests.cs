namespace MohammedRaouf.Contracts.Admin;

public sealed class SaveCourseRequest
{
    public string Title { get; set; } = string.Empty;

    public string Slug { get; set; } = string.Empty;

    public string ShortDescription { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public long PriceIQD { get; set; }

    public string? ThumbnailUrl { get; set; }

    public string? TrailerUrl { get; set; }

    public string Level { get; set; } = "Beginner";

    public string AccessType { get; set; } = "Lifetime";

    public int? AccessDurationDays { get; set; }

    public bool IsFeatured { get; set; }
}

public sealed class SaveSectionRequest
{
    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    public int? SortOrder { get; set; }
}

public sealed class SaveLessonRequest
{
    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    public int DurationSeconds { get; set; }

    public bool IsFreePreview { get; set; }

    public string? VideoProvider { get; set; }

    public string? VideoKey { get; set; }

    public int? SortOrder { get; set; }
}

public sealed class ReorderItemsRequest
{
    public List<ReorderItemRequest> Items { get; set; } = [];
}

public sealed class ReorderItemRequest
{
    public Guid Id { get; set; }

    public int SortOrder { get; set; }
}
