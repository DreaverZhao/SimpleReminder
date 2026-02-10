@echo off
echo Running Simple Reminder...
echo.

if not exist "bin\Release\net8.0-windows\SimpleReminder.exe" (
    echo ERROR: Application not built yet!
    echo Please run build.bat first.
    echo.
    pause
    exit /b 1
)

start "" "bin\Release\net8.0-windows\SimpleReminder.exe"
echo Application started in system tray.
echo Look for the icon in your taskbar notification area.
