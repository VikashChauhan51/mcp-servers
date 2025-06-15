using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Extensions.Mcp;
using Microsoft.Extensions.Logging;

namespace Mcp.Functions.Tools.HelloTools;

public class HelloTools(ILogger<HelloTools> logger)
{
    public const string PropertyType = "string";
    public const string ToolName = "hello";
    public const string ToolDescription =
        "Simple hello world MCP Tool that responses with a hello message.";

    [Function(nameof(SayHello))]
    public string SayHello(
        [McpToolTrigger(ToolName, ToolDescription)] ToolInvocationContext context
    )
    {
        logger.LogInformation("Saying hello");
        return "Hello I am MCP Tool!";
    }
}