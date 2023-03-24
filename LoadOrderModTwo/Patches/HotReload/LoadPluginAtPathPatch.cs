namespace LoadOrderMod.Patches.HotReload {
    using HarmonyLib;
    using ColossalFramework.Plugins;
    using System.IO;
    using System;
    using KianCommons;
    using static KianCommons.ReflectionHelpers;

    [HarmonyPatch(typeof(PluginManager), "LoadPluginAtPath")]
    public static class LoadPluginAtPathPatch {
        /// <summary>
        /// pluginInfo.name that is being added. the name of the containing directory.
        /// (different than IUserMod.Name).
        /// </summary>        
        public static string dirName;

        static void Prefix(string path) {
            LogCalled(path);
            try {
                dirName = Path.GetFileNameWithoutExtension(path);
            } catch (Exception ex) {
                Log.Exception(ex);
            }
        }
        static void Finalizer(Exception __exception) {
            LogCalled();
            dirName = null;
            if (__exception != null) Log.Exception(__exception);
        }
    }
}
