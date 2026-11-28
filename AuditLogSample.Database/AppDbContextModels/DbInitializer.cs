using System.Data;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace AuditLogSample.Database.AppDbContextModels;

public static class DbInitializer
{
    public static async Task InitializeAsync(AppDbContext dbContext, CancellationToken cancellationToken = default)
    {
        await dbContext.Database.EnsureCreatedAsync(cancellationToken);
        await AddColumnIfMissingAsync(dbContext, "Users", "MobileNo", "TEXT NULL", cancellationToken);
        await AddColumnIfMissingAsync(dbContext, "AuditLogs", "TargetLookupKey", "TEXT NULL", cancellationToken);
        await AddColumnIfMissingAsync(dbContext, "AuditLogs", "TargetUserName", "TEXT NULL", cancellationToken);
        await AddColumnIfMissingAsync(dbContext, "AuditLogs", "TargetUserMobileNo", "TEXT NULL", cancellationToken);
        await ExecuteNonQueryAsync(
            dbContext,
            "CREATE INDEX IF NOT EXISTS \"IX_Users_MobileNo\" ON \"Users\" (\"MobileNo\")",
            cancellationToken);
        await ExecuteNonQueryAsync(
            dbContext,
            "CREATE INDEX IF NOT EXISTS \"IX_AuditLogs_TargetUserId\" ON \"AuditLogs\" (\"TargetUserId\")",
            cancellationToken);

        var seedUser = await dbContext.Users.FindAsync(["user-001"], cancellationToken);
        if (seedUser is null && !await dbContext.Users.AnyAsync(cancellationToken))
        {
            dbContext.Users.Add(new TblUser
            {
                Id = "user-001",
                Name = "Alice Wong",
                Email = "alice@bank.com",
                MobileNo = "09123456789",
                Limit = 100_000,
                Status = "Approved",
                CreatedAt = DateTime.UtcNow.AddDays(-5)
            });

            await dbContext.SaveChangesAsync(cancellationToken);
        }
        else if (seedUser is not null && string.IsNullOrWhiteSpace(seedUser.MobileNo))
        {
            seedUser.MobileNo = "09123456789";
            await dbContext.SaveChangesAsync(cancellationToken);
        }
    }

    private static async Task AddColumnIfMissingAsync(
        AppDbContext dbContext,
        string tableName,
        string columnName,
        string columnDefinition,
        CancellationToken cancellationToken)
    {
        var connection = (SqliteConnection)dbContext.Database.GetDbConnection();
        var shouldClose = connection.State == ConnectionState.Closed;

        if (shouldClose)
        {
            await connection.OpenAsync(cancellationToken);
        }

        try
        {
            await using var command = connection.CreateCommand();
            command.CommandText = $"PRAGMA table_info({QuoteIdentifier(tableName)})";

            await using var reader = await command.ExecuteReaderAsync(cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
            {
                if (string.Equals(reader.GetString(1), columnName, StringComparison.OrdinalIgnoreCase))
                {
                    return;
                }
            }

            await reader.DisposeAsync();

            await using var alterCommand = connection.CreateCommand();
            alterCommand.CommandText =
                $"ALTER TABLE {QuoteIdentifier(tableName)} ADD COLUMN {QuoteIdentifier(columnName)} {columnDefinition}";
            await alterCommand.ExecuteNonQueryAsync(cancellationToken);
        }
        finally
        {
            if (shouldClose)
            {
                await connection.CloseAsync();
            }
        }
    }

    private static string QuoteIdentifier(string value)
    {
        return "\"" + value.Replace("\"", "\"\"") + "\"";
    }

    private static async Task ExecuteNonQueryAsync(
        AppDbContext dbContext,
        string commandText,
        CancellationToken cancellationToken)
    {
        var connection = (SqliteConnection)dbContext.Database.GetDbConnection();
        var shouldClose = connection.State == ConnectionState.Closed;

        if (shouldClose)
        {
            await connection.OpenAsync(cancellationToken);
        }

        try
        {
            await using var command = connection.CreateCommand();
            command.CommandText = commandText;
            await command.ExecuteNonQueryAsync(cancellationToken);
        }
        finally
        {
            if (shouldClose)
            {
                await connection.CloseAsync();
            }
        }
    }
}
