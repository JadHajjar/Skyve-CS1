namespace KianCommons.UI.MessageBox.WhatsNew {
    using System;

    /// <summary>
    /// Version message struct.
    /// </summary>
    public struct WhatsNewEntry {
        public static Version PriorVersion = new Version(0, 0, 0, 1);
        public Version version;
        public string[] messages;
        public override string ToString() =>
            $"WhatsNewEntry{{version={version}, messages={messages.ToSTR()}}}";
    }
}