namespace SkyveMod.Patches.HotReload {
    using HarmonyLib;
    using System;
    using ColossalFramework.Plugins;
    using KianCommons;
    using SkyveMod.Util;
    using System.Linq;
    using SkyveMod.UI;

    /// <summary>
    /// time each invocation.
    /// </summary>
    [HarmonyPatch(typeof(OptionsMainPanel), "RefreshPlugins")]
    public static class OptionsMainPanel_RefreshPluginsPatch {

        static bool Prefix(OptionsMainPanel __instance) {
            try {
                if (RemovePluginAtPathPatch.name != null) {
                    __instance.DropCategory(RemovePluginAtPathPatch.name);
                    MonoStatus.Instance?.ModUnloaded();
                    return false;
                } else if (LoadPluginAtPathPatch.dirName != null) {
                    var p = PluginManager.instance.GetPluginsInfo().FirstOrDefault(
                        item => item.isEnabled && item.name == LoadPluginAtPathPatch.dirName);
                    if (p != null)
                        __instance.AddCategory(p);
                    MonoStatus.Instance?.ModLoaded();
                    return false;
                } else {
                    return true; //proceed as normal.
                }
            } catch (Exception ex) {
                Log.Exception(ex);
                return true;
            }
        }
    }
}
