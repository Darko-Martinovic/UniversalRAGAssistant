using Azure.Search.Documents.Models;
using UniversalRAGAssistant.Models;
using UniversalRAGAssistant.Interfaces;

namespace UniversalRAGAssistant.Services
{
    public interface IRelevanceValidationService
    {
        Task<RelevanceValidationResult> ValidateSearchResults(string query, SearchResults<KnowledgeDocument> searchResults);
        double CalculateOverallRelevanceScore(List<DocumentRelevance> documents);
    }

    public class RelevanceValidationService : IRelevanceValidationService
    {
        private readonly IAzureOpenAIService _openAIService;
        private const double MINIMUM_RELEVANCE_THRESHOLD = 0.65;
        private const double HIGH_RELEVANCE_THRESHOLD = 0.85;

        public RelevanceValidationService(IAzureOpenAIService openAIService)
        {
            _openAIService = openAIService;
        }

        public async Task<RelevanceValidationResult> ValidateSearchResults(
            string query,
            SearchResults<KnowledgeDocument> searchResults)
        {
            var documentRelevances = new List<DocumentRelevance>();
            var totalDocuments = 0;
            var relevantDocuments = 0;
            var highQualityDocuments = 0;

            await foreach (var result in searchResults.GetResultsAsync())
            {
                totalDocuments++;
                var relevance = await ValidateDocumentRelevance(query, result);
                documentRelevances.Add(relevance);

                if (relevance.IsRelevant)
                {
                    relevantDocuments++;
                    if (relevance.FinalRelevanceScore >= HIGH_RELEVANCE_THRESHOLD)
                    {
                        highQualityDocuments++;
                    }
                }
            }

            return new RelevanceValidationResult
            {
                Query = query,
                TotalDocuments = totalDocuments,
                RelevantDocuments = relevantDocuments,
                HighQualityDocuments = highQualityDocuments,
                OverallRelevanceScore = CalculateOverallRelevanceScore(documentRelevances),
                DocumentRelevances = documentRelevances,
                ValidationTimestamp = DateTime.UtcNow
            };
        }

        private async Task<DocumentRelevance> ValidateDocumentRelevance(
            string query,
            SearchResult<KnowledgeDocument> searchResult)
        {
            var document = searchResult.Document;
            var vectorScore = searchResult.Score ?? 0.0;

            // Multi-layer validation
            var keywordRelevance = CalculateKeywordRelevance(query, document);
            var businessContextScore = CalculateBusinessContextScore(query, document);
            var semanticValidation = await ValidateSemanticRelevance(query, document);

            // Weighted relevance calculation
            var finalScore = (vectorScore * 0.4) +
                           (keywordRelevance * 0.2) +
                           (businessContextScore * 0.2) +
                           (semanticValidation * 0.2);

            return new DocumentRelevance
            {
                DocumentId = document.Id,
                DocumentTitle = document.Title,
                VectorScore = vectorScore,
                KeywordRelevance = keywordRelevance,
                BusinessContextScore = businessContextScore,
                SemanticValidation = semanticValidation,
                FinalRelevanceScore = finalScore,
                IsRelevant = finalScore >= MINIMUM_RELEVANCE_THRESHOLD,
                ValidationDetails = GenerateValidationDetails(query, document, finalScore)
            };
        }

        private double CalculateKeywordRelevance(string query, KnowledgeDocument document)
        {
            var queryWords = ExtractKeywords(query.ToLower());
            var documentWords = ExtractKeywords($"{document.Title} {document.Content}".ToLower());

            var matchingWords = queryWords.Intersect(documentWords).Count();
            var totalQueryWords = queryWords.Count();

            return totalQueryWords > 0 ? (double)matchingWords / totalQueryWords : 0.0;
        }

        private double CalculateBusinessContextScore(string query, KnowledgeDocument document)
        {
            // Belgian supermarket specific business terms
            var businessTerms = new[]
            {
                "delhaize", "colruyt", "carrefour", "lidl", "aldi",
                "pricing", "margin", "procurement", "customer", "store",
                "belgian", "belgium", "brussels", "antwerp", "ghent",
                "supermarket", "retail", "grocery", "fresh", "promotion"
            };

            var queryBusinessTerms = businessTerms.Where(term =>
                query.ToLower().Contains(term)).Count();
            var documentBusinessTerms = businessTerms.Where(term =>
                $"{document.Title} {document.Content}".ToLower().Contains(term)).Count();

            var businessContextOverlap = Math.Min(queryBusinessTerms, documentBusinessTerms);
            var maxPossibleOverlap = Math.Max(queryBusinessTerms, 1);

            return (double)businessContextOverlap / maxPossibleOverlap;
        }

        private async Task<double> ValidateSemanticRelevance(string query, KnowledgeDocument document)
        {
            try
            {
                var validationPrompt = $@"Rate the relevance of this business document to the user's question on a scale of 0.0 to 1.0.

Question: {query}

Document Title: {document.Title}
Document Content: {document.Content.Substring(0, Math.Min(300, document.Content.Length))}...

Provide only a decimal number between 0.0 and 1.0 representing relevance.";

                var response = await _openAIService.GenerateResponseWithContextAsync(
                    validationPrompt, "",
                    "You are a relevance validation assistant. Return only a decimal number between 0.0 and 1.0.");

                if (double.TryParse(response.Trim(), out double relevanceScore))
                {
                    return Math.Max(0.0, Math.Min(1.0, relevanceScore));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Semantic validation failed: {ex.Message}");
            }

            return 0.5; // Default neutral score if validation fails
        }

        public double CalculateOverallRelevanceScore(List<DocumentRelevance> documents)
        {
            if (!documents.Any()) return 0.0;

            return documents.Average(d => d.FinalRelevanceScore);
        }

        private List<string> ExtractKeywords(string text)
        {
            return text.Split(' ', StringSplitOptions.RemoveEmptyEntries)
                      .Where(word => word.Length > 3)
                      .Distinct()
                      .ToList();
        }

        private string GenerateValidationDetails(string query, KnowledgeDocument document, double score)
        {
            var quality = score >= HIGH_RELEVANCE_THRESHOLD ? "High" :
                         score >= MINIMUM_RELEVANCE_THRESHOLD ? "Medium" : "Low";

            return $"Query: '{query}' | Document: '{document.Title}' | Quality: {quality} | Score: {score:F3}";
        }
    }

    // Supporting model classes
    public class RelevanceValidationResult
    {
        public string Query { get; set; } = string.Empty;
        public int TotalDocuments { get; set; }
        public int RelevantDocuments { get; set; }
        public int HighQualityDocuments { get; set; }
        public double OverallRelevanceScore { get; set; }
        public List<DocumentRelevance> DocumentRelevances { get; set; } = new();
        public DateTime ValidationTimestamp { get; set; }

        public string GetValidationSummary()
        {
            var relevancePercentage = TotalDocuments > 0 ?
                (double)RelevantDocuments / TotalDocuments * 100 : 0;

            return $"Validation: {RelevantDocuments}/{TotalDocuments} relevant ({relevancePercentage:F1}%), " +
                   $"Overall Score: {OverallRelevanceScore:F3}, High Quality: {HighQualityDocuments}";
        }
    }

    public class DocumentRelevance
    {
        public string DocumentId { get; set; } = string.Empty;
        public string DocumentTitle { get; set; } = string.Empty;
        public double VectorScore { get; set; }
        public double KeywordRelevance { get; set; }
        public double BusinessContextScore { get; set; }
        public double SemanticValidation { get; set; }
        public double FinalRelevanceScore { get; set; }
        public bool IsRelevant { get; set; }
        public string ValidationDetails { get; set; } = string.Empty;
    }
}
