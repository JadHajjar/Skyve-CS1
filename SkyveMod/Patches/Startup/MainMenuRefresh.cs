using HarmonyLib;

using SkyveMod.Util;

using static KianCommons.ReflectionHelpers;

namespace SkyveMod.Patches.Startup;
[HarmonyPatch(typeof(MainMenu), "Refresh")]
public static class MainMenuRefresh
{
	static bool initial_ = true;
	public static void Postfix(MainMenu __instance)
	{
		LogCalled();
		if (initial_)
		{
			initial_ = false;
			__instance.gameObject.AddComponent<AutoLoad>();
		}
	}
}
