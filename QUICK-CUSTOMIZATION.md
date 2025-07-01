# 🔧 Quick Start: Customizing Your AI Assistant

## What You Can Do

Your Belgian Food Pricing Assistant is **incredibly flexible**! You can transform it into ANY comparison or information tool by simply changing the data files.

## 🚀 Instant Customization

### Step 1: Choose Your Topic
The app can become an expert in ANY field:
- 🏠 **Real Estate**: Property prices, rental markets
- 💻 **Technology**: Electronics pricing, product comparisons  
- 🚗 **Automotive**: Car prices, dealer comparisons
- 🏨 **Travel**: Hotel rates, restaurant prices
- 🎓 **Education**: Course fees, university costs
- 🛍️ **Shopping**: Any product category you want

### Step 2: Replace the Data
Simply edit `Data/documents.json` with your content:

**Current**: Belgian food pricing
```json
{
  "Id": "1",
  "Title": "Fresh Fruits Prices in Belgium - Store Comparison", 
  "Content": "Belgian apple prices by retailer: Delhaize €3.20/kg..."
}
```

**Transform to Real Estate**:
```json
{
  "Id": "1", 
  "Title": "Brussels Apartment Rental Prices by District",
  "Content": "Ixelles rental prices: 1-bedroom €800-1200/month..."
}
```

### Step 3: Run the App
```bash
dotnet run
```

**That's it!** The AI will automatically:
- ✅ Load your new content
- ✅ Generate smart embeddings  
- ✅ Create a searchable index
- ✅ Answer questions about YOUR topic

## 📋 Example Data Files Included

I've created sample files to demonstrate the flexibility:

1. **`documents-realestate-example.json`** - Belgian property market
2. **`documents-technology-example.json`** - Electronics pricing

To try them:
```bash
# Backup current data
cp Data/documents.json Data/documents-food-backup.json

# Try real estate data
cp Data/documents-realestate-example.json Data/documents.json

# Run the app
dotnet run
```

## 🎯 What Happens When You Change documents.json?

### The Magic Process:
1. **🔄 App Restart**: Next time you run the app
2. **📊 Data Loading**: Your new content is loaded automatically  
3. **🧠 AI Processing**: New embeddings generated for smart search
4. **🔍 Index Creation**: Search index rebuilt with your data
5. **💬 AI Adaptation**: Assistant becomes expert in YOUR topic

### Built-in Commands:
- Type `data` or `customize` to see current data source info
- Type `help` for example questions (adapts to your content)
- All styling and features work with ANY topic!

## 📈 Advanced Features

### Multiple Data Sources
Edit `appsettings.json` to use:
- **JSON files** (recommended): `"Type": "Json"`
- **CSV files**: `"Type": "Csv"` 
- **Text files**: `"Type": "TextFiles"`

### Smart Content Recognition
The AI automatically highlights:
- 💰 **Prices** (€/kg, €/month, €/hour) in yellow
- ⭐ **Best deals** and savings in green  
- 💎 **Premium options** in red
- **Any content pattern** you use consistently

## 🌟 Real-World Use Cases

This architecture is perfect for:
- **Local business directories**
- **Price comparison services** 
- **Educational content systems**
- **Technical documentation**
- **Market research tools**
- **Customer service knowledge bases**

## 💡 Pro Tips

1. **Rich Content**: Include specific prices, locations, dates
2. **Comparisons**: "Store A vs Store B" format works great
3. **Keywords**: Use terms your users will search for
4. **Document Size**: Keep under 2000 characters each
5. **Quantity**: 5-20 documents work best

Your AI assistant will instantly become an expert in whatever topic you provide! 🚀
