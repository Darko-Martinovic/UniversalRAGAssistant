@echo off
cls
echo.
echo ===============================================
echo    SWITCHING TO REAL ESTATE ASSISTANT
echo ===============================================
echo.

REM Backup current documents.json
if exist "Data\documents.json" (
    echo Backing up current configuration...
    copy "Data\documents.json" "Data\documents-current-backup.json" >nul
)

REM Switch to real estate
echo Activating Real Estate Assistant...
copy "Data\documents-realestate-example.json" "Data\documents.json" >nul

echo.
echo SUCCESS! Real Estate Assistant is now active
echo.
echo Current Assistant: BELGIAN REAL ESTATE ASSISTANT
echo Features: Property prices, rental markets, investment opportunities
echo Coverage: Brussels, Antwerp, Ghent, coastal areas, and more
echo.
echo Run the following command to start:
echo    dotnet run
echo.
pause
