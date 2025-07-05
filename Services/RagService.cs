using Azure.Search.Documents.Models;
using UniversalRAGAssistant.Models;

namespace UniversalRAGAssistant.Services
{
    public class RagService : IRagService
    {
        private readonly IAzureOpenAIService _openAIService;
        private readonly IAzureSearchService _searchService;

        public RagService(IAzureOpenAIService openAIService, IAzureSearchService searchService)
        {
            _openAIService = openAIService;
            _searchService = searchService;
        }

        public async Task<string> ProcessQueryAsync(
            string question,
            string systemPrompt,
            int documentCount
        )
        {
            // Generate embedding for the question
            var queryEmbedding = await _openAIService.GetEmbeddingAsync(question);

            // Search for relevant documents
            var searchResults = await _searchService.SearchRelevantDocumentsAsync(
                queryEmbedding,
                question,
                documentCount
            );

            // Build context from search results
            var context = await BuildContextFromSearchResults(searchResults);

            // Generate response using the context
            return await _openAIService.GenerateResponseWithContextAsync(
                question,
                context,
                systemPrompt
            );
        }

        private async Task<string> BuildContextFromSearchResults(
            SearchResults<KnowledgeDocument> searchResults
        )
        {
            var contextBuilder = new System.Text.StringBuilder();
            var resultCount = 0;

            await foreach (var result in searchResults.GetResultsAsync())
            {
                resultCount++;
                contextBuilder.AppendLine($"Document {resultCount}: {result.Document.Title}");
                contextBuilder.AppendLine(result.Document.Content);
                contextBuilder.AppendLine();
            }

            return contextBuilder.ToString().Trim();
        }
    }
}
