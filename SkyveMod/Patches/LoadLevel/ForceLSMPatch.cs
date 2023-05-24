using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using KianCommons.Patches;
using KianCommons;
using static KianCommons.ReflectionHelpers;
using UnityEngine;

namespace LoadOrderMod.Patches {
    [HarmonyPatch]
    public static class ForceLSMPatch {
        public static bool ForceLSM;
        //static FieldInfo fForceLSM = GetField(typeof(ForceLSMPatch), nameof(ForceLSM));
        static MethodInfo mGetKey => GetMethod(
            type:typeof(Input),
            name:nameof(Input.GetKey),
            bindingFlags: ALL,
            types:new[] { typeof(KeyCode)},
            throwOnError:true);
        static MethodInfo mActivateLSM = GetMethod(typeof(ForceLSMPatch), nameof(ActivateLSM));

        public static bool ActivateLSM(bool value0) {
            return value0 || ForceLSM;
        }


        static IEnumerable<MethodBase> TargetMethods() {
            var tLevelLoader = Type.GetType("LoadingScreenMod.LevelLoader, LoadingScreenMod", throwOnError: false);
            if (tLevelLoader != null) {
                yield return GetMethod(tLevelLoader, "LoadLevel");
            }
        }
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
            bool patched = false; // activated is the first bool variable.
            foreach (var code in instructions) {
                yield return code;
                if ((!patched) && code.Calls(mGetKey)) {
                    // Input.GetKey(KeyCode.LeftControl)
                    // add this
                    // | fForceLSM
                    yield return new CodeInstruction(OpCodes.Call, mActivateLSM);
                    patched = true;
                }
            }
            if (!patched)
                Log.Error("did not found 'Input.GetKey(KeyCode.LeftControl)'");
        }
    }
}
