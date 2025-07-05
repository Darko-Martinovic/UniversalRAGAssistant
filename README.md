# 🤖 Universal RAG Assistant - Azure OpenAI Platform

> **A Flexible AI Knowledge Platform That Adapts to ANY Topic by Simply Changing Data Files**

**Project Name**: `UniversalRAGAssistant`

This application demonstrates how to build a powerful Retrieval-Augmented Generation (RAG) system using Azure OpenAI and Azure Cognitive Search. **Currently configured as a Belgian Real Estate Assistant, but can be instantly transformed into ANY domain expert by replacing the data files.**

## 🌟 **Key Concept: Universal Customization**

**This is NOT just a real estate app** - it's a **universal AI assistant platform**. The current Belgian real estate theme is just one example. You can transform it into:

- 🛒 **Food Pricing Assistant** - Grocery stores, markets, product comparison
- 💻 **Technology Pricing Guide** - Electronics, gadgets, tech products
- 🎵 **Entertainment Guide** - Concerts, streaming, movies, festivals
- 🚗 **Automotive Advisor** - Car prices, dealer comparisons
- 🏨 **Travel Consultant** - Hotels, restaurants, tourist attractions
- 🎓 **Education Guide** - Course fees, university information
- 🛍️ **Shopping Expert** - ANY product category comparison
- 📚 **Knowledge Base** - Technical documentation, FAQ systems
- 🏢 **Business Intelligence** - Market research, industry insights

**Simply replace the content in `Data/documents.json` and restart the app!**

## 🚀 **Quick Topic Switch Demo**

### Current: Belgian Real Estate

```bash
# Ask: "What are rental prices in Brussels Ixelles?"
dotnet run
```

### Switch to Food Pricing in 30 seconds:

```bash
# 1. Backup current data
cp Data/documents.json Data/documents-realestate-backup.json

# 2. Switch to food pricing data
cp Data/documents.json.backup Data/documents.json

# 3. Restart app
dotnet run

# 4. Ask: "Where can I find cheap apples in Belgium?"
```

### Switch to Technology Pricing:

```bash
# Switch to technology data
cp Data/documents-technology-example.json Data/documents.json

# Restart and ask: "Where can I find the cheapest MacBook Pro?"
dotnet run
```

### Switch to Entertainment Guide:

```bash
# Switch to entertainment data
cp Data/documents-entertainment-example.json Data/documents.json

# Restart and ask: "What are ticket prices for Rock Werchter?"
dotnet run
```

**The AI instantly becomes an expert in your new domain!** ✨

## 📋 **Example Data Files Included**

### 1. **Real Estate** (Current) - `documents.json`

Property prices, rental markets, investment analysis across Belgian cities

### 2. **Food Pricing** - `documents.json.backup`

Belgian food prices, store comparisons, seasonal variations

### 3. **Technology** - `documents-technology-example.json`

Electronics pricing, store comparisons, warranty information

### 4. **Entertainment** - `documents-entertainment-example.json`

Concert tickets, streaming services, movie theaters, festivals

## 🎯 **How Universal Customization Works**

### The Magic Process:

1. **📝 Edit Content**: Replace `Data/documents.json` with your topic data
2. **🔄 Restart App**: `dotnet run`
3. **🧠 Auto-Processing**: App generates new AI embeddings
4. **🔍 Index Recreation**: Search index rebuilt for your content
5. **💬 Expert Mode**: AI becomes specialist in YOUR domain

### Document Structure (Universal):

```json
{
  "metadata": {
    "title": "YOUR ASSISTANT TITLE",
    "icon": "🏠",
    "flag": "🇧🇪",
    "welcomeMessage": "Welcome to your personal assistant!",
    "capabilityDescription": "I can help you with...",
    "additionalInfo": "Additional information..."
  },
  "documents": [
    {
      "Id": "1",
      "Title": "Your Topic - Comparison or Information Title",
      "Content": "Rich, detailed content with specific information, prices, comparisons, recommendations, and context that users will ask about."
    }
  ]
}
```

### What Stays the Same:

- ✅ **Beautiful Console Styling** - Colors, emojis, formatting (auto-adapts to your theme)
- ✅ **All Commands** - help, history, stats, data customization info
- ✅ **AI Intelligence** - Smart search, relevant responses
- ✅ **Session Features** - Progress tracking, tips, conversation history
- ✅ **Error Handling** - Robust fallbacks and user guidance

### What Adapts Automatically:

- 🎯 **AI Expertise** - Becomes expert in your topic
- 💡 **Content Highlighting** - Prices, deals, premium options
- 📊 **Search Results** - Finds relevant information from YOUR data
- 🎨 **Response Formatting** - Adapts to your content patterns

## 🛠️ **Advanced Features**

### **Interactive Commands**

- **Smart Conversations**: Natural language queries with context awareness
- **Session Statistics**: Track questions, response times, efficiency metrics
- **Conversation History**: Review previous questions and answers
- **Customization Info**: Built-in `data` command shows how to customize
- **Help System**: Dynamic examples that adapt to your content

### **Technical Excellence**

- **Vector Search**: Uses Azure Cognitive Search with semantic embeddings
- **Hybrid Search**: Combines keyword and vector similarity for best results
- **Real-time Processing**: Live AI embedding generation and indexing
- **Progress Indicators**: Beautiful loading animations and progress bars
- **Error Recovery**: Robust fallbacks and helpful error guidance
- **Multiple Data Sources**: JSON, CSV, text files, or hardcoded data

### **Content Intelligence**

- **Auto-Highlighting**: Prices (💰), deals (⭐), premium items (💎)
- **Smart Formatting**: Adapts to any content pattern
- **Text Wrapping**: Professional typography for console display
- **Response Timing**: Performance tracking and optimization

## 📚 **Complete Customization Resources**

### **Documentation Files**

- 📖 `DATA-CUSTOMIZATION-GUIDE.md` - Complete technical customization guide
- 🚀 `QUICK-CUSTOMIZATION.md` - Simple step-by-step instructions
- 🎨 `STYLING-ENHANCEMENTS.md` - Console UX feature documentation

### **Built-in Help**

- Type `data` or `customize` in the app for live customization guidance
- Examples and templates included for multiple domains
- Links to documentation and sample files

## Data Source Configuration

The application supports multiple data source types configured in `appsettings.json`:

### 1. JSON Files (Recommended)

```json
{
  "DataSource": {
    "Type": "Json",
    "FilePath": "Data/documents.json"
  },
  "IndexName": "your-topic-index"
}
```

**Perfect for**: Structured data, easy editing, version control

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
**Perfect for**: Spreadsheet users, bulk data import

### 3. Text Files

```json
{
  "DataSource": {
    "Type": "TextFiles",
    "DirectoryPath": "Data/TextFiles"
  }
}
```

**Perfect for**: Document collections, simple content management

### 4. Hardcoded (Fallback)

Automatic fallback when files are unavailable.

## 🚀 **Setup & Configuration**

### 1. **Environment Setup**

Copy the example environment file and configure your Azure services:

```bash
# Copy the example environment file
cp env.example .env

# Edit with your actual Azure service credentials
# (or set environment variables directly in your system)
```

> 📝 **Note**: See `env.example` for detailed configuration examples and descriptions.

**Required Environment Variables** (see `env.example` for details):

| Variable                        | Description                   | Example                                   |
| ------------------------------- | ----------------------------- | ----------------------------------------- |
| `AOAI_ENDPOINT`                 | Azure OpenAI service endpoint | `https://your-resource.openai.azure.com/` |
| `AOAI_APIKEY`                   | Azure OpenAI API key          | `your-api-key-here`                       |
| `CHATCOMPLETION_DEPLOYMENTNAME` | Chat model deployment         | `gpt-4`                                   |
| `EMBEDDING_DEPLOYMENTNAME`      | Embedding model deployment    | `text-embedding-ada-002`                  |
| `COGNITIVESEARCH_ENDPOINT`      | Azure Search endpoint         | `https://your-search.search.windows.net/` |
| `COGNITIVESEARCH_APIKEY`        | Azure Search API key          | `your-search-key-here`                    |

### 2. **Azure Services Setup**

**Azure OpenAI Service:**

- Create an Azure OpenAI resource in Azure Portal
- Deploy chat completion model (e.g., GPT-4)
- Deploy text embedding model (e.g., text-embedding-ada-002)
- Get endpoint URL and API key from "Keys and Endpoint" section

**Azure Cognitive Search:**

- Create an Azure Cognitive Search service
- Get endpoint URL and API key from "Keys" section
- The app will automatically create the search index

### 3. **Data Configuration**

Choose your data source in `appsettings.json`:

```json
{
  "DataSource": {
    "Type": "Json",
    "FilePath": "Data/documents.json"
  }
}
```

### 4. **Run the Application**

```bash
dotnet run
```

### 5. **Live Customization**

Use the `data` command in the app for real-time customization guidance.

## 🔄 **How It Works - Data Flow**

### **RAG Architecture Overview**

The application uses a **Retrieval-Augmented Generation (RAG)** pattern with Azure Cognitive Search for intelligent document retrieval:

```
📄 Documents → 🧠 AI Embeddings → 🔍 Search Index → 💬 User Query → 🎯 Similarity Search → 🤖 AI Response
```

### **Data Flow Process**

1. **📄 Document Processing**:

   - Load documents from JSON/CSV/text files
   - Generate AI embeddings (1536-dimensional vectors) for each document
   - Create `KnowledgeDocument` objects with text + vector data

2. **🔍 Index Creation**:

   - Create Azure Cognitive Search index with vector search capabilities
   - Upload documents using `IndexDocumentsBatch.Upload()`
   - Store both text fields and vector embeddings for hybrid search

3. **💬 Query Processing**:

   - Convert user questions to AI embeddings
   - Perform vector similarity search in Azure Cognitive Search
   - Retrieve most relevant documents based on semantic similarity

4. **🤖 Response Generation**:
   - Use retrieved context + user question to generate AI responses
   - Leverage Azure OpenAI for intelligent answer generation

### **Key Technical Features**

- **Vector Search**: 1536-dimensional embeddings for semantic understanding
- **Hybrid Search**: Combines keyword and vector similarity for best results
- **Auto-Indexing**: Automatic search index creation and document upload
- **Real-time Processing**: Live embedding generation and search operations
- **Scalable Architecture**: Works with 5 to thousands of documents

## 🎯 **Example Usage - Adapts to ANY Topic**

### Current Belgian Food Pricing Theme:

- _"What's the price of Belgian apples?"_
- _"Where can I find the best prices for vegetables in Brussels?"_
- _"Compare supermarket vs farmers market prices"_
- _"What are the seasonal price variations for fruits?"_

### Switch to Real Estate (30 seconds):

- _"What are rental prices in Brussels Ixelles?"_
- _"Compare property investment yields in different Belgian cities"_
- _"What's the cost of student housing in Ghent?"_
- _"Which areas offer the best real estate value?"_

### Switch to Technology Pricing:

- _"Where can I find the cheapest MacBook Pro in Belgium?"_
- _"Compare smartphone prices across different retailers"_
- _"What are the best deals on gaming equipment?"_
- _"Which store has the best warranty options?"_

**The AI expertise automatically adapts to whatever content you provide!**

## 💡 **Why This Approach is Revolutionary**

### **Traditional Approach**:

- Build separate apps for each domain
- Hard-code knowledge and logic
- Expensive development cycles
- Limited flexibility

### **This Universal Platform**:

- ✅ **One Codebase**: Handles unlimited domains
- ✅ **Zero Programming**: Content changes only
- ✅ **Instant Deployment**: Edit file, restart app
- ✅ **Professional Results**: Same quality regardless of topic
- ✅ **Cost Effective**: Reuse for multiple projects
- ✅ **Future Proof**: Easily update content as markets change

## 🏢 **Real-World Applications**

### **Business Use Cases**:

- **Local Businesses**: Product/service pricing guides
- **Consultants**: Industry-specific knowledge bases
- **Educators**: Course materials and FAQ systems
- **Real Estate**: Property information systems
- **Retail**: Product comparison tools
- **Healthcare**: Treatment options and costs
- **Legal**: Fee structures and service comparisons

### **Technical Teams**:

- **Documentation**: Product specs, troubleshooting guides
- **Support**: Customer service knowledge bases
- **Sales**: Competitive analysis tools
- **Marketing**: Market research platforms

## 📈 **Scalability & Performance**

- **Small Projects**: 5-20 documents work perfectly
- **Medium Projects**: Hundreds of documents supported
- **Large Projects**: Thousands of documents with Azure Search
- **Real-time Updates**: Change content anytime
- **Global Deployment**: Azure infrastructure worldwide
- **Cost Efficient**: Pay only for Azure services used

---

## 🚀 **Get Started in 5 Minutes**

1. **Clone & Setup**: Environment variables and Azure services
2. **Choose Topic**: Use included examples or create your own
3. **Run**: `dotnet run`
4. **Customize**: Edit documents.json for your domain
5. **Deploy**: Share with users or deploy to production

**Transform this app into YOUR domain expert today!** 🎯
