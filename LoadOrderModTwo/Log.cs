namespace KianCommons {
    extern alias Injections;
    using LogMain = Injections.KianCommons.Log;
    using System;
    using System.Diagnostics;
    using System.IO;
    public static class Log {
        public static bool ShowGap {
            get => LogMain.ShowGap;
            set => LogMain.ShowGap= value;
        }
        public static bool VERBOSE {
            get => LogMain.VERBOSE;
            set => LogMain.VERBOSE = value;
        }
        public static int FlushInterval {
            get => LogMain.FlushInterval;
            set => LogMain.FlushInterval = value;
        }

        public static bool Buffered {
            get => LogMain.Buffered;
            set => LogMain.Buffered = value;
        }

        public static void Flush() => LogMain.Flush();

        [Conditional("DEBUG")]
        public static void DebugWait(string message, int id, float seconds = 0.5f, bool copyToGameLog = true) =>
            LogMain.DebugWait(message, id, seconds, copyToGameLog);

        [Conditional("DEBUG")]
        public static void DebugWait(string message, object id = null, float seconds = 0.5f, bool copyToGameLog = true) =>
            LogMain.DebugWait(message, id, seconds, copyToGameLog);

        [Conditional("DEBUG")]
        public static void Debug(string message, bool copyToGameLog = true) => LogMain.Debug(message, copyToGameLog);

        public static void Info(string message, bool copyToGameLog = false) => LogMain.Info(message, copyToGameLog);

        public static void Error(string message, bool copyToGameLog = true) => LogMain.Error(message, copyToGameLog);

        public static void Warning(string message, bool copyToGameLog = true) => LogMain.Warning(message, copyToGameLog);

        public static void DisplayWarning(string message) {
            Warning(message, true);
            DebugOutputPanel.AddMessage(ColossalFramework.Plugins.PluginManager.MessageType.Warning, message);
        }

        public static void DisplayError(string message) {
            Error(message, true);
            DebugOutputPanel.AddMessage(ColossalFramework.Plugins.PluginManager.MessageType.Error, message);
        }

        public static void DisplayMesage(string message) {
            Info(message, true);
            DebugOutputPanel.AddMessage(ColossalFramework.Plugins.PluginManager.MessageType.Message, message);
            DebugOutputPanel.Show();
        }

        public static void Exception(this Exception ex, string m = "", bool showInPanel = true) => LogMain.Exception(ex, m, showInPanel);

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
            KianCommons.Log.Debug(m + " -> " + a.ToSTR());
            return a;
        }

        public static void Log(this Exception ex, string message, bool showInPannel = true) =>
            KianCommons.Log.Exception(ex, message, showInPannel);

        public static void Log(this Exception ex, bool showInPannel = true) =>
            KianCommons.Log.Exception(ex, "", showInPannel);

    }
}