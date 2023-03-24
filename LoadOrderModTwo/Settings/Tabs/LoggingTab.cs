namespace LoadOrderMod.Settings.Tabs {
    using ColossalFramework.UI;
    using KianCommons;
    using LoadOrderShared;
    using System;
    static class LoggingTab {
        static LoadOrderConfig Config => ConfigUtil.Config;

        static UIComponent logAssetLoadingTimesToggle_;

        public static void Make(ExtUITabstrip tabStrip) {
            UIHelper panelHelper = tabStrip.AddTabPage("Logging");
            panelHelper.AddCheckbox(
                "Log asset loading times",
                ConfigUtil.Config.LogAssetLoadingTimes,
                val => {
                    ConfigUtil.Config.LogAssetLoadingTimes = val;
                    ConfigUtil.SaveConfig();
                    logAssetLoadingTimesToggle_.isEnabled = val;
                });

            logAssetLoadingTimesToggle_ = panelHelper.AddCheckbox(
                "Per Mod",
                ConfigUtil.Config.LogPerModAssetLoadingTimes,
                val => {
                    ConfigUtil.Config.LogPerModAssetLoadingTimes = val;
                    ConfigUtil.SaveConfig();
                }) as UIComponent;
            Settings.Indent(logAssetLoadingTimesToggle_);

            logAssetLoadingTimesToggle_ = panelHelper.AddCheckbox(
                "Log per mod OnCreated() times",
                ConfigUtil.Config.LogPerModOnCreatedTimes,
                val => {
                    ConfigUtil.Config.LogPerModOnCreatedTimes = val;
                    ConfigUtil.SaveConfig();
                }) as UIComponent;
        }
    }
}
