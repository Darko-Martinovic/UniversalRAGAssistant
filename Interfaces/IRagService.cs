using UniversalRAGAssistant.Models;
using UniversalRAGAssistant.Services;

namespace UniversalRAGAssistant.Interfaces
{
    public interface IRagService
    {
        Task<string> ProcessQueryAsync(string question, string systemPrompt, int documentCount);
        Task<RelevanceValidationResult> GetRelevanceAnalysisAsync(string question, int documentCount);
    }
}