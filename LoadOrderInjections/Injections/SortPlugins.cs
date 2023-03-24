using ColossalFramework.PlatformServices;
using KianCommons;
using LoadOrderInjections.Util;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using PluginInfo = ColossalFramework.Plugins.PluginManager.PluginInfo;
using System;

namespace LoadOrderInjections.Injections {
    /// <summary>
    /// sort plugins and excludes excluded plugins
    /// </summary>
    public static class SortPlugins {
        /// <summary>
        /// 1st: harmony mod 
        /// 2nd: harmony 2 
        /// 3rd: harmony 1
        /// 4th: not harmony
        /// </summary>
        /// <returns>
        /// harmony mod : 0
        /// harmony 2   : 1
        /// harmony 1   : 2
        /// not harmony : 3
        /// </returns>
        public static int GetHarmonyOrder(PluginInfo p) {
            if (p.IsHarmonyMod())
                return 0;
            foreach (var asm in p.GetAssemblies()) {
                var name = asm.GetName().Name;
                if (name == "CitiesHarmony.API")
                    return 1;
                if (name == "0Harmony")
                    return 2;
            }
            return 3;
        }

        /// <summary>
        /// if parent folder is a number, it counts as WS
        /// </summary>
        static bool GetWSID(string name, out uint id) {
            bool isWS = uint.TryParse(name, out id) &&
                id != 0 &&
                id != PublishedFileId.invalid.AsUInt64;
            if (!isWS) id = 0;
            return isWS;
        }


        static bool IsOrderlessHarmony(PluginInfo p) => !p.HasLoadOrder() && p.IsHarmonyMod();

        public static int HarmonyComparison(PluginInfo p1, PluginInfo p2) {
            int ret;
            // order less harmony comes first
            ret = -IsOrderlessHarmony(p1).CompareTo(IsOrderlessHarmony(p2));
            if (ret != 0) return ret;

            ret = p1.GetLoadOrder().CompareTo(p2.GetLoadOrder());
            if (ret != 0) return ret; // if both 1000(default) then go to next line

            ret = GetHarmonyOrder(p1).CompareTo(GetHarmonyOrder(p2));
            if (ret != 0) return ret;

            // WS mod comes before local mod.
            ret = -GetWSID(p1.name, out uint id1).CompareTo(GetWSID(p2.name, out uint id2));
            if (ret != 0) return ret;


            ret = id1.CompareTo(id2); // compare WS mods
            if (ret != 0) return ret;

            ret = p1.name.CompareTo(p2.name); // compare local mods
            return ret;
        }


        public static void Sort(Dictionary<string, PluginInfo> plugins) {
            try {
                var list = plugins.
                    Where(pair => !CMPatchHelpers.IsDirectoryExcluded(pair.Key)).
                    ToList();

                Log.Info("Sorting assemblies ...", true);
                list.Sort((p1, p2) => HarmonyComparison(p1.Value, p2.Value));

                plugins.Clear();
                foreach (var pair in list)
                    plugins.Add(pair.Key, pair.Value);

                ReplaceAssembies.Init(plugins.Values.ToArray());
                Log.Info("\n=========================== plugins.Values: =======================", false);
                foreach (var p in plugins.Values) {
                    var dllFiles = Directory.GetFiles(p.modPath, "*.dll", SearchOption.AllDirectories);
                    // exclude assets.
                    if(!dllFiles.IsNullorEmpty()) {
                        string dlls = string.Join(", ", dllFiles);
                        Log.Debug(
                            $"loadOrder={p.GetLoadOrder()} path={p.modPath} dlls={{{dlls}}}"
                            , false);
                    }
                }
                Log.Info("\n=========================== END plugins.Values =====================\n", false);
            }catch(Exception ex) {
                Log.Exception(ex);
            }
        }
    }
}