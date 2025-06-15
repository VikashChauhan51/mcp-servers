using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Extensions.Mcp;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;

namespace Mcp.Functions.Tools.SqlTools;

public class SqlListTableTools(ILogger<SqlListTableTools> logger)
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
            using var connection = new SqlConnection("YourConnectionStringHere");
            await connection.OpenAsync();
            var command = new SqlCommand("SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE'", connection);
            using var reader = await command.ExecuteReaderAsync();
            var tables = new List<string>();
            while (await reader.ReadAsync())
                tables.Add(reader.GetString(0));
            return tables.Count > 0 ? string.Join(", ", tables) : "No tables found.";
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error listing tables.");
            return $"❌ Error: {ex.Message}";
        }
    }
}
