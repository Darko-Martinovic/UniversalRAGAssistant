using UniversalRAGAssistant.Models;

namespace UniversalRAGAssistant.Interfaces
{
    public interface IKnowledgeBaseService
    {
        Task<(List<DocumentInfo> documents, AppMetadata metadata)> LoadDocumentsAsync(AppConfiguration appConfig);
        Task SetupKnowledgeBaseAsync(List<DocumentInfo> documents);
    }
}