namespace UniversalRAGAssistant.Models
{
    public class KnowledgeDocument
    {
        public string Id { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public float[] ContentVector { get; set; } = Array.Empty<float>();
    }
}
