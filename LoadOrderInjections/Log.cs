namespace KianCommons {
    using ColossalFramework.UI;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Threading;
    using UnityEngine;

    /// <summary>
    /// A simple logging class.
    ///
    /// When mod activates, it creates a log file in same location as `output_log.txt`.
    /// Mac users: It will be in the Cities app contents.
    /// </summary>
    public static class Log {
        /// <summary>
        /// Set to <c>true</c> to include log level in log entries.
        /// </summary>
        private static readonly bool ShowLevel = true;

        /// <summary>
        /// Set to <c>true</c> to include timestamps in log entries.
        /// </summary>
        private static readonly bool ShowTimestamp = true;

        /// <summary>
        /// File name for log file.
        /// </summary>
        private static readonly string LogFileName = "LoadOrder.log";

        /// <summary>
        /// Full path and file name of log file.
        /// </summary>
        private static readonly string LogFilePath;

        /// <summary>
        /// Stopwatch used if <see cref="ShowTimestamp"/> is <c>true</c>.
        /// </summary>
        private static readonly Stopwatch Timer;

        private static StreamWriter filerWrier_;

        private static object LogLock = new object();

        public static bool ShowGap = false;

        public static bool VERBOSE { get; set; } = false;

        private static long prev_ms_;

        public static int FlushInterval = 500; //ms

        /// <summary>
        /// buffered logging is much faster but requires extra care for hot-reload/external modifications.
        /// to use Buffered mode with hot-reload: set when mod is enabled and unset when mod is disabled.
        /// IMPORTANT: Buffer must be set to false as part of cleanup (particularly for hot reload).
        /// Note: buffered mode is 20 times faster but only if you do not copy to game log.
        /// </summary>
        public static bool Buffered {
            get => filerWrier_ != null;
            set {
                Log.Called(value);
                if (value == Buffered) return;
                if (value) {
                    try {
                        filerWrier_ = new StreamWriter(LogFilePath, true);
                    } catch (Exception ex) {
                        Log.Exception(ex, "failed to setup log buffer");
                    }
                    FlushTread.Init();
                } else {
                    filerWrier_.Flush();
                    filerWrier_.Dispose();
                    filerWrier_ = null;
                    FlushTread.Terminate();
                }
            }
        }

        /// <summary>
        /// if buffered then lock the file and flush.
        /// otherwise return silently.
        /// </summary>
        public static void Flush() {
            if (filerWrier_ != null) {
                lock (LogLock)
                    filerWrier_?.Flush();
            }
        }

        public static class FlushTread {
            static bool isRunning_;
            private static Thread flushThraad_;

            public static void Init() {
                try {
                    if (isRunning_) return; //already initialized
                    Log.Info("Initializing Log.FlushTread");
                    flushThraad_ = new Thread(FlushThread);
                    flushThraad_.Name = "FlushThread";
                    flushThraad_.IsBackground = true;
                    isRunning_ = true;
                    flushThraad_.Start();
                } catch (Exception ex) {
                    Log.Exception(ex);
                }
            }

            public static void Terminate() {
                try {
                    Log.Info("FlushTread.Terminate() called");
                    isRunning_ = false;
                    flushThraad_.Join();
                    flushThraad_ = null;
                    Log.Flush();
                } catch (Exception ex) {
                    Log.Exception(ex);
                }
            }


            private static void FlushThread() {
                try {
                    while (isRunning_) {
                        Thread.Sleep(FlushInterval);
                        Log.Flush();
                    }
                    Log.Info("Flush Thread Exiting...");
                } catch (Exception ex) {
                    Log.Exception(ex);
                }
            }
        }

        public static Stopwatch GetSharedTimer() {
            var asm = AppDomain.CurrentDomain.GetAssemblies()
                .FirstOrDefault(_asm => _asm.GetName().Name == "LoadOrderIPatch");
            var t = asm?.GetType("LoadOrderIPatch.Patches.UnityLoggerPatch", throwOnError: false);
            return t?.GetField("m_Timer")?.GetValue(null) as Stopwatch;
        }

        static void TryDeleteFile(string path) {
            if (File.Exists(path))
                File.Delete(path);
        }

        /// <summary>
        /// Initializes static members of the <see cref="Log"/> class.
        /// Resets log file on startup.
        /// </summary>
        static Log() {
            try {
                var dir = Path.Combine(Application.dataPath, "Logs");
                if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

                LogFilePath = Path.Combine(dir, LogFileName);
                TryDeleteFile(LogFilePath);
                TryDeleteFile(Path.Combine(dir, "LoadOrderMod.log"));
                TryDeleteFile(Path.Combine(dir, "LoadOrderInjections.log"));

                if (ShowTimestamp) {
                    Timer = GetSharedTimer() ?? Stopwatch.StartNew();
                }

                AssemblyName details = typeof(Log).Assembly.GetName();
                Info($"Log file at {LogFilePath} now={DateTime.Now}", true);
                Info($"{details.Name} Version:{details.Version} " +
                     $"Commit:{ThisAssembly.Git.Commit} " +
                     $"CommitDate:{ThisAssembly.Git.CommitDate}", true);
            } catch (Exception ex) {
                Log.LogUnityException(ex);
            }
        }

        /// <summary>
        /// Log levels. Also output in log file.
        /// </summary>
        private enum LogLevel {
            Debug,
            Info,
            Error,
            Warning,
            Exception,
        }


        public const int MAX_WAIT_ID = 1000;
        static DateTime[] times_ = new DateTime[MAX_WAIT_ID];

        [Conditional("DEBUG")]
        public static void DebugWait(string message, int id, float seconds = 0.5f, bool copyToGameLog = true) {
            float diff = seconds + 1;
            if (id < 0) id = -id;
            id = System.Math.Abs(id % MAX_WAIT_ID);
            if (times_[id] != null) {
                var diff0 = DateTime.Now - times_[id];
                diff = diff0.Seconds;
            }
            if (diff >= seconds) {
                Log.Debug(message, copyToGameLog);
                times_[id] = DateTime.Now;
            }
        }

        [Conditional("DEBUG")]
        public static void DebugWait(string message, object id = null, float seconds = 0.5f, bool copyToGameLog = true) {
            if (id == null)
                id = Environment.StackTrace + message;
            DebugWait(message, id.GetHashCode(), seconds, copyToGameLog);

        }

        /// <summary>
        /// Logs debug trace, only in <c>DEBUG</c> builds.
        /// </summary>
        /// <param name="message">Log entry text.</param>
        /// <param name="copyToGameLog">If <c>true</c> will copy to the main game log file.</param>
        [Conditional("DEBUG")]
        public static void Debug(string message, bool copyToGameLog = true) {
            LogImpl(message, LogLevel.Debug, copyToGameLog);
        }

        /// <summary>
        /// Logs info message.
        /// </summary>
        /// 
        /// <param name="message">Log entry text.</param>
        /// <param name="copyToGameLog">If <c>true</c> will copy to the main game log file.</param>
        public static void Info(string message, bool copyToGameLog = false) {
            LogImpl(message, LogLevel.Info, copyToGameLog);
        }

        /// <summary>
        /// Logs error message and also outputs a stack trace.
        /// </summary>
        /// 
        /// <param name="message">Log entry text.</param>
        /// <param name="copyToGameLog">If <c>true</c> will copy to the main game log file.</param>
        public static void Error(string message, bool copyToGameLog = true) {
            LogImpl(message, LogLevel.Error, copyToGameLog);
        }

        public static void Warning(string message, bool copyToGameLog = true) {
            LogImpl(message, LogLevel.Warning, copyToGameLog);
        }
        static string ExceptionData(Exception ex) {
            var keys = ex.Data?.Keys;
            if (keys != null) {
                var data = new List<string>();
                foreach (var key in keys)
                    data.Add($"'{key}' : '{ex.Data[key]}'");
                if (data.Any())
                    return "Data: " + string.Join(" | ", data.ToArray());
            }
            return null;
        }

        static string LoaderExceptions(Exception ex) {
            if(ex is ReflectionTypeLoadException rtle) {
                return "LoaderExceptions = " + rtle.LoaderExceptions.ToSTR();
            }
            return null;
        }

        public static void Exception(this Exception ex, string m = "", bool showInPanel = true) {
            if (ex == null)
                Log.Error("null argument e was passed to Log.Exception()");
            try {
                string message = ex.ToString() + $"\n\t-- end of exception --";
                if (!string.IsNullOrEmpty(m))
                    message = m + " -> \n" + message;

                var data = ExceptionData(ex);
                if (!string.IsNullOrEmpty(data))
                    message = data + "\n" + message;

                var loaderExceptions = LoaderExceptions(ex);
                if (!string.IsNullOrEmpty(loaderExceptions))
                    message = loaderExceptions + "\n" + message;


                LogImpl(message, LogLevel.Exception, true);
                if (showInPanel)
                    UIView.ForwardException(ex);
            } catch (Exception ex2) {
                Log.LogUnityException(ex2);
            }
        }

        public static void LogUnityException(Exception ex, bool showInPanel = true) {
            UnityEngine.Debug.LogException(ex);
            if (showInPanel)
                UIView.ForwardException(ex);
        }

        public static void ShowModalException(string title, string message, bool error = false) {
            UIView.library.ShowModal<ExceptionPanel>("ExceptionPanel")
                .SetMessage(title, message, error);
            string m = title + " : " + message;
            if (error)
                Log.Error(m, true);
            else
                Log.Info(m, true);
        }

        static string nl = "\n";

        /// <summary>
        /// Write a message to log file.
        /// </summary>
        /// 
        /// <param name="message">Log entry text.</param>
        /// <param name="level">Logging level. If set to <see cref="LogLevel.Error"/> a stack trace will be appended.</param>
        private static void LogImpl(string message, LogLevel level, bool copyToGameLog) {
            try {
                var ticks = Timer.ElapsedTicks;
                string m = "";
                if (ShowLevel) {
                    int maxLen = Enum.GetNames(typeof(LogLevel)).Max(str => str.Length);
                    m += string.Format($"{{0, -{maxLen}}}", $"[{level}] ");
                }

                long ms = Timer.ElapsedMilliseconds;
                long gapms = ms - prev_ms_;
                prev_ms_ = ms;

                if (ShowTimestamp) {
                    m += $"{ms:#,0}ms | ";
                    if (ShowGap) {
                        m += $"gap={gapms:#,0}ms | ";
                    }
                }

                m += message;
                if (level == LogLevel.Error || level == LogLevel.Exception) {
                    m += nl + GetStackTrace();
                    m = nl + m + nl; // create line space to draw attention.
                }

                try {
                    lock (LogLock) {
                        if (filerWrier_ != null) {
                            filerWrier_.WriteLine(m);
                        } else {
                            using (StreamWriter w = File.AppendText(LogFilePath))
                                w.WriteLine(m);
                        }
                    }
                } catch (Exception ex) {
                    LogUnityException(ex, false);
                }

                if (copyToGameLog) {
                    // copying to game log is slow anyways so
                    // this is a good time to flush if necessary.
                    Flush();
                    m = " LoadOrder | " + m;
                    m = RemoveExtraNewLine(m);
                    switch (level) {
                        case LogLevel.Error:
                        case LogLevel.Exception:
                            UnityEngine.Debug.LogError(m);
                            break;
                        case LogLevel.Warning:
                            UnityEngine.Debug.LogWarning(m);
                            break;
                        default:
                            UnityEngine.Debug.Log(m);
                            break;
                    }
                }
                if (gapms > FlushInterval)
                    Log.Flush();
            } catch (Exception ex) {
                Log.LogUnityException(ex);
            }
        }

        static string GetStackTrace() {
            var st = new StackTrace();
            int i;
            for (i = 0; i < st.FrameCount; ++i) {
                var util = st.GetFrame(i).GetMethod().DeclaringType;
                bool utilFrame = util == typeof(Assertion) || util == typeof(Log);
                if (!utilFrame) break;
            }
            return new StackTrace(i - 1, true).ToString(); // keep the last assertion/log frame.
        }

        public static string RemoveExtraNewLine(string str)
            => str.Replace("\r\n", "\n");

        public static void LogToFileSimple(string file, string message) {
            using (StreamWriter w = File.AppendText(file)) {
                w.WriteLine(message);
                w.WriteLine(new StackTrace().ToString());
                w.WriteLine();
            }
        }

        public static void Called(params object[] args) => Info(ReflectionHelpers.CurrentMethod(2, args) + " called.", false);
        public static void Succeeded() => Info(ReflectionHelpers.CurrentMethod(2) + " succeeded!", false);
    }

    public static class LogExtensions {
        /// <summary>
        /// useful for easily debugging inline functions
        /// to be used like this example:
        /// TYPE inlinefunctionname(...) => expression
        /// TYPE inlinefunctionname(...) => expression.LogRet("message");
        /// </summary>
        public static T LogRet<T>(this T a, string m) {
            KianCommons.Log.Debug(m + " -> " + a);
            return a;
        }

        public static void Log(this Exception ex, string message, bool showInPannel = true) =>
            KianCommons.Log.Exception(ex, message, showInPannel);

        public static void Log(this Exception ex, bool showInPannel = true) =>
            KianCommons.Log.Exception(ex, "", showInPannel);

    }
}