namespace SkyveMod.Patches.Startup {
    using HarmonyLib;
    using static KianCommons.ReflectionHelpers;
    using UnityEngine;

    [HarmonyPatch(typeof(DLCPanel), "Start")] // does not have awake
    static class DLCPanel_Awake {
        static bool Prefix(DLCPanel __instance) {
            LogCalled();
            if (!Settings.ConfigUtil.Config.TurnOffSteamPanels)
                return true;
            GameObject.DestroyImmediate(__instance.gameObject);
            return false;
        }
    }
}
