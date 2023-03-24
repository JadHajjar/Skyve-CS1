namespace LoadOrderMod.Settings {
    extern alias Injections;
    using ColossalFramework.PlatformServices;
    using ColossalFramework.UI;
    using KianCommons;
    using System;
    using UnityEngine;
    using SteamUtilities = Injections.LoadOrderInjections.SteamUtilities;
    using LoadOrderMod.UI;
    using LoadOrderMod.Settings.Tabs;
    using KianCommons.UI;
    using LoadOrderShared;

    public static class Settings {
        static LoadOrderConfig Config => ConfigUtil.Config;

        public static void OnSettingsUI(UIHelper helper) {
            try {
                Log.Debug(Environment.StackTrace);
                if (!Helpers.InStartupMenu) {
                    helper.AddLabel("Only available in startup menu");
                }

                ExtUITabstrip tabStrip = ExtUITabstrip.Create(helper);
                SubscriptionsTab.Make(tabStrip);
                StartupTab.Make(tabStrip);
                LoggingTab.Make(tabStrip);
                DebugTab.Make(tabStrip);
            } catch (Exception ex) {
                Log.Exception(ex);
            }
        }

        public static void Indent(UIComponent c, int n = 1) {

            if (c.Find<UILabel>("Label") is UILabel label)
                label.padding = new RectOffset(22 * n, 0, 0, 0);

            if (c.Find<UISprite>("Unchecked") is UISprite check)
                check.relativePosition += new Vector3(22 * n, 0);
        }

        public static void Pairup(UICheckBox checkbox, UIButton button) {
            Assertion.NotNull(checkbox, "checkbox");
            Assertion.NotNull(button, "button");
            var container = checkbox.parent as UIComponent;
            Assertion.NotNull(container, "container");
            var panel = container.AddUIComponent<UIPanel>();
            panel.width = container.width;
            panel.height = button.height;

            checkbox.AlignTo(panel, UIAlignAnchor.TopLeft);
            checkbox.relativePosition += new Vector3(0, 10);
            button.AlignTo(panel, UIAlignAnchor.TopRight);
            button.relativePosition -= new Vector3(button.width, 0);
        }
    }
}
