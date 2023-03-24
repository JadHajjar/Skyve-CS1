namespace LoadOrderMod.Patches.HotReload {
    using HarmonyLib;
    using System;
    using static KianCommons.ReflectionHelpers;
    using ColossalFramework.Plugins;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Diagnostics;
    using KianCommons;
    using System.IO;
    using ColossalFramework.IO;

#if DEBUG
    [HarmonyPatch]
    public static class DebugStackTime {
        static IEnumerable<MethodBase> TargetMethods() {
            yield return GetMethod(typeof(PluginManager), "OnPluginFolderRemoved");
            yield return GetMethod(typeof(PluginManager), "OnPluginAdded");
        }

        static void Prefix(string path) => Log.Debug($"{new StackFrame(1).GetMethod().Name}.Prefix({path}) "/* + Helper.STCulled()*/);

        static void Postfix(string path) => Log.Debug($"{new StackFrame(1).GetMethod().Name}.Postfix({path})"/* + Helper.STCulled()*/);
    }

    [HarmonyPatch]
    public static class DebugStackTime2 {
        static IEnumerable<MethodBase> TargetMethods() {
            yield return GetMethod(typeof(PluginManager), "OnFileWatcherEventDeleted");
            yield return GetMethod(typeof(PluginManager), "OnFileWatcherEventCreated");
            yield return GetMethod(typeof(PluginManager), "OnFileWatcherEventChanged");
        }

        static void Prefix(ReporterEventArgs e) => Log.Debug($"{new StackFrame(1).GetMethod().Name}.Prefix({((FileSystemEventArgs)e.arguments).FullPath}) "/* + Helper.STCulled()*/);

        static void Postfix(ReporterEventArgs e) => Log.Debug($"{new StackFrame(1).GetMethod().Name}.Postfix({((FileSystemEventArgs)e.arguments).FullPath})"/* + Helper.STCulled()*/);
    }

    static class Helper { 
        static string[] redundants = new[]{
            "DebugStackTime.STCulled()",
            "at System.Environment.get_StackTrace()",
            "at ColossalFramework.Threading.Task",
            "at ColossalFramework.Threading.Dispatcher.RunTask(ColossalFramework.Threading.Task task)",
            "at ColossalFramework.Threading.Dispatcher.ProcessSingleTask(ColossalFramework.Threading.Task task)",
            "at ColossalFramework.Threading.Dispatcher.InternalProcessTasks()",
            "at ColossalFramework.Threading.Dispatcher.ProcessTasks()",
            "at ColossalFramework.Threading.ThreadHelper.FpsBoosterUpdate()",
            "at BehaviourUpdater.Updater.Update()",
            };

        internal static string STCulled() {
            var st = Environment.StackTrace.Remove("\r");
            foreach (var line in st.SplitLines()) {
                foreach (var r in redundants) {
                    if (line.Contains(r)) {
                        st = st.Remove(line);
                    }
                }
            }
            st = st.Remove("ColossalManaged, Version=0.3.0.0, Culture=neutral, PublicKeyToken=null");
            st = st.Remove("ColossalFramework.Plugins.PluginManager.ColossalFramework.Plugins.");
            st = st.Remove("ColossalFramework.Plugins.");
            return st.SplitLines(StringSplitOptions.RemoveEmptyEntries).JoinLines();
        }

    }
}
#endif