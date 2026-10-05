using System.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using MohammedRaouf.Application.Consultations;
using MohammedRaouf.Infrastructure.Persistence;

namespace MohammedRaouf.Infrastructure.Consultations;

public sealed class ConsultationNumberGenerator(ApplicationDbContext dbContext) : IConsultationNumberGenerator
{
    public async Task<string> NextAsync(CancellationToken cancellationToken = default)
    {
        var year = DateTimeOffset.UtcNow.Year;
        var connection = dbContext.Database.GetDbConnection();
        var openedHere = connection.State != ConnectionState.Open;
        if (openedHere)
        {
            await dbContext.Database.OpenConnectionAsync(cancellationToken);
        }

        try
        {
            await using var command = connection.CreateCommand();
            command.CommandText = """
                INSERT INTO "ConsultationRequestNumberCounters" ("Year", "LastValue")
                VALUES (@year, 1)
                ON CONFLICT ("Year") DO UPDATE
                SET "LastValue" = "ConsultationRequestNumberCounters"."LastValue" + 1
                RETURNING "LastValue";
                """;
            var yearParameter = command.CreateParameter();
            yearParameter.ParameterName = "year";
            yearParameter.Value = year;
            command.Parameters.Add(yearParameter);
            if (dbContext.Database.CurrentTransaction is { } transaction)
            {
                command.Transaction = transaction.GetDbTransaction();
            }

            var result = await command.ExecuteScalarAsync(cancellationToken);
            var value = Convert.ToInt32(result, System.Globalization.CultureInfo.InvariantCulture);
            return $"CONS-{year}-{value:D6}";
        }
        finally
        {
            if (openedHere)
            {
                await dbContext.Database.CloseConnectionAsync();
            }
        }
    }
}
