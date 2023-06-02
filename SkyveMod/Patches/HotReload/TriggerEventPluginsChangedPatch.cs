using ColossalFramework.Plugins;

using HarmonyLib;

using KianCommons;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Reflection.Emit;

using static ColossalFramework.Plugins.PluginManager;
using static KianCommons.ReflectionHelpers;

namespace SkyveMod.Patches.HotReload;
/// <summary>
/// time each invocation.
/// </summary>
[HarmonyPatch(typeof(PluginManager), "TriggerEventPluginsChanged")]
public static class TriggerEventPluginsChangedPatch
{
	//static PluginManager.PluginsChangedHandler 

	static readonly MethodInfo mInvoke = GetMethod(typeof(PluginsChangedHandler), "Invoke");
	static readonly MethodInfo mVerboseInvoke = GetMethod(typeof(TriggerEventPluginsChangedPatch), nameof(VerboseInvoke));
	static void VerboseInvoke(PluginsChangedHandler handler)
	{
		LogCalled();
		var timer = Stopwatch.StartNew();
		var d = (MulticastDelegate)handler;
		ExecuteDelegates(d.GetInvocationList(), verbose: true);
		var ms = timer.ElapsedMilliseconds;
		Log.Info($"{ThisMethod} finished! total duration={ms:#,0}ms");
	}

	static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
	{
		foreach (var code in instructions)
		{
			if (code.Calls(mInvoke))
			{
				yield return new CodeInstruction(OpCodes.Call, mVerboseInvoke);
			}
			else
			{
				yield return code;
			}
		}
	}
}
