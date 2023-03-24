using ColossalFramework.Packaging;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;
using KianCommons;
using static KianCommons.ReflectionHelpers;
using static LoadOrderMod.Util.LSMUtil;
using System.Linq;

namespace LoadOrderMod.Patches {
    [HarmonyPatch]
    public static class PrintLoadInfoPatch {
        static bool Prepare() => TargetMethods().Any();
        static IEnumerable<MethodBase> TargetMethods() {
            yield return GetMethod(typeof(LoadingManager), "LoadLevelCoroutine");
        }
        public static void Prefix([HarmonyArgument("asset")]Package.Asset savegame, string playerScene, string uiScene, SimulationMetaData ngs, bool forceEnvironmentReload) {
            if (ngs == null)
                return;
            Log.Info("LoadLevelCoroutine called with arguments: " +
                $"savegame={savegame.name} playerScene={playerScene} uiScene={uiScene} forceEnvironmentReload={forceEnvironmentReload}\n" +
                $"ngs=[ " +
                $"map:{ngs.m_CityName} " +
                $"theme:{(ngs.m_MapThemeMetaData?.name).ToSTR()} " +
                $"environment:{ngs.m_environment.ToSTR()} " +
                $"LHT:{ngs.m_invertTraffic} " +
                $"disableAchievements:{ngs.m_disableAchievements} " +
                $"updateMode={ngs.m_updateMode}\n" +
                $"filePath:{savegame.package?.packagePath}) " +
                "]\n" + Environment.StackTrace);
        }
    }

    [HarmonyPatch]
    static class PrintLoadInfoPatch2 {
        static bool Prepare() => TargetMethods().Any();
        static IEnumerable<MethodBase> TargetMethods() {
            foreach (var tLevelLoader in GetTypeFromLSMS("LevelLoader")) {
                yield return GetMethod(tLevelLoader, "LoadLevelCoroutine");
            }
        }

        static void Prefix(Package.Asset savegame, string playerScene, string uiScene, SimulationMetaData ngs, bool forceEnvironmentReload) {
            PrintLoadInfoPatch.Prefix(savegame, playerScene, uiScene, ngs, forceEnvironmentReload);
        }
    }

}
