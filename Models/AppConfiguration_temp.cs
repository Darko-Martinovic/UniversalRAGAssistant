namespace UniversalRAGAssistant.Models
{
    public class AppConfiguration
    {
        public string SearchEndpoint { get; set; } = string.Empty;
        public string SearchApiKey { get; set; } = string.Empty;
        public string SearchIndexName { get; set; } = string.Empty;
        public string OpenAIEndpoint { get; set; } = string.Empty;
        public string OpenAIApiKey { get; set; } = string.Empty;
        public string OpenAIModel { get; set; } = string.Empty;
    }
}
