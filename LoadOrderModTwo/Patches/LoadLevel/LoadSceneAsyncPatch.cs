using HarmonyLib;
using KianCommons;
using System.Diagnostics;
using UnityEngine.SceneManagement;

namespace LoadOrderMod.Patches {
    [HarmonyPatch(typeof(SceneManager))]
    [HarmonyPatch("LoadSceneAsync")]
    [HarmonyPatch(new[] { typeof(string), typeof(LoadSceneMode) })]
    public static class LoadSceneAsyncPatch {
        static Stopwatch sw_total = new Stopwatch();

        public static void Prefix(string sceneName) {
            Log.Info($"SceneManager.LoadSceneAsync({sceneName})", true);
            Log.Flush();
            sw_total.Reset();
            sw_total.Start();
        }
    }
}
