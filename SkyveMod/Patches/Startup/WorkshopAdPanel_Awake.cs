namespace SkyveMod.Patches.Startup {
    using HarmonyLib;
    using static KianCommons.ReflectionHelpers;
    using UnityEngine;

    [HarmonyPatch(typeof(WorkshopAdPanel), "Awake")]
    static class WorkshopAdPanel_Awake {
        static bool Prefix(WorkshopAdPanel __instance ) {
            LogCalled();
            if (!Settings.ConfigUtil.Config.HidePanels)
                return true;
            GameObject.DestroyImmediate(__instance.gameObject);
            return false;
        }
    }
}
