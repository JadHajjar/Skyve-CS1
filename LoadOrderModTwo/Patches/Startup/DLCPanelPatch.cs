namespace LoadOrderMod.Patches.Startup {
    using HarmonyLib;
    using static KianCommons.ReflectionHelpers;
    using UnityEngine;

    [HarmonyPatch(typeof(DLCPanelNew), "Start")] // does not have awake
    static class DLCPanelNew_Awake {
        static bool Prefix(DLCPanelNew __instance) {
            LogCalled();
            if (!Settings.ConfigUtil.Config.TurnOffSteamPanels)
                return true;
            GameObject.DestroyImmediate(__instance.gameObject);
            return false;
        }
    }
}
