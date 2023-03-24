using HarmonyLib;
using System;
using System.Reflection.Emit;
using System.Collections.Generic;
using KianCommons;
using static KianCommons.Patches.TranspilerUtils;
using ICities;
using System.Reflection;
using System.Diagnostics;
using LoadOrderMod.Settings;

namespace LoadOrderMod.Patches._LoadingWrapper {
    [HarmonyPatch(typeof(LoadingWrapper))]
    [HarmonyPatch("OnLoadingExtensionsCreated")]
    public static class OnCreatedPatch {
        static Stopwatch sw = new Stopwatch();
        static Stopwatch sw_total = new Stopwatch();

        public static ILoadingExtension BeforeOnCreated(ILoadingExtension loadingExtension) {
            if (ConfigUtil.Config.LogPerModOnCreatedTimes) {
                Log.Info($"calling {loadingExtension}.OnCreated()", copyToGameLog: false);
                sw.Reset();
                sw.Start();
            }
            return loadingExtension;
        }
        public static void AfterOnCreated() {
            if (ConfigUtil.Config.LogPerModOnCreatedTimes) {
                sw.Stop();
                var ms = sw.ElapsedMilliseconds;
                Log.Info($"OnCreated() successful. duration = {ms:#,0}ms", copyToGameLog: false);
            }
        }

        static MethodInfo mBeforeOnCreated_ = typeof(OnCreatedPatch).GetMethod(nameof(BeforeOnCreated))
            ?? throw new Exception("mBeforeOnCreated_ is null");
        static MethodInfo mAfterOnCreated_ = typeof(OnCreatedPatch).GetMethod(nameof(AfterOnCreated))
            ?? throw new Exception("mAfterOnCreated_ is null");
        static MethodInfo mOnCreated_ = typeof(ILoadingExtension).GetMethod(nameof(ILoadingExtension.OnCreated))
            ?? throw new Exception("mAfterOnCreated_ is null");
        static MethodInfo mGetItem_ = GetMethod(typeof(List<ILoadingExtension>), "get_Item");

        public static void Prefix(){
            Log.Info("LoadingWrapper.OnLoadingExtensionsCreated() started", true);
            sw_total.Reset();
            sw_total.Start();
        }

        public static IEnumerable<CodeInstruction> Transpiler(ILGenerator il, IEnumerable<CodeInstruction> instructions) {
            try {
                List<CodeInstruction> codes = instructions.ToCodeList();
                var Call_BeforeOnCreated = new CodeInstruction(OpCodes.Call, mBeforeOnCreated_);
                var Call_AfterOnCreated = new CodeInstruction(OpCodes.Call, mAfterOnCreated_);

                int index = codes.Search(c => c.Calls(mOnCreated_));
                InsertInstructions(codes, new[] { Call_AfterOnCreated }, index + 1, moveLabels:false); // insert after.

                index = codes.Search(c => c.Calls(mGetItem_)); 
                InsertInstructions(codes, new[] { Call_BeforeOnCreated }, index + 1, moveLabels: false); // insert after.

                return codes;
            }
            catch (Exception e) {
                Log.Error(e.ToString());
                throw e;
            }
        }

        public static void Postfix() {
            sw_total.Stop();
            var ms = sw_total.ElapsedMilliseconds;
            Log.Info($"LoadingWrapper.OnLoadingExtensionsCreated() finished. total duration = {ms:#,0}ms ", true);
        }
    }
}
