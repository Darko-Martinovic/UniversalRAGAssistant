using System.Text;
using System.Text.Json;
using UniversalRAGAssistant.Models;

namespace UniversalRAGAssistant.Services
{
    public class AzureOpenAIService : IAzureOpenAIService
    {
        private readonly HttpClient _httpClient;
        private readonly string _endpoint;
        private readonly string _apiKey;
        private readonly string _chatDeployment;
        private readonly string _embeddingDeployment;

        public AzureOpenAIService(
            HttpClient httpClient,
            string endpoint,
            string apiKey,
            string chatDeployment,
            string embeddingDeployment
        )
        {
            _httpClient = httpClient;
            _endpoint = endpoint;
            _apiKey = apiKey;
            _chatDeployment = chatDeployment;
            _embeddingDeployment = embeddingDeployment;

            // Setup HTTP client headers
            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");
        }

        public async Task<float[]> GetEmbeddingAsync(string text)
        {
            string url =
                $"{_endpoint.TrimEnd('/')}/openai/deployments/{_embeddingDeployment}/embeddings?api-version=2023-05-15";

            var request = new EmbeddingRequest { input = new[] { text } };
            string jsonRequest = JsonSerializer.Serialize(request);
            var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(url, content);
            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Embedding API error: {response.StatusCode}");
            }

            string jsonResponse = await response.Content.ReadAsStringAsync();
            var embeddingResponse = JsonSerializer.Deserialize<EmbeddingResponse>(jsonResponse);

            return embeddingResponse?.data?[0]?.embedding ?? Array.Empty<float>();
        }

        public async Task<string> GenerateResponseWithContextAsync(
            string question,
            string context,
            string systemPrompt
        )
        {
            string url =
                $"{_endpoint.TrimEnd('/')}/openai/deployments/{_chatDeployment}/chat/completions?api-version=2024-02-01";

            var fullSystemPrompt =
                $@"{systemPrompt}

Context:
{context}";

            var request = new ChatRequest
            {
                messages = new[]
                {
                    new ChatMessage { role = "system", content = fullSystemPrompt },
                    new ChatMessage { role = "user", content = question }
                },
                max_tokens = 500,
                temperature = 0.7f
            };

            string jsonRequest = JsonSerializer.Serialize(request);
            var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(url, content);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new Exception($"Chat API error: {response.StatusCode} - {errorContent}");
            }

            string jsonResponse = await response.Content.ReadAsStringAsync();
            var chatResponse = JsonSerializer.Deserialize<ChatResponse>(jsonResponse);

            return chatResponse?.choices?[0]?.message?.content
                ?? "Sorry, I couldn't generate a response.";
        }
    }
}
