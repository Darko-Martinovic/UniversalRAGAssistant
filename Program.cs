using DotNetEnv;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using UniversalRAGAssistant.Services;
using UniversalRAGAssistant.Interfaces;

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

                // Build service provider
                var serviceProvider = ConfigureServices();

                Console.ForegroundColor = ConsoleColor.Magenta;
                Console.WriteLine("🚀 Initializing Universal RAG Assistant...");
                Console.ResetColor();

                // Get services from DI container
                var configurationService = serviceProvider.GetRequiredService<IConfigurationService>();
                var knowledgeBaseService = serviceProvider.GetRequiredService<IKnowledgeBaseService>();
                var chatService = serviceProvider.GetRequiredService<IChatService>();

                // Load configuration
                var appConfig = await configurationService.LoadConfigurationAsync();

                // Setup knowledge base
                var (documents, metadata) = await knowledgeBaseService.LoadDocumentsAsync(appConfig);
                await knowledgeBaseService.SetupKnowledgeBaseAsync(documents);

                // Start interactive chat
                await chatService.StartInteractiveChatAsync(metadata, appConfig);
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"❌ Application error: {ex.Message}");
                Console.ResetColor();
                Environment.Exit(1);
            }
        }

        private static ServiceProvider ConfigureServices()
        {
            var services = new ServiceCollection();

            // Register HttpClient
            services.AddSingleton<HttpClient>();

            // Register configuration services
            services.AddSingleton<IConfigurationService, ConfigurationService>();

            // Register Azure services with factory patterns for configuration
            services.AddSingleton<IAzureOpenAIService>(serviceProvider =>
            {
                var httpClient = serviceProvider.GetRequiredService<HttpClient>();
                var configService = serviceProvider.GetRequiredService<IConfigurationService>();
                var (endpoint, apiKey, chatDeployment, embeddingDeployment) =
                    configService.LoadAzureOpenAIConfiguration();

                return new AzureOpenAIService(httpClient, endpoint, apiKey, chatDeployment, embeddingDeployment);
            });

            services.AddSingleton<IAzureSearchService>(serviceProvider =>
            {
                var configService = serviceProvider.GetRequiredService<IConfigurationService>();
                var (searchEndpoint, searchKey) = configService.LoadAzureSearchConfiguration();

                return new AzureSearchService(searchEndpoint, searchKey);
            });

            // Register relevance validation service
            services.AddTransient<IRelevanceValidationService, RelevanceValidationService>();

            // Register application services
            services.AddTransient<IRagService, RagService>();
            services.AddSingleton<IConsoleUIService, ConsoleUIService>();
            services.AddTransient<IKnowledgeBaseService, KnowledgeBaseService>();
            services.AddTransient<IChatService, ChatService>();

            return services.BuildServiceProvider();
        }
    }
}
