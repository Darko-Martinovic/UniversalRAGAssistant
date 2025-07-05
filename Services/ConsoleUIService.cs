using UniversalRAGAssistant.Models;
using UniversalRAGAssistant.Interfaces;

namespace UniversalRAGAssistant.Services
{
    public class ConsoleUIService : IConsoleUIService
    {
        private readonly Random _random = new Random();

        public void PrintStyledHeader()
        {
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine("╔══════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║                    UNIVERSAL RAG ASSISTANT                  ║");
            Console.WriteLine("║                    Powered by Azure OpenAI                  ║");
            Console.WriteLine("╚══════════════════════════════════════════════════════════════╝");
            Console.ResetColor();
        }

        public void PrintWelcomeMessage(AppMetadata metadata)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"\n{metadata.Icon} {metadata.Title}");
            if (!string.IsNullOrEmpty(metadata.Flag))
            {
                Console.WriteLine($"{metadata.Flag} {metadata.WelcomeMessage}");
            }
            Console.WriteLine($"\n{metadata.CapabilityDescription}");
            Console.WriteLine($"{metadata.AdditionalInfo}");
            Console.ResetColor();
        }

        public void PrintInstructions()
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("\n💡 INSTRUCTIONS:");
            Console.WriteLine("────────────────");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("• Type your question and press Enter");
            Console.WriteLine("• Type 'help' for example questions");
            Console.WriteLine("• Type 'history' to see recent conversations");
            Console.WriteLine("• Type 'stats' to see session statistics");
            Console.WriteLine("• Type 'quit' or 'exit' to end the session");
            Console.WriteLine("• Type 'info' to see data source information");
            Console.ResetColor();
        }

        public void PrintUserPrompt()
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write("\n🤔 You: ");
            Console.ResetColor();
        }

        public string ReadLineWithStyle()
        {
            return Console.ReadLine() ?? "";
        }

        public void PrintProcessingMessage()
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("🧠 Processing your question...");
            Console.ResetColor();
        }

        public CancellationTokenSource ShowLoadingAnimation(string message)
        {
            var cts = new CancellationTokenSource();
            var spinner = new[] { "⠋", "⠙", "⠹", "⠸", "⠼", "⠴", "⠦", "⠧", "⠇", "⠏" };
            var i = 0;

            Task.Run(
                async () =>
                {
                    while (!cts.Token.IsCancellationRequested)
                    {
                        Console.ForegroundColor = ConsoleColor.Cyan;
                        Console.Write($"\r{spinner[i % spinner.Length]} {message}");
                        Console.ResetColor();
                        await Task.Delay(100, cts.Token);
                        i++;
                    }
                },
                cts.Token
            );

            return cts;
        }

        public void PrintAIResponse(string response, int conversationCount)
        {
            PrintAIResponse(response, conversationCount, null);
        }

        public void PrintAIResponse(string response, int conversationCount, TimeSpan? responseTime)
        {
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.Write($"\n🤖 Assistant: ");
            Console.ResetColor();

            var wrappedResponse = WrapText(response, 80, "   ");
            Console.WriteLine(wrappedResponse);

            if (responseTime.HasValue)
            {
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.WriteLine($"   ⏱️  Response time: {responseTime.Value.TotalSeconds:F1}s");
                Console.ResetColor();
            }

            Console.WriteLine();
        }

        public void PrintSeparator()
        {
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine("────────────────────────────────────────────────────────────────");
            Console.ResetColor();
        }

        public void PrintHelpMessage(AppMetadata metadata)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("\n💡 EXAMPLE QUESTIONS:");
            Console.WriteLine("────────────────────");

            var examples =
                metadata.HelpExamples.Length > 0 ? metadata.HelpExamples : GetDefaultHelpExamples();

            foreach (var example in examples)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"• {example}");
            }

            Console.ResetColor();
            Console.WriteLine();
        }

        public void PrintConversationHistory(List<(string question, string answer)> history)
        {
            if (history.Count == 0)
            {
                Console.ForegroundColor = ConsoleColor.DarkYellow;
                Console.WriteLine("📝 No conversation history yet. Ask me a question!");
                Console.ResetColor();
                return;
            }

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"\n📚 CONVERSATION HISTORY ({history.Count} exchanges):");
            Console.ResetColor();

            for (int i = 0; i < Math.Min(history.Count, 5); i++) // Show last 5 exchanges
            {
                var (question, answer) = history[history.Count - 1 - i];
                Console.ForegroundColor = ConsoleColor.Blue;
                Console.WriteLine($"\n{history.Count - i}. Q: {question}");
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"   A: {answer.Substring(0, Math.Min(answer.Length, 100))}...");
            }
            Console.ResetColor();
            Console.WriteLine();
        }

        public void PrintTip(AppMetadata metadata)
        {
            var tips = metadata.Tips.Length > 0 ? metadata.Tips : GetDefaultTips();

            var tip = tips[_random.Next(tips.Length)];

            Console.ForegroundColor = ConsoleColor.DarkGreen;
            Console.WriteLine($"\n{tip}");
            Console.ResetColor();
        }

        public void PrintWarning(string message)
        {
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.WriteLine($"⚠️  {message}");
            Console.ResetColor();
        }

        public void PrintError(string message)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"❌ {message}");
            Console.ResetColor();
        }

        public void PrintGoodbye(AppMetadata metadata)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"\n🙏 Thank you for using {metadata.Title}!");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("💰 Happy shopping and save money on your groceries! 🛒");
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("👋 Goodbye!");
            Console.ResetColor();
        }

        public void PrintSessionSummary(
            int conversationCount,
            DateTime startTime,
            AppMetadata metadata
        )
        {
            var sessionDuration = DateTime.Now - startTime;

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("\n📊 SESSION SUMMARY:");
            Console.WriteLine("─────────────────────");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine($"💬 Questions asked: {conversationCount}");
            Console.WriteLine($"⏱️  Session duration: {sessionDuration:mm\\:ss}");

            if (conversationCount > 0)
            {
                var avgTime = sessionDuration.TotalSeconds / conversationCount;
                Console.WriteLine($"⚡ Average response time: ~{avgTime:F1}s per question");
            }

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"✨ Thanks for exploring {metadata.Title} with me!");
            Console.ResetColor();
        }

        public void PrintSessionStats(int conversationCount, DateTime startTime)
        {
            var sessionDuration = DateTime.Now - startTime;

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("\n📈 CURRENT SESSION STATISTICS:");
            Console.WriteLine("───────────────────────────────");
            Console.ResetColor();

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"🗣️  Questions asked: {conversationCount}");
            Console.WriteLine($"⌚ Session time: {sessionDuration:hh\\:mm\\:ss}");
            Console.WriteLine($"📅 Started at: {startTime:HH:mm:ss}");

            if (conversationCount > 0)
            {
                var avgTime = sessionDuration.TotalSeconds / conversationCount;
                Console.WriteLine($"⚡ Average response time: ~{avgTime:F1}s per question");
            }

            Console.ResetColor();
        }

        public void PrintSessionEncouragement(int conversationCount, AppMetadata metadata)
        {
            var encouragements =
                metadata.Encouragements.Length > 0
                    ? metadata.Encouragements
                    : GetDefaultEncouragements();

            var encouragement = encouragements[_random.Next(encouragements.Length)];

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"\n{encouragement}");
            Console.ResetColor();
        }

        public void PrintErrorAdvice(AppMetadata metadata)
        {
            var errorAdvice =
                metadata.ErrorAdvice.Length > 0 ? metadata.ErrorAdvice : GetDefaultErrorAdvice();

            var advice = errorAdvice[_random.Next(errorAdvice.Length)];

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"\n💡 {advice}");
            Console.ResetColor();
        }

        public void PrintDataSourceInfo(AppConfiguration appConfig)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("\n📊 DATA SOURCE INFORMATION:");
            Console.WriteLine("────────────────────────────");
            Console.ResetColor();

            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine($"📁 Data Source Type: {appConfig.DataSource.Type}");

            switch (appConfig.DataSource.Type)
            {
                case DataSourceType.Json:
                    Console.WriteLine($"📄 JSON File: {appConfig.DataSource.FilePath}");
                    break;
                case DataSourceType.Csv:
                    Console.WriteLine($"📊 CSV File: {appConfig.DataSource.FilePath}");
                    break;
                case DataSourceType.TextFiles:
                    Console.WriteLine($"📁 Directory: {appConfig.DataSource.DirectoryPath}");
                    break;
            }

            Console.WriteLine($"🔍 RAG Document Count: {appConfig.RagDocumentCount}");
            Console.WriteLine($"⚙️  Configuration File: appsettings.json");
            Console.ResetColor();
            Console.WriteLine();
        }

        private string WrapText(string text, int maxLength, string indent = "")
        {
            var words = text.Split(' ');
            var lines = new List<string>();
            var currentLine = indent;

            foreach (var word in words)
            {
                if ((currentLine + word).Length > maxLength)
                {
                    lines.Add(currentLine.TrimEnd());
                    currentLine = indent + word + " ";
                }
                else
                {
                    currentLine += word + " ";
                }
            }

            if (!string.IsNullOrWhiteSpace(currentLine))
            {
                lines.Add(currentLine.TrimEnd());
            }

            return string.Join("\n", lines);
        }

        private string[] GetDefaultHelpExamples()
        {
            return new[]
            {
                "💬 'Tell me about available options and pricing'",
                "📊 'Compare different products or services'",
                "💰 'What are the best deals and discounts?'",
                "🏪 'Where can I find the best prices?'",
                "📈 'What are current market trends?'",
                "🔍 'Help me find specific information'",
                "📋 'Give me a detailed comparison'",
                "💡 'What are your recommendations?'"
            };
        }

        private string[] GetDefaultTips()
        {
            return new[]
            {
                "💡 Pro tip: Be specific in your questions for better answers!",
                "💡 Pro tip: Use keywords related to your domain for more relevant results.",
                "💡 Pro tip: Ask follow-up questions to dive deeper into topics.",
                "💡 Pro tip: Use the 'help' command to see example questions.",
                "💡 Pro tip: Check the conversation history to review previous answers."
            };
        }

        private string[] GetDefaultEncouragements()
        {
            return new[]
            {
                "🎉 Great question! Keep exploring to discover more insights!",
                "🚀 You're doing great! Each question helps you learn more.",
                "💪 Excellent progress! You're making the most of this assistant.",
                "🌟 Wonderful! Your curiosity is leading to valuable discoveries.",
                "🎯 Perfect! You're asking the right questions to get the best information."
            };
        }

        private string[] GetDefaultErrorAdvice()
        {
            return new[]
            {
                "Try rephrasing your question with different keywords",
                "Check if your question is related to the available data domain",
                "Use the 'help' command to see example questions",
                "Make sure your question is clear and specific",
                "Try breaking down complex questions into simpler parts"
            };
        }
    }
}
