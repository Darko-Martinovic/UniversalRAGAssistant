@echo off
cls
echo.
echo ===============================================
echo    RESTORE FROM BACKUP
echo ===============================================
echo.

if not exist "Data\documents-current-backup.json" (
    echo No backup file found!
    echo Backup files are created automatically when you switch assistants.
    echo.
    pause
    exit /b
)

echo Found backup: documents-current-backup.json
echo.
echo WARNING: This will overwrite your current configuration!
echo.
set /p confirm=Are you sure you want to restore? (Y/N): 

if /i "%confirm%"=="Y" (
    echo.
    echo Restoring from backup...
    copy "Data\documents-current-backup.json" "Data\documents.json" >nul
    echo.
    echo SUCCESS! Configuration restored from backup
    echo.
    echo Run the following command to start:
    echo    dotnet run
) else (
    echo.
    echo Restore cancelled.
)

echo.
pause
