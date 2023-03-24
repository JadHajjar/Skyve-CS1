namespace LoadOrderMod.Patches.HotReload {
    using HarmonyLib;
    using System;
    using static KianCommons.ReflectionHelpers;
    using ColossalFramework.Plugins;
    using System.Collections.Generic;
    using System.Reflection.Emit;
    using static ColossalFramework.Plugins.PluginManager;
    using System.Reflection;
    using System.Diagnostics;
    using KianCommons;

    /// <summary>
    /// time each invocation.
    /// </summary>
    [HarmonyPatch(typeof(PluginManager), "TriggerEventPluginsChanged")]
    public static class TriggerEventPluginsChangedPatch {
        //static PluginManager.PluginsChangedHandler 

        static MethodInfo mInvoke = GetMethod(typeof(PluginsChangedHandler), "Invoke");
        static MethodInfo mVerboseInvoke = GetMethod(typeof(TriggerEventPluginsChangedPatch), nameof(VerboseInvoke));
        static void VerboseInvoke(PluginsChangedHandler handler) {
            LogCalled();
            var timer = Stopwatch.StartNew();
            var d = (MulticastDelegate)handler;
            ExecuteDelegates(d.GetInvocationList(), verbose: true);
            var ms = timer.ElapsedMilliseconds;
            Log.Info($"{ThisMethod} finished! total duration={ms:#,0}ms");
        }

        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
            foreach (var code in instructions) {
                if (code.Calls(mInvoke))
                    yield return new CodeInstruction(OpCodes.Call, mVerboseInvoke);
                else
                    yield return code;
            }
        }
    }
}
