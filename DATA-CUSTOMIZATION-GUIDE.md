# 📚 Data Customization Guide - Belgian Food Pricing Assistant

## 🎯 Overview

The Belgian Food Pricing Assistant is designed to be **highly customizable**. You can easily modify the knowledge base to focus on different topics, regions, or data types by simply changing the data files. The app will automatically adapt to your new content!

## 📄 How Data Loading Works

### Current Setup

- **Primary Data Source**: `Data/documents.json`
- **Configuration**: `appsettings.json` controls which data source to use
- **Automatic Processing**: The app generates AI embeddings for any content you provide
- **Smart Search**: Azure Cognitive Search indexes your data for intelligent retrieval

### What Happens When You Change documents.json?

1. **🔄 Automatic Reload**: Next time you run the app, it will load your new content
2. **🧠 AI Embeddings**: New embeddings are generated for your updated content
3. **🔍 Search Index**: The search index is recreated with your new data
4. **💬 AI Responses**: The AI will answer questions based on your new content

## 🛠️ Customization Options

### 1. **Topic Customization**

You can change `documents.json` to cover ANY topic:

#### **Food & Restaurants** (Current Theme)

```json
{
  "Id": "1",
  "Title": "Restaurant Prices in Brussels",
  "Content": "Fine dining: €80-150 per person, Mid-range: €25-45, Budget: €8-20..."
}
```

#### **Real Estate Prices**

```json
{
  "Id": "1",
  "Title": "Brussels Apartment Rental Prices",
  "Content": "1-bedroom apartments: Ixelles €800-1200/month, Etterbeek €700-1000/month..."
}
```

#### **Technology Products**

```json
{
  "Id": "1",
  "Title": "Laptop Prices in Belgium Electronics Stores",
  "Content": "MacBook Pro M3: MediaMarkt €2299, Coolblue €2249, Krëfel €2399..."
}
```

#### **Travel & Tourism**

```json
{
  "Id": "1",
  "Title": "Belgian Hotel Prices by City",
  "Content": "Brussels hotels: Luxury €200-400/night, Business €80-150/night..."
}
```

### 2. **Data Source Flexibility**

Edit `appsettings.json` to use different data sources:

#### **JSON Files** (Recommended)

```json
{
  "DataSource": {
    "Type": "Json",
    "FilePath": "Data/your-custom-data.json"
  }
}
```

#### **CSV Files**

```json
{
  "DataSource": {
    "Type": "Csv",
    "FilePath": "Data/your-data.csv"
  }
}
```

#### **Text Files**

```json
{
  "DataSource": {
    "Type": "TextFiles",
    "DirectoryPath": "Data/YourTextFiles"
  }
}
```

### 3. **Document Structure**

Each document should follow this structure:

```json
{
  "Id": "unique-identifier",
  "Title": "Descriptive Title",
  "Content": "Rich, detailed content with specific information, prices, comparisons, etc."
}
```

## 🎨 Theme Customization Examples

### Example 1: Belgian Car Prices

```json
[
  {
    "Id": "1",
    "Title": "Used Car Prices Belgium - Compact Cars",
    "Content": "Volkswagen Golf (2019): AutoScout24 €18,000-22,000, CarPass €17,500-21,500, Dealer €20,000-25,000. Toyota Yaris (2020): Online €16,000-19,000, Dealer €18,000-22,000..."
  }
]
```

### Example 2: Belgian University Costs

```json
[
  {
    "Id": "1",
    "Title": "University Tuition Fees Belgium",
    "Content": "KU Leuven: EU students €938/year, Non-EU €4,175/year. UGent: EU €938/year, Non-EU €4,175/year. VUB: EU €938/year, Non-EU €4,175/year..."
  }
]
```

## 🚀 Quick Customization Steps

### Step 1: Backup Current Data

```bash
cp Data/documents.json Data/documents-backup.json
```

### Step 2: Edit Your Content

Open `Data/documents.json` and replace with your custom data

### Step 3: Update App Configuration (Optional)

Edit `appsettings.json` to change the index name:

```json
{
  "IndexName": "your-custom-topic-index"
}
```

### Step 4: Run the App

```bash
dotnet run
```

The app will automatically:

- ✅ Load your new content
- ✅ Generate AI embeddings
- ✅ Create a new search index
- ✅ Adapt the AI responses to your topic

## 💡 Best Practices

### Content Writing Tips

1. **Be Specific**: Include exact prices, locations, dates
2. **Use Comparisons**: "Store A vs Store B pricing"
3. **Add Context**: Seasonal variations, quality differences
4. **Include Keywords**: Terms users might search for

### Performance Optimization

- **Document Size**: Keep individual documents under 2000 characters
- **Document Count**: 5-20 documents work well for most topics
- **Content Quality**: Rich, detailed content improves AI responses

### Data Organization

- Use clear, descriptive titles
- Group related information in single documents
- Include cross-references between documents

## 🔧 Advanced Customization

### Custom Data Processing

You can extend the `DocumentLoader` class to handle:

- Different file formats (XML, YAML)
- Database connections
- API data sources
- Real-time data feeds

### AI Response Customization

Modify the system prompt in `GenerateResponseWithContext()` to change:

- Response tone and style
- Specific output format
- Domain-specific guidance

## 🎯 Real-World Use Cases

1. **Local Business Directory**: Restaurant/shop information
2. **Price Comparison Tool**: Any product category
3. **Educational Content**: Course information, study guides
4. **Travel Guide**: Tourist attractions, accommodation
5. **Technical Documentation**: Product specs, troubleshooting
6. **Market Research**: Industry insights, trends

## 📋 Template Documents

### Generic Price Comparison Template

```json
{
  "Id": "template-1",
  "Title": "[Product Category] Prices in [Location] - Store Comparison",
  "Content": "[Product A]: Store1 €X.XX, Store2 €Y.YY, Store3 €Z.ZZ. [Product B]: Store1 €A.AA, Store2 €B.BB. Best deals: [specific recommendations]."
}
```

The app's flexibility makes it perfect for ANY comparison or information lookup scenario!
