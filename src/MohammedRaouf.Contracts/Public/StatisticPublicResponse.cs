namespace MohammedRaouf.Contracts.Public;

public sealed class StatisticPublicResponse
{
    public required Guid Id { get; init; }

    public required string Key { get; init; }

    public required string Label { get; init; }

    public required string DisplayValue { get; init; }

    public required int SortOrder { get; init; }
}
