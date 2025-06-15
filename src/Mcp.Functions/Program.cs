using Mcp.Functions.Tools.HelloTools;
using Mcp.Functions.Tools.SqlTools;
using Mcp.Functions.Tools.TransactionTools;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.Hosting;

var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();
builder.EnableMcpToolMetadata();

// Application Insights isn't enabled by default. See https://aka.ms/AAt8mw4.
// builder.Services
//     .AddApplicationInsightsTelemetryWorkerService()
//     .ConfigureFunctionsApplicationInsights();

builder
    .ConfigureMcpTool(HelloTools.ToolName);

builder.ConfigureMcpTool(SqlTools.ToolName)
    .WithProperty(SqlTools.PropertyName, SqlTools.PropertyType, SqlTools.PropertyDescription);

builder.ConfigureMcpTool(TransactionTools.ToolName)
    .WithProperty(TransactionTools.PropertyName, TransactionTools.PropertyType, TransactionTools.PropertyDescription);

builder.Build().Run();
