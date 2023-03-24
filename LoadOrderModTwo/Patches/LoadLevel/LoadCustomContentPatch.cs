using HarmonyLib;
using KianCommons;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using static KianCommons.Patches.TranspilerUtils;
using static LoadOrderMod.Util.LSMUtil;
namespace LoadOrderMod.Patches {
    [HarmonyPatch]
    public static class LoadCustomContentPatch {
        static IEnumerable<MethodBase> TargetMethods() {
            yield return GetCoroutineMoveNext(typeof(LoadingManager), "LoadCustomContent");
            foreach(var tAssetLoader in GetTypeFromLSMS("AssetLoader")) {
                yield return GetCoroutineMoveNext(tAssetLoader, "LoadCustomContent");
            }
        }
        
        static Stopwatch sw_total = new Stopwatch();
        static Stopwatch sw = new Stopwatch();
        static int counter = 0;

        public static void Prefix() {
            if (counter == 0) {
                Log.Info($"LoadCustomContent() started ...", true);
                Log.Info($"LoadCustomContent.MoveNext() first loop. counter={counter}", false);
                Log.Flush();
                sw_total.Reset();
                sw_total.Start();
            } else {
                Log.Info($"LoadCustomContent.MoveNext() continues. counter={counter}", false);
            }
            sw.Reset();
            sw.Start();
            counter++;
        }

        public static void Postfix(IEnumerator<object> __instance, bool __result) {
            float ms = sw.ElapsedMilliseconds;
            if (__result == false) {
                float ms_total = sw_total.ElapsedMilliseconds;
                Log.Info($"LoadCustomContent.MoveNext() braked. duration = {ms:#,0}ms ", false);
                Log.Info($"LoadCustomContent() finished! total duration = {ms_total:#,0}ms", true);
                Log.Flush();
            } else {
                object current = __instance.Current;
                Log.Info($"LoadCustomContent.MoveNext() yielded {current ?? "<null>"}. duration = {ms:#,0}ms ", false);
            }
        }
    }
}
