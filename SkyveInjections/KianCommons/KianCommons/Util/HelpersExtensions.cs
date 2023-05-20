namespace KianCommons {
    using ICities;
    using System;
    using UnityEngine;
    using UnityEngine.SceneManagement;
    using System.Linq;
    using ColossalFramework.Threading;

    [Obsolete]
    internal static class HelpersExtensions {
        internal static bool InSimulationThread() =>
            System.Threading.Thread.CurrentThread == SimulationManager.instance.m_simulationThread;

        [Obsolete]
        internal static bool VERBOSE {
            get => Log.VERBOSE;
            set => Log.VERBOSE = value;
        }

        internal static bool[] ALL_BOOL = new bool[] { false, true };

        internal static AppMode currentMode => SimulationManager.instance.m_ManagersWrapper.loading.currentMode;
        internal static bool CheckGameMode(AppMode mode) {
            try {
                if (currentMode == mode)
                    return true;
            } catch { }
            return false;
        }


        /// <summary>
        /// determines if simulation is inside game/editor. useful to detect hot-reload.
        /// </summary>
        internal static bool InGameOrEditor => !InStartup;

        internal static bool IsActive => InGameOrEditor;

        internal static bool InStartup => Helpers.InStartupMenu;


        /// <summary>
        /// checks if game is loaded in and user is playing a city. (returns false early in the loading process)
        /// </summary>
        internal static bool InGame => CheckGameMode(AppMode.Game);

        /// <summary>
        /// checks if game is loaded in asset editor mod. (returns false early in the loading process)
        /// </summary>
        internal static bool InAssetEditor => CheckGameMode(AppMode.AssetEditor);

        internal static bool ShiftIsPressed => Helpers.ShiftIsPressed;

        internal static bool ControlIsPressed => Helpers.ControlIsPressed;

        internal static bool AltIsPressed => Helpers.AltIsPressed;
    }

    internal static class Helpers {
        internal static void Swap<T>(ref T a, ref T b) {
            var t = a;
            a = b;
            b = t;
        }

        internal static string[] StartupScenes = new[] { "IntroScreen", "IntroScreen2", "Startup", "MainMenu" };
        internal static bool InStartupMenu => StartupScenes.Contains(SceneManager.GetActiveScene().name);

        internal static bool ShiftIsPressed => Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);

        internal static bool ControlIsPressed => Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);

        internal static bool AltIsPressed => Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt);

        internal static bool InSimulationThread() =>
            System.Threading.Thread.CurrentThread == SimulationManager.instance.m_simulationThread;
        internal static bool InMainThread() =>
            Dispatcher.currentSafe == ThreadHelper.dispatcher;
    }


}
