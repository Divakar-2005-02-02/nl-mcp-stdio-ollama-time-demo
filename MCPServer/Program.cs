using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

// This sets up DI, logging, and lifetime management
var builder = Host.CreateApplicationBuilder();

// CRITICAL FOR MCP OVER STDIO:
// MCP uses STDOUT as a strict protocol channel.
// Any log output written to STDOUT will corrupt the MCP message stream
// and cause random deserialization errors or silent client crashes.
//
// This configuration removes all default log providers and forces
// ALL logs to STDERR, keeping STDOUT exclusively for MCP protocol traffic.
builder.Logging.ClearProviders();
builder.Logging.AddConsole(options =>
{
    options.LogToStandardErrorThreshold = LogLevel.Trace;
});

builder.Services
    .AddMcpServer() // Core MCP server services
    .WithStdioServerTransport() // Use STDIO transport
    .WithToolsFromAssembly(); // Auto-discover tools via attributes

// This blocks until the client disconnects
await builder.Build().RunAsync();

