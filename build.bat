@echo off
echo Building Simple Reminder...
echo.

@REM Check if dotnet CLI is installed
dotnet --version >nul 2>&1
if %errorlevel% neq 0 (
    echo ERROR: .NET SDK is not installed or not added to PATH.
    echo Please install the .NET SDK from https://dotnet.microsoft.com/download
    pause
    exit /b 1
)

echo Restoring packages...
dotnet restore
if %errorlevel% neq 0 (
    echo Failed to restore packages
    pause
    exit /b 1
)

echo.
echo Building project...
dotnet build -c Release
if %errorlevel% neq 0 (
    echo Build failed
    pause
    exit /b 1
)

echo.
echo ========================================
echo Build successful!
echo ========================================
echo.
echo The application has been built to:
echo bin\Release\net8.0-windows\SimpleReminder.exe
echo.
echo You can now run the application by executing:
echo bin\Release\net8.0-windows\SimpleReminder.exe
echo.
pause
