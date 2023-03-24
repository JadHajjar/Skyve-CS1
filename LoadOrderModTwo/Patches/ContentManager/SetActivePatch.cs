using ColossalFramework.Packaging;
using HarmonyLib;
using KianCommons;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Reflection.Emit;
using static KianCommons.ReflectionHelpers;
using System.Linq;

namespace LoadOrderMod.Patches.ContentManager {
    [HarmonyPatch(typeof(EntryData), "SetActive")]
    public static class SetActivePatch {
        static MethodInfo mForcedAssetStateChanged =
            new Action(PackageManager.ForceAssetStateChanged).Method;
        static MethodInfo mTriger =
            new Action(AssetStateChangedInvoker.Trigger).Method;

        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
            int count = 0;
            foreach (var code in instructions) {
                if (code.Calls(mForcedAssetStateChanged)) {
                    count++;
                    yield return new CodeInstruction(code) { operand = mTriger } // inherits labels
                        .LogRet($"replaced {code} with ");
                } else {
                    yield return code;
                }
            }
            Assertion.AssertEqual(count, 1, "count");
        }
    }
}
