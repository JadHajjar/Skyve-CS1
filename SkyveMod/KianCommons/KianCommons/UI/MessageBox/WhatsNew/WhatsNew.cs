namespace KianCommons.UI.MessageBox.WhatsNew {
    using System;
    using ColossalFramework;
    using KianCommons.UI.MessageBox;
    using KianCommons;

    /// <summary>
    /// "What's new" message box.  Based on macsergey's code in Intersection Marking Tool (Node Markup) mod.
    /// </summary>
    internal abstract class WhatsNew {
        public static Version Version = typeof(WhatsNew).VersionOf();
        public abstract SavedString SavedVersion { get; }
        public abstract WhatsNewEntry[] Messages{ get; }
        public abstract string ModName { get; }


        /// <summary>
        /// 'Don't show again' button action.
        /// </summary>
        /// <returns>True (always)</returns>
        internal void DontShowAgain() {
            Log.Called();
            // Save current version to settings file.
            Log.Debug("current Version=" + Version.ToString());
            SavedVersion.value = Version.ToString(3);
            Log.Debug($"{SavedVersion.name} set to " + SavedVersion.value);
        }

        internal void Reset() {
            SavedVersion.value = "0.0.0";
        }

        /// <summary>
        /// Check if there's been an update since the last notification, and if so, show the update.
        /// </summary>
        internal void ShowWhatsNew() {
            // Get last notified version and current mod version.
            Log.Debug($"{SavedVersion.name}: exists={SavedVersion.exists} value={SavedVersion.value}");
            var lastNotifiedVersion = new Version(SavedVersion.value);

            // Don't show notification if we're already up to (or ahead of) the first what's new message.
            if (lastNotifiedVersion < Messages[0].version) {
                Log.Info("displaying what's new message", true);
                WhatsNewMessageBox messageBox = MessageBoxBase.ShowModal<WhatsNewMessageBox>();
                messageBox.Title = ModName + " " + Version.ToString(3); ;
                messageBox.DSAButton.eventClicked += (component, clickEvent) => DontShowAgain();
                messageBox.SetMessages(ModName, lastNotifiedVersion, Messages);
            }
        }

        internal void Display() {
            // Get last notified version and current mod version.
            var lastNotifiedVersion = new Version(SavedVersion.value);
            WhatsNewMessageBox messageBox = MessageBoxBase.ShowModal<WhatsNewMessageBox>();
            messageBox.Title = ModName + " " + Version.ToString(3); ;
            messageBox.DSAButton.eventClicked += (component, clickEvent) => DontShowAgain();
            messageBox.SetMessages(ModName, lastNotifiedVersion, Messages);
        }
    }

    internal abstract class WhatsNew<T> : WhatsNew
        where T : WhatsNew, new() {
        public static T Instance { get; } = new T();

        static ToolController ToolController => ToolsModifierControl.toolController;
        public void Regsiter() {
            ToolController.eventEditPrefabChanged -= ToolController_eventEditPrefabChanged;
            ToolController.eventEditPrefabChanged += ToolController_eventEditPrefabChanged;
        }

        private void ToolController_eventEditPrefabChanged(PrefabInfo info) {
            if (ToolController.m_editPrefabInfo is NetInfo) {
                ToolController.eventEditPrefabChanged -= ToolController_eventEditPrefabChanged;
                ShowWhatsNew();
            }
        }

        internal void AddSettings(UIHelper helper) {
            helper.AddButton("Display What's new", Display);
        }
    }
}

