using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Extensions.Mcp;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Mcp.Functions.Tools.TransactionTools;

public class TransactionTools(ILogger<TransactionTools> logger)
{
    public const string ToolName = "CheckFraud";
    public const string ToolDescription = "Checks for transaction fraud using basic rules.";
    public const string PropertyName = "transaction";
    public const string PropertyDescription = "The transaction request object.";
    public const string PropertyType = "object";

    [Function(nameof(CheckFraud))]
    public string CheckFraud(
        [McpToolTrigger(ToolName, ToolDescription)]
    ToolInvocationContext context
    )
    {
        string? rawTransaction = context?.Arguments?.Values.FirstOrDefault()?.ToString();
        if (string.IsNullOrWhiteSpace(rawTransaction))
        {
            logger.LogInformation("Raw transaction query is null or empty.");
            return "Invalid transaction.";
        }
        Transaction? transaction = JsonSerializer.Deserialize<Transaction>(
     rawTransaction,
     new JsonSerializerOptions
     {
         PropertyNamingPolicy = JsonNamingPolicy.CamelCase
     });

        if (transaction is null)
        {
            logger.LogInformation("The transaction is null.");
            return "Invalid Transaction.";
        }
        logger.LogInformation("Running fraud check for transaction amount: {Amount}", transaction.Amount);

        var isSuspicious = transaction.Amount > 10000 || transaction.Country != "IN";
        return isSuspicious ? "⚠️ Suspicious Transaction Detected" : "✅ Transaction looks fine";
    }
}
