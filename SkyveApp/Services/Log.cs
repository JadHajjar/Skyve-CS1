using Extensions;

using SkyveApp.Services.Interfaces;

using SlickControls;

using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Forms;

namespace SkyveApp.Services;
/// <summary>
/// A simple logging class.
///
/// When mod activates, it creates a log file in same location as `output_log.txt`.
/// Mac users: It will be in the Cities app contents.
/// </summary>
public class Logger : ILogger
{
    /// <summary>
    /// Set to <c>true</c> to include log level in log entries.
    /// </summary>
    private readonly bool ShowLevel = true;

    /// <summary>
    /// Set to <c>true</c> to include timestamp in log entries.
    /// </summary>
    private readonly bool ShowTimestamp = true;

    private readonly string assemblyName_;

    /// <summary>
    /// File name for log file.
    /// </summary>
    private readonly string LogFileName;

    /// <summary>
    /// Full path and file name of log file.
    /// </summary>
    public string LogFilePath { get; }

    /// <summary>
    /// Stopwatch used if <see cref="ShowTimestamp"/> is <c>true</c>.
    /// </summary>
    private readonly Stopwatch? Timer;

    private readonly object fileLock = new object();
    /// <summary>
    /// Initializes members of the <see cref="Log"/> class.
    /// Resets log file on startup.
    /// </summary>
    public Logger()
    {
		assemblyName_ = System.Reflection.Assembly.GetExecutingAssembly().GetName().Name;
		LogFileName = assemblyName_ + ".log";
		LogFilePath = LogFileName;

        try
        {
            if (!Program.IsRunning)
            {
                return;
            }

            var folder = CrossIO.Combine(ISave.CustomSaveDirectory, ISave.AppName);
            LogFilePath = CrossIO.Combine(folder, LogFileName);

            Directory.CreateDirectory(folder);

            try
            {
                if (CrossIO.FileExists(LogFilePath))
                {
					CrossIO.DeleteFile(LogFilePath);
                }
            }
            catch { }

            if (ShowTimestamp)
            {
                Timer = Stopwatch.StartNew();
            }

            var details = typeof(Logger).Assembly.GetName();
            Info($"{details.Name} v{details.Version}");
            Info($"Now  = {DateTime.Now}");
            Info($"Here = {Program.CurrentDirectory}");
        }
        catch (Exception ex)
        {
            Exception(ex, "", true);
        }
    }

    /// <summary>
    /// Log levels. Also output in log file.
    /// </summary>
    private enum LogLevel
    {
        Debug,
        Info,
        Warning,
        Error,
        Exception,
    }

    public const int MAX_WAIT_ID = 1000;
    private readonly DateTime[] times_ = new DateTime[MAX_WAIT_ID];

#if DEBUG
    public void Debug(string message)
    {
        LogImpl(message, LogLevel.Debug);
    }
#endif

    /// <summary>
    /// Logs info message.
    /// </summary>
    /// 
    /// <param name="message">Log entry text.</param>
    /// <param name="copyToGameLog">If <c>true</c> will copy to the main game log file.</param>
    public void Info(string message)
    {
        LogImpl(message, LogLevel.Info);
    }

    /// <summary>
    /// Logs info message.
    /// </summary>
    /// 
    /// <param name="message">Log entry text.</param>
    /// <param name="copyToGameLog">If <c>true</c> will copy to the main game log file.</param>
    public void Warning(string message)
    {
        LogImpl(message, LogLevel.Warning);
    }


    /// <summary>
    /// Logs error message and also outputs a stack trace.
    /// </summary>
    /// 
    /// <param name="message">Log entry text.</param>
    /// <param name="copyToGameLog">If <c>true</c> will copy to the main game log file.</param>
    public void Error(string message)
    {
        LogImpl(message, LogLevel.Error);
    }

    public void Exception(Exception e, string m, bool showInPanel = false)
    {
        try
        {
            var message = e.ToString() + $"\n\t-- {assemblyName_}:end of inner stack trace --";
            if (!string.IsNullOrEmpty(m))
            {
                message = m + " -> \n" + message;
            }

            LogImpl(message, LogLevel.Exception);

            if (showInPanel)
            {
                MessagePrompt.Show(e, m);
            }
        }
        catch (Exception ex)
        {
            new ThreadExceptionDialog(new Exception("could not show advanced exception panel.", ex)).ShowDialog();
            Process.GetCurrentProcess().Kill();
        }
    }

    private readonly string nl = Environment.NewLine;
    private bool loggingFailed;

    /// <summary>
    /// Write a message to log file.
    /// </summary>
    /// 
    /// <param name="message">Log entry text.</param>
    /// <param name="level">Logging level. If set to <see cref="LogLevel.Error"/> a stack trace will be appended.</param>
    private void LogImpl(string message, LogLevel level)
    {
        try
        {
            if (loggingFailed)
            {
                return;
            }

            var ticks = Timer?.ElapsedTicks ?? 0;
            var m = "";
            if (ShowLevel)
            {
                var maxLen = Enum.GetNames(typeof(LogLevel)).Select(str => str.Length).Max();
                m += string.Format($"{{0, -{maxLen}}}", $"[{level}] ");
            }

            if (ShowTimestamp)
            {
                var secs = ticks / Stopwatch.Frequency;
                var fraction = ticks % Stopwatch.Frequency;
                m += string.Format($"{secs:n0}.{fraction:D7} | ");
            }

            m += message + nl;

            if (Program.IsRunning)
            {
                if (level is LogLevel.Exception)
                {
                    m += new StackTrace(true).ToString() + nl + nl;
                }

                lock (fileLock)
                {
                    using var w = File.AppendText(LogFilePath);
                    w.Write(m);
                }
            }
        }
        catch (Exception ex)
        {
            loggingFailed = true;
            MessagePrompt.Show(ex, "Logging failed");
        }
    }

    internal void LogToFileSimple(string file, string message)
    {
        using var w = File.AppendText(file);
        w.WriteLine(message);
        w.WriteLine(new StackTrace().ToString());
        w.WriteLine();
    }

    internal void Succeeded()
    {
        Info(CurrentMethod(2) + " succeeded!");
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    internal string CurrentMethod(int i = 1, params object[] args)
    {
        var method = new StackFrame(i).GetMethod();
        return $"{method.DeclaringType.Name}.{method.Name}({args.ListStrings(", ")})";
    }
}