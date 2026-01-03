namespace MCPServer
{
    // Tool container class

    // Marks this class as containing MCP tools
    [McpServerToolType]
    public static class TimeTool
    {
        // Exposed to MCP clients
        [McpServerTool, Description("Get the current time for a city")]
        public static string GetCurrentTime(string city) =>
            $"It is {DateTime.Now:HH:mm} in {city}.";
    }
}
