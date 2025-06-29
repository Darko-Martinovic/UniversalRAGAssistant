using System.Text;
using System.Text.Json;
using DotNetEnv;
using Azure;
using Azure.Search.Documents;
using Azure.Search.Documents.Indexes;
using Azure.Search.Documents.Indexes.Models;
using Azure.Search.Documents.Models;
using AzureOpenAIConsole.Models;

namespace AzureOpenAIConsole
{
    class Program
    {
        private static readonly HttpClient httpClient = new HttpClient();
        private static string endpoint;
        private static string apiKey;
        private static string chatDeployment;
        private static string embeddingDeployment;
        private static SearchClient searchClient;

        static async Task Main(string[] args)
        {
            // Load environment variables
            Env.Load();

            LoadConfiguration();
            SetupSearchClient();

            Console.WriteLine("=== Azure OpenAI + Search RAG Demo ===\n");

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
            Console.WriteLine("Setting up knowledge base...");

            // Sample documents about Paris
            var documents = new[]
            {
                new
                {
                    Id = "1",
                    Title = "Eiffel Tower",
                    Content = "The Eiffel Tower is a wrought-iron lattice tower on the Champ de Mars in Paris, France. It is named after the engineer Gustave Eiffel. The tower is 330 metres tall and was the world's tallest man-made structure until 1930."
                },
                new
                {
                    Id = "2",
                    Title = "Louvre Museum",
                    Content = "The Louvre is the world's largest art museum and a historic monument in Paris, France. It is home to the Mona Lisa and Venus de Milo. The museum is housed in the Louvre Palace, originally built as a fortress in the late 12th century."
                },
                new
                {
                    Id = "3",
                    Title = "Notre-Dame Cathedral",
                    Content = "Notre-Dame de Paris is a medieval Catholic cathedral on the Île de la Cité in the 4th arrondissement of Paris. The cathedral is considered to be one of the finest examples of French Gothic architecture."
                },
                new
                {
                    Id = "4",
                    Title = "Champs-Élysées",
                    Content = "The Champs-Élysées is an avenue in the 8th arrondissement of Paris, France. It is 1.9 kilometres long and is the most famous street in Paris. It connects Place de la Concorde and Place Charles de Gaulle."
                }
            };

            // Create or update search index
            await CreateSearchIndex();

            // Generate embeddings and upload documents
            var knowledgeDocs = new List<KnowledgeDocument>();

            foreach (var doc in documents)
            {
                Console.WriteLine($"Processing: {doc.Title}");
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

            // Upload to search index
            await UploadDocuments(knowledgeDocs);
            Console.WriteLine("Knowledge base setup complete!\n");
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
                Console.WriteLine("Search index created/updated successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Index creation error: {ex.Message}");
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
            Console.WriteLine("Ask me anything about Paris attractions! (type 'quit' to exit)\n");

            while (true)
            {
                Console.Write("You: ");
                string userQuestion = Console.ReadLine();

                if (string.IsNullOrEmpty(userQuestion) || userQuestion.ToLower() == "quit")
                    break;

                try
                {
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

                    // 4. Generate response with context
                    string response = await GenerateResponseWithContext(userQuestion, context);

                    Console.WriteLine($"\nAI: {response}\n");
                    Console.WriteLine("---");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                }
            }
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
                $@"You are a helpful assistant answering questions about Paris attractions. 
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
    }
}
