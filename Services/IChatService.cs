using UniversalRAGAssistant.Models;

namespace UniversalRAGAssistant.Services
{
    public interface IChatService
    {
        Task StartInteractiveChatAsync(AppMetadata metadata, AppConfiguration appConfig);
    }
}