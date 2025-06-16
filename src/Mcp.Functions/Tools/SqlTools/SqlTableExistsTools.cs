using Dapper;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Extensions.Mcp;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace Mcp.Functions.Tools.SqlTools;

public class SqlTableExistsTools(ILogger<SqlTableExistsTools> logger, NpgsqlConnection connection)
{
    public const string ToolName = "TableExists";
    public const string ToolDescription = "Checks if a table exists in the SQL Server database.";
    public const string PropertyType = "string";
    public const string PropertyName = "tableName";
    public const string PropertyDescription = "The name of the table to check for existence. Example: 'Customers'.";

    [Function(nameof(TableExists))]
    public async Task<string> TableExists(
        [McpToolTrigger(
            ToolName,
            ToolDescription
        )]
        ToolInvocationContext context
    )
    {
        string? tableName = context?.Arguments?.Values.FirstOrDefault()?.ToString();
        if (string.IsNullOrWhiteSpace(tableName))
            return "❌ Please provide a table name.";

        try
        {
            if (connection.State != System.Data.ConnectionState.Open)
                await connection.OpenAsync();

            var sql = "SELECT COUNT(*) FROM information_schema.tables WHERE table_name = @table";
            int count = await connection.ExecuteScalarAsync<int>(sql, new { table = tableName });

            return count > 0 ? $"✅ Table '{tableName}' exists." : $"❌ Table '{tableName}' does not exist.";
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error checking table existence.");
            return $"❌ Error: {ex.Message}";
        }
    }
}
