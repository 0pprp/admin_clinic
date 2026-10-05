using Microsoft.EntityFrameworkCore;
using MohammedRaouf.Infrastructure.Persistence;
using Npgsql;

namespace MohammedRaouf.IntegrationTests;

public sealed class PostgresFixture : IAsyncLifetime
{
    public string ConnectionString { get; private set; } = string.Empty;

    public static string BuildConnectionString(string database = "mohammed_raouf_test")
    {
        var username = Environment.GetEnvironmentVariable("POSTGRES_USER") ?? "mr_dev";
        var password = Environment.GetEnvironmentVariable("POSTGRES_PASSWORD") ?? "change_me_dev_only";
        var host = Environment.GetEnvironmentVariable("POSTGRES_HOST") ?? "127.0.0.1";
        var port = Environment.GetEnvironmentVariable("POSTGRES_PORT") ?? "5433";
        return $"Host={host};Port={port};Database={database};Username={username};Password={password}";
    }

    public async Task InitializeAsync()
    {
        var username = Environment.GetEnvironmentVariable("POSTGRES_USER") ?? "mr_dev";
        var password = Environment.GetEnvironmentVariable("POSTGRES_PASSWORD") ?? "change_me_dev_only";
        var host = Environment.GetEnvironmentVariable("POSTGRES_HOST") ?? "127.0.0.1";
        var port = Environment.GetEnvironmentVariable("POSTGRES_PORT") ?? "5433";
        const string testDatabase = "mohammed_raouf_test";

        var adminConnectionString =
            $"Host={host};Port={port};Database=postgres;Username={username};Password={password}";
        ConnectionString = BuildConnectionString(testDatabase);

        await using (var connection = new NpgsqlConnection(adminConnectionString))
        {
            await connection.OpenAsync();
            await using var existsCommand = new NpgsqlCommand(
                "SELECT 1 FROM pg_database WHERE datname = @name",
                connection);
            existsCommand.Parameters.AddWithValue("name", testDatabase);
            var exists = await existsCommand.ExecuteScalarAsync();
            if (exists is null)
            {
                await using var createCommand = new NpgsqlCommand(
                    $"CREATE DATABASE \"{testDatabase}\"",
                    connection);
                await createCommand.ExecuteNonQueryAsync();
            }
        }

        await using var dbContext = CreateContext();
        await dbContext.Database.MigrateAsync();
    }

    public ApplicationDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseNpgsql(ConnectionString)
            .Options;

        return new ApplicationDbContext(options);
    }

    public Task DisposeAsync() => Task.CompletedTask;
}
