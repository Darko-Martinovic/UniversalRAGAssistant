@echo off
cls
echo.
echo ===============================================
echo    SWITCHING TO ENTERTAINMENT ASSISTANT
echo ===============================================
echo.

REM Backup current documents.json
if exist "Data\documents.json" (
    echo Backing up current configuration...
    copy "Data\documents.json" "Data\documents-current-backup.json" >nul
)

REM Switch to entertainment
echo Activating Entertainment Assistant...
copy "Data\documents-entertainment-example.json" "Data\documents.json" >nul

echo.
echo SUCCESS! Entertainment Assistant is now active
echo.
echo Current Assistant: BELGIAN ENTERTAINMENT PRICING ASSISTANT
echo Features: Concert tickets, streaming services, movie theaters
echo Coverage: Festivals, gaming, sports, comedy shows, and more
echo.
echo Run the following command to start:
echo    dotnet run
echo.
pause
