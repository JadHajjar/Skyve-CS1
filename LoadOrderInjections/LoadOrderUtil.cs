using KianCommons;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using static ColossalFramework.Plugins.PluginManager;
using LoadOrderShared;
using ColossalFramework;
using ColossalFramework.IO;
using ColossalFramework.Plugins;
using ColossalFramework.PlatformServices;

namespace LoadOrderInjections.Util {
    internal static class LoadOrderUtil {
        static LoadOrderConfig config_;
        public static string LocalLOMData => Path.Combine(DataLocation.localApplicationData, "LoadOrderTwo");
        public static LoadOrderConfig Config {
            get {
                try {
                    return config_ ??=
                        LoadOrderConfig.Deserialize()
                        ?? new LoadOrderConfig();
                } catch (Exception ex) {
                    Log.Exception(ex);
                    return null;
                }
            }
        }

        internal static int GetLoadOrder(this PluginInfo p) => 1000;

        public static bool HasArg(string arg) =>
            Environment.GetCommandLineArgs().Any(_arg => _arg == arg);

      //  internal const string LoadOrderSettingsFile = "LoadOrder";

        internal static string DllName(this PluginInfo p) => p.userModInstance?.GetType()?.Assembly?.GetName()?.Name;
        internal static bool IsHarmonyMod(this PluginInfo p) => p.name == "2040656402" || p.name == "CitiesHarmony";
    }
}
