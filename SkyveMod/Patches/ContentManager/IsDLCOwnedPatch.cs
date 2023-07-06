using HarmonyLib;

using SkyveMod.Settings;

using SkyveShared;

using System.Linq;

namespace SkyveMod.Patches.ContentManager;
[HarmonyPatch(typeof(SteamHelper), nameof(SteamHelper.IsDLCOwned))]
public static class IsDLCOwnedPatch
{
	static DlcConfig DlcConfig = DlcConfig.Deserialize();
	static void Postfix(SteamHelper.DLC dlc, ref bool __result)
	{
		if (__result)
		{
			__result = !DlcConfig.RemovedDLCs.Contains((uint)dlc);
		}
	}
}