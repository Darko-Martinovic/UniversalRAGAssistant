@echo off
cls
color 0A
echo.
echo ===============================================================================
echo                     UNIVERSAL RAG ASSISTANT SWITCHER                       
echo                           Choose Your AI Assistant                            
echo ===============================================================================
echo.
echo AVAILABLE ASSISTANTS:
echo.
echo    [1] Real Estate Assistant     - Property prices, rentals, investments
echo    [2] Food Pricing Assistant    - Grocery stores, markets, food deals  
echo    [3] Technology Assistant      - Electronics, laptops, tech gadgets
echo    [4] Entertainment Assistant   - Concerts, movies, streaming services
echo    [5] Supermarket Business      - Business intelligence for retailers
echo.
echo    [6] Show current configuration
echo    [7] Restore from backup
echo    [0] Exit
echo.
set /p choice=Enter your choice (0-7): 

if "%choice%"=="1" (
    call switch-to-realestate.bat
    goto menu
)
if "%choice%"=="2" (
    call switch-to-food.bat
    goto menu
)
if "%choice%"=="3" (
    call switch-to-technology.bat
    goto menu
)
if "%choice%"=="4" (
    call switch-to-entertainment.bat
    goto menu
)
if "%choice%"=="5" (
    call switch-to-supermarket-business.bat
    goto menu
)
if "%choice%"=="6" (
    call show-current.bat
    goto menu
)
if "%choice%"=="7" (
    call restore-backup.bat
    goto menu
)
if "%choice%"=="0" (
    echo.
    echo 👋 Goodbye! Thanks for using Universal RAG Assistant!
    echo.
    pause
    exit
)

echo.
echo ❌ Invalid choice. Please try again.
echo.
pause
:menu
assistant-switcher.bat
