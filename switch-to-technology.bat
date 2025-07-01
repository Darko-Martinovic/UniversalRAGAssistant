@echo off
cls
echo.
echo ===============================================
echo    💻 SWITCHING TO TECHNOLOGY ASSISTANT
echo ===============================================
echo.

REM Backup current documents.json
if exist "Data\documents.json" (
    echo 📁 Backing up current configuration...
    copy "Data\documents.json" "Data\documents-current-backup.json" >nul
)

REM Switch to technology
echo 💻 Activating Technology Assistant...
copy "Data\documents-technology-example.json" "Data\documents.json" >nul

echo.
echo ✅ SUCCESS! Technology Assistant is now active
echo.
echo 🎯 Current Assistant: BELGIAN TECH PRICING ASSISTANT 🇧🇪
echo 💡 Features: Electronics prices, laptop deals, smartphone comparisons
echo 📊 Coverage: MediaMarkt, Coolblue, Krëfel, warranties, and more
echo.
echo 🚀 Run the following command to start:
echo    dotnet run
echo.
pause
