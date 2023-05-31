using HarmonyLib;

using UnityEngine;

using static KianCommons.ReflectionHelpers;

namespace SkyveMod.Patches.Startup;
[HarmonyPatch(typeof(NewsFeedPanel), "Awake")]
static class NewsFeedPanel_Awake
{
	static bool Prefix(NewsFeedPanel __instance)
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
