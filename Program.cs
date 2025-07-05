using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using DotNetEnv;
using Azure;
using Azure.Search.Documents;
using Azure.Search.Documents.Indexes;
using Azure.Search.Documents.Indexes.Models;
using Azure.Search.Documents.Models;
using UniversalRAGAssistant.Models;
using UniversalRAGAssistant.Services;

namespace UniversalRAGAssistant
{
    class Program
    {
        static async Task Main(string[] args)
        {
            try
            {
                // Load environment variables
                Env.Load();

                // Initialize services
                var services = InitializeServices();

                // Load configuration
                var appConfig = await services.ConfigurationService.LoadConfigurationAsync();

                Console.ForegroundColor = ConsoleColor.Magenta;
                Console.WriteLine("🚀 Initializing Universal RAG Assistant...");
                Console.ResetColor();

                // Setup knowledge base
                var (documents, metadata) = await services.KnowledgeBaseService.LoadDocumentsAsync(appConfig);
                await services.KnowledgeBaseService.SetupKnowledgeBaseAsync(documents);

                // Start interactive chat
                await services.ChatService.StartInteractiveChatAsync(metadata, appConfig);
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"❌ Application error: {ex.Message}");
                Console.ResetColor();
                Environment.Exit(1);
            }
        }

        private static ServiceContainer InitializeServices()
        {
            // Create HTTP client
            var httpClient = new HttpClient();

            // Load Azure OpenAI configuration
            var (endpoint, apiKey, chatDeployment, embeddingDeployment) =
                new ConfigurationService().LoadAzureOpenAIConfiguration();

            // Load Azure Search configuration
            var (searchEndpoint, searchKey) =
                new ConfigurationService().LoadAzureSearchConfiguration();

            // Initialize services
            var configurationService = new ConfigurationService();
            var openAIService = new AzureOpenAIService(httpClient, endpoint, apiKey, chatDeployment, embeddingDeployment);
            var searchService = new AzureSearchService(searchEndpoint, searchKey);
            var ragService = new RagService(openAIService, searchService);
            var uiService = new ConsoleUIService();
            var knowledgeBaseService = new KnowledgeBaseService(openAIService, searchService, uiService);
            var chatService = new ChatService(ragService, uiService);

            return new ServiceContainer
            {
                ConfigurationService = configurationService,
                AzureOpenAIService = openAIService,
                AzureSearchService = searchService,
                RagService = ragService,
                ConsoleUIService = uiService,
                KnowledgeBaseService = knowledgeBaseService,
                ChatService = chatService
            };
        }
    }

    // Simple service container for dependency injection
    public class ServiceContainer
    {
        public IConfigurationService ConfigurationService { get; set; } = null!;
        public IAzureOpenAIService AzureOpenAIService { get; set; } = null!;
        public IAzureSearchService AzureSearchService { get; set; } = null!;
        public IRagService RagService { get; set; } = null!;
        public IConsoleUIService ConsoleUIService { get; set; } = null!;
        public IKnowledgeBaseService KnowledgeBaseService { get; set; } = null!;
        public IChatService ChatService { get; set; } = null!;
    }
}
