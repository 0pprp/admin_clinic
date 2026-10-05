using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace MohammedRaouf.Infrastructure.Persistence;

public sealed class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            var database = Environment.GetEnvironmentVariable("POSTGRES_DB") ?? "mohammed_raouf_dev";
            var username = Environment.GetEnvironmentVariable("POSTGRES_USER") ?? "mr_dev";
            var password = Environment.GetEnvironmentVariable("POSTGRES_PASSWORD") ?? "change_me_dev_only";
            connectionString =
                $"Host=127.0.0.1;Port=5433;Database={database};Username={username};Password={password}";
        }

        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
        optionsBuilder.UseNpgsql(connectionString);

        return new ApplicationDbContext(optionsBuilder.Options);
    }
}
