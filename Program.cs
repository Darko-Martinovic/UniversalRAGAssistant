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
        private static int ragDocumentCount;

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
            List<DocumentInfo> documents = new List<DocumentInfo>();

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
                        Console.WriteLine("⚠️  Invalid data source type configured");
                        Console.ForegroundColor = ConsoleColor.White;
                        Console.WriteLine("Please check your appsettings.json configuration.");
                        Console.ResetColor();
                        Environment.Exit(1);
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
                Console.WriteLine("⚠️  Application is not properly configured");
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine("Please ensure your data source is properly configured in appsettings.json");
                Console.WriteLine("and the specified data files exist.");
                Console.ResetColor();
                Environment.Exit(1);
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
                    Title = "Competitive Pricing Intelligence - Market Position Analysis",
                    Content = "Market positioning by chain: Delhaize (premium, 15-20% above market average, focus on quality and convenience), Carrefour (mainstream, 5-10% above average, hypermarket strategy), Colruyt (value-quality balance, market average pricing, strong loyalty program), Lidl (discount leader, 20-25% below average, limited SKU strategy), Aldi (ultra-discount, 25-30% below average, private label focus). Pricing gaps analysis: Fresh produce shows biggest variance (up to 40% between premium and discount), packaged goods more standardized (10-15% variance). Strategic opportunity: Mid-market positioning between Colruyt and Carrefour shows 8-12% margin potential with proper execution."
                },
                new DocumentInfo
                {
                    Id = "2",
                    Title = "Procurement Strategy - Supplier Cost Analysis Belgium",
                    Content = "Wholesale pricing intelligence: Direct farm procurement saves 35-45% vs distributors (seasonal produce), Belgian suppliers offer 10-15% cost advantage over imports (dairy, vegetables), Volume discounts: 5% at €100K monthly, 12% at €500K, 18% at €1M+ monthly purchases. Seasonal procurement windows: Asparagus (April-June, lock prices in February), Strawberries (May-August, contract in March), Apples (September-February, secure storage capacity by July). Private label opportunities: 25-35% higher margins vs national brands, minimum €2M investment for dedicated production lines. Supply chain optimization: Regional distribution centers reduce costs by 8-12%, automated ordering systems cut procurement overhead by 15-20%."
                },
                new DocumentInfo
                {
                    Id = "3",
                    Title = "Regional Market Analysis - Store Performance Optimization",
                    Content = "Brussels market dynamics: Higher operating costs (rent +40%, labor +25%), premium positioning essential, target demographics favor convenience and quality over price. Average basket: €45-65, frequency 2.1x/week. Antwerp opportunities: Port logistics advantage (-15% distribution costs), mixed demographics allow dual positioning, growing expatriate community values international brands. Average basket: €35-50, frequency 2.4x/week. Ghent student market: Price-sensitive segment, bulk buying patterns, promotional responsiveness +180% vs average. Average basket: €25-35, frequency 3.2x/week. Optimal store formats: Brussels (convenience, 800-1200m²), Antwerp (hypermarket, 2000-3500m²), Ghent (compact super, 1000-1500m²)."
                },
                new DocumentInfo
                {
                    Id = "4",
                    Title = "Customer Analytics - Shopping Behavior Intelligence",
                    Content = "Shopping pattern analysis: Peak hours (Thursday 17-19h, Friday 16-20h, Saturday 9-12h, Sunday 15-18h), Basket composition: Planned purchases 65%, impulse purchases 35% (opportunity for strategic placement). Customer segmentation: Price-sensitive (35%, focus on promotions and private label), Quality-focused (25%, premium positioning and fresh categories), Convenience-oriented (40%, ready meals and extended hours). Loyalty program effectiveness: Colruyt Xtra (85% participation, +22% basket value), Carrefour stamps (60% participation, +15% frequency), Delhaize Plus (+18% retention, premium tier activation). Cross-selling opportunities: Fresh produce → prepared foods (conversion rate 28%), Dairy → bread/bakery (45% correlation), Wine → cheese/delicatessen (65% premium basket uplift)."
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
                Size = ragDocumentCount,
                Select = { "Id", "Title", "Content" }
            };

            // Add vector search
            var vectorQuery = new VectorizedQuery(queryEmbedding.ToArray())
            {
                KNearestNeighborsCount = ragDocumentCount,
                Fields = { "ContentVector" }
            };
            searchOptions.VectorSearch = new() { Queries = { vectorQuery } };

            return await searchClient.SearchAsync<KnowledgeDocument>(query, searchOptions);
        }

        static async Task<string> GenerateResponseWithContext(string question, string context)
        {
            string url =
                $"{endpoint.TrimEnd('/')}/openai/deployments/{chatDeployment}/chat/completions?api-version=2024-02-01";

            // Use dynamic system prompt from metadata, with fallback to default
            var systemPrompt = !string.IsNullOrEmpty(appMetadata?.SystemPrompt)
                ? $@"{appMetadata.SystemPrompt}

Context:
{context}"
                : $@"You are a helpful assistant answering questions based on the provided context. 
Use the following context to answer the user's question. If the context doesn't contain relevant information, say so.

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

                // Set RAG document count from configuration with validation
                ragDocumentCount = appConfig.RagDocumentCount;
                if (ragDocumentCount < 1 || ragDocumentCount > 10)
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine($"⚠️  Invalid RagDocumentCount value '{ragDocumentCount}' in appsettings.json. Using default value of 3.");
                    Console.ResetColor();
                    ragDocumentCount = 3;
                }
                else
                {
                    Console.WriteLine($"RAG Document Count: {ragDocumentCount}");
                }
            }
            else
            {
                appConfig = new AppConfiguration();
                ragDocumentCount = 3; // Default value
                Console.WriteLine("Using default configuration");
            }
        }
    }
}
