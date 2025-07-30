using System;
using System.IO;
using System.Threading;

namespace Bluscream
{
    public static class Logging
    {
        private static readonly object _lockObject = new object();
        private static string? _logFilePath;
        private static bool _initialized = false;
        private static bool _consoleOutput = true;

        public static void Initialize(string? logFilePath = null, bool consoleOutput = true)
        {
            if (_initialized) return;

            _logFilePath = logFilePath;
            _consoleOutput = consoleOutput;
            
            // Only initialize file logging if logFilePath is not null or whitespace
            if (!string.IsNullOrWhiteSpace(_logFilePath))
            {
                try
                {
                    // Ensure the directory exists
                    var directory = Path.GetDirectoryName(_logFilePath);
                    if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                    {
                        Directory.CreateDirectory(directory);
                    }

                    // Write initial log entry
                    Log("Logging system initialized", "Logging");
                    _initialized = true;
                }
                catch (Exception ex)
                {
                    // Fallback to console if file logging fails (always show initialization errors)
                    Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] [Logging] Failed to initialize logging: {ex.Message}");
                }
            }
            else
            {
                // Console-only logging mode
                Log("Logging system initialized (console-only mode)", "Logging");
                _initialized = true;
            }
        }

        public static void Log(string message, string? category = null, Exception? exception = null)
        {
            var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
            var categoryText = !string.IsNullOrEmpty(category) ? $"[{category}]" : "";
            var logMessage = $"[{timestamp}]{categoryText} {message}";

            // Write to console if enabled
            if (_consoleOutput)
            {
                Console.WriteLine(logMessage);
            }

            // Write to file if initialized and logFilePath is not null or whitespace
            if (_initialized && !string.IsNullOrWhiteSpace(_logFilePath))
            {
                try
                {
                    lock (_lockObject)
                    {
                        File.AppendAllText(_logFilePath, logMessage + Environment.NewLine);
                    }
                }
                catch (Exception ex)
                {
                    // Don't recursively call Log to avoid infinite loops (always show file write errors)
                    Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] [Logging] Failed to write to log file: {ex.Message}");
                }
            }

            // Log exception details if provided
            if (exception != null)
            {
                var exceptionMessage = $"[{timestamp}]{categoryText} Exception: {exception.Message}";
                var stackTrace = $"[{timestamp}]{categoryText} StackTrace: {exception.StackTrace}";

                if (_consoleOutput)
                {
                    Console.WriteLine(exceptionMessage);
                    Console.WriteLine(stackTrace);
                }

                if (_initialized && !string.IsNullOrWhiteSpace(_logFilePath))
                {
                    try
                    {
                        lock (_lockObject)
                        {
                            File.AppendAllText(_logFilePath, exceptionMessage + Environment.NewLine);
                            File.AppendAllText(_logFilePath, stackTrace + Environment.NewLine);
                        }
                    }
                    catch
                    {
                        // Ignore file write errors for exception logging
                    }
                }
            }
        }

        public static void LogError(string message, Exception? exception = null)
        {
            Log($"ERROR: {message}", "Error", exception);
        }

        public static void LogWarning(string message)
        {
            Log($"WARNING: {message}", "Warning");
        }

        public static void LogInfo(string message, string? category = null)
        {
            Log(message, category ?? "Info");
        }

        public static void LogDebug(string message)
        {
            Log(message, "Debug");
        }
    }
} 