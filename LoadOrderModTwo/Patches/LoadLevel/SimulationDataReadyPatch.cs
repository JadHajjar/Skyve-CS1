using HarmonyLib;
using KianCommons;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Reflection.Emit;
using static KianCommons.Patches.TranspilerUtils;
using static LoadingManager;
using static LoadOrderMod.Util.LSMUtil;

namespace LoadOrderMod.Patches {
    [HarmonyPatch]
    public static class SimulationDataReadyPatch {
        static IEnumerable<MethodBase> TargetMethods() {
            yield return GetCoroutineMoveNext(typeof(LoadingManager), "LoadLevelCoroutine");
            foreach(var tLevelLoader in GetTypeFromLSMS("LevelLoader")){
                yield return GetCoroutineMoveNext(tLevelLoader, "LoadLevelCoroutine");
            }
        }

        static Stopwatch sw = new Stopwatch();
        static Stopwatch sw_total = new Stopwatch();

        public static void SpecialInvoke(SimulationDataReadyHandler e) {
            if (e is null) {
                Log.Exception(new ArgumentNullException("e"));
                return;
            }

            Log.Info($"invoking LoadingManager.m_SimulationDataReady()", copyToGameLog: true);
            Log.Flush();
            sw_total.Reset();
            sw_total.Start();

            foreach (SimulationDataReadyHandler @delegate in e.GetInvocationList()) {
                string name = @delegate.Method.FullDescription();
                Log.Info($"invoking " + name, copyToGameLog: true);
                sw.Reset();
                sw.Start();
                try {
                    @delegate();
                    sw.Stop();
                    Log.Info($"{name} successful! " +
                        $"duration = {sw.ElapsedMilliseconds:#,0}ms", copyToGameLog: true);
                } catch (Exception ex) {
                    Log.Exception(ex);
                }
            }
            sw_total.Stop();
            Log.Info($"LoadingManager.m_SimulationDataReady() successful! " +
                $"total duration = {sw_total.ElapsedMilliseconds:#,0}ms", copyToGameLog: true);
            Log.Flush();
        }

        static MethodInfo mSpecialInvoke = typeof(SimulationDataReadyPatch).GetMethod(nameof(SpecialInvoke), true);
        static MethodInfo m_Invoke = typeof(SimulationDataReadyHandler).GetMethod(nameof(SimulationDataReadyHandler.Invoke), true);

        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, MethodBase original) {
            foreach (var code in instructions) {
                if (code.Calls(m_Invoke)) {
                    var m = code.operand as MethodInfo;
                    string name = m.DeclaringType.FullName + "::" + m.Name;
                    var name2 = original.DeclaringType.FullName + "::" + original.Name;
                    Log.Info($"replacing call to {name} with SpecialInvoke() in {name2}");
                    var ret = new CodeInstruction(OpCodes.Call, mSpecialInvoke);
                    MoveLabels(code, ret);
                    yield return ret;
                } else {
                    yield return code;
                }
            }
        }
    }
}
