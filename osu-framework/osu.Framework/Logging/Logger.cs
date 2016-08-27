//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu-framework/master/LICENCE

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using osu.Framework.IO;
using osu.Framework.Threading;

namespace osu.Framework.Logging
{
    public class Logger
    {
        /// <summary>
        /// Directory to place all log files.
        /// </summary>
        public static string LogDirectory
        {
            get
            {
                return logDirectory;
            }
            set
            {
                logDirectory = value;
                hasLogDirectory = null; //we should check again whether the directory exists.
            }
        }

        /// <summary>
        /// Global control over logging.
        /// </summary>
        public static bool Enabled = true;

        /// <summary>
        /// An identifier used in log file headers to figure where the log file came from.
        /// </summary>
        public static string UserIdentifier = Environment.UserName;

        /// <summary>
        /// An identifier used in log file headers to figure where the log file came from.
        /// </summary>
        public static string VersionIdentifier = @"unknown";

        /// <summary>
        /// Add a plain-text phrase which should always be filtered from logs.
        /// Useful for avoiding logging of credentials.
        /// </summary>
        public static void AddFilteredText(string text)
        {
            if (string.IsNullOrEmpty(text)) return;

            filters.Add(text);
        }

        /// <summary>
        /// Removes phrases which should be filtered from logs.
        /// Useful for avoiding logging of credentials.
        /// </summary>
        public static string ApplyFilters(string message)
        {
            foreach (string f in filters)
                message = message.Replace(f, string.Empty.PadRight(f.Length, '*'));

            return message;
        }

        public static void Error(Exception e, string description, LoggingTarget target = LoggingTarget.Runtime)
        {
            Log($@"ERROR: {description}", target, LogLevel.Error);
            Log(e.ToString(), target, LogLevel.Error);
        }

        /// <summary>
        /// Log an arbitrary string to a specific log target.
        /// </summary>
        /// <param name="message">The message to log. Can include newline (\n) characters to split into multiple lines.</param>
        /// <param name="target">The logging target (file).</param>
        /// <param name="level">The verbosity level.</param>
        public static void Log(string message, LoggingTarget target = LoggingTarget.Runtime, LogLevel level = LogLevel.Verbose)
        {
            try
            {
                GetLogger(target, true).Add(message, level);
            }
            catch { }
        }

        /// <summary>
        /// Logs a message to the given log target and also displays a print statement. 
        /// </summary>
        /// <param name="message">The message to log. Can include newline (\n) characters to split into multiple lines.</param>
        /// <param name="target">The logging target (file).</param>
        /// <param name="level">The verbosity level.</param>
        public static void LogPrint(string message, LoggingTarget target = LoggingTarget.Runtime, LogLevel level = LogLevel.Verbose)
        {
#if DEBUG
            System.Diagnostics.Debug.Print(message);
#endif
            Log(message, target, level);
        }

        /// <summary>
        /// For classes that regularly log to the same target, this method may be preferred over the static Log method.
        /// </summary>
        /// <param name="target">The logging target.</param>
        /// <param name="clearOnConstruct">Decides whether we clear any existing content from the log the first time we construct this logger.</param>
        /// <returns></returns>
        public static Logger GetLogger(LoggingTarget target = LoggingTarget.Runtime, bool clearOnConstruct = false)
        {
            Logger l;
            if (!staticLoggers.TryGetValue(target, out l))
            {
                staticLoggers[target] = (l = new Logger(target));
                if (clearOnConstruct) l.Clear();
            }

            return l;
        }

        public LoggingTarget Target { get; private set; }

        public string Filename => logDirectory == null ? null : Path.Combine(logDirectory, $@"{Target.ToString().ToLower()}.log");

        private Logger(LoggingTarget target = LoggingTarget.Runtime)
        {
            Target = target;
        }

        [Conditional("DEBUG")]
        public void Debug(string message = @"")
        {
            Add(message);
        }

        /// <summary>
        /// Log an arbitrary string to current log.
        /// </summary>
        /// <param name="message">The message to log. Can include newline (\n) characters to split into multiple lines.</param>
        /// <param name="level">The verbosity level.</param>
        public void Add(string message = @"", LogLevel level = LogLevel.Verbose)
        {
#if Public
            if (level < LogLevel.Important) return;
#endif

#if !DEBUG
            if (level <= LogLevel.Debug) return;
#endif

            if (!Enabled) return;

            message = ApplyFilters(message);

            //split each line up.
            string[] lines = message.TrimEnd().Replace(@"\r\n", @"\n").Split('\n');
            for (int i = 0; i < lines.Length; i++)
            {
                string s = lines[i];
                lines[i] = $@"{DateTime.UtcNow.ToString(NumberFormatInfo.InvariantInfo)}: {s.Trim()}";
            }

            backgroundScheduler.Add(delegate
            {
                ensureLogDirectoryExists();
                if (!hasLogDirectory.Value)
                    return;

                try
                {
                    File.AppendAllLines(Filename, lines);
                }
                catch { }
            }, true);
        }

        /// <summary>
        /// Deletes log file from disk.
        /// </summary>
        /// <param name="lastLogSuffix">If specified, creates a copy of the last log file with specified suffix.</param>
        public void Clear(string lastLogSuffix = null)
        {
            if (Filename == null) return;

            backgroundScheduler.Add(delegate
            {
                if (!string.IsNullOrEmpty(lastLogSuffix))
                    FileSafety.FileMove(Filename, Filename.Replace(@".log", $@"_{lastLogSuffix}.log"));
                else
                    FileSafety.FileDelete(Filename);
            }, true);

            addHeader();
        }

        private void addHeader()
        {
            Add($@"----------------------------------------------------------");
            Add($@"{Target} Log for {UserIdentifier}");
            Add($@"osu! version {VersionIdentifier}");
            Add($@"Running on {Environment.OSVersion}, {Environment.ProcessorCount} cores");
            Add($@"----------------------------------------------------------");
        }

        static List<string> filters = new List<string>();
        static Dictionary<LoggingTarget, Logger> staticLoggers = new Dictionary<LoggingTarget, Logger>();
        static ThreadedScheduler backgroundScheduler = new ThreadedScheduler();
        static bool? hasLogDirectory;
        static string logDirectory;

        private void ensureLogDirectoryExists()
        {
            if (hasLogDirectory.HasValue)
                return;

            try
            {
                hasLogDirectory = Directory.CreateDirectory(logDirectory) != null;
            }
            catch
            {
                hasLogDirectory = false;
            }
        }
    }

    public enum LogLevel
    {
        Debug,
        Verbose,
        Important,
        Error,
    }

    public enum LoggingTarget
    {
        Runtime,
        Network,
        Tournament,
        Update,
        Performance,
        Debug
    }
}
