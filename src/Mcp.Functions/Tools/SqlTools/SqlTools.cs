using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Extensions.Mcp;
using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;

namespace Mcp.Functions.Tools.SqlTools;

public class SqlTools(ILogger<SqlTools> logger)
{
    public const string ToolName = "ExecuteSql";
    public const string PropertyType = "string";
    public const string ToolDescription = "Executes a safe SELECT SQL query on SQL Server.";
    public const string PropertyName = "query";
    public const string PropertyDescription = "The SQL select query.";

    [Function(nameof(ExecuteSql))]
    public async Task<string> ExecuteSql(
        [McpToolTrigger(ToolName, ToolDescription)]
        ToolInvocationContext context
    )
    {
        string? query = context?.Arguments?.Values.FirstOrDefault()?.ToString();
        if (string.IsNullOrWhiteSpace(query))
        {
            logger.LogInformation("Sql query is null or empty.");
            return "Invalid query.";
        }

        logger.LogInformation("The sql query is:{Query}", query);
        if (!IsSelectOnly(query))
            return "❌ Only SELECT statements are allowed.";

        return "Valid query.";
    }

    private static bool IsSelectOnly(string sql)
    {
        var trimmed = sql.Trim().ToLowerInvariant();
        return trimmed.StartsWith("select") && !trimmed.Contains(";")
            && !Regex.IsMatch(trimmed, @"\b(insert|update|delete|drop|alter|exec|merge)\b");
    }
}

