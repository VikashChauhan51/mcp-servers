using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Extensions.Mcp;
using Microsoft.Extensions.Logging;
using Microsoft.Data.SqlClient;

namespace Mcp.Functions.Tools.SqlTools;

public class SqlListColumnTools(ILogger<SqlListColumnTools> logger)
{
    public const string ToolName = "ListColumns";
    public const string ToolDescription = "Lists all columns for a specified table in the SQL Server database.";

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
            using var connection = new SqlConnection("YourConnectionStringHere");
            await connection.OpenAsync();
            var command = new SqlCommand(
                "SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = @table",
                connection);
            command.Parameters.AddWithValue("@table", tableName);
            using var reader = await command.ExecuteReaderAsync();
            var columns = new List<string>();
            while (await reader.ReadAsync())
                columns.Add(reader.GetString(0));
            return columns.Count > 0 ? string.Join(", ", columns) : $"No columns found for table '{tableName}'.";
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error listing columns.");
            return $"❌ Error: {ex.Message}";
        }
    }
}
