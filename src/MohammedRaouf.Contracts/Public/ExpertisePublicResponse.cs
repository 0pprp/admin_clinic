namespace MohammedRaouf.Contracts.Public;

public sealed class ExpertisePublicResponse
{
    public required Guid Id { get; init; }

    public required string Title { get; init; }

    public required string Description { get; init; }

    public string? IconKey { get; init; }

    public required int SortOrder { get; init; }
}
