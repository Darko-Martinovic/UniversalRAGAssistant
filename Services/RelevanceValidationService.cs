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
        private const double MINIMUM_RELEVANCE_THRESHOLD = 0.60; // Lowered from 0.65 to be more inclusive
        private const double HIGH_RELEVANCE_THRESHOLD = 0.80; // Lowered from 0.85 to be more inclusive

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

            // Collect all results and find max score
            var results = new List<SearchResult<KnowledgeDocument>>();
            await foreach (var result in searchResults.GetResultsAsync())
            {
                results.Add(result);
            }
            var maxScore = results.Count > 0 ? results.Max(r => r.Score ?? 0.0) : 0.0;

            foreach (var result in results)
            {
                totalDocuments++;
                var relevance = ValidateDocumentRelevance(query, result, maxScore);
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

        private DocumentRelevance ValidateDocumentRelevance(
            string query,
            SearchResult<KnowledgeDocument> searchResult,
            double maxScore)
        {
            var document = searchResult.Document;
            var rawVectorScore = searchResult.Score ?? 0.0;
            var vectorScore = (maxScore > 0) ? (rawVectorScore / maxScore) : 0.0;

            // Multi-layer validation
            var keywordRelevance = CalculateKeywordRelevance(query, document);
            var businessContextScore = CalculateBusinessContextScore(query, document);
            var semanticValidation = ValidateSemanticRelevance(query, document);

            // Enhanced weighted relevance calculation with configurable weights
            var relevance = new DocumentRelevance
            {
                DocumentId = document.Id,
                DocumentTitle = document.Title,
                VectorScore = vectorScore,
                KeywordRelevance = keywordRelevance,
                BusinessContextScore = businessContextScore,
                SemanticValidation = semanticValidation,
                ContentLength = document.Content.Length,
                ValidationTimestamp = DateTime.UtcNow
            };

            // Calculate final score with configurable weights
            relevance.FinalRelevanceScore = (relevance.VectorScore * relevance.VectorWeight) +
                                           (relevance.KeywordRelevance * relevance.KeywordWeight) +
                                           (relevance.BusinessContextScore * relevance.BusinessWeight) +
                                           (relevance.SemanticValidation * relevance.SemanticWeight);

            // Determine relevance category and confidence
            relevance.IsRelevant = relevance.FinalRelevanceScore >= MINIMUM_RELEVANCE_THRESHOLD;
            relevance.RelevanceCategory = GetRelevanceCategory(relevance.FinalRelevanceScore);
            relevance.ConfidenceLevel = GetConfidenceLevel(relevance);
            relevance.ValidationDetails = GenerateValidationDetails(query, document, relevance.FinalRelevanceScore);

            // Debug output for each document
            Console.WriteLine($"🔍 DEBUG: {document.Title}");
            Console.WriteLine($"   Raw Vector: {rawVectorScore:F3} | Normalized Vector: {vectorScore:F3} | Keywords: {keywordRelevance:F3} | Business: {businessContextScore:F3} | Semantic: {semanticValidation:F3}");
            Console.WriteLine($"   Final Score: {relevance.FinalRelevanceScore:F3} | Relevant: {relevance.IsRelevant} | Threshold: {MINIMUM_RELEVANCE_THRESHOLD}");

            return relevance;
        }

        private string GetRelevanceCategory(double score)
        {
            if (score >= 0.80) return "High";
            if (score >= 0.60) return "Medium";
            return "Low";
        }

        private string GetConfidenceLevel(DocumentRelevance relevance)
        {
            // Calculate confidence based on score consistency and content quality
            var scoreVariance = Math.Abs(relevance.VectorScore - relevance.KeywordRelevance) +
                               Math.Abs(relevance.BusinessContextScore - relevance.SemanticValidation);

            var contentQuality = relevance.ContentLength > 200 ? 1.0 : relevance.ContentLength / 200.0;
            var consistencyScore = 1.0 - (scoreVariance / 4.0); // Normalize variance

            var confidenceScore = (relevance.FinalRelevanceScore * 0.6) +
                                 (consistencyScore * 0.3) +
                                 (contentQuality * 0.1);

            if (confidenceScore >= 0.8) return "High";
            if (confidenceScore >= 0.6) return "Medium";
            return "Low";
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

        private double ValidateSemanticRelevance(string query, KnowledgeDocument document)
        {
            // Simplified semantic validation using keyword density and content analysis
            // This avoids expensive API calls and provides more consistent results
            
            var queryWords = ExtractKeywords(query.ToLower());
            var documentText = $"{document.Title} {document.Content}".ToLower();
            
            // Calculate keyword density
            var keywordMatches = queryWords.Count(word => documentText.Contains(word));
            var keywordDensity = queryWords.Count > 0 ? (double)keywordMatches / queryWords.Count : 0.0;
            
            // Calculate content relevance based on title and content overlap
            var titleRelevance = CalculateTitleRelevance(query, document.Title);
            var contentRelevance = CalculateContentRelevance(query, document.Content);
            
            // Weighted combination
            var semanticScore = (keywordDensity * 0.4) + (titleRelevance * 0.3) + (contentRelevance * 0.3);
            
            return Math.Max(0.0, Math.Min(1.0, semanticScore));
        }
        
        private double CalculateTitleRelevance(string query, string title)
        {
            var queryWords = ExtractKeywords(query.ToLower());
            var titleWords = ExtractKeywords(title.ToLower());
            
            var matchingWords = queryWords.Intersect(titleWords).Count();
            var totalQueryWords = queryWords.Count;
            
            return totalQueryWords > 0 ? (double)matchingWords / totalQueryWords : 0.0;
        }
        
        private double CalculateContentRelevance(string query, string content)
        {
            var queryWords = ExtractKeywords(query.ToLower());
            var contentWords = ExtractKeywords(content.ToLower());
            
            var matchingWords = queryWords.Intersect(contentWords).Count();
            var totalQueryWords = queryWords.Count;
            
            // Bonus for content length (more content = more potential relevance)
            var contentBonus = Math.Min(0.2, content.Length / 10000.0); // Max 20% bonus for long content
            
            var baseRelevance = totalQueryWords > 0 ? (double)matchingWords / totalQueryWords : 0.0;
            return Math.Min(1.0, baseRelevance + contentBonus);
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

        public string GetDetailedRelevanceReport()
        {
            var report = new System.Text.StringBuilder();

            report.AppendLine("📊 DOCUMENT RELEVANCE ANALYSIS");
            report.AppendLine("═══════════════════════════════");
            report.AppendLine($"🔍 Query: \"{Query}\"");
            report.AppendLine($"📅 Analysis Time: {ValidationTimestamp:yyyy-MM-dd HH:mm:ss}");
            report.AppendLine($"📈 Overall Relevance Score: {OverallRelevanceScore:F3}");
            report.AppendLine($"✅ Relevant Documents: {RelevantDocuments}/{TotalDocuments}");
            report.AppendLine($"⭐ High Quality Documents: {HighQualityDocuments}");
            report.AppendLine();

            if (DocumentRelevances.Any())
            {
                report.AppendLine("📄 INDIVIDUAL DOCUMENT ANALYSIS:");
                report.AppendLine("─────────────────────────────────");

                var sortedDocuments = DocumentRelevances
                    .OrderByDescending(d => d.FinalRelevanceScore)
                    .ToList();

                for (int i = 0; i < sortedDocuments.Count; i++)
                {
                    var doc = sortedDocuments[i];
                    var rank = i + 1;
                    var qualityIcon = doc.IsHighQuality ? "⭐" : doc.IsMediumQuality ? "✅" : "⚠️";
                    var confidenceIcon = doc.ConfidenceLevel == "High" ? "🟢" :
                                       doc.ConfidenceLevel == "Medium" ? "🟡" : "🔴";

                    report.AppendLine($"{rank}. {qualityIcon} {doc.DocumentTitle}");
                    report.AppendLine($"   📊 Final Score: {doc.FinalRelevanceScore:F3} ({doc.RelevanceCategory})");
                    report.AppendLine($"   🎯 Confidence: {confidenceIcon} {doc.ConfidenceLevel}");
                    report.AppendLine($"   📏 Content Length: {doc.ContentLength:N0} characters");
                    report.AppendLine($"   🔍 Score Breakdown: {doc.GetDetailedBreakdown()}");

                    if (i < sortedDocuments.Count - 1) report.AppendLine();
                }
            }

            return report.ToString();
        }

        public string GetRelevanceSummary()
        {
            var highQuality = DocumentRelevances.Count(d => d.IsHighQuality);
            var mediumQuality = DocumentRelevances.Count(d => d.IsMediumQuality);
            var lowQuality = DocumentRelevances.Count(d => d.IsLowQuality);

            return $"📊 Relevance Summary: ⭐{highQuality} High | ✅{mediumQuality} Medium | ⚠️{lowQuality} Low | " +
                   $"Overall: {OverallRelevanceScore:F3}";
        }
    }

    public class DocumentRelevance
    {
        public string DocumentId { get; set; } = string.Empty;
        public string DocumentTitle { get; set; } = string.Empty;
        public string DocumentCategory { get; set; } = string.Empty;
        public string DocumentSource { get; set; } = string.Empty;
        public DateTime DocumentLastUpdated { get; set; }

        // Individual relevance scores
        public double VectorScore { get; set; }
        public double KeywordRelevance { get; set; }
        public double BusinessContextScore { get; set; }
        public double SemanticValidation { get; set; }

        // Weighted calculation components
        public double VectorWeight { get; set; } = 0.5;  // Increased weight for vector similarity
        public double KeywordWeight { get; set; } = 0.2;
        public double BusinessWeight { get; set; } = 0.2;
        public double SemanticWeight { get; set; } = 0.1; // Reduced weight for semantic validation

        // Final assessment
        public double FinalRelevanceScore { get; set; }
        public bool IsRelevant { get; set; }
        public string RelevanceCategory { get; set; } = string.Empty; // "High", "Medium", "Low"
        public string ConfidenceLevel { get; set; } = string.Empty; // "High", "Medium", "Low"
        public string ValidationDetails { get; set; } = string.Empty;

        // Additional metadata
        public int ContentLength { get; set; }
        public int KeywordMatches { get; set; }
        public int BusinessTermMatches { get; set; }
        public DateTime ValidationTimestamp { get; set; } = DateTime.UtcNow;

        // Helper methods
        public string GetRelevanceSummary()
        {
            return $"Document: {DocumentTitle} | Score: {FinalRelevanceScore:F3} | Category: {RelevanceCategory} | Confidence: {ConfidenceLevel}";
        }

        public string GetDetailedBreakdown()
        {
            return $"Vector: {VectorScore:F3} | Keywords: {KeywordRelevance:F3} | Business: {BusinessContextScore:F3} | Semantic: {SemanticValidation:F3}";
        }

        public bool IsHighQuality => FinalRelevanceScore >= 0.80;
        public bool IsMediumQuality => FinalRelevanceScore >= 0.60 && FinalRelevanceScore < 0.80;
        public bool IsLowQuality => FinalRelevanceScore < 0.60;
    }
}
