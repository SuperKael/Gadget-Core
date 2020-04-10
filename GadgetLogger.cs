using GadgetCore.API;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace GadgetCore
{
    /// <summary>
    /// Class used for mod-specific logging.
    /// </summary>
    public class GadgetLogger : IDisposable
    {
        private static Dictionary<string, StreamWriter> streams = new Dictionary<string, StreamWriter>();
        /// <summary>
        /// The name of this log file.
        /// </summary>
        public readonly string LogName;
        /// <summary>
        /// The name of this loggger.
        /// </summary>
        public readonly string LoggerName;

        private StreamWriter streamWriter;

        /// <summary>
        /// Creates a new GadgetLogger instance.
        /// </summary>
        public GadgetLogger(string LogName, string LoggerName)
        {
            this.LogName = LogName;
            this.LoggerName = LoggerName;
            if (streams.ContainsKey(LogName))
            {
                streamWriter = streams[LogName];
            }
            else
            {
                if (!Directory.Exists(GadgetPaths.LogsPath)) Directory.CreateDirectory(GadgetPaths.LogsPath);
                streams.Add(LogName, streamWriter = new StreamWriter(Path.Combine(GadgetPaths.LogsPath, LogName + ".log")));
                streamWriter.AutoFlush = true;
            }
        }

        /// <summary>
        /// Logs a line of text to both the in-game console, as well as the log file.
        /// </summary>
        public void LogConsole(object text, GadgetConsole.MessageSeverity severity = GadgetConsole.MessageSeverity.INFO)
        {
            switch (severity)
            {
                case GadgetConsole.MessageSeverity.WARN:
                    LogWarning(text);
                    break;
                case GadgetConsole.MessageSeverity.ERROR:
                    LogError(text);
                    break;
                default:
                    GadgetConsole.Print(text?.ToString() ?? "null", LoggerName, severity);
                    Log(text);
                    break;
            }
        }

        /// <summary>
        /// Logs an informational line of text into the log file.
        /// </summary>
        public void Log(object text)
        {
            foreach (string line in (text?.ToString() ?? "null").Replace("\r", "").Split('\n'))
                streamWriter.WriteLine("[" + DateTime.Now + "]" + (!string.IsNullOrEmpty(LoggerName) ? "[" + LoggerName + "]" : "") + "[Info] " + line);
        }

        /// <summary>
        /// Logs a warning as a line of text into the log file.
        /// </summary>
        public void LogWarning(object text, bool includeConsole = true)
        {
            if (includeConsole) GadgetConsole.Print(text?.ToString() ?? "null", LoggerName, GadgetConsole.MessageSeverity.WARN);
            foreach (string line in (text?.ToString() ?? "null").Replace("\r", "").Split('\n'))
                streamWriter.WriteLine("[" + DateTime.Now + "]" + (!string.IsNullOrEmpty(LoggerName) ? "[" + LoggerName + "]" : "") + "[Warning] " + line);
        }

        /// <summary>
        /// Logs an error as a line of text into the log file.
        /// </summary>
        public void LogError(object text, bool includeConsole = true)
        {
            if (includeConsole) GadgetConsole.Print(text?.ToString() ?? "null", LoggerName, GadgetConsole.MessageSeverity.ERROR);
            foreach (string line in (text?.ToString() ?? "null").Replace("\r", "").Split('\n'))
                streamWriter.WriteLine("[" + DateTime.Now + "]" + (!string.IsNullOrEmpty(LoggerName) ? "[" + LoggerName + "]" : "") + "[Error] " + line);
        }

        /// <summary>
        /// Disposes the internal <see cref="StreamWriter"/> used by this logger.
        /// </summary>
        public void Dispose()
        {
            streamWriter.Dispose();
        }
    }
}
