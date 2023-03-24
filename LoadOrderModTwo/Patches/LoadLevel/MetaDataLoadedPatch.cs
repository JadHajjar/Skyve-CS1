using HarmonyLib;
using KianCommons;
using System.Diagnostics;

namespace LoadOrderMod.Patches {
    [HarmonyPatch(typeof(LoadingManager))]
    [HarmonyPatch("MetaDataLoaded")]
    public static class MetaDataLoadedPatch {
        static Stopwatch sw_total = new Stopwatch();

        public static void Prefix() {
            Log.Info("LoadingManager.MetaDataLoaded() started", true);
            Log.Flush();
            sw_total.Reset();
            sw_total.Start();
        }

        public static void Postfix() {
            sw_total.Stop();
            float ms = sw_total.ElapsedMilliseconds;
            Log.Info($"LoadingManager.MetaDataLoaded() finished. total duration = {ms:#,0}ms ", true);
            Log.Flush();
        }
    }
}
