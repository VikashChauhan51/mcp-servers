using Dapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Extensions.Mcp;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using Npgsql;
using System.Text.RegularExpressions;

namespace Mcp.Functions.Tools.SqlTools;

public class SqlQueryTools(ILogger<SqlQueryTools> logger, NpgsqlConnection connection)
{
    // Tool metadata
    public const string ToolName = "ExecuteSql";
    public const string ToolDescription =
        "Executes a safe, read-only SQL SELECT query on a SQL Server database. " +
        "Only SELECT statements without semicolons or data-modifying commands are allowed.";

    public const string PropertyName = "query";
    public const string PropertyDescription =
        "A read-only SQL SELECT statement. Must not contain data manipulation or DDL operations. " +
        "Example: SELECT * FROM Customers WHERE Country = 'IN'";
    public const string PropertyType = "string";

    /// <summary>
    /// MCP Tool that validates and executes a SELECT SQL query.
    /// </summary>
    /// <param name="context">Tool invocation context containing the SQL query.</param>
    /// <returns>Validation or execution result as a string message.</returns>
    [Function(nameof(ExecuteSql))]
    public async Task<string> ExecuteSql(
        [McpToolTrigger(
            ToolName,
            ToolDescription
        )]
        ToolInvocationContext context
    )
    {
        string? query = context?.Arguments?.Values.FirstOrDefault()?.ToString();

        if (string.IsNullOrWhiteSpace(query))
        {
            logger.LogWarning("SQL query was missing from the input.");
            return "❌ Invalid input. Please provide a valid SELECT SQL query.";
        }

        logger.LogInformation("Received SQL query: {Query}", query);

        if (!IsSelectOnly(query))
        {
            logger.LogWarning("Rejected unsafe or non-SELECT SQL query: {Query}", query);
            return "❌ Only basic SELECT queries are allowed. No semicolons or data-modifying statements (INSERT, UPDATE, DELETE, etc.) are permitted.";
        }

        try
        {
            if (connection.State != System.Data.ConnectionState.Open)
                await connection.OpenAsync();

            // Use Dapper to execute the query and get results as a list of dictionaries
            var results = (await connection.QueryAsync(query))
                .Select(row => (IDictionary<string, object>)row)
                .Select(dict => dict.ToDictionary(kv => kv.Key, kv => kv.Value))
                .ToList();

            if (results.Count == 0)
                return "✅ Query executed successfully. No rows returned.";

            // Convert results to a simple string table for output
            var output = new System.Text.StringBuilder();
            var columns = results[0].Keys;
            output.AppendLine(string.Join(" | ", columns));
            output.AppendLine(new string('-', columns.Count * 10));
            foreach (var row in results)
            {
                output.AppendLine(string.Join(" | ", row.Values));
            }
            return output.ToString();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error executing SQL query.");
            return $"❌ Error executing query: {ex.Message}";
        }
    }

    /// <summary>
    /// Ensures that the provided SQL is a SELECT-only query without harmful keywords.
    /// </summary>
    private static bool IsSelectOnly(string sql)
    {
        var trimmed = sql.Trim().ToLowerInvariant();

        return trimmed.StartsWith("select")
            && !trimmed.Contains(";") // disallow chaining multiple queries
            && !Regex.IsMatch(trimmed, @"\b(insert|update|delete|drop|alter|exec|merge|truncate)\b", RegexOptions.IgnoreCase);
    }
}