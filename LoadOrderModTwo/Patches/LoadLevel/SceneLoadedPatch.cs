using HarmonyLib;
using KianCommons;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using static LoadOrderMod.Util.LSMUtil;
using static KianCommons.Patches.TranspilerUtils;

namespace LoadOrderMod.Patches {
    [HarmonyPatch]
    public static class SceneLoadedPatch {
        static IEnumerable<MethodBase> TargetMethods() {
            yield return GetCoroutineMoveNext(typeof(LoadingManager), "LoadLevelCoroutine");
            foreach(var tLevelLoader in GetTypeFromLSMS("LevelLoader")) {
                yield return GetCoroutineMoveNext(tLevelLoader, "LoadLevelCoroutine");
            }
        }

        public static void LogSceneEnded(string uiScene) {
            Log.Info($"Loading Scene `{uiScene}` completed");
            Log.Flush();
        }

        static MethodInfo mLogSceneEnded =
            typeof(SceneLoadedPatch).GetMethod(nameof(LogSceneEnded), true);
        static MethodInfo mEndLoading = 
            typeof(LoadingProfiler).
            GetMethod(nameof(LoadingProfiler.EndLoading));

        public static IEnumerable<CodeInstruction> Transpiler(
            IEnumerable<CodeInstruction> instructions, MethodBase original) {
            try {
                var codes = instructions.ToCodeList();

                var loadThis = new CodeInstruction(OpCodes.Ldarg_0);
                int index = codes.FindLastIndex(
                    c => c.opcode == OpCodes.Ldfld && (c.operand as FieldInfo).Name == "uiScene");
                var loadFieldUIScene = codes[index].Clone();
                index = codes.Search(c => c.Calls(mEndLoading), startIndex: index);
                var call_LogSceneEnded = new CodeInstruction(OpCodes.Call, mLogSceneEnded);

                InsertInstructions(
                    codes,
                    new[] {
                        loadThis,
                        loadFieldUIScene,
                        call_LogSceneEnded,
                        },
                    index + 1);//insert after LoadingProfiler.EndLoading
                return codes;

            } catch (Exception e) {
                Log.Error(e.ToString());
                throw e;
            }
        }
    }
}

