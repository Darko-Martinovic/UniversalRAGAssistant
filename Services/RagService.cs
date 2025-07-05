using Azure.Search.Documents.Models;
using UniversalRAGAssistant.Models;
using UniversalRAGAssistant.Interfaces;

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
            var relevanceThreshold = 0.7; // Minimum relevance score (70%)
            var highQualityDocuments = new List<(string title, string content, double score)>();

            await foreach (var result in searchResults.GetResultsAsync())
            {
                // Extract relevance score from Azure Cognitive Search
                var relevanceScore = result.Score ?? 0.0;

                // Validate relevance threshold
                if (relevanceScore >= relevanceThreshold)
                {
                    resultCount++;
                    highQualityDocuments.Add((result.Document.Title, result.Document.Content, relevanceScore));

                    contextBuilder.AppendLine($"Document {resultCount}: {result.Document.Title} (Relevance: {relevanceScore:F2})");
                    contextBuilder.AppendLine(result.Document.Content);
                    contextBuilder.AppendLine();
                }
            }

            // Log validation metrics for monitoring
            var averageScore = highQualityDocuments.Count > 0 ?
                highQualityDocuments.Average(d => d.score) : 0.0;

            Console.WriteLine($"📊 Relevance Validation:");
            Console.WriteLine($"   ✅ Retrieved: {highQualityDocuments.Count} high-quality documents");
            Console.WriteLine($"   📈 Average relevance score: {averageScore:F3}");
            Console.WriteLine($"   🎯 Threshold: {relevanceThreshold} (70% minimum)");

            return contextBuilder.ToString().Trim();
        }
    }
}
