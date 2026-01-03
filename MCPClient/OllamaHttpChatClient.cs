using System.Text;
using System.Text.Json;

namespace MCPClient
{
    public class OllamaHttpChatClient
    {
        private readonly HttpClient _httpClient = new();
        private readonly Uri _baseUri;
        private readonly string _modelId;

        public OllamaHttpChatClient(Uri baseUri, string modelId = "mistral:7b")
        {
            _baseUri = baseUri;
            _modelId = modelId;
        }

        public async Task<string> GetChatResponseAsync(IEnumerable<ChatMessage> messages)
        {
            // Build request body
            var requestBody = new
            {
                model = _modelId,
                messages = messages.Select(m => new
                {
                    role = m.Role.ToString().ToLower(),
                    content = string.Join("\n", m.Contents)
                }).ToArray()
            };

            var jsonRequest = JsonSerializer.Serialize(requestBody);

            var response = await _httpClient.PostAsync(
                new Uri(_baseUri, "/api/chat"),
                new StringContent(jsonRequest, Encoding.UTF8, "application/json")
            );

            if (!response.IsSuccessStatusCode)
            {
                var errorText = await response.Content.ReadAsStringAsync();
                throw new Exception($"Ollama API error: {errorText}");
            }

            var rawResponse = await response.Content.ReadAsStringAsync();
            Console.WriteLine("=== RAW RESPONSE ===");
            Console.WriteLine(rawResponse);
            Console.WriteLine("===================");

            // Parse NDJSON lines
            var sb = new StringBuilder();
            foreach (var line in rawResponse.Split('\n', StringSplitOptions.RemoveEmptyEntries))
            {
                try
                {
                    using var doc = JsonDocument.Parse(line);
                    if (doc.RootElement.TryGetProperty("message", out var message) &&
                        message.TryGetProperty("content", out var content))
                    {
                        sb.Append(content.GetString());
                    }
                }
                catch
                {
                    // Ignore parsing errors for partial or empty lines
                }
            }

            var fullResponse = sb.ToString();
            Console.WriteLine("\nAI Response:");
            Console.WriteLine(fullResponse);

            return fullResponse;
        }
    }

}
