using MCPClient;

// Create a logger factory so BOTH the MCP client
// and the STDIO transport can emit diagnostic logs.

using var loggerFactory = LoggerFactory.Create(builder =>
{
    builder.AddConsole();
    builder.SetMinimumLevel(LogLevel.Trace);
});

// This transport launches the MCP server process
// and communicates with it over STDIN / STDOUT
// IMPORTANT: Update the Command path to point to the MCP server executable
// on *your own machine*. This path is environment-specific and might differ
// across developers, build configurations, and deployment targets.
var stdioTransport = new StdioClientTransport(
    new StdioClientTransportOptions
    {
        Name = "Time MCP Server",
        Command = @"..\..\..\..\MCPServer\bin\Debug\net9.0\MCPServer.exe"
    },
    loggerFactory
);

// Create MCP client 
await using var mcpClient = await McpClientFactory.CreateAsync(
    stdioTransport,
    new McpClientOptions
    {
        ClientInfo = new() { Name = "Time Client", Version = "1.0.0" }
    },
    loggerFactory
);

// List MCP tools
IList<McpClientTool> mcpTools = await mcpClient.ListToolsAsync();

Console.WriteLine("MCP tools discovered:");
foreach (var t in mcpTools)
    Console.WriteLine($"  {t.Name} — {t.Description}");

// Call the GetCurrentTime tool
var timeTool = mcpTools.FirstOrDefault(t => t.Name == "GetCurrentTime");
string toolResult = "Tool not found!";
if (timeTool != null)
{
    var response = await timeTool.CallAsync(new Dictionary<string, object>
    {
        ["city"] = "Illzach, France"
    });

    // Extract text from tool response content
    toolResult = string.Join("\n", response.Content.Select(c => c.Text));
}

Console.WriteLine($"\nMCP Tool Response:\n{toolResult}");

// Create Ollama client
var ollamaClient = new OllamaHttpChatClient(new Uri("http://localhost:11434/"), "mistral:7b");

// Conversation messages
var messages = new List<ChatMessage>
        {
            new(ChatRole.System, "You are a helpful assistant that can use MCP tools."),
            new(ChatRole.User, "What is the current (CET) time in Illzach, France?"),
            new(ChatRole.Assistant, $"According to the MCP tool: {toolResult}")
        };

// Call Ollama and capture AI response
string aiChatResponse = await ollamaClient.GetChatResponseAsync(messages);

// Print AI response
Console.WriteLine("\nAI Response (from Program.Main):");
Console.WriteLine(aiChatResponse);