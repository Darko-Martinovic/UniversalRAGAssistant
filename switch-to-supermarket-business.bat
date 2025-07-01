@echo off
color 0A
echo.
echo ===============================================
echo    📊 SWITCHING TO SUPERMARKET BUSINESS INTELLIGENCE
echo ===============================================

echo 📄 Backing up current configuration...
if exist "Data\documents.json" (
    copy "Data\documents.json" "Data\documents.json.backup" >nul 2>&1
)

echo 📊 Activating Supermarket Business Intelligence...
copy "Data\documents-supermarket-business.json" "Data\documents.json" >nul 2>&1

if errorlevel 1 (
    echo ❌ ERROR: Could not activate Supermarket Business Intelligence
    echo 📁 Make sure documents-supermarket-business.json exists in Data folder
    pause
    exit /b 1
)

echo ✅ SUCCESS! Supermarket Business Intelligence is now active
echo.
echo 🎯 Current Assistant: BELGIAN SUPERMARKET BUSINESS INTELLIGENCE 🇧🇪  
echo 💼 Features: Competitive analysis, procurement optimization, market insights
echo 📈 Coverage: Pricing strategy, inventory management, customer analytics
echo 🔧 Run the following command to start:
echo    dotnet run
echo.
pause
