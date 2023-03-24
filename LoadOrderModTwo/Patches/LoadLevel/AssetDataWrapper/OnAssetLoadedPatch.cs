namespace LoadOrderMod.Patches._AssetDataWrapper {
    using HarmonyLib;
    using ICities;
    using KianCommons;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Reflection;
    using System.Reflection.Emit;
    using static KianCommons.Patches.TranspilerUtils;
    using System;
    using LoadOrderShared;

    [HarmonyPatch(typeof(AssetDataWrapper))]
    [HarmonyPatch(nameof(AssetDataWrapper.OnAssetLoaded))]
    public static class OnAssetLoadedPatch {
        static Stopwatch sw = new Stopwatch();
        static Stopwatch sw_total = new Stopwatch();
        static LoadOrderConfig Config =>
            LoadOrderMod.Settings.ConfigUtil.Config;

        public static IAssetDataExtension BeforeOnAssetLoaded(IAssetDataExtension extension) {
            if(Config.LogAssetLoadingTimes &&
                Config.LogPerModAssetLoadingTimes) {
                Assertion.Assert(extension is IAssetDataExtension);
                Log.Info($"calling {extension}.OnAssetLoaded()", copyToGameLog: false);
                sw.Reset();
                sw.Start();
            }
            return extension;
        }
        public static void AfterOnAssetLoaded() {
            if(Config.LogAssetLoadingTimes &&
                Config.LogPerModAssetLoadingTimes) {
                sw.Stop();
                var ms = sw.ElapsedMilliseconds;
                Log.Info($"OnAssetLoaded() successful! duration = {ms:#,0}ms", copyToGameLog: false);
            }
        }

        static MethodInfo mBeforeOnAssetLoaded_ =
            GetMethod(typeof(OnAssetLoadedPatch), nameof(BeforeOnAssetLoaded));
        static MethodInfo mAfterOnAssetLoaded_ =
            GetMethod(typeof(OnAssetLoadedPatch), nameof(AfterOnAssetLoaded));
        static MethodInfo mOnAssetLoaded_ =
            GetMethod(typeof(IAssetDataExtension), nameof(IAssetDataExtension.OnAssetLoaded));
        static MethodInfo mGetItem =
            GetMethod(typeof(List<IAssetDataExtension>), "get_Item");

        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
            try {
                List<CodeInstruction> codes = instructions.ToCodeList();
                var Call_BeforeOnAssetLoaded = new CodeInstruction(OpCodes.Call, mBeforeOnAssetLoaded_);
                var Call_AfterOnAssetLoaded = new CodeInstruction(OpCodes.Call, mAfterOnAssetLoaded_);

                // insert call after IAssetDataExtension is loaded into the stack.
                int index = codes.Search(_c => _c.Calls(mGetItem));
                InsertInstructions(codes, new[] { Call_BeforeOnAssetLoaded }, index + 1);

                int index2 = codes.Search(c=> c.Calls(mOnAssetLoaded_), startIndex:index);
                InsertInstructions(codes, new[] { Call_AfterOnAssetLoaded }, index2 + 1, moveLabels: false); // insert after.

                return codes;
            } catch (Exception ex) {
                Log.Exception(ex);
                throw ex;
            }
        }
        public static void Prefix(string name) {
            if(!Config.LogAssetLoadingTimes)return;
            Log.Info("AssetDataWrapper.OnAssetLoaded() started for " + name, false);
            sw_total.Reset();
            sw_total.Start();
        }
        public static void Postfix() {
            if(!Config.LogAssetLoadingTimes)return;
            sw_total.Stop();
            var ms = sw_total.ElapsedMilliseconds;
            Log.Info($"AssetDataWrapper.OnAssetLoaded() finished. total asset duration = {ms:#,0}ms", false);
        }
    }
}
