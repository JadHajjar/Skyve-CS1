using LoadOrderMod.Util;
namespace LoadOrderMod.Patches.Startup {
    using HarmonyLib;
    using static KianCommons.ReflectionHelpers;

    [HarmonyPatch(typeof(MainMenu), "Refresh")]
    public static class MainMenuRefresh {
        static bool initial_ = true;
        public static void Postfix(MainMenu __instance) {
            LogCalled();
            if (initial_) {
                initial_ = false;
                __instance.gameObject.AddComponent<AutoLoad>();
            }
        }
    }
}
