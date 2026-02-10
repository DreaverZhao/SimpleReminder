using System;
using System.IO;
using System.Text.Json;

namespace SimpleReminder
{
    public class ReminderConfig
    {
        public int ReminderIntervalMinutes { get; set; } = 30;
        public int IdleThresholdMinutes { get; set; } = 5;
        public int RepeatReminderIntervalMinutes { get; set; } = 5;
        public string[] ReminderMessages { get; set; } = new[]
        {
            "Time to stand up and stretch! 🧘",
            "Remember to drink some water! 💧",
            "Take a break and rest your eyes! 👀",
            "Walk around for a few minutes! 🚶",
            "Time for a quick stretch break! 🤸"
        };

        private static string ConfigPath => Path.Combine(
            AppContext.BaseDirectory,
            "config.json");

        public static ReminderConfig Load()
        {
            try
            {
                if (File.Exists(ConfigPath))
                {
                    string json = File.ReadAllText(ConfigPath);
                    var config = JsonSerializer.Deserialize<ReminderConfig>(json);
                    return config ?? new ReminderConfig();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading config: {ex.Message}");
            }

            // Create default config
            var defaultConfig = new ReminderConfig();
            defaultConfig.Save();
            return defaultConfig;
        }

        public void Save()
        {
            try
            {
                string directory = Path.GetDirectoryName(ConfigPath) ?? "";
                Directory.CreateDirectory(directory);

                var options = new JsonSerializerOptions
                {
                    WriteIndented = true
                };
                string json = JsonSerializer.Serialize(this, options);
                File.WriteAllText(ConfigPath, json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving config: {ex.Message}");
            }
        }
    }
}
