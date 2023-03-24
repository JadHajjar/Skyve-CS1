#if DEBUG
namespace LoadOrderMod.Patches {
    using ColossalFramework;
    using ColossalFramework.Packaging;
    using ColossalFramework.UI;
    using HarmonyLib;
    using KianCommons;
    using System;
    using System.Diagnostics;
    using System.Linq;
    using static Helpers;
    using static KianCommons.ReflectionHelpers;

    public static class Helpers {
        public static string STCulled {
            get {
                var ret = Environment.StackTrace;
                try {
                    var lines = ret.SplitLines().ToList();
                    lines[1] = " at get_STCulled";
                    lines.RemoveAt(0);
                    lines.RemoveRange(lines.Count - 7, 7);
                    return lines.JoinLines();
                } catch { }
                return ret;
            }
        }
    }
    //[HarmonyPatch(typeof(CategoryContentPanel), "RequestDetailsCoroutine")]
    //public static class RequestDetailsCoroutine {
    //    public static void Prefix() => Log.Debug(STCulled);
    //}

    //[HarmonyPatch(typeof(CategoryContentPanel), "RefreshEntries")]
    //public static class RefreshEntries {
    //    public static void Prefix() => Log.Debug(STCulled);
    //    public static void Postfix() => Log.Debug(STCulled);
    //}

    //[HarmonyPatch(typeof(ContentManagerPanel), "OnEnableAll")]
    //public static class OnEnableAll {
    //    public static void Prefix() => Log.Debug(STCulled);
    //    public static void Postfix() => Log.Debug(STCulled);
    //}

    //[HarmonyPatch(typeof(ContentManagerPanel), "BindEnableDisableAll")]
    //public static class BindEnableDisableAll {
    //    public static void Prefix() => Log.Debug(STCulled);
    //    public static void Postfix() => Log.Debug(STCulled);
    //}

    #region SetActiveAllTimers
    [HarmonyPatch(typeof(CategoryContentPanel), "SetActiveAll")]
    public static class SetActiveAll {
        public static void Prefix() {
            Log.Debug(STCulled);
            SetActive.timer.Reset();
            IsDLCEnabled.timer.Reset();
            ActivateSideAssets.timer.Reset();
        }
        public static void Postfix() {
            Log.Debug(STCulled);
            {
                var ms = SetActive.timer.ElapsedMilliseconds;
                Log.Debug($"SetActive accumulative duration = {ms:#,0} :");
            }
            {
                var ms = IsDLCEnabled.timer.ElapsedMilliseconds;
                Log.Debug($"\tIsDLCEnabled accumulative duration = {ms:#,0}");
            }

            {
                var ms = ActivateSideAssets.timer.ElapsedMilliseconds;
                Log.Debug($"\tActivateSideAssets accumulative duration = {ms:#,0}");
            }
        }
    }


    [HarmonyPatch(typeof(EntryData), "SetActive")]
    public static class SetActive {
        public static Stopwatch timer = new Stopwatch();
        public static void Prefix() => timer.Start();
        public static void Postfix() => timer.Stop();
    }

    [HarmonyPatch(typeof(EntryData), "IsDLCEnabled")]
    public static class IsDLCEnabled {
        public static Stopwatch timer = new Stopwatch();
        public static void Prefix() => timer.Start();
        public static void Postfix() => timer.Stop();
    }

    [HarmonyPatch(typeof(EntryData), "ActivateSideAssets")]
    public static class ActivateSideAssets {
        public static Stopwatch timer = new Stopwatch();
        public static void Prefix() => timer.Start();
        public static void Postfix() => timer.Stop();
    }

    #endregion SetActiveAllTimers

    //[HarmonyPatch(typeof(EntryData), "SetActive")]
    //public static class SetActive2 {
    //    public static void Prefix() => Log.Debug(STCulled);
    //    public static void Postfix() => Log.Debug(STCulled);
    //}

    //[HarmonyPatch(typeof(PackageEntry), "OnCheckedChanged")]
    //public static class PackageEntry_OnCheckedChanged {
    //    public static void Prefix() => Log.Debug(STCulled);
    //    public static void Postfix() => Log.Debug(STCulled);
    //}

    #region event handlers
    //[HarmonyPatch(typeof(ToolsMenu), "Refresh")]
    //public static class ToolsMenu_Refresh {
    //    public static void Prefix() => Log.Debug(STCulled);
    //    public static void Postfix() => Log.Debug(STCulled);
    //}
    //[HarmonyPatch(typeof(MainMenu), "Refresh")]
    //public static class MainMenu_Refresh {
    //    public static void Prefix() => Log.Debug(STCulled);
    //    public static void Postfix() => Log.Debug(STCulled);
    //}

    //[HarmonyPatch(typeof(ContentManagerPanel), "RefreshCategory")]
    //public static class ContentManagerPanel_RefreshCategory {
    //    static string Getname(ContentManagerPanel instance, UIComponent container) {
    //        if (instance != Singleton<ContentManagerPanel>.instance)
    //            Log.Error("could not find ContentManagerPanel instance");
    //        foreach (var f in instance.GetType().GetFields(ALL)) {
    //            if (f.Name.EndsWith("Container")) {
    //                if (container == f.GetValue(instance))
    //                    return f.Name;
    //            }
    //        }
    //        return "name not found";
    //    }
    //    static Stopwatch timer = new Stopwatch();
    //    public static void Prefix(ContentManagerPanel __instance, UIComponent container) {
    //        timer.Reset(); timer.Start();
    //        Log.Debug($"{Getname(__instance, container)} : " + STCulled);
    //    }
    //    public static void Postfix(ContentManagerPanel __instance, UIComponent container) {
    //        var ms = timer.ElapsedMilliseconds;
    //        Log.Debug($"{Getname(__instance, container)} - duration={ms:#,0}ms \n" + STCulled);
    //    }
    //}

    //[HarmonyPatch(typeof(CategoryContentPanel), "Refresh")]
    //public static class CategoryContentPanel_Refresh {


    //    public static void Prefix() => Log.Debug(STCulled);
    //    public static void Postfix() => Log.Debug(STCulled);
    //}

    [HarmonyPatch(typeof(ContentManagerPanel), "Refresh")]
    public static class ContentManagerPanel_Refresh {
        static Stopwatch timer = new Stopwatch();
        public static void Prefix() {
            timer.Reset(); timer.Start();
            Log.Debug(STCulled);
        }
        public static void Postfix() {
            var ms = timer.ElapsedMilliseconds;
            Log.Debug($"duration={ms:#,0}ms\n" + STCulled);
        }
    }
    [HarmonyPatch(typeof(OptionsMainPanel), "RefreshPlugins")]
    public static class OptionsMainPanel_RefreshPlugins {
        static Stopwatch timer = new Stopwatch();
        public static void Prefix() {
            timer.Reset(); timer.Start();
            Log.Debug(STCulled);
        }
        public static void Postfix() {
            var ms = timer.ElapsedMilliseconds;
            Log.Debug($"duration={ms:#,0}ms\n" + STCulled);
        }
    }

    //[HarmonyPatch(typeof(ContentManagerPanel), "RefreshPlugins")]
    //public static class ContentManagerPanel_RefreshPlugins {
    //    public static void Prefix() => Log.Debug(STCulled);
    //    public static void Postfix() => Log.Debug(STCulled);
    //}
    //[HarmonyPatch(typeof(ContentManagerPanel), "OnWorkshopSubscriptionChanged")]
    //public static class ContentManagerPanel_OnWorkshopSubscriptionChanged {
    //    public static void Prefix() => Log.Debug(STCulled);
    //    public static void Postfix() => Log.Debug(STCulled);
    //}

    //[HarmonyPatch(typeof(ColossalFramework.Globalization.LocaleManager), "LoadPackagesLocale")]
    //public static class LoadPackagesLocale {
    //    // 180ms
    //    public static void Prefix() => Log.Debug(STCulled);
    //    public static void Postfix() => Log.Debug(STCulled);
    //}
    //[HarmonyPatch(typeof(OptionsGraphicsPanel), "RefreshColorCorrectionLUTs")]
    //public static class RefreshColorCorrectionLUTs {
    //    // 6ms
    //    public static void Prefix() => Log.Debug(STCulled);
    //    public static void Postfix() => Log.Debug(STCulled);
    //}


    #endregion
}
#endif