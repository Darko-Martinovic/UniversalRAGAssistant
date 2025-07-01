namespace UniversalRAGAssistant.Models
{
    public class DataConfiguration
    {
        public AppMetadata Metadata { get; set; } = new AppMetadata();
        public List<DocumentInfo> Documents { get; set; } = new List<DocumentInfo>();
    }

    public class AppMetadata
    {
        public string Title { get; set; } = "Universal RAG Assistant";
        public string Icon { get; set; } = "🤖";
        public string Flag { get; set; } = "";
        public string WelcomeMessage { get; set; } = "Welcome to your AI-powered assistant!";
        public string CapabilityDescription { get; set; } = "I can help you find information and answer questions";
        public string AdditionalInfo { get; set; } = "Ask me anything about the available data";
    }
}
