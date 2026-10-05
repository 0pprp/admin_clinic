namespace MohammedRaouf.Application.Purchases;

public interface IRequestNumberGenerator
{
    Task<string> NextAsync(CancellationToken cancellationToken = default);
}
