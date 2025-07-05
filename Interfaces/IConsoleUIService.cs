using UniversalRAGAssistant.Models;

namespace UniversalRAGAssistant.Interfaces
{
    public interface IConsoleUIService
    {
        void PrintStyledHeader();
        void PrintWelcomeMessage(AppMetadata metadata);
        void PrintInstructions();
        void PrintUserPrompt();
        string ReadLineWithStyle();
        void PrintProcessingMessage();
        CancellationTokenSource ShowLoadingAnimation(string message);
        void PrintAIResponse(string response, int conversationCount);
        void PrintAIResponse(string response, int conversationCount, TimeSpan? responseTime);
        void PrintSeparator();
        void PrintHelpMessage(AppMetadata metadata);
        void PrintConversationHistory(List<(string question, string answer)> history);
        void PrintTip(AppMetadata metadata);
        void PrintWarning(string message);
        void PrintError(string message);
        void PrintGoodbye(AppMetadata metadata);
        void PrintSessionSummary(int conversationCount, DateTime startTime, AppMetadata metadata);
        void PrintSessionStats(int conversationCount, DateTime startTime);
        void PrintSessionEncouragement(int conversationCount, AppMetadata metadata);
        void PrintErrorAdvice(AppMetadata metadata);
        void PrintDataSourceInfo(AppConfiguration appConfig);
    }
}