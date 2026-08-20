using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace AuditLogSample.Data;

public static class SqliteSchemaInitializer
{
    public static async Task InitializeAsync(AuditLogDbContext dbContext)
    {
        await dbContext.Database.EnsureCreatedAsync();
        await AddColumnIfMissingAsync(dbContext, "Users", "MobileNo", "TEXT NULL");
        await AddColumnIfMissingAsync(dbContext, "AuditLogs", "TargetLookupKey", "TEXT NULL");
        await AddColumnIfMissingAsync(dbContext, "AuditLogs", "TargetUserName", "TEXT NULL");
        await AddColumnIfMissingAsync(dbContext, "AuditLogs", "TargetUserMobileNo", "TEXT NULL");
        await ExecuteNonQueryAsync(
            dbContext,
            "CREATE INDEX IF NOT EXISTS \"IX_Users_MobileNo\" ON \"Users\" (\"MobileNo\")");
        await ExecuteNonQueryAsync(
            dbContext,
            "CREATE INDEX IF NOT EXISTS \"IX_AuditLogs_TargetUserId\" ON \"AuditLogs\" (\"TargetUserId\")");
    }

    private static async Task AddColumnIfMissingAsync(
        AuditLogDbContext dbContext,
        string tableName,
        string columnName,
        string columnDefinition)
    {
        var connection = (SqliteConnection)dbContext.Database.GetDbConnection();
        var shouldClose = connection.State == System.Data.ConnectionState.Closed;

        if (shouldClose)
        {
            await connection.OpenAsync();
        }

        try
        {
            await using var command = connection.CreateCommand();
            command.CommandText = $"PRAGMA table_info({QuoteIdentifier(tableName)})";

            await using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
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
            await alterCommand.ExecuteNonQueryAsync();
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

    private static async Task ExecuteNonQueryAsync(AuditLogDbContext dbContext, string commandText)
    {
        var connection = (SqliteConnection)dbContext.Database.GetDbConnection();
        var shouldClose = connection.State == System.Data.ConnectionState.Closed;

        if (shouldClose)
        {
            await connection.OpenAsync();
        }

        try
        {
            await using var command = connection.CreateCommand();
            command.CommandText = commandText;
            await command.ExecuteNonQueryAsync();
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
