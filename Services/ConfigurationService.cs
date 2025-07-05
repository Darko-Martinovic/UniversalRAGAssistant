using System.Text.Json;
using UniversalRAGAssistant.Models;

namespace UniversalRAGAssistant.Services
{
    public class ConfigurationService : IConfigurationService
    {
        public async Task<AppConfiguration> LoadConfigurationAsync()
        {
            try
            {
                if (!File.Exists("appsettings.json"))
                {
                    throw new FileNotFoundException(
                        "appsettings.json not found. Please ensure the configuration file exists."
                    );
                }

                var jsonContent = await File.ReadAllTextAsync("appsettings.json");

                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

                var appConfig = JsonSerializer.Deserialize<AppConfiguration>(jsonContent, options);

                if (appConfig == null)
                {
                    throw new InvalidOperationException("Failed to deserialize appsettings.json");
                }

                // Validate configuration
                if (appConfig.RagDocumentCount <= 0)
                {
                    throw new InvalidOperationException("RagDocumentCount must be greater than 0");
                }

                return appConfig;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(
                    $"Error loading configuration: {ex.Message}",
                    ex
                );
            }
        }

        public (
            string endpoint,
            string apiKey,
            string chatDeployment,
            string embeddingDeployment
        ) LoadAzureOpenAIConfiguration()
        {
            var endpoint =
                Environment.GetEnvironmentVariable("AOAI_ENDPOINT")
                ?? throw new InvalidOperationException("AOAI_ENDPOINT not set");

            var apiKey =
                Environment.GetEnvironmentVariable("AOAI_APIKEY")
                ?? throw new InvalidOperationException("AOAI_APIKEY not set");

            var chatDeployment =
                Environment.GetEnvironmentVariable("CHATCOMPLETION_DEPLOYMENTNAME")
                ?? throw new InvalidOperationException("CHATCOMPLETION_DEPLOYMENTNAME not set");

            var embeddingDeployment =
                Environment.GetEnvironmentVariable("EMBEDDING_DEPLOYMENTNAME")
                ?? throw new InvalidOperationException("EMBEDDING_DEPLOYMENTNAME not set");

            return (endpoint, apiKey, chatDeployment, embeddingDeployment);
        }

        public (string searchEndpoint, string searchKey) LoadAzureSearchConfiguration()
        {
            var searchEndpoint =
                Environment.GetEnvironmentVariable("COGNITIVESEARCH_ENDPOINT")
                ?? throw new InvalidOperationException("COGNITIVESEARCH_ENDPOINT not set");

            var searchKey =
                Environment.GetEnvironmentVariable("COGNITIVESEARCH_APIKEY")
                ?? throw new InvalidOperationException("COGNITIVESEARCH_APIKEY not set");

            return (searchEndpoint, searchKey);
        }
    }
}
