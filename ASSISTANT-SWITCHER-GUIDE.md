# 🚀 Universal RAG Assistant - Quick Switch System

This folder contains batch scripts that allow you to instantly switch between different AI assistant topics and run them immediately.

## 📋 **Available Assistants**

### 🏠 **Real Estate Assistant** (Currently Active)

- **Focus**: Property prices, rental markets, investment opportunities
- **Coverage**: Brussels, Antwerp, Ghent, coastal areas, luxury markets
- **Use Cases**: "What are rental prices in Brussels Ixelles?", "Best investment areas in Belgium"

### 🛒 **Food Pricing Assistant**

- **Focus**: Grocery prices, store comparisons, seasonal deals
- **Coverage**: Fruits, vegetables, delicatessen, organic products, markets
- **Use Cases**: "Where can I find cheap apples?", "Compare supermarket prices"

### 💻 **Technology Assistant**

- **Focus**: Electronics prices, laptop deals, smartphone comparisons
- **Coverage**: MediaMarkt, Coolblue, Krëfel, warranties, gaming equipment
- **Use Cases**: "Cheapest MacBook Pro in Belgium?", "Best laptop deals"

### 🎵 **Entertainment Assistant**

- **Focus**: Concert tickets, streaming services, movie theaters
- **Coverage**: Festivals, gaming, sports events, comedy shows, nightlife
- **Use Cases**: "Rock Werchter ticket prices?", "Netflix vs Disney+ pricing"

---

## 🎯 **How to Use**

### **Option 1: Interactive Menu (Recommended)**

```bash
assistant-switcher.bat
```

Opens a user-friendly menu where you can:

- ✅ Choose any assistant
- ✅ View current configuration
- ✅ Restore from backup
- ✅ See assistant descriptions

### **Option 2: Direct Switch Scripts**

Switch and stay in current session:

```bash
switch-to-realestate.bat     # 🏠 Real Estate
switch-to-food.bat           # 🛒 Food Pricing
switch-to-technology.bat     # 💻 Technology
switch-to-entertainment.bat  # 🎵 Entertainment
```

### **Option 3: Quick Run Scripts (Power User)**

Switch and immediately start the assistant:

```bash
run-realestate.bat          # 🏠 Switch + Run Real Estate
run-food.bat                # 🛒 Switch + Run Food Pricing
run-technology.bat          # 💻 Switch + Run Technology
run-entertainment.bat       # 🎵 Switch + Run Entertainment
```

### **Option 4: Utility Scripts**

```bash
show-current.bat            # 📄 Show which assistant is active
restore-backup.bat          # 🔄 Restore previous configuration
```

---

## 🔧 **What Happens When You Switch**

1. **📁 Automatic Backup**: Your current `documents.json` is saved as `documents-current-backup.json`
2. **🔄 File Replacement**: The selected assistant's data file replaces `documents.json`
3. **🎨 UI Update**: The app title, icon, and welcome messages automatically change
4. **🧠 AI Retraining**: New embeddings are generated for the new topic
5. **🚀 Ready to Use**: Ask questions specific to the new domain

---

## 📂 **File Structure**

```
UniversalRAGAssistant/
├── Data/
│   ├── documents.json                      # 🎯 Active assistant data
│   ├── documents-current-backup.json       # 💾 Auto-backup of previous
│   ├── documents-realestate-example.json   # 🏠 Real Estate template
│   ├── documents-food-example.json         # 🛒 Food Pricing template
│   ├── documents-technology-example.json   # 💻 Technology template
│   └── documents-entertainment-example.json # 🎵 Entertainment template
├── assistant-switcher.bat                  # 🎯 Interactive menu
├── switch-to-*.bat                         # 🔄 Switch scripts
├── run-*.bat                              # 🚀 Quick run scripts
└── show-current.bat                       # 📄 Status checker
```

---

## 💡 **Pro Tips**

### **Fast Switching Workflow**

```bash
# Switch to food pricing and start immediately
run-food.bat

# In another terminal, check current config
show-current.bat

# Switch to technology without running
switch-to-technology.bat
```

### **Testing Multiple Assistants**

```bash
# Try different assistants quickly
run-realestate.bat       # Ask: "Rental prices in Brussels?"
# Ctrl+C to stop, then:
run-technology.bat       # Ask: "Cheapest laptop deals?"
# Ctrl+C to stop, then:
run-entertainment.bat    # Ask: "Concert ticket prices?"
```

### **Backup and Restore**

- ✅ Every switch automatically creates a backup
- ✅ Use `restore-backup.bat` to undo the last switch
- ✅ Original files are never modified (they're templates)

---

## 🎯 **Example Usage Scenarios**

### **Real Estate Research**

```bash
run-realestate.bat
# Ask: "What are rental prices in Brussels Ixelles?"
# Ask: "Best investment opportunities in coastal Belgium?"
# Ask: "Compare Brussels vs Antwerp property prices"
```

### **Grocery Shopping**

```bash
run-food.bat
# Ask: "Where can I find the cheapest organic vegetables?"
# Ask: "Compare Delhaize vs Lidl prices for weekly shopping"
# Ask: "Best farmers markets for seasonal produce?"
```

### **Tech Shopping**

```bash
run-technology.bat
# Ask: "Where can I find the cheapest MacBook Pro in Belgium?"
# Ask: "Compare gaming laptop prices across stores"
# Ask: "Best smartphone deals this month?"
```

### **Entertainment Planning**

```bash
run-entertainment.bat
# Ask: "What are Rock Werchter 2025 ticket prices?"
# Ask: "Compare streaming service costs in Belgium"
# Ask: "Best value cinema tickets in Brussels?"
```

---

## 🚨 **Troubleshooting**

### **If switching doesn't work:**

1. Make sure you're in the project root directory
2. Check that all `Data/*.json` files exist
3. Run `show-current.bat` to verify current state

### **If the app shows old data:**

1. Restart the app completely (`Ctrl+C` then `dotnet run`)
2. The app regenerates embeddings on startup with new data

### **To reset everything:**

```bash
restore-backup.bat              # Restore previous config
# OR copy any example file manually:
copy "Data\documents-realestate-example.json" "Data\documents.json"
```

---

## 🎉 **Happy Assistant Switching!**

You now have a powerful system to instantly transform your AI assistant into an expert on any topic. Each assistant has specialized knowledge and provides contextual responses for its domain.

**Start with**: `assistant-switcher.bat` for the full interactive experience! 🚀
