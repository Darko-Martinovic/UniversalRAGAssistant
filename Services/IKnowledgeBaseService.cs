using UniversalRAGAssistant.Models;

namespace UniversalRAGAssistant.Services
{
    public interface IKnowledgeBaseService
    {
        Task<(List<DocumentInfo> documents, AppMetadata metadata)> LoadDocumentsAsync(AppConfiguration appConfig);
        Task SetupKnowledgeBaseAsync(List<DocumentInfo> documents);
    }
}