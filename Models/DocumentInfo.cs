namespace UniversalRAGAssistant.Models
{
    public class DocumentInfo
    {
        public string Id { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Source { get; set; } = string.Empty;
        public DateTime LastUpdated { get; set; } = DateTime.Now;
    }
}
