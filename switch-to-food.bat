@echo off
cls
echo.
echo ===============================================
echo    🛒 SWITCHING TO FOOD PRICING ASSISTANT
echo ===============================================
echo.

REM Backup current documents.json
if exist "Data\documents.json" (
    echo 📁 Backing up current configuration...
    copy "Data\documents.json" "Data\documents-current-backup.json" >nul
)

REM Switch to food pricing
echo 🛒 Activating Food Pricing Assistant...
copy "Data\documents-food-example.json" "Data\documents.json" >nul

echo.
echo ✅ SUCCESS! Food Pricing Assistant is now active
echo.
echo 🎯 Current Assistant: BELGIAN FOOD PRICING ASSISTANT 🇧🇪
echo 💡 Features: Grocery prices, store comparisons, seasonal deals
echo 📊 Coverage: Fruits, vegetables, delicatessen, organic products
echo.
echo 🚀 Run the following command to start:
echo    dotnet run
echo.
pause
