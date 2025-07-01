@echo off
cls
echo.
echo ===============================================
echo    📄 CURRENT ASSISTANT CONFIGURATION
echo ===============================================
echo.

if not exist "Data\documents.json" (
    echo ❌ No documents.json found!
    echo.
    pause
    exit /b
)

REM Check which assistant is currently active by looking at the title in metadata
findstr /c:"REAL ESTATE ASSISTANT" "Data\documents.json" >nul
if %errorlevel%==0 (
    echo 🎯 Current Assistant: 🏠 BELGIAN REAL ESTATE ASSISTANT 🇧🇪
    echo 💡 Focus: Property prices, rental markets, investment opportunities
    goto end
)

findstr /c:"FOOD PRICING ASSISTANT" "Data\documents.json" >nul
if %errorlevel%==0 (
    echo 🎯 Current Assistant: 🛒 BELGIAN FOOD PRICING ASSISTANT 🇧🇪
    echo 💡 Focus: Grocery prices, store comparisons, seasonal deals
    goto end
)

findstr /c:"TECH PRICING ASSISTANT" "Data\documents.json" >nul
if %errorlevel%==0 (
    echo 🎯 Current Assistant: 💻 BELGIAN TECH PRICING ASSISTANT 🇧🇪
    echo 💡 Focus: Electronics prices, laptop deals, smartphone comparisons
    goto end
)

findstr /c:"ENTERTAINMENT PRICING ASSISTANT" "Data\documents.json" >nul
if %errorlevel%==0 (
    echo 🎯 Current Assistant: 🎵 BELGIAN ENTERTAINMENT PRICING ASSISTANT 🇧🇪
    echo 💡 Focus: Concert tickets, streaming services, movie theaters
    goto end
)

echo 🎯 Current Assistant: 🤖 CUSTOM OR UNKNOWN CONFIGURATION
echo 💡 Focus: Custom data or unrecognized format

:end
echo.
echo 📁 File: Data\documents.json
echo 📊 To start the assistant: dotnet run
echo.

if exist "Data\documents-current-backup.json" (
    echo 💾 Backup available: documents-current-backup.json
) else (
    echo ⚠️  No backup file found
)

echo.
pause
