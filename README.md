# Belgian Food Pricing Assistant - Azure OpenAI RAG Console Application

This application demonstrates how to build a Retrieval-Augmented Generation (RAG) system using Azure OpenAI and Azure Cognitive Search, specifically focused on Belgian food pricing information including fruits, vegetables, and delicatessen products.

## Features

- **Belgian Food Price Database**: Comprehensive pricing information for Belgian fruits, vegetables, and deli products
- **Flexible Data Sources**: Load pricing data from JSON files, CSV files, text files, or hardcoded data
- **Vector Search**: Uses Azure Cognitive Search with vector embeddings for semantic search of pricing information
- **Interactive Chat**: Ask questions about Belgian food prices and get AI responses based on the knowledge base
- **Shopping Advice**: Get recommendations on where to find best prices and seasonal variations
- **Configurable**: Easy configuration through `appsettings.json`

## Data Source Configuration

The application supports multiple data source types configured in `appsettings.json`:

### 1. JSON Files (Default)

```json
{
  "DataSource": {
    "Type": "Json",
    "FilePath": "Data/documents.json"
  }
}
```

The JSON file contains Belgian food pricing information:

```json
[
  {
    "Id": "1",
    "Title": "Document Title",
    "Content": "Document content here..."
  }
]
```

### 2. CSV Files

```json
{
  "DataSource": {
    "Type": "Csv",
    "FilePath": "Data/documents.csv"
  }
}
```

CSV format: `Id,Title,Content`

### 3. Text Files

```json
{
  "DataSource": {
    "Type": "TextFiles",
    "DirectoryPath": "Data/TextFiles"
  }
}
```

Each `.txt` file in the directory becomes a document. The filename becomes the title.

### 4. Hardcoded (Fallback)

```json
{
  "DataSource": {
    "Type": "HardCoded"
  }
}
```

Uses predefined documents in the code.

## Setup

1. Set up your environment variables:

   ```
   AOAI_ENDPOINT=your_azure_openai_endpoint
   AOAI_APIKEY=your_azure_openai_api_key
   CHATCOMPLETION_DEPLOYMENTNAME=your_chat_deployment_name
   EMBEDDING_DEPLOYMENTNAME=your_embedding_deployment_name
   COGNITIVESEARCH_ENDPOINT=your_search_endpoint
   COGNITIVESEARCH_APIKEY=your_search_api_key
   ```

2. Configure your data source in `appsettings.json`

3. Add your documents to the appropriate location based on your configuration

4. Run the application: `dotnet run`

## How It Works

### RAG Process

1. **Document Loading**: Documents are loaded from your configured data source
2. **Embedding Generation**: Each document is converted to vector embeddings using Azure OpenAI
3. **Index Creation**: Documents and embeddings are stored in Azure Cognitive Search
4. **Query Processing**: User questions are converted to embeddings
5. **Similarity Search**: Relevant documents are found using vector similarity
6. **Response Generation**: Azure OpenAI generates responses using the retrieved context

### Azure Search Integration

- Creates a search index with vector fields for semantic search
- Supports hybrid search (keyword + vector)
- Automatically handles embedding dimensions and vector search configuration

## Benefits of External File Loading

- **Easy Updates**: Modify your knowledge base without changing code
- **Scalability**: Support large document collections
- **Multiple Formats**: Choose the format that works best for your data
- **Version Control**: Track changes to your knowledge base
- **Team Collaboration**: Different team members can contribute documents

## Example Usage

After running the application, you can ask questions like:

- "What's the price of Belgian apples?"
- "Where can I find the best prices for vegetables in Brussels?"
- "How much does Belgian cheese cost?"
- "What are the seasonal price variations for fruits?"
- "Compare supermarket vs farmers market prices"
- "What's the cost of organic vegetables in Belgium?"
- "Which Belgian delicatessen products offer the best value?"

The AI will respond based on the documents in your configured data source.
