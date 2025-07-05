using Azure.Search.Documents.Models;
using UniversalRAGAssistant.Models;
using UniversalRAGAssistant.Interfaces;
using UniversalRAGAssistant.Services;

namespace UniversalRAGAssistant.Services
{
    public class RagService : IRagService
    {
        private readonly IAzureOpenAIService _openAIService;
        private readonly IAzureSearchService _searchService;
        private readonly IRelevanceValidationService _relevanceService;

        public RagService(
            IAzureOpenAIService openAIService,
            IAzureSearchService searchService,
            IRelevanceValidationService relevanceService)
        {
            _openAIService = openAIService;
            _searchService = searchService;
            _relevanceService = relevanceService;
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

            // Validate relevance and get detailed analysis
            var relevanceResult = await _relevanceService.ValidateSearchResults(question, searchResults);

            // Build context from validated search results
            var context = await BuildContextFromSearchResults(searchResults, relevanceResult);

            // Generate response using the context
            return await _openAIService.GenerateResponseWithContextAsync(
                question,
                context,
                systemPrompt
            );
        }

        private async Task<string> BuildContextFromSearchResults(
            SearchResults<KnowledgeDocument> searchResults,
            RelevanceValidationResult relevanceResult)
        {
            var contextBuilder = new System.Text.StringBuilder();
            var resultCount = 0;
            var relevanceThreshold = 0.60; // Match the validation service threshold (60%)
            var highQualityDocuments = new List<(string title, string content, double score)>();

            // Debug: Show all documents and their scores
            Console.WriteLine($"🔍 DEBUG: Analyzing {relevanceResult.DocumentRelevances.Count} documents:");
            foreach (var doc in relevanceResult.DocumentRelevances.OrderByDescending(d => d.FinalRelevanceScore))
            {
                Console.WriteLine($"   📄 {doc.DocumentTitle}: {doc.FinalRelevanceScore:F3} (Vector: {doc.VectorScore:F3}, Keywords: {doc.KeywordRelevance:F3}, Business: {doc.BusinessContextScore:F3}, Semantic: {doc.SemanticValidation:F3})");
            }

            // Use validated relevance results instead of raw search scores
            var validatedDocuments = relevanceResult.DocumentRelevances
                .Where(d => d.FinalRelevanceScore >= relevanceThreshold)
                .OrderByDescending(d => d.FinalRelevanceScore)
                .ToList();

            Console.WriteLine($"🎯 DEBUG: {validatedDocuments.Count} documents meet threshold {relevanceThreshold}");

            foreach (var docRelevance in validatedDocuments)
            {
                resultCount++;
                highQualityDocuments.Add((docRelevance.DocumentTitle, "", docRelevance.FinalRelevanceScore));

                contextBuilder.AppendLine($"Document {resultCount}: {docRelevance.DocumentTitle} (Relevance: {docRelevance.FinalRelevanceScore:F3}, Confidence: {docRelevance.ConfidenceLevel})");

                // Find the actual document content from search results
                await foreach (var result in searchResults.GetResultsAsync())
                {
                    if (result.Document.Id == docRelevance.DocumentId)
                    {
                        contextBuilder.AppendLine(result.Document.Content);
                        break;
                    }
                }
                contextBuilder.AppendLine();
            }

            // Enhanced validation metrics
            var averageScore = highQualityDocuments.Count > 0 ?
                highQualityDocuments.Average(d => d.score) : 0.0;

            Console.WriteLine($"📊 Enhanced Relevance Validation:");
            Console.WriteLine($"   ✅ Retrieved: {highQualityDocuments.Count} high-quality documents");
            Console.WriteLine($"   📈 Average relevance score: {averageScore:F3}");
            Console.WriteLine($"   🎯 Threshold: {relevanceThreshold} (65% minimum)");
            Console.WriteLine($"   🔍 Overall validation score: {relevanceResult.OverallRelevanceScore:F3}");

            return contextBuilder.ToString().Trim();
        }

        public async Task<RelevanceValidationResult> GetRelevanceAnalysisAsync(
            string question,
            int documentCount)
        {
            var queryEmbedding = await _openAIService.GetEmbeddingAsync(question);
            var searchResults = await _searchService.SearchRelevantDocumentsAsync(
                queryEmbedding,
                question,
                documentCount
            );

            return await _relevanceService.ValidateSearchResults(question, searchResults);
        }
    }
}
