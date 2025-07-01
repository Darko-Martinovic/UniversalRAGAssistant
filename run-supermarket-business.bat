@echo off
call switch-to-supermarket-business.bat
if errorlevel 1 exit /b 1
echo.
echo ⚡ Ready! Starting Supermarket Business Intelligence...
dotnet run
