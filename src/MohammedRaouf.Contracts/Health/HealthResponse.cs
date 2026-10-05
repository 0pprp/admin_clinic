namespace MohammedRaouf.Contracts.Health;

public sealed class HealthResponse
{
    public required string Status { get; init; }

    public required string Service { get; init; }

    public required DateTimeOffset TimestampUtc { get; init; }
}
