namespace KianCommons.UI.MessageBox.WhatsNew {
    using System;
    using System.Collections.Generic;
    using UnityEngine;
    using ColossalFramework.UI;

    /// <summary>
    /// 'What's new' message box.
    /// </summary>
    public class WhatsNewMessageBox : DontShowAgainMessageBox {
        protected override float MaxContentHeight => 600;

        /// <summary>
        /// Sets the 'what's new' messages to display.
        /// </summary>
        /// <param name="lastNotifiedVersion">Last notified version (version messages equal to or earlier than this will be minimized</param>
        /// <param name="messages">Version update messages to display, in order (newest versions first), with a list of items (as translation keys) for each version</param>
        public void SetMessages(string modName, Version lastNotifiedVersion, WhatsNewEntry[] messages) {
            // Iterate through each provided version and add it to the messagebox.
            foreach (WhatsNewEntry message in messages) {
                VersionMessage versionMessage = ScrollableContent.AddUIComponent<VersionMessage>();
                versionMessage.width = ContentWidth;
                versionMessage.SetText(modName, message);
                // Add spacer below.
                AddSpacer();

                Log.Debug($"message.version <= lastNotifiedVersion : {message.version} <= {lastNotifiedVersion} : {message.version <= lastNotifiedVersion}");
                // Hide version messages that have already been notified (always showing versions with headers).
                if (message.version < lastNotifiedVersion) {
                    versionMessage.IsCollapsed = true;
                }
            }
        }


        /// <summary>
        /// Update message for a given version.
        /// </summary>
        public class VersionMessage : UIPanel {
            // Components.
            private readonly UIButton minimizeButton;
            public List<ListItem> listItems;

            // Version title.
            private string versionTitle;

            // Visibility state.
            private bool isExpanded;


            /// <summary>
            /// Sets message expanded/collapsed state.
            /// </summary>
            public bool IsCollapsed { set { isExpanded = value; ToggleExpanded(); } }


            /// <summary>
            /// Constructor - performs basic setup.
            /// </summary>
            public VersionMessage() {
                // Init list before we do anything else.
                listItems = new List<ListItem>();

                // Basic setup.
                autoLayout = true;
                autoLayoutDirection = LayoutDirection.Vertical;
                autoFitChildrenVertically = true;
                autoLayoutPadding = new RectOffset(0, 0, 2, 2);

                // Add minimize button (which will also be the version label).
                minimizeButton = AddUIComponent<UIButton>();
                minimizeButton.height = 20f;
                minimizeButton.horizontalAlignment = UIHorizontalAlignment.Left;
                minimizeButton.color = Color.white;
                minimizeButton.textHorizontalAlignment = UIHorizontalAlignment.Left;

                // Toggle visible (minimized) state when clicked.
                minimizeButton.eventClick += (component, eventParam) => ToggleExpanded();
            }


            /// <summary>
            /// Sets version message text.
            /// </summary>
            /// <param name="message">What's new message to display</param>
            public void SetText(string modeName, WhatsNewEntry message) {
                // Set version header and message text.
                if (message.version == WhatsNewEntry.PriorVersion) {
                    versionTitle = "highlights from prior versions";
                } else {
                    versionTitle = modeName + " " + message.version.ToString();
                }

                // Add message elements as separate list items.
                for (int i = 0; i < message.messages.Length; ++i) {
                    try {
                        // Message text is either a translation key or direct text, depending on the messageKeys setting.
                        ListItem newMessageLabel = AddUIComponent<ListItem>();
                        listItems.Add(newMessageLabel);
                        newMessageLabel.Text = message.messages[i];

                        // Make sure initial width is set properly.
                        newMessageLabel.width = width;
                    } catch (Exception ex) {
                        ex.Log();
                    }
                }

                // Always start maximized.
                isExpanded = true;

                // Set state indicator.
                UpdateState();
            }


            /// <summary>
            /// Handles size changed events, for e.g. when visibility changes.  Called by game as needed.
            /// </summary>
            protected override void OnSizeChanged() {
                base.OnSizeChanged();

                // Set width of button and label to match new width of list item (whose width has been set by the MessageBox).
                if (minimizeButton != null) {
                    minimizeButton.width = width;
                };

                // Set width of each item label.
                if (listItems != null) {
                    foreach (ListItem listItem in listItems) {
                        listItem.width = width;
                    }
                };
            }


            /// <summary>
            /// Toggles expanded/collapsed state of the update messages.
            /// </summary>
            private void ToggleExpanded() {
                // Toggle state and update state indicator.
                isExpanded = !isExpanded;
                UpdateState();

                // Show/hide each list item according to state.
                foreach (ListItem listItem in listItems) {
                    listItem.isVisible = isExpanded;
                }
            }


            /// <summary>
            /// Sets expaned/collapsed state indicator.
            /// </summary>
            private void UpdateState() => minimizeButton.text = (isExpanded ? "▼ " : "► ") + versionTitle;
        }
    }
}