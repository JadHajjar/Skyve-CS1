namespace SkyveMod.Settings.Tabs {
    extern alias Injections;
    using ColossalFramework.PlatformServices;
    using ColossalFramework.UI;
    using KianCommons;
    using System;
    using UnityEngine;
    using SteamUtilities = Injections.SkyveInjections.SteamUtilities;
    using SkyveMod.UI;
    using KianCommons.UI;
    using SkyveShared;

    static class StartupTab {
        static SkyveConfig Config => ConfigUtil.Config;

        public static void Make(ExtUITabstrip tabStrip) {
            UIHelper panelHelper = tabStrip.AddTabPage("Startup");

            panelHelper.AddCheckbox(
                "Hide steam panels",
                ConfigUtil.Config.HidePanels,
                val => {
                    ConfigUtil.Config.HidePanels = val;
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

			panelHelper.AddSpace(10);
			panelHelper.AddLabel("Changing those settings requires a restart to take effect.", textColor: Color.yellow);
		}
    }
}
