namespace LoadOrderMod.Patches.ContentManager {
    using HarmonyLib;
    using KianCommons;
    using LoadOrderMod.Settings;
    using System.Linq;
    using System;
    using System.Collections.Generic;
    using System.Reflection;

    [HarmonyPatch(typeof(SteamHelper), nameof(SteamHelper.IsDLCOwned))]
    public static class IsDLCOwnedPatch {
        static SteamHelper.DLC[] ExcludedDLCs;

        static IEnumerable<SteamHelper.DLC> DLCsStartingWith(string name) {
            foreach (SteamHelper.DLC dlc in Enum.GetValues(typeof(SteamHelper.DLC))) {
                if (dlc.ToString().StartsWith(name, StringComparison.OrdinalIgnoreCase)) {
                    yield return dlc;
                }
            }
        }

        static void Prepare(MethodBase original) {
            Log.Called(original);
            if (ExcludedDLCs != null) return;
            var dlcs = new List<SteamHelper.DLC>();
            foreach(string item in ConfigUtil.Config.ExcludedDLCs) {
                if (item == "MusicDLCs") {
                    var radioDLCs = DLCsStartingWith("RadioStation");
                    dlcs.AddRange(radioDLCs);
                } else if (item == "Football") {
                    var footballDLCs = DLCsStartingWith("Football");
                    dlcs.AddRange(footballDLCs);
                } else {
                    try {
                        dlcs.Add((SteamHelper.DLC)Enum.Parse(typeof(SteamHelper.DLC), item));
                    } catch (Exception ex) {
                        Log.Warning($"could not find DLC {item}.\n" + ex);
                    }
                }
            }
            ExcludedDLCs = dlcs.ToArray();
            Log.Info($"ExcludedDLCs={ExcludedDLCs.ToSTR()}");
        }


        static void Postfix(SteamHelper.DLC dlc, ref bool __result) {
            if (__result) {
                __result = __result && !ExcludedDLCs.Contains(dlc);
            }
        }
    }
}
