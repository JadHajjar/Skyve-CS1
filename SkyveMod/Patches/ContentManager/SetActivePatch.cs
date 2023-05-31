using ColossalFramework.Packaging;

using HarmonyLib;

using KianCommons;

using System;
using System.Collections.Generic;
using System.Reflection;

namespace SkyveMod.Patches.ContentManager;
[HarmonyPatch(typeof(EntryData), "SetActive")]
public static class SetActivePatch
{
	static readonly MethodInfo mForcedAssetStateChanged =
		new Action(PackageManager.ForceAssetStateChanged).Method;
	static readonly MethodInfo mTriger =
		new Action(AssetStateChangedInvoker.Trigger).Method;

	public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
	{
		var count = 0;
		foreach (var code in instructions)
		{
			if (code.Calls(mForcedAssetStateChanged))
			{
				count++;
				yield return new CodeInstruction(code) { operand = mTriger } // inherits labels
					.LogRet($"replaced {code} with ");
			}
			else
			{
				yield return code;
			}
		}
		Assertion.AssertEqual(count, 1, "count");
	}
}
