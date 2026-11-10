using System.Net.Http.Json;
using System.Text.Json;

namespace SmartNotes.Services
{
    public class OpenAIService
    {
        private readonly HttpClient _httpClient;

        public OpenAIService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<string> SummarizeAsync(string content)
        {

            // Create the prompt
            var prompt = $"Summarize this note in 2-3 sentences:\n\n{content}";

            // Send the prompt to Ollama
            var response = await _httpClient.PostAsJsonAsync(
                "http://localhost:11434/api/generate",
                new
                {
                    model = "llama3.2:3b",
                    prompt = prompt,
                    stream = false
                });

            // Throw an exception if the request failed
            response.EnsureSuccessStatusCode();

            // Read the response JSON
            var responseJson = await response.Content.ReadAsStringAsync();

            // Parse the JSON and extract the summary
            using var document = JsonDocument.Parse(responseJson);

            return document.RootElement
                .GetProperty("response")
                .GetString() ?? "No summary generated.";
        }
    }
}