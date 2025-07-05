using UniversalRAGAssistant.Models;

namespace UniversalRAGAssistant.Services
{
    public interface IConfigurationService
    {
        Task<AppConfiguration> LoadConfigurationAsync();
        (string endpoint, string apiKey, string chatDeployment, string embeddingDeployment) LoadAzureOpenAIConfiguration();
        (string searchEndpoint, string searchKey) LoadAzureSearchConfiguration();
    }
} 