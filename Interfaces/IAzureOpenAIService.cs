namespace UniversalRAGAssistant.Interfaces
{
    public interface IAzureOpenAIService
    {
        Task<float[]> GetEmbeddingAsync(string text);
        Task<string> GenerateResponseWithContextAsync(string question, string context, string systemPrompt);
    }
}