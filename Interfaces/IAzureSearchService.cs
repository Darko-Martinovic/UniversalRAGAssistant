using Azure.Search.Documents.Models;
using UniversalRAGAssistant.Models;

namespace UniversalRAGAssistant.Interfaces
{
    public interface IAzureSearchService
    {
        Task CreateSearchIndexAsync();
        Task UploadDocumentsAsync(List<KnowledgeDocument> documents);
        Task<SearchResults<KnowledgeDocument>> SearchRelevantDocumentsAsync(float[] queryEmbedding, string query, int documentCount);
    }
}