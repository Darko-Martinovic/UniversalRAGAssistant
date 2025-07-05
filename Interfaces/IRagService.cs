namespace UniversalRAGAssistant.Interfaces
{
    public interface IRagService
    {
        Task<string> ProcessQueryAsync(string question, string systemPrompt, int documentCount);
    }
}