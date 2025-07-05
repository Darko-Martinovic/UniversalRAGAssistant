using Azure;
using Azure.Search.Documents;
using Azure.Search.Documents.Indexes;
using Azure.Search.Documents.Indexes.Models;
using Azure.Search.Documents.Models;
using UniversalRAGAssistant.Models;
using UniversalRAGAssistant.Interfaces;

namespace UniversalRAGAssistant.Services
{
    public class AzureSearchService : IAzureSearchService
    {
        private readonly SearchClient _searchClient;
        private readonly SearchIndexClient _searchIndexClient;

        public AzureSearchService(string searchEndpoint, string searchKey)
        {
            var searchCredential = new AzureKeyCredential(searchKey);
            _searchClient = new SearchClient(
                new Uri(searchEndpoint),
                "knowledge-index",
                searchCredential
            );
            _searchIndexClient = new SearchIndexClient(new Uri(searchEndpoint), searchCredential);
        }

        public async Task CreateSearchIndexAsync()
        {
            var index = new SearchIndex("knowledge-index")
            {
                Fields =
                {
                    new SearchField("id", SearchFieldDataType.String) { IsKey = true },
                    new SearchField("title", SearchFieldDataType.String)
                    {
                        IsSearchable = true,
                        IsFilterable = true
                    },
                    new SearchField("content", SearchFieldDataType.String) { IsSearchable = true },
                    new SearchField(
                        "contentVector",
                        SearchFieldDataType.Collection(SearchFieldDataType.Single)
                    )
                    {
                        IsSearchable = true,
                        VectorSearchDimensions = 1536,
                        VectorSearchProfileName = "my-vector-config"
                    }
                },
                VectorSearch = new VectorSearch
                {
                    Profiles =
                    {
                        new VectorSearchProfile("my-vector-config", "my-vector-algorithm")
                    },
                    Algorithms =
                    {
                        new HnswAlgorithmConfiguration("my-vector-algorithm")
                        {
                            Parameters = new HnswParameters
                            {
                                M = 4,
                                EfConstruction = 400,
                                EfSearch = 500,
                                Metric = VectorSearchAlgorithmMetric.Cosine
                            }
                        }
                    }
                }
            };

            await _searchIndexClient.CreateOrUpdateIndexAsync(index);
        }

        public async Task UploadDocumentsAsync(List<KnowledgeDocument> documents)
        {
            var batch = new List<IndexDocumentsAction<KnowledgeDocument>>();
            foreach (var doc in documents)
            {
                batch.Add(IndexDocumentsAction.Upload(doc));
            }

            var batchOperations = new IndexDocumentsBatch<KnowledgeDocument>();
            foreach (var action in batch)
            {
                batchOperations.Actions.Add(action);
            }
            await _searchClient.IndexDocumentsAsync(batchOperations);
        }

        public async Task<SearchResults<KnowledgeDocument>> SearchRelevantDocumentsAsync(
            float[] queryEmbedding,
            string query,
            int documentCount
        )
        {
            // For better relevance scores, let's try pure text search first
            // This should give us scores similar to Azure portal (10+ range)
            var searchOptions = new SearchOptions
            {
                // Pure text search for high relevance scores
                SearchFields = { "title", "content" },
                Select = { "id", "title", "content" },
                Size = documentCount,
                QueryType = SearchQueryType.Simple,
                IncludeTotalCount = true,
                // Use BM25 scoring algorithm (same as Azure portal)
                ScoringProfile = null,
                // Add search highlights for debugging
                HighlightFields = { "title", "content" }
            };

            return await _searchClient.SearchAsync<KnowledgeDocument>(query, searchOptions);
        }
    }
}
