namespace UniversalRAGAssistant.Services
{
    public interface IRagService
    {
        Task<string> ProcessQueryAsync(string question, string systemPrompt, int documentCount);
    }
}