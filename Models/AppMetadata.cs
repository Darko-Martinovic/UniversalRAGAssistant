namespace UniversalRAGAssistant.Models
{
    public class AppMetadata
    {
        public string Title { get; set; } = "Universal RAG Assistant";
        public string Icon { get; set; } = "🤖";
        public string Flag { get; set; } = "";
        public string WelcomeMessage { get; set; } = "Welcome to your AI-powered assistant!";
        public string CapabilityDescription { get; set; } = "I can help you find information and answer questions";
        public string AdditionalInfo { get; set; } = "Ask me anything about the available data";
        public string SystemPrompt { get; set; } = "You are a helpful assistant answering questions based on the provided context. Use the following context to answer the user's question. If the context doesn't contain relevant information, say so.";

        // New fields for dynamic help content
        public string[] HelpExamples { get; set; } = Array.Empty<string>();
        public string[] Tips { get; set; } = Array.Empty<string>();
        public string[] Encouragements { get; set; } = Array.Empty<string>();
        public string[] ErrorAdvice { get; set; } = Array.Empty<string>();
    }
}