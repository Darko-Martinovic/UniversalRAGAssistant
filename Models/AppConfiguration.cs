namespace AzureOpenAIConsole.Models
{
    public class AppConfiguration
    {
        public DataSourceConfig DataSource { get; set; } = new DataSourceConfig();
        public string IndexName { get; set; } = "knowledge-index";
    }

    public class DataSourceConfig
    {
        public DataSourceType Type { get; set; } = DataSourceType.Json;
        public string FilePath { get; set; } = "Data/documents.json";
        public string DirectoryPath { get; set; } = "Data/TextFiles";
    }

    public enum DataSourceType
    {
        Json,
        Csv,
        TextFiles,
        HardCoded
    }
}
