using System.Text.RegularExpressions;
using UniversalRAGAssistant.Models;

namespace UniversalRAGAssistant.Services
{
    public class DocumentQualityAssessment
    {
        public DocumentQualityReport AssessDocumentQuality(DocumentInfo document)
        {
            var report = new DocumentQualityReport
            {
                DocumentId = document.Id,
                DocumentTitle = document.Title,
                AssessmentDate = DateTime.UtcNow
            };

            // Assess multiple quality dimensions
            report.ContentLength = AssessContentLength(document.Content);
            report.QuantifiedData = AssessQuantifiedData(document.Content);
            report.BusinessTerms = AssessBusinessTerminology(document.Content);
            report.ActionableInsights = AssessActionableContent(document.Content);
            report.Structure = AssessDocumentStructure(document.Content);
            report.Freshness = AssessFreshness(document.Content);

            // Calculate overall quality score
            report.OverallQualityScore = CalculateOverallScore(report);
            report.QualityGrade = GetQualityGrade(report.OverallQualityScore);
            report.Recommendations = GenerateRecommendations(report);

            return report;
        }

        private double AssessContentLength(string content)
        {
            var wordCount = content.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length;
            
            // Optimal range: 100-500 words per document
            if (wordCount < 50) return 0.3; // Too short
            if (wordCount >= 50 && wordCount <= 100) return 0.6; // Minimal
            if (wordCount >= 100 && wordCount <= 500) return 1.0; // Optimal
            if (wordCount <= 1000) return 0.8; // Good but long
            return 0.6; // Too long for optimal AI processing
        }

        private double AssessQuantifiedData(string content)
        {
            // Look for specific metrics, percentages, and quantified insights
            var quantifierPatterns = new[]
            {
                @"\d+%",           // Percentages
                @"€\d+",           // Euro amounts
                @"\d+\.\d+",       // Decimal numbers
                @"\d+K",           // Thousands
                @"\d+M",           // Millions
                @"\d+x",           // Multipliers
                @"\+\d+%",         // Growth percentages
                @"-\d+%"           // Decline percentages
            };

            var quantifierCount = quantifierPatterns
                .Sum(pattern => Regex.Matches(content, pattern).Count);

            // Score based on quantified data density
            if (quantifierCount >= 10) return 1.0; // Excellent quantification
            if (quantifierCount >= 5) return 0.8;  // Good quantification
            if (quantifierCount >= 2) return 0.6;  // Some quantification
            return 0.3; // Poor quantification
        }

        private double AssessBusinessTerminology(string content)
        {
            var businessTerms = new[]
            {
                // Belgian supermarket specific
                "delhaize", "colruyt", "carrefour", "lidl", "aldi", "spar",
                // Business intelligence terms
                "margin", "revenue", "roi", "kpi", "performance", "optimization",
                "strategy", "competitive", "market", "customer", "pricing",
                "procurement", "efficiency", "profitability", "segmentation",
                // Geographic context
                "belgian", "belgium", "brussels", "antwerp", "ghent", "wallonia"
            };

            var termCount = businessTerms
                .Count(term => content.ToLower().Contains(term));

            var termDensity = (double)termCount / businessTerms.Length;
            return Math.Min(1.0, termDensity * 3); // Cap at 1.0
        }

        private double AssessActionableContent(string content)
        {
            var actionableIndicators = new[]
            {
                "recommend", "should", "strategy", "action", "implement",
                "optimize", "improve", "increase", "reduce", "focus",
                "target", "opportunity", "advantage", "leverage"
            };

            var actionableCount = actionableIndicators
                .Count(indicator => content.ToLower().Contains(indicator));

            return Math.Min(1.0, (double)actionableCount / 5); // 5+ actionable terms = perfect score
        }

        private double AssessDocumentStructure(string content)
        {
            var structureScore = 0.0;

            // Check for clear sections/organization
            if (content.Contains(":")) structureScore += 0.3; // Has categorized content
            if (content.Contains("(") && content.Contains(")")) structureScore += 0.2; // Has parenthetical details
            if (content.Length > 200 && content.Split('.').Length > 3) structureScore += 0.3; // Multiple sentences
            if (Regex.IsMatch(content, @"\d+\)|\d+\.")) structureScore += 0.2; // Numbered lists

            return Math.Min(1.0, structureScore);
        }

        private double AssessFreshness(string content)
        {
            // Look for date indicators
            var currentYear = DateTime.Now.Year;
            var datePatterns = new[]
            {
                currentYear.ToString(),
                (currentYear - 1).ToString(),
                "2024", "2025", // Specific recent years
                "July", "June", "August", // Recent months
                "Q1", "Q2", "Q3", "Q4" // Quarterly data
            };

            var hasFreshDates = datePatterns.Any(pattern => content.Contains(pattern));
            return hasFreshDates ? 1.0 : 0.5; // Recent dates = fresh content
        }

        private double CalculateOverallScore(DocumentQualityReport report)
        {
            return (report.ContentLength * 0.15 +
                   report.QuantifiedData * 0.25 +
                   report.BusinessTerms * 0.20 +
                   report.ActionableInsights * 0.20 +
                   report.Structure * 0.10 +
                   report.Freshness * 0.10);
        }

        private string GetQualityGrade(double score)
        {
            if (score >= 0.9) return "A+ (Excellent)";
            if (score >= 0.8) return "A (Very Good)";
            if (score >= 0.7) return "B+ (Good)";
            if (score >= 0.6) return "B (Acceptable)";
            if (score >= 0.5) return "C (Needs Improvement)";
            return "D (Poor - Requires Revision)";
        }

        private List<string> GenerateRecommendations(DocumentQualityReport report)
        {
            var recommendations = new List<string>();

            if (report.ContentLength < 0.7)
            {
                recommendations.Add("📝 Add more detailed content (aim for 100-500 words)");
            }

            if (report.QuantifiedData < 0.7)
            {
                recommendations.Add("📊 Include more specific metrics, percentages, and quantified data");
            }

            if (report.BusinessTerms < 0.6)
            {
                recommendations.Add("🏢 Add more business-specific terminology and context");
            }

            if (report.ActionableInsights < 0.6)
            {
                recommendations.Add("🎯 Include more actionable recommendations and strategic insights");
            }

            if (report.Structure < 0.6)
            {
                recommendations.Add("📋 Improve document structure with clear sections and organization");
            }

            if (report.Freshness < 0.8)
            {
                recommendations.Add("📅 Update with current dates and recent data");
            }

            if (report.OverallQualityScore >= 0.8)
            {
                recommendations.Add("✅ Excellent quality! This document will generate high-quality AI responses.");
            }

            return recommendations;
        }

        public string GenerateQualityReport(List<DocumentInfo> documents)
        {
            var reports = documents.Select(AssessDocumentQuality).ToList();
            var averageScore = reports.Average(r => r.OverallQualityScore);
            var highQualityDocs = reports.Count(r => r.OverallQualityScore >= 0.7);

            var report = $@"
📊 DOCUMENT QUALITY ASSESSMENT REPORT
=====================================
📅 Assessment Date: {DateTime.UtcNow:yyyy-MM-dd HH:mm}
📄 Total Documents: {documents.Count}
⭐ High Quality Documents: {highQualityDocs} ({(double)highQualityDocs/documents.Count*100:F1}%)
📈 Average Quality Score: {averageScore:F3} ({GetQualityGrade(averageScore)})

🔍 INDIVIDUAL DOCUMENT SCORES:
";

            foreach (var docReport in reports.OrderByDescending(r => r.OverallQualityScore))
            {
                report += $@"
📄 {docReport.DocumentTitle}
   Overall Score: {docReport.OverallQualityScore:F3} ({docReport.QualityGrade})
   Content: {docReport.ContentLength:F2} | Data: {docReport.QuantifiedData:F2} | Business: {docReport.BusinessTerms:F2}
   Actions: {docReport.ActionableInsights:F2} | Structure: {docReport.Structure:F2} | Fresh: {docReport.Freshness:F2}
";

                if (docReport.Recommendations.Any())
                {
                    report += $"   💡 Recommendations: {string.Join(", ", docReport.Recommendations)}\n";
                }
            }

            return report;
        }
    }

    public class DocumentQualityReport
    {
        public string DocumentId { get; set; } = string.Empty;
        public string DocumentTitle { get; set; } = string.Empty;
        public DateTime AssessmentDate { get; set; }
        
        // Quality dimensions (0.0 - 1.0)
        public double ContentLength { get; set; }
        public double QuantifiedData { get; set; }
        public double BusinessTerms { get; set; }
        public double ActionableInsights { get; set; }
        public double Structure { get; set; }
        public double Freshness { get; set; }
        
        // Overall assessment
        public double OverallQualityScore { get; set; }
        public string QualityGrade { get; set; } = string.Empty;
        public List<string> Recommendations { get; set; } = new List<string>();
    }
}
