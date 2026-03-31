using System;
using System.IO;
using System.Web.Hosting;

namespace CheapDeal.WebApp.Helpers
{
    public static class LogHelper
    {
        private static readonly string LogDir = Path.Combine(
            HostingEnvironment.MapPath("~/") ?? AppDomain.CurrentDomain.BaseDirectory,
            "App_Data",
            "Logs"
        );

        private static readonly string LogPath = Path.Combine(
            LogDir,
            $"email_{DateTime.Now:yyyy-MM-dd}.log"
        );

        static LogHelper()
        {
            // T?o th? m?c n?u ch?a t?n t?i
            try
            {
                if (!Directory.Exists(LogDir))
                {
                    Directory.CreateDirectory(LogDir);
                }
            }
            catch { }
        }

        public static void LogEmail(string message)
        {
            try
            {
                if (!Directory.Exists(LogDir))
                {
                    Directory.CreateDirectory(LogDir);
                }

                string logMessage = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] {message}";
                string todayLogPath = Path.Combine(LogDir, $"email_{DateTime.Now:yyyy-MM-dd}.log");
                
                lock (LogDir)
                {
                    File.AppendAllText(todayLogPath, logMessage + Environment.NewLine);
                }

                System.Diagnostics.Debug.WriteLine(logMessage);
            }
            catch (Exception ex)
            {
                // N?u file logging fail, ít nh?t v?n có Debug output
                System.Diagnostics.Debug.WriteLine($"[LOG ERROR] {ex.Message}");
            }
        }
    }
}

