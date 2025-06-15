using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Extensions.Mcp;
using Microsoft.Extensions.Logging;
using Microsoft.Data.SqlClient;

namespace Mcp.Functions.Tools.SqlTools;

public class SqlTableExistsTools(ILogger<SqlTableExistsTools> logger)
{
    public const string ToolName = "TableExists";
    public const string ToolDescription = "Checks if a table exists in the SQL Server database.";

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
            using var connection = new SqlConnection("YourConnectionStringHere");
            await connection.OpenAsync();
            var command = new SqlCommand(
                "SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = @table",
                connection);
            command.Parameters.AddWithValue("@table", tableName);
            int count = (int)await command.ExecuteScalarAsync();
            return count > 0 ? $"✅ Table '{tableName}' exists." : $"❌ Table '{tableName}' does not exist.";
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error checking table existence.");
            return $"❌ Error: {ex.Message}";
        }
    }
}
