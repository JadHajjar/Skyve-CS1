namespace LoadOrderMod.Patches.ContentManager {
    using ColossalFramework;
    using ColossalFramework.PlatformServices;
    using HarmonyLib;
    using KianCommons;
    using LoadOrderMod.UI.EntryAction;
    using LoadOrderMod.UI.EntryStatus;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    [HarmonyPatch(typeof(PackageEntry))]
    static class PackageEntryPatch {
        [HarmonyPostfix]
        [HarmonyPatch(nameof(PackageEntry.SetEntry))]
        static void SetEntry_Postfix(PackageEntry __instance, EntryData data) {
            //Log.Called($"entry: {data.publishedFileId} {data.entryName}");
            EntryStatusPanel.UpdateDownloadStatusSprite(__instance);
            EntryActionPanel.UpdateEntry(__instance);

        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(PackageEntry.Reset))]
        static void Reset_prefix(PackageEntry __instance) {
            //Log.Called(__instance.entryName);
            EntryStatusPanel.RemoveDownloadStatusSprite(__instance);
            EntryActionPanel.Remove(__instance);
        }
    }

}
