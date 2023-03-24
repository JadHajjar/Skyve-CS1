namespace LoadOrderMod.Patches.LoadLevel {
    using HarmonyLib;
    using ICities;
    using KianCommons;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Reflection;
    using System.Reflection.Emit;
    using KianCommons.Patches;

    [HarmonyPatch(typeof(OptionsMainPanel), "AddUserMods")]
    public static class LogOnSettingsUIPatch {
        static Stopwatch sw = new Stopwatch();
        static Stopwatch sw_total = new Stopwatch();

        static IUserMod BeforeSettingsUI(IUserMod userMod) {
            Log.Info("calling OnSettingsUI() for " + userMod.Name);
            sw.Reset();
            sw.Start();
            return userMod;
        }
        static void AfterSettingsUI() {
            sw.Stop();
            var ms = sw.ElapsedMilliseconds;
            Log.Info($"OnSettingsUI() successful. duration = {ms:#,0}ms");
        }

        static MethodInfo mBeforeSettingsUI =
            typeof(LogOnSettingsUIPatch).GetMethod(nameof(BeforeSettingsUI), true);
        static MethodInfo mAfterSettingsUI =
            typeof(LogOnSettingsUIPatch).GetMethod(nameof(AfterSettingsUI), true);
        static MethodInfo mInvoke =
            typeof(MethodBase).GetMethod(
                nameof(MethodBase.Invoke),
                new[] { typeof(object), typeof(object[]) },
                throwOnError: true);

        static void Prefix() {
            Log.Info("OptionsMainPanel.AddUserMods() started (calls OnSettingsUI for all mods)", true);
            Log.Flush();
            sw_total.Reset();
            sw_total.Start();
        }

        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, MethodBase original) {
            try {
                List<CodeInstruction> codes = instructions.ToCodeList();
                var Call_BeforeSettingsUI = new CodeInstruction(OpCodes.Call, mBeforeSettingsUI);
                var Call_AfterSettingsUI = new CodeInstruction(OpCodes.Call, mAfterSettingsUI);

                int index = codes.Search(c => c.Calls(mInvoke));
                codes.InsertInstructions(index + 1, new[] { Call_AfterSettingsUI }); // insert after.

				// insert after instances[0]
				index = codes.Search((CodeInstruction c) => c.IsLdLoc(typeof(IUserMod), original), index, -1, true);
                codes.InsertInstructions(index+1, new[] { Call_BeforeSettingsUI }); 

                return codes;
            } catch(Exception e) {
                Log.Error(e.ToString());
                throw e;
            }
        }

        public static void Postfix() {
            sw_total.Stop();
            var ms = sw_total.ElapsedMilliseconds;
            Log.Info($"OptionsMainPanel.AddUserMods() finished. total duration = {ms:#,0}ms ", true);
            Log.Flush();
        }
    }
}
