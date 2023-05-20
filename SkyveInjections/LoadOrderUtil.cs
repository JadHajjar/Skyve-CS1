using KianCommons;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using static ColossalFramework.Plugins.PluginManager;
using SkyveShared;
using ColossalFramework;
using ColossalFramework.IO;
using ColossalFramework.Plugins;
using ColossalFramework.PlatformServices;

namespace SkyveInjections.Util {
    internal static class LoadOrderUtil {
        static SkyveConfig config_;
        public static string LocalLOMData => Path.Combine(DataLocation.localApplicationData, "Skyve");
        public static SkyveConfig Config {
            get {
                try {
                    return config_ ??=
                        SkyveConfig.Deserialize()
                        ?? new SkyveConfig();
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
