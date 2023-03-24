namespace LoadOrderMod.Patches._LoadingWrapper {
    using HarmonyLib;
    using System;
    using System.Reflection.Emit;
    using System.Collections.Generic;
    using KianCommons;
    using static KianCommons.Patches.TranspilerUtils;
    using ICities;
    using System.Reflection;
    using System.Diagnostics;
    using ColossalFramework.Plugins;
    using System.Linq;
    using LoadOrderMod.Settings;
    using LoadOrderMod.UI;
	using LoadOrderMod.Util;
	using System.Threading;

	[HarmonyPatch(typeof(LoadingWrapper))]
    [HarmonyPatch("OnLevelLoaded")]
    public static class OnLevelLoadedpatch {
        static Stopwatch sw = new Stopwatch();
        static Stopwatch sw_total = new Stopwatch();

        public static ILoadingExtension BeforeOnLevelLoaded(ILoadingExtension loadingExtension) {
            Log.Info($"calling {loadingExtension}.OnLevelLoaded()", copyToGameLog: true);
#if DEBUG
            var asm = loadingExtension.GetType().Assembly;
            var p = PluginManager.instance.GetPluginsInfo().Single(_p => _p.ContainsAssembly(asm));
            if(p.HasLoadOrder())
                Log.Debug($"loadOrder={p.GetLoadOrder()}", true);
#endif

            sw.Reset();
            sw.Start();
            return loadingExtension;
        }
        public static void AfterOnLevelLoaded() {
            sw.Stop();
            var ms = sw.ElapsedMilliseconds;
            Log.Info($"OnLevelLoaded() successful. duration = {ms:#,0}ms", copyToGameLog: true);
          
            new Thread(new ThreadStart(SpriteDumper.Dump)) { IsBackground = true }.Start();
		}

		static MethodInfo mBeforeOnLevelLoaded_ =
            typeof(OnLevelLoadedpatch).GetMethod(nameof(BeforeOnLevelLoaded), throwOnError: true);
        static MethodInfo mAfterOnLevelLoaded_ =
            typeof(OnLevelLoadedpatch).GetMethod(nameof(AfterOnLevelLoaded), throwOnError: true);
        static MethodInfo mOnLevelLoaded_ =
            typeof(ILoadingExtension).GetMethod(nameof(ILoadingExtension.OnLevelLoaded), throwOnError: true);

        [HarmonyPriority(Priority.First)]
        [HarmonyPrefix]
        public static void Prefix0() {
            Log.Info("LoadingWrapper.OnLevelLoaded() started (before other prefixes)", true);
            Log.Flush();
        }


        [HarmonyPriority(Priority.Last)]
        public static void Prefix() {
            Log.Info("LoadingWrapper.OnLevelLoaded() started (after other prefixes)", true);
            Log.Flush();
            sw_total.Reset();
            sw_total.Start();
        }

        [HarmonyPriority(Priority.First)]
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
            try {
                List<CodeInstruction> codes = instructions.ToCodeList();
                var Call_BeforeOnLevelLoaded = new CodeInstruction(OpCodes.Call, mBeforeOnLevelLoaded_);
                var Call_AfterOnLevelLoaded = new CodeInstruction(OpCodes.Call, mAfterOnLevelLoaded_);

                int index = codes.Search(c => c.opcode == OpCodes.Ldarg_1); //ldarg mode
                InsertInstructions(codes, new[] { Call_BeforeOnLevelLoaded }, index);

                int index2 = codes.Search(c => c.Calls(mOnLevelLoaded_), startIndex: index);
                InsertInstructions(codes, new[] { Call_AfterOnLevelLoaded }, index2+1, moveLabels:false); // insert after.

                return codes;
            }
            catch (Exception e) {
                Log.Error(e.ToString());
                throw e;
            }
        }

        [HarmonyPriority(Priority.First)]
        public static void Postfix() {
            sw_total.Stop();
            var ms = sw_total.ElapsedMilliseconds;
            MonoStatus.Ensure();
            Log.Info($"LoadingWrapper.OnLevelLoaded() finished (before other postfixes). total duration = {ms:#,0}ms ", true);
            Log.Flush();
        }

        [HarmonyPriority(Priority.Last)]
        static Exception Finalizer(Exception __exception) {
            if(__exception != null) {
                Log.Exception(__exception, "a mod caused error in postfix of OnLevelLoaded");
            } else {
                Log.Info("LoadingWrapper.OnLevelLoaded() finalized (after postfixes)", true);
            }
            return null;
        }
    }
}
