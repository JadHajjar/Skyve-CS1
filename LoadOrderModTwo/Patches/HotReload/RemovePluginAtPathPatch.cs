namespace LoadOrderMod.Patches.HotReload {
    using HarmonyLib;
    using ColossalFramework.Plugins;
    using System.Collections.Generic;
    using static ColossalFramework.Plugins.PluginManager;
    using System;
    using KianCommons;
    using static KianCommons.ReflectionHelpers;
    using LoadOrderMod.Util;

    [HarmonyPatch(typeof(PluginManager), "RemovePluginAtPath")]
    public static class RemovePluginAtPathPatch {
        /// <summary>
        /// pluginInfo.name that is being removed. the name of the containing directory.
        /// (different than IUserMod.Name).
        /// </summary>        
        public static string name;

        static void Prefix(string path, Dictionary<string, PluginInfo> ___m_Plugins) {
            try {
                LogCalled(path);
                if (___m_Plugins.TryGetValue(path, out var p) && p.isEnabled) {
                    name = p.GetUserModName();
                }
            } catch (Exception ex) {
                Log.Exception(ex);
            }
        }

        static void Finalizer(Exception __exception) {
            LogCalled();
            name = null;
            if (__exception != null) Log.Exception(__exception);
        }

    }
}
