namespace KianCommons.UI.MessageBox.WhatsNew {
    using ColossalFramework.UI;

    /// <summary>
    /// Message box with separate paragraphs and/or lists of dot points, with 'close' and 'dont show again' buttons.
    /// </summary>
    public class DontShowAgainMessageBox : ListMessageBox
    {
        // Don't Show Again button.
        private UIButton dsaButton;

        // Number of buttons for this panel (for layout).
        protected override int NumButtons => 2;

        // Accessor.
        public UIButton DSAButton => dsaButton;

        /// <summary>
        /// Adds buttons to the message box.
        /// </summary>
        public override void AddButtons()
        {
            base.AddButtons();

            // Add don't show again button.
            dsaButton = AddButton(2, NumButtons, Close);
            dsaButton.text = "Don't Show Again";
        }
    }
}