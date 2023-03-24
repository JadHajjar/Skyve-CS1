namespace LoadOrderIPatch {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Mono.Cecil;
    using Patch.API;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;
    using LoadOrderIPatch.Patches;
    using System.IO;

    internal static class LogExtension {
        public static void Log(this Exception ex) => LoadOrderIPatch.Log.Exception(ex);
    }

    internal static class Log {
        const string FILENAME = "LoadOrderIPatch.log";
        static string FilePath => Path.Combine(Entry.GamePaths.LogsPath, FILENAME);
        public static void Init() {
            if(File.Exists(FilePath)) File.Delete(FilePath);

            var details = typeof(Log).Assembly.GetName();
            Info($"Log file at {FilePath} now={DateTime.Now}");
            Info($"{details.Name} Version:{details.Version} " +
                 $"Commit:{ThisAssembly.Git.Commit} " +
                 $"CommitDate:{ThisAssembly.Git.CommitDate}");
        }

        static ILogger iLogger_ => Entry.Logger;

        public static void Info(string text) {
            iLogger_.Info("[LoadOrderIPatch] " + text);
            LogImpl("Info", text);
        }
        public static void Warning(string text) {
            iLogger_.Info("[Warning] [LoadOrderIPatch] " + text);
            LogImpl("Warning", text);
        }

        public static void Error(string text) {
            iLogger_.Error("[LoadOrderIPatch] " + text);
            LogImpl("Error", text + "\n" + Environment.StackTrace);
        }
        public static void Exception(this Exception ex) {
            iLogger_.Error("[Exception] [LoadOrderIPatch] " + ex.Message);
            LogImpl("Exception", ex.ToString() + "\nException logged at:\n" + Environment.StackTrace);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void Successful() {
            string caller = new StackFrame(1).GetMethod().Name;
            Log.Info($"Successfully applied {caller}!");

        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void StartPatching() {
            string caller = new StackFrame(1).GetMethod().Name;
            Log.Info($"{caller} started ...");
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void Called(params object [] args) {
            string caller = new StackFrame(1).GetMethod().Name;
            Log.Info($"{caller}({JoinArgs(args)}) called ..." );
        }

        private static string JoinArgs(object[] args) {
            if (args == null || !args.Any())
                return "";
            else
                return string.Join(", ", args.Select(a => a.ToString()).ToArray());
        }

        static object logLock_ = new();

        static void LogImpl(string level, string text) {
            lock(logLock_)
                File.AppendAllText(FilePath, $"[{level}] {text}\n");
        }
    }
}
