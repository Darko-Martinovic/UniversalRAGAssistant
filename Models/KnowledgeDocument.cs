namespace AzureOpenAIConsole.Models
{
    public class KnowledgeDocument
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public IReadOnlyList<float> ContentVector { get; set; }
    }
}
