using ColossalFramework.Plugins;

using HarmonyLib;

using KianCommons.Patches;

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

using static KianCommons.ReflectionHelpers;

using PluginInfo = ColossalFramework.Plugins.PluginManager.PluginInfo;

namespace SkyveMod.Patches.ContentManager;
[HarmonyPatch(typeof(PluginInfo), "set_isEnabled")]
public static class PluginInfo_SetIsEnable
{
	static readonly MethodInfo mTriggerEventPluginsStateChanged =
		GetMethod(typeof(PluginManager), "TriggerEventPluginsStateChanged");
	static readonly MethodInfo mTrigger =
		new Action(PluginsStateChangedInvoker.Trigger).Method;

	public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
	{
		var codes = instructions.ToCodeList();
		var index = codes.FindIndex(_c => _c.Calls(mTriggerEventPluginsStateChanged));

		// call PluginManager.get_instance() + labels. // replace this to inherit labels
		// call ContentManagerPanel.EnablePackageEvents
		codes.ReplaceInstruction(index - 1, new CodeInstruction(OpCodes.Call, mTrigger));
		codes.RemoveAt(index);

		return codes;
	}
}
