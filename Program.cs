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
        private static readonly HttpClient httpClient = new HttpClient();
        private static string endpoint;
        private static string apiKey;
        private static string chatDeployment;
        private static string embeddingDeployment;
        private static SearchClient searchClient;
        private static AppConfiguration appConfig;
        private static AppMetadata appMetadata;

        static async Task Main(string[] args)
        {
            // Load environment variables
            Env.Load();

            // Load application configuration
            await LoadApplicationConfiguration();

            LoadConfiguration();
            SetupSearchClient();

            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine("🚀 Initializing Universal RAG Assistant...");
            Console.ResetColor();

            // Setup demo data
            await SetupKnowledgeBase();

            // Interactive chat with RAG
            await InteractiveChatWithRAG();
        }

        static void LoadConfiguration()
        {
            endpoint =
                Environment.GetEnvironmentVariable("AOAI_ENDPOINT")
                ?? throw new InvalidOperationException("AOAI_ENDPOINT not set");

            apiKey =
                Environment.GetEnvironmentVariable("AOAI_APIKEY")
                ?? throw new InvalidOperationException("AOAI_APIKEY not set");

            chatDeployment =
                Environment.GetEnvironmentVariable("CHATCOMPLETION_DEPLOYMENTNAME")
                ?? throw new InvalidOperationException("CHATCOMPLETION_DEPLOYMENTNAME not set");

            embeddingDeployment =
                Environment.GetEnvironmentVariable("EMBEDDING_DEPLOYMENTNAME")
                ?? throw new InvalidOperationException("EMBEDDING_DEPLOYMENTNAME not set");

            // Setup HTTP client headers
            httpClient.DefaultRequestHeaders.Clear();
            httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");
        }

        static void SetupSearchClient()
        {
            string searchEndpoint =
                Environment.GetEnvironmentVariable("COGNITIVESEARCH_ENDPOINT")
                ?? throw new InvalidOperationException("COGNITIVESEARCH_ENDPOINT not set");

            string searchKey =
                Environment.GetEnvironmentVariable("COGNITIVESEARCH_APIKEY")
                ?? throw new InvalidOperationException("COGNITIVESEARCH_APIKEY not set");

            var searchCredential = new AzureKeyCredential(searchKey);
            searchClient = new SearchClient(
                new Uri(searchEndpoint),
                "knowledge-index",
                searchCredential
            );
        }

        static async Task SetupKnowledgeBase()
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("🔧 Setting up knowledge base...");
            Console.ResetColor();

            // Load documents from external source
            var documentLoader = new DocumentLoader();
            List<DocumentInfo> documents;

            try
            {
                switch (appConfig.DataSource.Type)
                {
                    case DataSourceType.Json:
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine($"📄 Loading data from JSON: {appConfig.DataSource.FilePath}");
                        Console.ResetColor();

                        // Load both documents and metadata
                        var dataConfig = await documentLoader.LoadDataConfigurationAsync(appConfig.DataSource.FilePath);
                        documents = dataConfig.Documents;
                        appMetadata = dataConfig.Metadata;
                        break;

                    case DataSourceType.Csv:
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine($"📊 Loading data from CSV: {appConfig.DataSource.FilePath}");
                        Console.ResetColor();
                        documents = await documentLoader.LoadDocumentsFromCsvAsync(appConfig.DataSource.FilePath);
                        appMetadata = new AppMetadata(); // Use default metadata for CSV
                        break;

                    case DataSourceType.TextFiles:
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine($"📁 Loading data from text files: {appConfig.DataSource.DirectoryPath}");
                        Console.ResetColor();
                        documents = await documentLoader.LoadDocumentsFromTextFilesAsync(appConfig.DataSource.DirectoryPath);
                        appMetadata = new AppMetadata(); // Use default metadata for text files
                        break;

                    default:
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("⚠️  Using hardcoded documents (fallback)");
                        Console.ResetColor();
                        documents = GetHardcodedDocuments();
                        appMetadata = new AppMetadata(); // Use default metadata for fallback
                        break;
                }

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"✅ Successfully loaded {documents.Count} documents");
                Console.ResetColor();
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"❌ Error loading documents: {ex.Message}");
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("🔄 Falling back to hardcoded documents");
                Console.ResetColor();
                documents = GetHardcodedDocuments();
            }

            // Create or update search index
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("🔍 Creating Azure Cognitive Search index...");
            Console.ResetColor();
            await CreateSearchIndex();

            // Generate embeddings and upload documents
            var knowledgeDocs = new List<KnowledgeDocument>();
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("🧠 Generating AI embeddings for smart search...");
            Console.ResetColor();

            int processed = 0;
            foreach (var doc in documents)
            {
                processed++;

                // Enhanced progress display with percentage and progress bar
                var percentage = (processed * 100) / documents.Count;
                var progressBar = new string('█', percentage / 5) + new string('░', 20 - (percentage / 5));

                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.Write($"\r   [{processed}/{documents.Count}] ");
                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write($"[{progressBar}] {percentage}% ");
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write($"Processing: {doc.Title.Substring(0, Math.Min(doc.Title.Length, 40))}...");
                Console.ResetColor();

                var embedding = await GetEmbedding(doc.Content);

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

            // Upload to search index
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("📤 Uploading to search index...");
            Console.ResetColor();
            await UploadDocuments(knowledgeDocs);

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("🎉 Knowledge base setup complete!");
            Console.ResetColor();
            Console.WriteLine();
        }

        static List<DocumentInfo> GetHardcodedDocuments()
        {
            return new List<DocumentInfo>
            {
                new DocumentInfo
                {
                    Id = "1",
                    Title = "Eiffel Tower",
                    Content = "The Eiffel Tower is a wrought-iron lattice tower on the Champ de Mars in Paris, France. It is named after the engineer Gustave Eiffel. The tower is 330 metres tall and was the world's tallest man-made structure until 1930."
                },
                new DocumentInfo
                {
                    Id = "2",
                    Title = "Louvre Museum",
                    Content = "The Louvre is the world's largest art museum and a historic monument in Paris, France. It is home to the Mona Lisa and Venus de Milo. The museum is housed in the Louvre Palace, originally built as a fortress in the late 12th century."
                },
                new DocumentInfo
                {
                    Id = "3",
                    Title = "Notre-Dame Cathedral",
                    Content = "Notre-Dame de Paris is a medieval Catholic cathedral on the Île de la Cité in the 4th arrondissement of Paris. The cathedral is considered to be one of the finest examples of French Gothic architecture."
                },
                new DocumentInfo
                {
                    Id = "4",
                    Title = "Champs-Élysées",
                    Content = "The Champs-Élysées is an avenue in the 8th arrondissement of Paris, France. It is 1.9 kilometres long and is the most famous street in Paris. It connects Place de la Concorde and Place Charles de Gaulle."
                }
            };
        }

        static async Task CreateSearchIndex()
        {
            var searchEndpoint = Environment.GetEnvironmentVariable("COGNITIVESEARCH_ENDPOINT");
            var searchKey = Environment.GetEnvironmentVariable("COGNITIVESEARCH_APIKEY");

            var indexClient = new SearchIndexClient(
                new Uri(searchEndpoint),
                new AzureKeyCredential(searchKey)
            );

            var definition = new SearchIndex("knowledge-index")
            {
                Fields =
                {
                    new SimpleField("Id", SearchFieldDataType.String) { IsKey = true },
                    new SearchableField("Title") { IsFilterable = true },
                    new SearchableField("Content") { IsFilterable = true },
                    new SearchField(
                        "ContentVector",
                        SearchFieldDataType.Collection(SearchFieldDataType.Single)
                    )
                    {
                        IsSearchable = true,
                        VectorSearchDimensions = 1536,
                        VectorSearchProfileName = "default"
                    }
                },
                VectorSearch = new()
                {
                    Profiles = { new VectorSearchProfile("default", "algorithm") },
                    Algorithms = { new HnswAlgorithmConfiguration("algorithm") }
                }
            };

            try
            {
                await indexClient.CreateOrUpdateIndexAsync(definition);
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("✅ Search index created/updated successfully");
                Console.ResetColor();
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"❌ Index creation error: {ex.Message}");
                Console.ResetColor();
            }
        }

        static async Task<float[]> GetEmbedding(string text)
        {
            string url =
                $"{endpoint.TrimEnd('/')}/openai/deployments/{embeddingDeployment}/embeddings?api-version=2023-05-15";

            var request = new EmbeddingRequest { input = new[] { text } };
            string jsonRequest = JsonSerializer.Serialize(request);
            var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

            var response = await httpClient.PostAsync(url, content);
            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Embedding API error: {response.StatusCode}");
            }

            string jsonResponse = await response.Content.ReadAsStringAsync();
            var embeddingResponse = JsonSerializer.Deserialize<EmbeddingResponse>(jsonResponse);

            return embeddingResponse.data[0].embedding;
        }

        static async Task UploadDocuments(List<KnowledgeDocument> documents)
        {
            try
            {
                var batch = IndexDocumentsBatch.Upload(documents);
                await searchClient.IndexDocumentsAsync(batch);
                Console.WriteLine("Documents uploaded to search index.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Upload error: {ex.Message}");
            }
        }

        static async Task InteractiveChatWithRAG()
        {
            // Set up console for better experience
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Console.CursorVisible = true;

            PrintStyledHeader();
            PrintWelcomeMessage();
            PrintInstructions();

            int conversationCount = 0;
            var conversationHistory = new List<(string question, string answer)>();
            var startTime = DateTime.Now;

            while (true)
            {
                PrintSeparator();
                PrintUserPrompt();

                string userQuestion = ReadLineWithStyle();

                if (string.IsNullOrEmpty(userQuestion))
                {
                    PrintWarning("Please enter a question or type 'quit' to exit.");
                    continue;
                }

                string normalizedInput = userQuestion.ToLower().Trim();

                if (normalizedInput == "quit" || normalizedInput == "exit" || normalizedInput == "bye")
                {
                    PrintSessionSummary(conversationCount, startTime);
                    PrintGoodbye();
                    break;
                }

                if (normalizedInput == "help" || normalizedInput == "?")
                {
                    PrintHelpMessage();
                    continue;
                }

                if (normalizedInput == "history" || normalizedInput == "hist")
                {
                    PrintConversationHistory(conversationHistory);
                    continue;
                }

                if (normalizedInput == "clear" || normalizedInput == "cls")
                {
                    PrintStyledHeader();
                    PrintInstructions();
                    continue;
                }

                if (normalizedInput == "data" || normalizedInput == "customize" || normalizedInput == "config")
                {
                    PrintDataSourceInfo();
                    continue;
                }

                if (normalizedInput == "stats")
                {
                    PrintSessionStats(conversationCount, startTime);
                    continue;
                }

                try
                {
                    var responseStartTime = DateTime.Now;
                    PrintProcessingMessage();

                    // Show animated loading while processing
                    var loadingTask = ShowLoadingAnimation("Analyzing your question");

                    // 1. Get embedding for user question
                    var questionEmbedding = await GetEmbedding(userQuestion);

                    // 2. Search for relevant documents
                    var searchResults = await SearchRelevantDocuments(
                        questionEmbedding,
                        userQuestion
                    );

                    // 3. Build context from search results
                    var contextParts = new List<string>();
                    await foreach (var result in searchResults.GetResultsAsync())
                    {
                        contextParts.Add($"Title: {result.Document.Title}\nContent: {result.Document.Content}");
                    }
                    string context = string.Join("\n\n", contextParts);

                    // Stop loading animation
                    loadingTask?.Cancel();
                    Console.WriteLine(); // New line after loading

                    // 4. Generate response with context
                    string response = await GenerateResponseWithContext(userQuestion, context);

                    conversationCount++;
                    conversationHistory.Add((userQuestion, response));

                    var responseTime = DateTime.Now - responseStartTime;
                    PrintAIResponse(response, conversationCount, responseTime);

                    if (conversationCount % 3 == 0)
                    {
                        PrintTip();
                    }

                    if (conversationCount % 5 == 0)
                    {
                        PrintSessionEncouragement(conversationCount);
                    }
                }
                catch (Exception ex)
                {
                    PrintError($"Sorry, I encountered an error: {ex.Message}");
                    PrintErrorAdvice();
                }
            }
        }

        static void PrintStyledHeader()
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine();
            Console.WriteLine($"                    {appMetadata?.Icon} {appMetadata?.Title} {appMetadata?.Flag}");
            Console.WriteLine("                     Powered by Azure OpenAI + RAG");
            Console.WriteLine();
            Console.ResetColor();

            // Add current time and welcome banner
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine($"                           🕒 Session started: {DateTime.Now:HH:mm:ss}");
            Console.ResetColor();
            Console.WriteLine();
        }

        static void PrintWelcomeMessage()
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"🎉 {appMetadata?.WelcomeMessage}");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"💡 {appMetadata?.CapabilityDescription}");
            Console.WriteLine($"📊 {appMetadata?.AdditionalInfo}");
            Console.ResetColor();
            Console.WriteLine();
        }

        static void PrintInstructions()
        {
            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.WriteLine("📝 AVAILABLE COMMANDS:");
            Console.WriteLine($"   • Ask any question related to {appMetadata?.CapabilityDescription?.ToLower()}");
            Console.WriteLine("   • Type 'help' or '?' for example questions");
            Console.WriteLine("   • Type 'history' or 'hist' to see conversation history");
            Console.WriteLine("   • Type 'stats' to see session statistics");
            Console.WriteLine("   • Type 'data' or 'customize' to see data customization info");
            Console.WriteLine("   • Type 'clear' or 'cls' to refresh the screen");
            Console.WriteLine("   • Type 'quit', 'exit', or 'bye' to end the session");
            Console.ResetColor();
            Console.WriteLine();
        }

        static void PrintUserPrompt()
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("🙋 ");
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.Write("You");
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write(": ");
            Console.ResetColor();
        }

        static string ReadLineWithStyle()
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            var input = Console.ReadLine();
            Console.ResetColor();
            return input ?? "";
        }

        static void PrintProcessingMessage()
        {
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.WriteLine("🔍 Searching through pricing database...");
            Console.ResetColor();
        }

        static CancellationTokenSource ShowLoadingAnimation(string message)
        {
            var cts = new CancellationTokenSource();

            Task.Run(async () =>
            {
                var frames = new[] { "⠋", "⠙", "⠹", "⠸", "⠼", "⠴", "⠦", "⠧", "⠇", "⠏" };
                int frame = 0;

                while (!cts.Token.IsCancellationRequested)
                {
                    Console.ForegroundColor = ConsoleColor.DarkYellow;
                    Console.Write($"\r{frames[frame % frames.Length]} {message}...");
                    Console.ResetColor();
                    frame++;

                    try
                    {
                        await Task.Delay(200, cts.Token);
                    }
                    catch (OperationCanceledException)
                    {
                        break;
                    }
                }

                // Clear the loading line
                Console.Write("\r" + new string(' ', message.Length + 10) + "\r");
            }, cts.Token);

            return cts;
        }

        static void PrintAIResponse(string response, int conversationCount)
        {
            PrintAIResponse(response, conversationCount, null);
        }

        static void PrintAIResponse(string response, int conversationCount, TimeSpan? responseTime)
        {
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.Write("🤖 ");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write("AI Assistant");
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write($" (Response #{conversationCount}");

            if (responseTime.HasValue)
            {
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.Write($" • {responseTime.Value.TotalSeconds:F1}s");
            }

            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("):");

            // Decorative line under AI header
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine("   " + new string('─', 60));
            Console.ResetColor();
            Console.WriteLine();

            // Format the response with better structure and text wrapping
            var lines = response.Split('\n');
            foreach (var line in lines)
            {
                var trimmedLine = line.Trim();
                if (string.IsNullOrEmpty(trimmedLine)) continue;

                if (trimmedLine.StartsWith("€") || trimmedLine.Contains("/kg") || trimmedLine.Contains("/L"))
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine($"   💰 {WrapText(trimmedLine, 75, "       ")}");
                }
                else if (trimmedLine.Contains("Best") || trimmedLine.Contains("Cheapest") || trimmedLine.Contains("savings"))
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"   ⭐ {WrapText(trimmedLine, 75, "       ")}");
                }
                else if (trimmedLine.Contains("Premium") || trimmedLine.Contains("expensive"))
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"   💎 {WrapText(trimmedLine, 75, "       ")}");
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.WriteLine($"   {WrapText(trimmedLine, 77, "   ")}");
                }
            }
            Console.ResetColor();
            Console.WriteLine();
        }

        static string WrapText(string text, int maxLength, string indent = "")
        {
            if (text.Length <= maxLength)
                return text;

            var words = text.Split(' ');
            var lines = new List<string>();
            var currentLine = "";

            foreach (var word in words)
            {
                if ((currentLine + " " + word).Length <= maxLength)
                {
                    currentLine += (currentLine.Length > 0 ? " " : "") + word;
                }
                else
                {
                    if (!string.IsNullOrEmpty(currentLine))
                        lines.Add(currentLine);
                    currentLine = word;
                }
            }

            if (!string.IsNullOrEmpty(currentLine))
                lines.Add(currentLine);

            return string.Join("\n" + indent, lines);
        }

        static void PrintSeparator()
        {
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine();
            var currentTime = DateTime.Now.ToString("HH:mm:ss");
            var separator = $"───────────────────────── {currentTime} ─────────────────────────";
            Console.WriteLine(separator);
            Console.ResetColor();
        }

        static void PrintHelpMessage()
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("\n💡 EXAMPLE QUESTIONS YOU CAN ASK:");
            Console.ResetColor();

            var examples = GetHelpExamples();

            foreach (var example in examples)
            {
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine($"   {example}");
            }
            Console.ResetColor();
            Console.WriteLine();
        }

        static string[] GetHelpExamples()
        {
            // Determine assistant type based on metadata
            var title = appMetadata?.Title?.ToUpper() ?? "";
            
            if (title.Contains("REAL ESTATE") || title.Contains("PROPERTY"))
            {
                return new[]
                {
                    "🏠 'What are rental prices in Brussels Ixelles?'",
                    "🏢 'Compare property investment yields in Antwerp vs Ghent'",
                    "💰 'Best areas for affordable housing in Belgium?'",
                    "🏘️ 'Student housing prices near KU Leuven?'",
                    "🏖️ 'Coastal property prices in Ostend vs Knokke'",
                    "📈 'ROI analysis for Liège property investments'",
                    "🏛️ 'Luxury real estate market in Brussels embassy quarter'",
                    "💡 'Best property investment strategy for Belgium'"
                };
            }
            else if (title.Contains("SUPERMARKET") || title.Contains("BUSINESS") || title.Contains("INTELLIGENCE"))
            {
                return new[]
                {
                    "📊 'What is Lidl vs Aldi competitive positioning strategy?'",
                    "💰 'Optimal procurement timing for seasonal produce'",
                    "🎯 'Customer segmentation analysis for Belgian markets'",
                    "📈 'ROI analysis for promotional campaigns'",
                    "🏪 'Store format optimization for different regions'",
                    "📦 'Inventory turnover rates by product category'",
                    "🔍 'Market expansion opportunities in Wallonia'",
                    "💼 'Private label margin optimization strategies'"
                };
            }
            else if (title.Contains("ENTERTAINMENT") || title.Contains("MUSIC") || title.Contains("CONCERT"))
            {
                return new[]
                {
                    "🎵 'What are Rock Werchter ticket prices this year?'",
                    "🎬 'Compare movie theater prices in Brussels'",
                    "📺 'Netflix vs Disney+ pricing in Belgium?'",
                    "� 'Best gaming console deals in Belgian stores?'",
                    "🎪 'Upcoming festival prices and lineups'",
                    "🎭 'Theater ticket prices in Antwerp vs Ghent'",
                    "🎤 'Comedy show tickets - where to find deals?'",
                    "🕺 'Nightlife and club entry prices in Brussels'"
                };
            }
            else if (title.Contains("TECHNOLOGY") || title.Contains("TECH") || title.Contains("ELECTRONICS"))
            {
                return new[]
                {
                    "💻 'Cheapest MacBook Pro prices in Belgium?'",
                    "📱 'Compare iPhone deals at MediaMarkt vs Coolblue'",
                    "🖥️ 'Best laptop deals for students?'",
                    "⌚ 'Smart watch prices and warranty options'",
                    "🎮 'Gaming PC components - where to buy cheapest?'",
                    "📷 'Camera equipment prices at Belgian tech stores'",
                    "🔌 'Best electronics warranty policies comparison'",
                    "💾 'SSD and storage device price trends'"
                };
            }
            else if (title.Contains("FOOD") || title.Contains("GROCERY") || title.Contains("PRICING"))
            {
                return new[]
                {
                    "�🍎 'Where can I find the cheapest apples in Belgium?'",
                    "🥕 'Compare carrot prices at Delhaize vs Lidl'",
                    "🧀 'Which store has the best cheese prices?'",
                    "🛒 'How can I save money on weekly grocery shopping?'",
                    "🌱 'What are organic vegetable prices in Brussels?'",
                    "📅 'What are seasonal price variations for strawberries?'",
                    "🏪 'Compare farmers market vs supermarket prices'",
                    "💰 'Best budget shopping strategy for families'"
                };
            }
            else
            {
                // Generic examples
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
        }

        static void PrintConversationHistory(List<(string question, string answer)> history)
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

        static void PrintTip()
        {
            var tips = new[]
            {
                "💡 Pro tip: Weekend farmers markets often have the best deals!",
                "💡 Pro tip: Aldi and Lidl typically offer the lowest prices for basic items.",
                "💡 Pro tip: Buy seasonal produce to save 20-40% on your grocery bill.",
                "💡 Pro tip: Compare organic prices - sometimes the premium is worth it!",
                "💡 Pro tip: Colruyt offers great quality-price balance for most products."
            };

            var random = new Random();
            var tip = tips[random.Next(tips.Length)];

            Console.ForegroundColor = ConsoleColor.DarkGreen;
            Console.WriteLine($"\n{tip}");
            Console.ResetColor();
        }

        static void PrintWarning(string message)
        {
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.WriteLine($"⚠️  {message}");
            Console.ResetColor();
        }

        static void PrintError(string message)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"❌ {message}");
            Console.ResetColor();
        }

        static void PrintGoodbye()
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"\n🙏 Thank you for using {appMetadata?.Title}!");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("💰 Happy shopping and save money on your groceries! 🛒");
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("👋 Goodbye!");
            Console.ResetColor();
        }

        static void PrintSessionSummary(int conversationCount, DateTime startTime)
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
            Console.WriteLine("✨ Thanks for exploring Belgian food prices with me!");
            Console.ResetColor();
        }

        static void PrintSessionStats(int conversationCount, DateTime startTime)
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
                var efficiency = conversationCount / Math.Max(sessionDuration.TotalMinutes, 1);
                Console.WriteLine($"🚀 Efficiency: {efficiency:F1} questions per minute");
            }

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Keep asking questions to discover more savings! 💰");
            Console.ResetColor();
            Console.WriteLine();
        }

        static void PrintSessionEncouragement(int conversationCount)
        {
            var encouragements = new[]
            {
                $"🎉 Great job! You've asked {conversationCount} questions - you're becoming a savings expert!",
                $"💪 {conversationCount} questions down! You're really committed to finding the best deals!",
                $"🌟 Impressive! {conversationCount} queries shows you're serious about smart shopping!",
                $"🏆 {conversationCount} questions explored! You're mastering Belgian food pricing!",
                $"🎯 Excellent progress! {conversationCount} questions brings you closer to maximum savings!"
            };

            var random = new Random();
            var encouragement = encouragements[random.Next(encouragements.Length)];

            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine($"\n{encouragement}");
            Console.ResetColor();
        }
        static void PrintErrorAdvice()
        {
            var advice = new[]
            {
                "💡 Try rephrasing your question or ask about specific products.",
                "🔄 Sometimes trying again helps - the AI learns from each interaction!",
                "📝 Be specific about stores, products, or price ranges for better results.",
                "🛠️  If problems persist, try asking simpler questions first."
            };

            var random = new Random();
            var selectedAdvice = advice[random.Next(advice.Length)];

            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.WriteLine($"{selectedAdvice}");
            Console.ResetColor();
        }

        static void PrintDataSourceInfo()
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("\n🗂️  DATA SOURCE & CUSTOMIZATION INFO:");
            Console.WriteLine("═══════════════════════════════════════");
            Console.ResetColor();

            // Current data source info
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"📄 Current Data Source: {appConfig.DataSource.Type}");

            if (appConfig.DataSource.Type == DataSourceType.Json)
            {
                Console.WriteLine($"📁 File: {appConfig.DataSource.FilePath}");
            }
            else if (appConfig.DataSource.Type == DataSourceType.Csv)
            {
                Console.WriteLine($"📁 File: {appConfig.DataSource.FilePath}");
            }
            else if (appConfig.DataSource.Type == DataSourceType.TextFiles)
            {
                Console.WriteLine($"📁 Directory: {appConfig.DataSource.DirectoryPath}");
            }

            Console.WriteLine($"🔍 Search Index: {appConfig.IndexName}");
            Console.ResetColor();
            Console.WriteLine();

            // Customization instructions
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("🛠️  HOW TO CUSTOMIZE THE APP:");
            Console.ResetColor();

            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("1. 📝 Edit Data/documents.json to change the knowledge base");
            Console.WriteLine("2. 🔄 Restart the app - it will automatically process your new data");
            Console.WriteLine("3. 🧠 New AI embeddings will be generated for your content");
            Console.WriteLine("4. 🤖 The AI will answer questions based on your new information");
            Console.WriteLine();

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("📚 CUSTOMIZATION IDEAS:");
            Console.ResetColor();
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine("   • 🏠 Real estate prices by neighborhood");
            Console.WriteLine("   • 🚗 Car prices from different dealers");
            Console.WriteLine("   • 🏨 Hotel rates across Belgian cities");
            Console.WriteLine("   • 💻 Electronics prices at tech stores");
            Console.WriteLine("   • 🎓 University course fees and costs");
            Console.WriteLine("   • 🍕 Restaurant prices and reviews");
            Console.ResetColor();
            Console.WriteLine();

            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine("📄 EXAMPLE DOCUMENT STRUCTURE:");
            Console.ResetColor();
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.WriteLine(@"{
  ""Id"": ""1"",
  ""Title"": ""Your Topic - Comparison Title"",
  ""Content"": ""Detailed information with specific prices, locations, 
             comparisons between options, and helpful recommendations.""
}");
            Console.ResetColor();
            Console.WriteLine();

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("💡 TIP: See DATA-CUSTOMIZATION-GUIDE.md for complete instructions!");
            Console.ResetColor();
            Console.WriteLine();
        }

        static async Task<SearchResults<KnowledgeDocument>> SearchRelevantDocuments(
            float[] queryEmbedding,
            string query
        )
        {
            var searchOptions = new SearchOptions
            {
                Size = 3,
                Select = { "Id", "Title", "Content" }
            };

            // Add vector search
            var vectorQuery = new VectorizedQuery(queryEmbedding.ToArray())
            {
                KNearestNeighborsCount = 3,
                Fields = { "ContentVector" }
            };
            searchOptions.VectorSearch = new() { Queries = { vectorQuery } };

            return await searchClient.SearchAsync<KnowledgeDocument>(query, searchOptions);
        }

        static async Task<string> GenerateResponseWithContext(string question, string context)
        {
            string url =
                $"{endpoint.TrimEnd('/')}/openai/deployments/{chatDeployment}/chat/completions?api-version=2024-02-01";

            var systemPrompt =
                $@"You are a helpful assistant answering questions about Belgian food prices, including fruits, vegetables, and delicatessen products. 
Use the following context to answer the user's question. If the context doesn't contain relevant information, say so.
Be specific about prices, seasonal variations, and provide helpful shopping advice for Belgian consumers.

Context:
{context}";

            var request = new ChatRequest
            {
                messages = new[]
                {
                    new ChatMessage { role = "system", content = systemPrompt },
                    new ChatMessage { role = "user", content = question }
                },
                max_tokens = 500,
                temperature = 0.7f
            };

            string jsonRequest = JsonSerializer.Serialize(request);
            var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

            var response = await httpClient.PostAsync(url, content);

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Chat API error: {response.StatusCode}");
            }

            string jsonResponse = await response.Content.ReadAsStringAsync();
            var chatResponse = JsonSerializer.Deserialize<ChatResponse>(jsonResponse);

            return chatResponse.choices[0].message.content;
        }

        static async Task LoadApplicationConfiguration()
        {
            string configPath = "appsettings.json";
            if (File.Exists(configPath))
            {
                string configJson = await File.ReadAllTextAsync(configPath);
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    Converters = { new JsonStringEnumConverter() }
                };
                appConfig = JsonSerializer.Deserialize<AppConfiguration>(configJson, options) ?? new AppConfiguration();
                Console.WriteLine($"Loaded configuration: Data source = {appConfig.DataSource.Type}");
            }
            else
            {
                appConfig = new AppConfiguration();
                Console.WriteLine("Using default configuration");
            }
        }
    }
}
