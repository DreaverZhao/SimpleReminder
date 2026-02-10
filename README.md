# Simple Reminder

A lightweight Windows system tray application that reminds you to take breaks, stand up, drink water, and more.

## Features

- 🪶 **Lightweight**: Runs in the system tray with minimal resource usage
- 💧 **Smart Reminders**: Periodic notifications for health breaks
- 🎯 **Idle Detection**: Automatically pauses when you're away from your computer
- ⚙️ **Configurable**: Customize reminder intervals and messages
- 🔔 **Windows Notifications**: Uses native Windows 10/11 toast notifications
- ⏱️ **Live Countdown**: Hover over the tray icon to see time remaining until next reminder

## Installation

### Prerequisites
- Windows 10 or later
- .NET 8.0 Runtime

### Build from Source

1. Install [.NET 8.0 SDK](https://dotnet.net/download)
2. Clone or download this repository
3. Open a terminal in the project directory
4. Run: `./build.bat` to build the application
5. Run: `./run.bat` to start the application

## Usage

1. Run `SimpleReminder.exe`
2. The app will appear in your system tray (notification area)
3. Hover over the tray icon to see the countdown to the next reminder
4. Right-click the tray icon for options:
   - **Pause/Resume**: Temporarily disable reminders
   - **Settings**: View current configuration
   - **Exit**: Close the application

## Configuration

The application reads its configuration from `config.json` located in the same directory as the executable.

After building, the configuration file is automatically copied to:
```
bin\Release\net8.0-windows\config.json
```

You can edit either the source `config.json` in the project root or the one in the output directory. If you edit the source file, remember to rebuild the application to copy the updated configuration.

You can edit this file to customize:

```json
{
  "ReminderIntervalMinutes": 30,
  "IdleThresholdMinutes": 5,
  "RepeatReminderIntervalMinutes": 5,
  "ReminderMessages": [
    "Time to stand up and stretch! 🧘",
    "Remember to drink some water! 💧",
    "Take a break and rest your eyes! 👀",
    "Walk around for a few minutes! 🚶",
    "Time for a quick stretch break! 🤸"
  ]
}
```

### Configuration Options

- **ReminderIntervalMinutes**: How often to show reminders (default: 30 minutes)
- **IdleThresholdMinutes**: How long to wait before considering the user idle (default: 5 minutes)
- **RepeatReminderIntervalMinutes**: How often to repeat reminders if the user ignores them (default: 5 minutes)
- **ReminderMessages**: Array of messages to rotate through

## How It Works

1. **Timer-Based Reminders**: Shows notifications at regular intervals
2. **Idle Detection**: Uses Windows API to detect user activity
   - If no mouse/keyboard input for the idle threshold, reminders are paused
   - Automatically resumes when you return
3. **Toast Notifications**: Uses Windows 10/11 native notification system

## Run on Startup (Optional)

To run the app automatically when Windows starts:

1. Press `Win + R`
2. Type `shell:startup` and press Enter
3. Create a shortcut to `SimpleReminder.exe` in this folder

## License

Free to use and modify.
