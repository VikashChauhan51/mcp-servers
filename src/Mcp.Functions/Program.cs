using Mcp.Functions.Tools.HelloTools;
using Mcp.Functions.Tools.SqlTools;
using Mcp.Functions.Tools.TransactionTools;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Npgsql;

var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();

// Register services directly on builder.Services
builder.Services.AddTransient<NpgsqlConnection>(sp =>
{
    var configuration = sp.GetRequiredService<IConfiguration>();
    var connectionString = configuration["PostgresConnection"];
    return new NpgsqlConnection(connectionString);
});

builder.EnableMcpToolMetadata();

// Application Insights isn't enabled by default. See https://aka.ms/AAt8mw4.
// builder.Services
//     .AddApplicationInsightsTelemetryWorkerService()
//     .ConfigureFunctionsApplicationInsights();

builder
    .ConfigureMcpTool(HelloTools.ToolName);

builder.ConfigureMcpTool(SqlQueryTools.ToolName)
    .WithProperty(SqlQueryTools.PropertyName, SqlQueryTools.PropertyType, SqlQueryTools.PropertyDescription);

builder.ConfigureMcpTool(SqlListColumnTools.ToolName)
    .WithProperty(SqlListColumnTools.PropertyName, SqlListColumnTools.PropertyType, SqlListColumnTools.PropertyDescription);

builder.ConfigureMcpTool(SqlListTableTools.ToolName);

builder.ConfigureMcpTool(SqlTableExistsTools.ToolName)
    .WithProperty(SqlTableExistsTools.PropertyName, SqlTableExistsTools.PropertyType, SqlTableExistsTools.PropertyDescription);

builder.ConfigureMcpTool(TransactionTools.ToolName)
    .WithProperty(TransactionTools.PropertyName, TransactionTools.PropertyType, TransactionTools.PropertyDescription);

builder.Build().Run();
