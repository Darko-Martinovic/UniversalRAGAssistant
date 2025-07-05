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
            var relevanceThreshold = 1.0; // Text search threshold (BM25 scores are much higher)
            var highQualityDocuments = new List<(string title, string content, double score)>();
            var allDocuments = new List<(string title, double score)>();

            await foreach (var result in searchResults.GetResultsAsync())
            {
                // Extract relevance score from Azure Cognitive Search
                var relevanceScore = result.Score ?? 0.0;
                var documentTitle = result.Document.Title ?? "[NO TITLE]";
                var documentId = result.Document.Id ?? "[NO ID]";

                allDocuments.Add((documentTitle, relevanceScore));

                Console.WriteLine($"🔍 Debug: Document '{documentTitle}' (ID: {documentId}) - Score: {relevanceScore:F4}");

                // Validate relevance threshold
                if (relevanceScore >= relevanceThreshold)
                {
                    resultCount++;
                    highQualityDocuments.Add((documentTitle, result.Document.Content, relevanceScore));

                    contextBuilder.AppendLine($"Document {resultCount}: {documentTitle} (Relevance: {relevanceScore:F2})");
                    contextBuilder.AppendLine(result.Document.Content);
                    contextBuilder.AppendLine();
                }
            }

            // Log validation metrics for monitoring
            var averageScore = highQualityDocuments.Count > 0 ?
                highQualityDocuments.Average(d => d.score) : 0.0;

            Console.WriteLine($"📊 Relevance Validation:");
            Console.WriteLine($"   🔍 Total documents found: {allDocuments.Count}");
            Console.WriteLine($"   ✅ Retrieved: {highQualityDocuments.Count} documents above threshold");
            Console.WriteLine($"   📈 Average relevance score: {averageScore:F3}");
            Console.WriteLine($"   🎯 Threshold: {relevanceThreshold} (BM25 text search scoring)");

            if (allDocuments.Count > 0)
            {
                Console.WriteLine($"   📋 Top document scores:");
                foreach (var doc in allDocuments.OrderByDescending(d => d.score).Take(5))
                {
                    Console.WriteLine($"      • {doc.title}: {doc.score:F4}");
                }
            }

            return contextBuilder.ToString().Trim();
        }
    }
}
