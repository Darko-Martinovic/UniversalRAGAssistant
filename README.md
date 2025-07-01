# 🤖 Universal RAG Assistant - Azure OpenAI Platform

> **A Flexible AI Knowledge Platform That Adapts to ANY Topic by Simply Changing Data Files**

**Project Name**: `UniversalRAGAssistant`

This application demonstrates how to build a powerful Retrieval-Augmented Generation (RAG) system using Azure OpenAI and Azure Cognitive Search. **Currently configured as a Belgian Food Pricing Assistant, but can be instantly transformed into ANY domain expert by replacing the data files.**

## 🌟 **Key Concept: Universal Customization**

**This is NOT just a food pricing app** - it's a **universal AI assistant platform**. The current Belgian food pricing theme is just one example. You can transform it into:

- 🏠 **Real Estate Assistant** - Property prices, rental markets
- 💻 **Technology Pricing Guide** - Electronics, gadgets, tech products
- 🚗 **Automotive Advisor** - Car prices, dealer comparisons
- 🏨 **Travel Consultant** - Hotels, restaurants, tourist attractions
- 🎓 **Education Guide** - Course fees, university information
- 🛍️ **Shopping Expert** - ANY product category comparison
- 📚 **Knowledge Base** - Technical documentation, FAQ systems
- 🏢 **Business Intelligence** - Market research, industry insights

**Simply replace the content in `Data/documents.json` and restart the app!**

## 🚀 **Quick Topic Switch Demo**

### Current: Belgian Food Pricing

```bash
# Ask: "Where can I find cheap apples in Belgium?"
dotnet run
```

### Switch to Real Estate in 30 seconds:

```bash
# 1. Backup current data
cp Data/documents.json Data/documents-food-backup.json

# 2. Switch to real estate data
cp Data/documents-realestate-example.json Data/documents.json

# 3. Restart app
dotnet run

# 4. Ask: "What are rental prices in Brussels Ixelles?"
```

### Switch to Technology Pricing:

```bash
# Switch to technology data
cp Data/documents-technology-example.json Data/documents.json

# Restart and ask: "Where can I find the cheapest MacBook Pro?"
dotnet run
```

**The AI instantly becomes an expert in your new domain!** ✨

## 📋 **Example Data Files Included**

### 1. **Food Pricing** (Default) - `documents.json`

Belgian food prices, store comparisons, seasonal variations

### 2. **Real Estate** - `documents-realestate-example.json`

Property prices, rental markets, investment analysis across Belgian cities

### 3. **Technology** - `documents-technology-example.json`

Electronics pricing, store comparisons, warranty information

## 🎯 **How Universal Customization Works**

### The Magic Process:

1. **📝 Edit Content**: Replace `Data/documents.json` with your topic data
2. **🔄 Restart App**: `dotnet run`
3. **🧠 Auto-Processing**: App generates new AI embeddings
4. **🔍 Index Recreation**: Search index rebuilt for your content
5. **💬 Expert Mode**: AI becomes specialist in YOUR domain

### Document Structure (Universal):

```json
[
  {
    "Id": "1",
    "Title": "Your Topic - Comparison or Information Title",
    "Content": "Rich, detailed content with specific information, prices, comparisons, recommendations, and context that users will ask about."
  }
]
```

### What Stays the Same:

- ✅ **Beautiful Console Styling** - Colors, emojis, formatting
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

## Setup

1. **Environment Variables**: Set up your Azure services:

   ```bash
   AOAI_ENDPOINT=your_azure_openai_endpoint
   AOAI_APIKEY=your_azure_openai_api_key
   CHATCOMPLETION_DEPLOYMENTNAME=your_chat_deployment_name
   EMBEDDING_DEPLOYMENTNAME=your_embedding_deployment_name
   COGNITIVESEARCH_ENDPOINT=your_search_endpoint
   COGNITIVESEARCH_APIKEY=your_search_api_key
   ```

2. **Configure Your Topic**: Choose your data source in `appsettings.json`

3. **Add Your Content**:

   - **Quick Start**: Use included example files
   - **Custom Content**: Edit `Data/documents.json` with your topic
   - **Multiple Files**: Use CSV or text file options

4. **Run the Application**:

   ```bash
   dotnet run
   ```

5. **Customize Live**: Use the `data` command for customization guidance

## How It Works - Universal RAG Architecture

### Universal RAG Process

1. **📄 Document Loading**: Load content from ANY domain (food, real estate, tech, etc.)
2. **🧠 Embedding Generation**: Convert YOUR content to AI vector embeddings
3. **🔍 Index Creation**: Store documents in Azure Cognitive Search for intelligent retrieval
4. **💬 Query Processing**: Convert user questions to vector embeddings
5. **🎯 Similarity Search**: Find most relevant content from YOUR knowledge base
6. **🤖 Response Generation**: AI generates expert answers using retrieved context

### Azure Search Integration

- **Semantic Search**: Understanding meaning, not just keywords
- **Vector Similarity**: Advanced AI-powered content matching
- **Hybrid Search**: Best of both keyword and semantic search
- **Auto-Configuration**: Handles embedding dimensions and search setup
- **Scalable**: Works with small datasets or thousands of documents

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
