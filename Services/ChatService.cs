using UniversalRAGAssistant.Models;
using UniversalRAGAssistant.Interfaces;

namespace UniversalRAGAssistant.Services
{
    public class ChatService : IChatService
    {
        private readonly IRagService _ragService;
        private readonly IConsoleUIService _uiService;

        public ChatService(IRagService ragService, IConsoleUIService uiService)
        {
            _ragService = ragService;
            _uiService = uiService;
        }

        public async Task StartInteractiveChatAsync(
            AppMetadata metadata,
            AppConfiguration appConfig
        )
        {
            var conversationHistory = new List<(string question, string answer)>();
            var startTime = DateTime.Now;
            var conversationCount = 0;

            _uiService.PrintStyledHeader();
            _uiService.PrintWelcomeMessage(metadata);
            _uiService.PrintInstructions();

            while (true)
            {
                _uiService.PrintUserPrompt();
                var userInput = _uiService.ReadLineWithStyle().Trim();

                if (string.IsNullOrEmpty(userInput))
                {
                    continue;
                }

                var lowerInput = userInput.ToLower();

                // Handle special commands
                if (lowerInput == "quit" || lowerInput == "exit")
                {
                    _uiService.PrintSessionSummary(conversationCount, startTime, metadata);
                    _uiService.PrintGoodbye(metadata);
                    break;
                }
                else if (lowerInput == "help")
                {
                    _uiService.PrintHelpMessage(metadata);
                    continue;
                }
                else if (lowerInput == "history")
                {
                    _uiService.PrintConversationHistory(conversationHistory);
                    continue;
                }
                else if (lowerInput == "stats")
                {
                    _uiService.PrintSessionStats(conversationCount, startTime);
                    continue;
                }
                else if (lowerInput == "info")
                {
                    _uiService.PrintDataSourceInfo(appConfig);
                    continue;
                }

                // Process the question
                conversationCount++;
                var questionStartTime = DateTime.Now;

                try
                {
                    _uiService.PrintProcessingMessage();
                    var loadingCts = _uiService.ShowLoadingAnimation(
                        "Searching knowledge base and generating response..."
                    );

                    var response = await _ragService.ProcessQueryAsync(
                        userInput,
                        metadata.SystemPrompt,
                        appConfig.RagDocumentCount
                    );

                    loadingCts.Cancel();
                    var responseTime = DateTime.Now - questionStartTime;

                    _uiService.PrintAIResponse(response, conversationCount, responseTime);
                    conversationHistory.Add((userInput, response));

                    // Show tip every 3 questions
                    if (conversationCount % 3 == 0)
                    {
                        _uiService.PrintTip(metadata);
                    }

                    // Show encouragement every 5 questions
                    if (conversationCount % 5 == 0)
                    {
                        _uiService.PrintSessionEncouragement(conversationCount, metadata);
                    }
                }
                catch (Exception ex)
                {
                    _uiService.PrintError($"Error processing your question: {ex.Message}");
                    _uiService.PrintErrorAdvice(metadata);
                }

                _uiService.PrintSeparator();
            }
        }
    }
}
