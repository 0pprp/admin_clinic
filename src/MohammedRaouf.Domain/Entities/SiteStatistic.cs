namespace MohammedRaouf.Domain.Entities;

public class SiteStatistic
{
    public Guid Id { get; set; }

    public string Key { get; set; } = string.Empty;

    public string Label { get; set; } = string.Empty;

    public string DisplayValue { get; set; } = string.Empty;

    public bool IsPlaceholder { get; set; } = true;

    public int SortOrder { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }
}
