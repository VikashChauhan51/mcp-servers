using Dapper;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Extensions.Mcp;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace Mcp.Functions.Tools.SqlTools;

public class SqlListColumnTools(ILogger<SqlListColumnTools> logger, NpgsqlConnection connection)
{
    public const string ToolName = "ListColumns";
    public const string PropertyName = "tableName";
    public const string PropertyType = "string";
    public const string ToolDescription = "Lists all columns for a specified table in the SQL Server database.";
    public const string PropertyDescription = "The name of the table to list columns for. Example: 'Customers'.";

    [Function(nameof(ListColumns))]
    public async Task<string> ListColumns(
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

            var columns = (await connection.QueryAsync<string>(
                "SELECT column_name FROM information_schema.columns WHERE table_name = @table",
                new { table = tableName }
            )).ToList();

            return columns.Count > 0
                ? string.Join(", ", columns)
                : $"No columns found for table '{tableName}'.";
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error listing columns.");
            return $"❌ Error: {ex.Message}";
        }
    }
}
