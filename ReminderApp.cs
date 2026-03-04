using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace SimpleReminder
{
    public class ReminderApp : ApplicationContext
    {
        private NotifyIcon? trayIcon;
        private System.Threading.Timer? repeatReminderTimer;
        private System.Windows.Forms.Timer? mainTimer;
        private ReminderConfig config;
        private int currentReminderIndex = 0;
        private string? lastReminderMessage;
        private int remainingSeconds;
        private bool isPaused = false;
        private bool wasIdle = false;

        public ReminderApp()
        {
            config = ReminderConfig.Load();
            InitializeTrayIcon();
            ResetReminderCountdown();
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

            // Setup main timer (ticks every second)
            mainTimer = new System.Windows.Forms.Timer();
            mainTimer.Interval = 1000; // 1 second
            mainTimer.Tick += OnMainTimerTick;
            mainTimer.Start();
        }

        private void ResetReminderCountdown()
        {
            remainingSeconds = config.ReminderIntervalMinutes * 60;
        }

        private void OnMainTimerTick(object? sender, EventArgs e)
        {
            if (isPaused)
            {
                UpdateTooltipDisplay("Simple Reminder - Paused");
                return;
            }

            // Check if user is idle
            bool isIdle = UserActivityMonitor.IsUserIdle(config.IdleThresholdMinutes);
            
            // Detect idle state transitions
            if (isIdle && !wasIdle)
            {
                // User just became idle - stop repeat reminders
                repeatReminderTimer?.Dispose();
                repeatReminderTimer = null;
            }
            else if (!isIdle && wasIdle)
            {
                // User just returned from idle - welcome back and reset countdown
                ShowNotification("Welcome Back!", "Let's get back to work!");
                ResetReminderCountdown();
            }
            
            wasIdle = isIdle;
            
            if (!isIdle)
            {
                // User is active, decrement countdown
                remainingSeconds--;

                if (remainingSeconds <= 0)
                {
                    // Time for a reminder!
                    TriggerReminder();
                }
            }
            
            // Update tooltip display (shows idle state if idle)
            UpdateTooltipDisplay(isIdle);
        }

        private void TriggerReminder()
        {
            // Get next reminder message
            string message = config.ReminderMessages[currentReminderIndex];
            currentReminderIndex = (currentReminderIndex + 1) % config.ReminderMessages.Length;

            // Show notification
            ShowNotification("Time for a Break!", message);

            // Store the message for potential repeat reminders
            lastReminderMessage = message;

            // Reset the countdown
            ResetReminderCountdown();

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

        private void UpdateTooltipDisplay(bool isIdle)
        {
            if (trayIcon == null) return;

            if (isIdle)
            {
                int minutes = remainingSeconds / 60;
                int seconds = remainingSeconds % 60;
                if (minutes >= 1)
                {
                    trayIcon.Text = $"Simple Reminder - Paused (idle) - {minutes}m {seconds}s";
                }
                else
                {
                    trayIcon.Text = $"Simple Reminder - Paused (idle) - {seconds}s";
                }
            }
            else if (remainingSeconds >= 60)
            {
                int minutes = remainingSeconds / 60;
                int seconds = remainingSeconds % 60;
                trayIcon.Text = $"Simple Reminder - Next in {minutes}m {seconds}s";
            }
            else
            {
                trayIcon.Text = $"Simple Reminder - Next in {remainingSeconds}s";
            }
        }

        private void UpdateTooltipDisplay(string text)
        {
            if (trayIcon != null)
            {
                trayIcon.Text = text;
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

            if (isPaused)
            {
                // Resume
                isPaused = false;
                menuItem.Text = "Pause";
            }
            else
            {
                // Pause
                isPaused = true;
                repeatReminderTimer?.Dispose();
                repeatReminderTimer = null;
                menuItem.Text = "Resume";
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
            repeatReminderTimer?.Dispose();
            mainTimer?.Dispose();
            trayIcon.Visible = false;
            Application.Exit();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                repeatReminderTimer?.Dispose();
                mainTimer?.Dispose();
                trayIcon?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
