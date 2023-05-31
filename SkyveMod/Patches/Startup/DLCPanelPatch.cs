using HarmonyLib;

using UnityEngine;

using static KianCommons.ReflectionHelpers;

namespace SkyveMod.Patches.Startup;
[HarmonyPatch(typeof(DLCPanel), "Start")] // does not have awake
static class DLCPanel_Awake
{
	static bool Prefix(DLCPanel __instance)
	{
		LogCalled();
		if (!Settings.ConfigUtil.Config.HidePanels)
		{
			return true;
		}

		GameObject.DestroyImmediate(__instance.gameObject);
		return false;
	}
}
