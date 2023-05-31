using HarmonyLib;

using ICities;

using KianCommons;
using KianCommons.Patches;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Reflection.Emit;

namespace SkyveMod.Patches.LoadLevel;
[HarmonyPatch(typeof(OptionsMainPanel), "AddUserMods")]
public static class LogOnSettingsUIPatch
{
	static readonly Stopwatch sw = new Stopwatch();
	static readonly Stopwatch sw_total = new Stopwatch();

	static IUserMod BeforeSettingsUI(IUserMod userMod)
	{
		Log.Info("calling OnSettingsUI() for " + userMod.Name);
		sw.Reset();
		sw.Start();
		return userMod;
	}
	static void AfterSettingsUI()
	{
		sw.Stop();
		var ms = sw.ElapsedMilliseconds;
		Log.Info($"OnSettingsUI() successful. duration = {ms:#,0}ms");
	}

	static readonly MethodInfo mBeforeSettingsUI =
		typeof(LogOnSettingsUIPatch).GetMethod(nameof(BeforeSettingsUI), true);
	static readonly MethodInfo mAfterSettingsUI =
		typeof(LogOnSettingsUIPatch).GetMethod(nameof(AfterSettingsUI), true);
	static readonly MethodInfo mInvoke =
		typeof(MethodBase).GetMethod(
			nameof(MethodBase.Invoke),
			new[] { typeof(object), typeof(object[]) },
			throwOnError: true);

	static void Prefix()
	{
		Log.Info("OptionsMainPanel.AddUserMods() started (calls OnSettingsUI for all mods)", true);
		Log.Flush();
		sw_total.Reset();
		sw_total.Start();
	}

	static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, MethodBase original)
	{
		try
		{
			var codes = instructions.ToCodeList();
			var Call_BeforeSettingsUI = new CodeInstruction(OpCodes.Call, mBeforeSettingsUI);
			var Call_AfterSettingsUI = new CodeInstruction(OpCodes.Call, mAfterSettingsUI);

			var index = codes.Search(c => c.Calls(mInvoke));
			codes.InsertInstructions(index + 1, new[] { Call_AfterSettingsUI }); // insert after.

			// insert after instances[0]
			index = codes.Search((CodeInstruction c) => c.IsLdLoc(typeof(IUserMod), original), index, -1, true);
			codes.InsertInstructions(index + 1, new[] { Call_BeforeSettingsUI });

			return codes;
		}
		catch (Exception e)
		{
			Log.Error(e.ToString());
			throw e;
		}
	}

	public static void Postfix()
	{
		sw_total.Stop();
		var ms = sw_total.ElapsedMilliseconds;
		Log.Info($"OptionsMainPanel.AddUserMods() finished. total duration = {ms:#,0}ms ", true);
		Log.Flush();
	}
}
