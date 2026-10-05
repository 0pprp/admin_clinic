namespace MohammedRaouf.Application.Identity;

public interface IIdentityDataSeeder
{
    Task SeedAsync(CancellationToken cancellationToken = default);
}
