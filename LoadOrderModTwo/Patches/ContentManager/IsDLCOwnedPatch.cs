using HarmonyLib;

using LoadOrderMod.Settings;

using System.Linq;

namespace LoadOrderMod.Patches.ContentManager;
[HarmonyPatch(typeof(SteamHelper), nameof(SteamHelper.IsDLCOwned))]
public static class IsDLCOwnedPatch
{
	static void Postfix(SteamHelper.DLC dlc, ref bool __result)
	{
		if (__result)
		{
			__result = !ConfigUtil.Config.RemovedDLCs.Contains((uint)dlc);
		}
	}
}