using HarmonyLib;
using KianCommons;
using System.Diagnostics;

namespace LoadOrderMod.Patches {
    [HarmonyPatch(typeof(LoadingManager))]
    [HarmonyPatch("PreLoadLevel")]
    public static class PreLoadLevelPatch {
        static Stopwatch sw_total = new Stopwatch();

        public static void Prefix() {
            Log.Info("LoadingManager.PreLoadLevel() started", true);
            Log.Flush();
            sw_total.Reset();
            sw_total.Start();
            LOMAssetDataExtension.Init();
        }

        public static void Postfix() {
            sw_total.Stop();
            var ms = sw_total.ElapsedMilliseconds;
            Log.Info($"LoadingManager.PreLoadLevel() finished. total duration = {ms:#,0}ms ", true);
            Log.Flush();
        }
    }
}
