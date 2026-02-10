using System;
using System.Runtime.InteropServices;

namespace SimpleReminder
{
    public static class UserActivityMonitor
    {
        [DllImport("user32.dll")]
        private static extern bool GetLastInputInfo(ref LASTINPUTINFO plii);

        [StructLayout(LayoutKind.Sequential)]
        private struct LASTINPUTINFO
        {
            public uint cbSize;
            public uint dwTime;
        }

        /// <summary>
        /// Checks if the user has been idle for more than the specified threshold
        /// </summary>
        /// <param name="idleThresholdMinutes">Idle threshold in minutes</param>
        /// <returns>True if user is idle, false otherwise</returns>
        public static bool IsUserIdle(int idleThresholdMinutes)
        {
            LASTINPUTINFO lastInputInfo = new LASTINPUTINFO();
            lastInputInfo.cbSize = (uint)Marshal.SizeOf(lastInputInfo);

            if (!GetLastInputInfo(ref lastInputInfo))
            {
                return false; // If we can't determine, assume user is active
            }

            uint idleTime = (uint)Environment.TickCount - lastInputInfo.dwTime;
            double idleMinutes = idleTime / 1000.0 / 60.0;

            return idleMinutes >= idleThresholdMinutes;
        }

        /// <summary>
        /// Gets the number of minutes the user has been idle
        /// </summary>
        /// <returns>Idle time in minutes</returns>
        public static double GetIdleTimeMinutes()
        {
            LASTINPUTINFO lastInputInfo = new LASTINPUTINFO();
            lastInputInfo.cbSize = (uint)Marshal.SizeOf(lastInputInfo);

            if (!GetLastInputInfo(ref lastInputInfo))
            {
                return 0;
            }

            uint idleTime = (uint)Environment.TickCount - lastInputInfo.dwTime;
            return idleTime / 1000.0 / 60.0;
        }
    }
}
