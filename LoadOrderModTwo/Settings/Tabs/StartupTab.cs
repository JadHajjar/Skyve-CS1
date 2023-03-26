namespace LoadOrderMod.Settings.Tabs {
    extern alias Injections;
    using ColossalFramework.PlatformServices;
    using ColossalFramework.UI;
    using KianCommons;
    using System;
    using UnityEngine;
    using SteamUtilities = Injections.LoadOrderInjections.SteamUtilities;
    using LoadOrderMod.UI;
    using KianCommons.UI;
    using LoadOrderShared;

    static class StartupTab {
        static LoadOrderConfig Config => ConfigUtil.Config;

        public static void Make(ExtUITabstrip tabStrip) {
            UIHelper panelHelper = tabStrip.AddTabPage("Startup");
            panelHelper.AddLabel("restart required to take effect.", textColor: Color.yellow);
            panelHelper.AddSpace(10);

            panelHelper.AddButton("Reset load orders", OnResetLoadOrdersClicked);

            panelHelper.AddCheckbox(
                "remove ad panels",
                ConfigUtil.Config.TurnOffSteamPanels,
                val => {
                    ConfigUtil.Config.TurnOffSteamPanels = val;
                    ConfigUtil.SaveConfig();
                });

            var c2 = panelHelper.AddCheckbox(
                "Improve content manager",
                ConfigUtil.Config.FastContentManager,
                val => {
                    ConfigUtil.Config.FastContentManager = val;
                    ConfigUtil.SaveConfig();
                }) as UIComponent;
            c2.tooltip = "faster content manager";

            var c4 = panelHelper.AddCheckbox(
                "Cache asset details for the tool.",
                ConfigUtil.Config.UGCCache,
                val => {
                    ConfigUtil.Config.UGCCache = val;
                    ConfigUtil.SaveConfig();
                }) as UICheckBox;

            var c5 = panelHelper.AddCheckbox(
                "Hide steam download errors (Ignorance is bliss!)",
                ConfigUtil.Config.IgnoranceIsBliss,
                val => {
                    ConfigUtil.Config.IgnoranceIsBliss = val;
                    ConfigUtil.SaveConfig();
                }) as UICheckBox;
        }

        static void OnResetLoadOrdersClicked() {
            Log.Debug("Reset Load Orders pressed");
            ConfigUtil.SaveConfig();
        }
    }
}
