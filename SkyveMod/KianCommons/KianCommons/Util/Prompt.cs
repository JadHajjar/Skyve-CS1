namespace KianCommons {
    using ColossalFramework;
    using ColossalFramework.UI;
    using System;
    using UnityEngine.SceneManagement;
    /// <summary>
    /// Use this class to display small but annoying dialog prompts to the user.
    ///
    /// TODO: At some point add more panels, such as:
    /// * ConfirmPanel
    /// * ExitConfirmPanel
    /// * MessageBoxPanel
    /// * TutorialPanel
    /// * TutorialAdvisorPanel
    /// </summary>
    public static class Prompt {

        /// <summary>
        /// Display a warning prompt in the centre of the screen.
        /// </summary>
        /// 
        /// <param name="title">Dialog title.</param>
        /// <param name="message">Dialog body text.</param>
        public static void Warning(string title, string message) {
            Log.Warning(message);
            ExceptionPanel(title, message, false);
        }

        /// <summary>
        /// Display an error prompt in the centre of the screen.
        /// </summary>
        /// <param name="title">Dialog title.</param>
        /// <param name="message">Dialog body text.</param>
        public static void Error(string title, string message) {
            Log.Error(message);
            ExceptionPanel(title, message, true);
        }

        /// <summary>
        /// Display an exception message in the center of the screen, optionally
        /// styled as an error.
        /// </summary>
        /// 
        /// <param name="title">Dialog title.</param>
        /// <param name="message">Dialog body text.</param>
        /// <param name="isError">If <c>true</c>, the dialog is styled as an error.</param>
        internal static void ExceptionPanel(string title, string message, bool isError) {
            Action prompt = () => {
                UIView.library
                    .ShowModal<ExceptionPanel>("ExceptionPanel")
                    .SetMessage(title, message, isError);
            };

            try {
                if (SceneManager.GetActiveScene().name == "Game") {
                    Singleton<SimulationManager>.instance.m_ThreadingWrapper.QueueMainThread(prompt);
                } else {
                    prompt();
                }
            } catch (Exception ex) {
                ex.Log();
            }
        }
    }
}
