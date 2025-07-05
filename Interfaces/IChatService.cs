using UniversalRAGAssistant.Models;

namespace UniversalRAGAssistant.Interfaces
{
    public interface IChatService
    {
        Task StartInteractiveChatAsync(AppMetadata metadata, AppConfiguration appConfig);
    }
}