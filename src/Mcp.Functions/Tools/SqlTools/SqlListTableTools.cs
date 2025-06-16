using Dapper;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Extensions.Mcp;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace Mcp.Functions.Tools.SqlTools;

public class SqlListTableTools(ILogger<SqlListTableTools> logger, NpgsqlConnection connection)
{
    public const string ToolName = "ListTables";
    public const string ToolDescription = "Lists all tables in the connected SQL Server database.";

    [Function(nameof(ListTables))]
    public async Task<string> ListTables(
        [McpToolTrigger(
            ToolName,
            ToolDescription
        )]
        ToolInvocationContext context
    )
    {

        try
        {
            if (connection.State != System.Data.ConnectionState.Open)
                await connection.OpenAsync();

            var tables = (await connection.QueryAsync<string>(
                "SELECT table_name FROM information_schema.tables WHERE table_type = 'BASE TABLE' AND table_schema = 'public'"
            )).ToList();

            return tables.Count > 0
                ? string.Join(", ", tables)
                : "No tables found.";
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error listing tables.");
            return $"❌ Error: {ex.Message}";
        }
    }
}
