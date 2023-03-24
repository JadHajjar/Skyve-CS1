using HarmonyLib;
using KianCommons;
using KianCommons.Patches;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using static KianCommons.ReflectionHelpers;
using ColossalFramework.Plugins;
using ColossalFramework.Packaging;

namespace LoadOrderMod.Patches.ContentManager {
    using PluginInfo = PluginManager.PluginInfo;

    [HarmonyPatch(typeof(PluginInfo), "set_isEnabled")]
    public static class PluginInfo_SetIsEnable {
        static MethodInfo mTriggerEventPluginsStateChanged =
            GetMethod(typeof(PluginManager), "TriggerEventPluginsStateChanged");
        static MethodInfo mTrigger =
            new Action(PluginsStateChangedInvoker.Trigger).Method;

        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
            var codes = instructions.ToCodeList();
            int index = codes.FindIndex(_c => _c.Calls(mTriggerEventPluginsStateChanged));

            // call PluginManager.get_instance() + labels. // replace this to inherit labels
            // call ContentManagerPanel.EnablePackageEvents
            codes.ReplaceInstruction(index-1, new CodeInstruction(OpCodes.Call, mTrigger));
            codes.RemoveAt(index);

            return codes;
        }
    }
}
