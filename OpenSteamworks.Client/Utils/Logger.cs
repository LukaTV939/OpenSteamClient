using System.Collections.Concurrent;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics.X86;
using System.Runtime.Versioning;
using System.Text;
using Microsoft.VisualBasic;
using OpenSteamClient.Logging;

namespace OpenSteamworks.Client;

public class Logger : ILogger {
    private struct LogData {
        public Logger logger;
        public DateTime Timestamp;
        public LogLevel Level;
        public string Message;
        public string Category;
        public bool FullLine;

        public LogData(Logger logger, LogLevel level, string msg, string category, bool fullLine) {
            this.logger = logger;
            this.Timestamp = DateTime.Now;
            this.Level = level;
            this.Message = msg;
            this.Category = category;
            this.FullLine = fullLine;
        }
    }

    public static ILogger GeneralLogger {
        get {
            if (GeneralLoggerOverride != null) {
                return GeneralLoggerOverride;
            }

            return LazyGeneralLogger.Value;
        }

        internal set {
            GeneralLoggerOverride = value;
        }
    }

    private static ILogger? GeneralLoggerOverride;
    private readonly static Lazy<ILogger> LazyGeneralLogger = new(() => new Logger("General"));

    public string Name { get; set; } = "";
    public string? LogfilePath { get; init; } = null;

    /// <summary>
    /// Should the logger prefix messages it receives?
    /// </summary>
    public bool AddPrefix { get; set; } = true;

    /// <summary>
    /// If this logger is a sublogger, this is it's name.
    /// </summary>
    private string subLoggerName { get; set; } = "";

    /// <summary>
    /// If this logger is a sublogger, this is it's parent.
    /// </summary>
    private Logger? parentLogger { get; set; }

    // https://no-color.org/
    private static bool disableColors = Environment.GetEnvironmentVariable("NO_COLOR") != null;
    private object logStreamLock = new();
    private FileStream? logStream;
    private static List<Logger> loggers = new();
    private static readonly Thread logThread;

    public class DataReceivedEventArgs : EventArgs {
        public LogLevel Level { get; set; }
        public string Text { get; set; }
        public string AnsiColorSequence { get; set; }
        public string AnsiResetCode { get; set; }
        public bool FullLine { get; set; }
        public DataReceivedEventArgs(LogLevel level, string text, string ansiColorSequence, string ansiResetCode) {
            this.Level = level;
            this.Text = text;
            this.FullLine = text.EndsWith(Environment.NewLine);
            this.AnsiColorSequence = ansiColorSequence;
            this.AnsiResetCode = ansiResetCode;
        }
    }

    public static event EventHandler<DataReceivedEventArgs>? DataReceived;

    static Logger() {
        logThread = new(LogThreadMain);
        logThread.Name = "LogThread";
        logThread.Start();
    }

    private Logger(string name, string? filepath = "") {
        this.Name = name;
        this.LogfilePath = filepath;

        loggers.Add(this);
        if (!string.IsNullOrEmpty(filepath)) {
            if (File.Exists(filepath)) {
                // Delete if over 4MB
                var fi = new FileInfo(filepath);
                if ((fi.Length / 1024 / 1024) > 4) {
                    fi.Delete();
                }
            }
            logStream = File.Open(filepath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.ReadWrite);
            logStream.Seek(logStream.Length, SeekOrigin.Begin);
        }
        
        if (!hasRanWindowsHack && OperatingSystem.IsWindows()) {
            RunWindowsConsoleColorsHack();
        }
    }

    private static readonly ConcurrentQueue<LogData> dataToLog = new();
    private static void LogThreadMain() {
        while (true)
        {
            if (dataToLog.IsEmpty) {
                System.Threading.Thread.Sleep(50);
                continue;
            }

            if (dataToLog.TryDequeue(out LogData data)) {
                if (data.FullLine) {
                    MessageInternal(data.logger, data.Timestamp, data.Level, data.Message, data.Category);
                } else {
                    WriteInternal(data.logger, data.Message);
                }
            }
        }
    }

    public static Logger GetLogger(string name, string? filepath = "") {
        foreach (var item in loggers)
        {
            if (item.Name == name && item.LogfilePath == filepath) {
                return item;
            }
        }

        return new Logger(name, filepath);
    }

    /// <summary>
    /// Creates a sub-logger. Uses the logstream of the current logger, and sets subname as a category name for each print.
    /// </summary>
    /// <param name="subName"></param>
    /// <returns></returns>
    public Logger CreateSubLogger(string subName) {
        var logger = new Logger("", "");
        logger.subLoggerName = subName;
        logger.parentLogger = this;
        return logger;
    }

    private static void MessageInternal(Logger logger, DateTime timestamp, LogLevel level, string message, string category) {
        if (logger.parentLogger != null) {
            var actualCategory = logger.subLoggerName;
            if (!string.IsNullOrEmpty(category)) {
                actualCategory += "/" + category;
            }
            
            MessageInternal(logger.parentLogger, timestamp, level, message, actualCategory);
            return;
        }

        string formatted = message;
        if (logger.AddPrefix) {
            // welp. we can't just use the system's date format, but we also need to use the system's time at the same time, which won't include milliseconds and will always have AM/PM appended, even on 24-hour clocks. So use the objectively better formatting system of dd/MM/yyyy and always use 24-hour time (which will also make it easier for the devs reviewing bug reports)
            formatted = string.Format("[{0} {1}{2}: {3}] {4}", timestamp.ToString("dd/MM/yyyy HH:mm:ss.ff"), logger.Name, string.IsNullOrEmpty(category) ? "" : $"/{category}", level.ToString(), message);
        }

		if (!formatted.EndsWith(Environment.NewLine)) {
			formatted += Environment.NewLine;
		}

        string ansiColorCode = string.Empty;
        string ansiResetCode = string.Empty;

        if (!disableColors) {
            ansiResetCode = "\x1b[0m";
            if (level == LogLevel.Fatal) {
                ansiColorCode = "\x1b[91m";
            } else if (level == LogLevel.Error) {
                ansiColorCode = "\x1b[31m";
            } else if (level == LogLevel.Warning) {
                ansiColorCode = "\x1b[33m";
            } else if (level == LogLevel.Info) {
                //ansiColorCode = "\x1b[37m";
            } else if (level == LogLevel.Debug) {
                ansiColorCode = "\x1b[2;37m";
            }
        }

        Console.Write(ansiColorCode + formatted + ansiResetCode);

        if (logger.logStream != null) {
            lock (logger.logStreamLock)
            {
                logger.logStream.Write(Encoding.UTF8.GetBytes(formatted));

                //TODO: Implement a more robust system with debouncing and/or per lines since last flush
                logger.logStream.Flush();
            }
        }

        DataReceived?.Invoke(logger, new(level, formatted, ansiColorCode, ansiResetCode));
    }

    private void AddData(LogLevel level, string message) {
		foreach (var line in message.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries))
		{
			dataToLog.Enqueue(new LogData(this, level, line, string.Empty, true));
		}
        
    }

	private static void WriteInternal(Logger logger, string message) {
        Console.Write(message);
        if (logger.logStream != null) {
            lock (logger.logStreamLock)
            {
                logger.logStream.Write(Encoding.Default.GetBytes(message));
            }
        }

        DataReceived?.Invoke(logger, new(LogLevel.Info, message, string.Empty, string.Empty));
    }

    /// <inheritdoc/>
    public void Write(LogLevel level, string message) {
        AddData(level, message);
    }

    ~Logger() {
        loggers.Remove(this);
    }

    private static bool hasRanWindowsHack = false;

    /// <summary>
    /// Windows is stuck using legacy settings unless you tell it explicitly to use "ENABLE_VIRTUAL_TERMINAL_PROCESSING". Why???
    /// </summary>
    [SupportedOSPlatform("windows")]
    private static void RunWindowsConsoleColorsHack() {
        hasRanWindowsHack = true;
        const int STD_INPUT_HANDLE = -10;
        const int STD_OUTPUT_HANDLE = -11;
        const uint ENABLE_VIRTUAL_TERMINAL_PROCESSING = 0x0004;
        const uint DISABLE_NEWLINE_AUTO_RETURN = 0x0008;
        const uint ENABLE_VIRTUAL_TERMINAL_INPUT = 0x0200;

        [DllImport("kernel32.dll")]
        static extern bool GetConsoleMode(IntPtr hConsoleHandle, out uint lpMode);

        [DllImport("kernel32.dll")]
        static extern bool SetConsoleMode(IntPtr hConsoleHandle, uint dwMode);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern IntPtr GetStdHandle(int nStdHandle);

        [DllImport("kernel32.dll")]
        static extern uint GetLastError();

        var iStdIn = GetStdHandle(STD_INPUT_HANDLE);
        var iStdOut = GetStdHandle(STD_OUTPUT_HANDLE);

        if (!GetConsoleMode(iStdIn, out uint inConsoleMode))
        {
            Console.WriteLine("[Windows Console Color Hack] failed to get input console mode");
            disableColors = true;
            return;
        }
        if (!GetConsoleMode(iStdOut, out uint outConsoleMode))
        {
            Console.WriteLine("[Windows Console Color Hack] failed to get output console mode");
            disableColors = true;
            return;
        }

        inConsoleMode |= ENABLE_VIRTUAL_TERMINAL_INPUT;
        outConsoleMode |= ENABLE_VIRTUAL_TERMINAL_PROCESSING | DISABLE_NEWLINE_AUTO_RETURN;

        if (!SetConsoleMode(iStdIn, inConsoleMode))
        {
            Console.WriteLine($"[Windows Console Color Hack] failed to set input console mode, error code: {GetLastError()}");
            disableColors = true;
            return;
        }

        if (!SetConsoleMode(iStdOut, outConsoleMode))
        {
            Console.WriteLine($"[Windows Console Color Hack] failed to set output console mode, error code: {GetLastError()}");
            disableColors = true;
            return;
        }
    }
}