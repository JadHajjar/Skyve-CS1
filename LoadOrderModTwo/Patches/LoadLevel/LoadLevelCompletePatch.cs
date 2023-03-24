namespace LoadOrderMod.Patches {
    using HarmonyLib;
    using ICities;
    using KianCommons;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Reflection;
    using System.Reflection.Emit;
    using static KianCommons.Patches.TranspilerUtils;
    using static LoadingManager;

    [HarmonyPatch]
    public static class LoadLevelCompletePatch {
        static MethodBase TargetMethod() {
            return GetCoroutineMoveNext(typeof(LoadingManager), "LoadLevelComplete");
        }

        static Stopwatch sw = new Stopwatch();
        static Stopwatch sw_total = new Stopwatch();
        static SimulationManager.UpdateMode UpdateMode => SimulationManager.instance.m_metaData.m_updateMode;

        public static void SpecialInvoke(LevelLoadedHandler e, SimulationManager.UpdateMode mode) {
            if(e is null) {
                Log.Exception(new ArgumentNullException("e"));
                return;
            }

            Log.Info($"invoking LoadingManager.m_levelLoaded()", copyToGameLog: true);
            Log.Flush();
            sw_total.Reset();
            sw_total.Start();

            foreach(LevelLoadedHandler @delegate in e.GetInvocationList()) {
                string name = @delegate.Method.FullDescription();
                Log.Info($"invoking " + name, copyToGameLog: true);
                sw.Reset();
                sw.Start();
                try {
                    @delegate(mode);
                    sw.Stop();
                    Log.Info($"{name} successful! " +
                        $"duration = {sw.ElapsedMilliseconds:#,0}ms", copyToGameLog: true);
                } catch(Exception ex) {
                    Log.Exception(ex);
                }
            }
            sw_total.Stop();
            Log.Info($"LoadingManager.m_levelLoaded() successful! " +
                $"total duration = {sw_total.ElapsedMilliseconds:#,0}ms", copyToGameLog: true);
            Log.Flush();
        }


        static MethodInfo mSpecialInvoke =
            typeof(LoadLevelCompletePatch).GetMethod(nameof(SpecialInvoke), true);

        static MethodInfo mInvoke =
            typeof(LevelLoadedHandler).GetMethod(nameof(LevelLoadedHandler.Invoke), true);

        [HarmonyPriority(Priority.First)]
        public static IEnumerable<CodeInstruction> Transpiler(
            MethodBase original, IEnumerable<CodeInstruction> instructions) {
            foreach(var code in instructions) {
                if(code.Calls(mInvoke)) {
                    var m = code.operand as MethodInfo;
                    string name = m.DeclaringType.FullName + "::" + m.Name;
                    var name2 = original.DeclaringType.FullName + "::" + original.Name;
                    Log.Info(
                        $"replacing call to {name} with SpecialInvoke() in {name2}");
                    var replacement = new CodeInstruction(OpCodes.Call, mSpecialInvoke);
                    MoveLabels(code, replacement); // there are no labels but just in case.
                    yield return replacement;

                } else {
                    yield return code;
                }
            }
        }

        [HarmonyPriority(Priority.Last)]
        static Exception Finalizer(Exception __exception) {
            LoadingManager.instance.QueueLoadingAction(OnLoadingFinished());
            if(__exception != null) {
                Log.Exception(__exception);
            } else {
                Log.Info("LoadLevelComplete() finalized.", true);
            }
            return null;
        }

        private static IEnumerator OnLoadingFinished() {
            Log.Info("LoadOrderMod:GAME LOADING HAS FINISHED", true);
            if(Util.AutoLoad.ParseCommandLine("terminate|terminateOnLoad", out _)) {
                Process.GetCurrentProcess().Kill();
            }
            yield return 0;
        }
    }
}
