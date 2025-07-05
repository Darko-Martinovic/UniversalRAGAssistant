using UniversalRAGAssistant.Models;

namespace UniversalRAGAssistant.Services
{
    public class KnowledgeBaseService : IKnowledgeBaseService
    {
        private readonly IAzureOpenAIService _openAIService;
        private readonly IAzureSearchService _searchService;
        private readonly IConsoleUIService _uiService;

        public KnowledgeBaseService(
            IAzureOpenAIService openAIService,
            IAzureSearchService searchService,
            IConsoleUIService uiService
        )
        {
            _openAIService = openAIService;
            _searchService = searchService;
            _uiService = uiService;
        }

        public async Task<(List<DocumentInfo> documents, AppMetadata metadata)> LoadDocumentsAsync(
            AppConfiguration appConfig
        )
        {
            _uiService.PrintWarning("🔧 Setting up knowledge base...");

            var documentLoader = new DocumentLoader();
            List<DocumentInfo> documents = new List<DocumentInfo>();
            AppMetadata metadata = new AppMetadata();

            try
            {
                switch (appConfig.DataSource.Type)
                {
                    case DataSourceType.Json:
                        _uiService.PrintWarning(
                            $"📄 Loading data from JSON: {appConfig.DataSource.FilePath}"
                        );

                        // Load both documents and metadata
                        var dataConfig = await documentLoader.LoadDataConfigurationAsync(
                            appConfig.DataSource.FilePath
                        );
                        documents = dataConfig.Documents;
                        metadata = dataConfig.Metadata;
                        break;

                    case DataSourceType.Csv:
                        _uiService.PrintWarning(
                            $"📊 Loading data from CSV: {appConfig.DataSource.FilePath}"
                        );
                        documents = await documentLoader.LoadDocumentsFromCsvAsync(
                            appConfig.DataSource.FilePath
                        );
                        metadata = new AppMetadata(); // Use default metadata for CSV
                        break;

                    case DataSourceType.TextFiles:
                        _uiService.PrintWarning(
                            $"📁 Loading data from text files: {appConfig.DataSource.DirectoryPath}"
                        );
                        documents = await documentLoader.LoadDocumentsFromTextFilesAsync(
                            appConfig.DataSource.DirectoryPath
                        );
                        metadata = new AppMetadata(); // Use default metadata for text files
                        break;

                    default:
                        _uiService.PrintError("⚠️  Invalid data source type configured");
                        _uiService.PrintWarning(
                            "Please check your appsettings.json configuration."
                        );
                        Environment.Exit(1);
                        break;
                }

                _uiService.PrintWarning($"✅ Successfully loaded {documents.Count} documents");
            }
            catch (Exception ex)
            {
                _uiService.PrintError($"❌ Error loading documents: {ex.Message}");
                _uiService.PrintWarning("⚠️  Application is not properly configured");
                _uiService.PrintWarning(
                    "Please ensure your data source is properly configured in appsettings.json"
                );
                _uiService.PrintWarning("and the specified data files exist.");
                Environment.Exit(1);
            }

            return (documents, metadata);
        }

        public async Task SetupKnowledgeBaseAsync(List<DocumentInfo> documents)
        {
            // Create or update search index
            _uiService.PrintWarning("🔍 Creating Azure Cognitive Search index...");
            await _searchService.CreateSearchIndexAsync();

            // Generate embeddings and upload documents
            var knowledgeDocs = new List<KnowledgeDocument>();
            _uiService.PrintWarning("🧠 Generating AI embeddings for smart search...");

            int processed = 0;
            foreach (var doc in documents)
            {
                processed++;

                // Enhanced progress display with percentage and progress bar
                var percentage = (processed * 100) / documents.Count;
                var progressBar =
                    new string('█', percentage / 5) + new string('░', 20 - (percentage / 5));

                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.Write($"\r   [{processed}/{documents.Count}] ");
                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write($"[{progressBar}] {percentage}% ");
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write(
                    $"Processing: {doc.Title.Substring(0, Math.Min(doc.Title.Length, 40))}..."
                );
                Console.ResetColor();

                var embedding = await _openAIService.GetEmbeddingAsync(doc.Content);

                knowledgeDocs.Add(
                    new KnowledgeDocument
                    {
                        Id = doc.Id,
                        Title = doc.Title,
                        Content = doc.Content,
                        ContentVector = embedding
                    }
                );
            }

            Console.WriteLine(); // New line after progress

            // Upload documents to search index
            _uiService.PrintWarning("📤 Uploading documents to Azure Cognitive Search...");
            await _searchService.UploadDocumentsAsync(knowledgeDocs);

            _uiService.PrintWarning("✅ Knowledge base setup complete!");
        }
    }
}
