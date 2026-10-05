using MohammedRaouf.Domain.Identity;

namespace MohammedRaouf.Domain.Entities;

public class SiteSetting
{
    public Guid Id { get; set; }

    public string Key { get; set; } = string.Empty;

    public string ValueJson { get; set; } = "{}";

    public DateTimeOffset UpdatedAt { get; set; }

    public Guid? UpdatedBy { get; set; }

    public ApplicationUser? UpdatedByUser { get; set; }
}
