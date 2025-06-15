using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Extensions.Mcp;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Mcp.Functions.Tools.TransactionTools;

public class TransactionTools(ILogger<TransactionTools> logger)
{
    // Tool metadata
    public const string ToolName = "CheckFraud";
    public const string ToolDescription =
        "Analyzes a financial transaction to determine potential fraud based on predefined rules. "
        + "Flags suspicious activity such as unusually high amounts or foreign origin.";

    // Argument metadata
    public const string PropertyName = "transaction";
    public const string PropertyDescription =
        "The transaction request object containing properties such as amount, country, and user details. "
        + "Expected JSON format: { \"amount\": number, \"country\": string, \"userId\": string }.";
    public const string PropertyType = "object";

    /// <summary>
    /// MCP Tool that checks whether a transaction appears fraudulent.
    /// </summary>
    /// <param name="context">Tool invocation context containing the input arguments.</param>
    /// <returns>A user-friendly message indicating whether the transaction is suspicious or not.</returns>
    [Function(nameof(CheckFraud))]
    public string CheckFraud(
        [McpToolTrigger(
            ToolName,
            ToolDescription
        )]
        ToolInvocationContext context
    )
    {
        // Extract transaction JSON from the first argument
        string? rawTransaction = context?.Arguments?.Values.FirstOrDefault()?.ToString();

        if (string.IsNullOrWhiteSpace(rawTransaction))
        {
            logger.LogWarning("Transaction data is missing or empty.");
            return "❌ Invalid input. Please provide a valid transaction object.";
        }

        Transaction? transaction;
        try
        {
            transaction = JsonSerializer.Deserialize<Transaction>(
                rawTransaction,
                new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                }
            );
        }
        catch (JsonException ex)
        {
            logger.LogError(ex, "Failed to deserialize transaction.");
            return "❌ Malformed transaction input. Ensure the JSON structure is correct.";
        }

        if (transaction is null)
        {
            logger.LogWarning("Deserialized transaction is null.");
            return "❌ Transaction data is invalid or incomplete.";
        }

        logger.LogInformation("Processing fraud check for transaction amount: {Amount}, country: {Country}",
            transaction.Amount, transaction.Country);

        // Define fraud detection rules
        bool isHighAmount = transaction.Amount > 10000;
        bool isForeignCountry = !string.Equals(transaction.Country, "IN", StringComparison.OrdinalIgnoreCase);

        bool isSuspicious = isHighAmount || isForeignCountry;

        if (isSuspicious)
        {
            return $"⚠️ Suspicious Transaction Detected.\n" +
                   $"Reason(s): {(isHighAmount ? "Amount exceeds threshold. " : "")}" +
                   $"{(isForeignCountry ? "Origin is outside India." : "")}";
        }

        return "✅ Transaction appears to be legitimate.";
    }
}
