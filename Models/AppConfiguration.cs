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
        public DataSourceConfig DataSource { get; set; } = new DataSourceConfig();
        public string IndexName { get; set; } = "knowledge-index";
        public AppMetadata Metadata { get; set; } = new AppMetadata();
    }

    public class DataSourceConfig
    {
        public DataSourceType Type { get; set; } = DataSourceType.Json;
        public string FilePath { get; set; } = "Data/documents.json";
        public string DirectoryPath { get; set; } = "Data/TextFiles";
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

    public class DataConfiguration
    {
        public AppMetadata Metadata { get; set; } = new AppMetadata();
        public List<DocumentInfo> Documents { get; set; } = new List<DocumentInfo>();
    }

    public enum DataSourceType
    {
        Json,
        Csv,
        TextFiles
    }
}
