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
        public int RagDocumentCount { get; set; } = 3;
    }

    public class DataSourceConfig
    {
        public DataSourceType Type { get; set; } = DataSourceType.Json;
        public string FilePath { get; set; } = "Data/documents.json";
        public string DirectoryPath { get; set; } = "Data/TextFiles";
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
