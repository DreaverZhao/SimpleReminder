using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace SimpleReminder
{
    public class ReminderApp : ApplicationContext
    {
        private NotifyIcon? trayIcon;
        private System.Threading.Timer? reminderTimer;
        private System.Threading.Timer? repeatReminderTimer;
        private System.Windows.Forms.Timer? tooltipUpdateTimer;
        private ReminderConfig config;
        private int currentReminderIndex = 0;
        private string? lastReminderMessage;
        private DateTime nextReminderTime;

        public ReminderApp()
        {
            config = ReminderConfig.Load();
            InitializeTrayIcon();
            StartReminderTimer();
        }

        private void InitializeTrayIcon()
        {
            // Load the application icon
            Icon appIcon;
            try
            {
                // Get icon from the executable
                appIcon = Icon.ExtractAssociatedIcon(Application.ExecutablePath) ?? SystemIcons.Information;
            }
            catch
            {
                appIcon = SystemIcons.Information;
            }

            trayIcon = new NotifyIcon()
            {
                Icon = appIcon,
                Visible = true,
                Text = "Simple Reminder - Running"
            };

            var contextMenu = new ContextMenuStrip();
            contextMenu.Items.Add("Pause", null!, OnPauseResume);
            contextMenu.Items.Add("Settings", null!, OnSettings);
            contextMenu.Items.Add("-");
            contextMenu.Items.Add("Exit", null!, OnExit);

            trayIcon.ContextMenuStrip = contextMenu;
            trayIcon.DoubleClick += OnTrayIconDoubleClick;

            // Setup tooltip update timer (update every second)
            tooltipUpdateTimer = new System.Windows.Forms.Timer();
            tooltipUpdateTimer.Interval = 1000; // 1 second
            tooltipUpdateTimer.Tick += UpdateTooltip;
            tooltipUpdateTimer.Start();
        }

        private void StartReminderTimer()
        {
            // Start timer with configured interval (convert minutes to milliseconds)
            int intervalMs = config.ReminderIntervalMinutes * 60 * 1000;
            nextReminderTime = DateTime.Now.AddMilliseconds(intervalMs);
            reminderTimer = new System.Threading.Timer(
                OnTimerElapsed,
                null,
                intervalMs,
                intervalMs);
        }

        private void OnTimerElapsed(object? state)
        {
            // Update next reminder time
            int intervalMs = config.ReminderIntervalMinutes * 60 * 1000;
            nextReminderTime = DateTime.Now.AddMilliseconds(intervalMs);

            // Check if user is idle
            if (UserActivityMonitor.IsUserIdle(config.IdleThresholdMinutes))
            {
                return; // Skip reminder if user is idle
            }

            // Get next reminder message
            string message = config.ReminderMessages[currentReminderIndex];
            currentReminderIndex = (currentReminderIndex + 1) % config.ReminderMessages.Length;

            // Show notification
            ShowNotification("Time for a Break!", message);

            // Store the message for potential repeat reminders
            lastReminderMessage = message;

            // Start repeat reminder timer to check if user is still active
            StartRepeatReminderTimer();
        }

        private void StartRepeatReminderTimer()
        {
            // Stop any existing repeat timer
            repeatReminderTimer?.Dispose();

            // Start timer with configured repeat interval (convert minutes to milliseconds)
            int intervalMs = config.RepeatReminderIntervalMinutes * 60 * 1000;
            repeatReminderTimer = new System.Threading.Timer(
                OnRepeatReminderCheck,
                null,
                intervalMs,
                System.Threading.Timeout.Infinite); // One-shot timer
        }

        private void UpdateTooltip(object? sender, EventArgs e)
        {
            if (trayIcon == null) return;

            if (reminderTimer == null)
            {
                trayIcon.Text = "Simple Reminder - Paused";
                return;
            }

            TimeSpan remaining = nextReminderTime - DateTime.Now;
            
            if (remaining.TotalSeconds < 0)
            {
                trayIcon.Text = "Simple Reminder - Next reminder soon...";
            }
            else if (remaining.TotalMinutes >= 1)
            {
                int minutes = (int)remaining.TotalMinutes;
                int seconds = remaining.Seconds;
                trayIcon.Text = $"Simple Reminder - Next in {minutes}m {seconds}s";
            }
            else
            {
                int seconds = (int)remaining.TotalSeconds;
                trayIcon.Text = $"Simple Reminder - Next in {seconds}s";
            }
        }

        private void OnRepeatReminderCheck(object? state)
        {
            // Check if user is still active (not idle)
            if (!UserActivityMonitor.IsUserIdle(config.IdleThresholdMinutes))
            {
                // User is still active, show reminder again
                ShowNotification("Still Working?", lastReminderMessage ?? "Time for a break!");

                // Schedule another repeat check
                StartRepeatReminderTimer();
            }
            // If user is idle, stop the repeat cycle (timer is already one-shot)
        }

        private void ShowNotification(string title, string message)
        {
            // Use Windows balloon tip notifications (simple and compatible)
            trayIcon?.ShowBalloonTip(5000, title, message, ToolTipIcon.Info);
        }

        private void OnPauseResume(object? sender, EventArgs e)
        {
            var menuItem = sender as ToolStripMenuItem;
            if (menuItem == null) return;

            if (reminderTimer == null)
            {
                // Resume
                StartReminderTimer();
                menuItem.Text = "Pause";
            }
            else
            {
                // Pause
                reminderTimer?.Dispose();
                reminderTimer = null;
                repeatReminderTimer?.Dispose();
                repeatReminderTimer = null;
                menuItem.Text = "Resume";
                if (trayIcon != null)
                    trayIcon.Text = "Simple Reminder - Paused";
            }
        }

        private void OnSettings(object? sender, EventArgs e)
        {
            MessageBox.Show(
                $"Current Settings:\n\n" +
                $"Reminder Interval: {config.ReminderIntervalMinutes} minutes\n" +
                $"Idle Threshold: {config.IdleThresholdMinutes} minutes\n" +
                $"Repeat Reminder Interval: {config.RepeatReminderIntervalMinutes} minutes\n\n" +
                $"To change settings, edit the 'config.json' file.",
                "Settings",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }

        private void OnTrayIconDoubleClick(object? sender, EventArgs e)
        {
            ShowNotification("Simple Reminder", "Application is running in the system tray.");
        }

        private void OnExit(object? sender, EventArgs e)
        {
            reminderTimer?.Dispose();
            repeatReminderTimer?.Dispose();
            tooltipUpdateTimer?.Dispose();
            trayIcon.Visible = false;
            Application.Exit();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                reminderTimer?.Dispose();
                repeatReminderTimer?.Dispose();
                tooltipUpdateTimer?.Dispose();
                trayIcon?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
