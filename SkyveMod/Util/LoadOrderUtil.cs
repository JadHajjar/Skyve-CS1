using ColossalFramework;
using ColossalFramework.Packaging;
using KianCommons;
using SkyveShared;
using System;
using System.Linq;
using UnityEngine;
using static ColossalFramework.Plugins.PluginManager;
using static KianCommons.ReflectionHelpers;

namespace SkyveMod.Util {
    internal static class LoadOrderUtil {
        static SkyveConfig Config =>
            Settings.ConfigUtil.Config;

        internal const ulong WSId = 2448824112ul;

        public static void ApplyGameLoggingImprovements() {
            Application.SetStackTraceLogType(LogType.Log, StackTraceLogType.None);
            Application.SetStackTraceLogType(LogType.Warning, StackTraceLogType.None);
            Application.SetStackTraceLogType(LogType.Error, StackTraceLogType.ScriptOnly);
            Application.SetStackTraceLogType(LogType.Exception, StackTraceLogType.ScriptOnly);
            Debug.Log("************************** Removed logging stacktrace bloat **************************");
        }

        public static void TurnOffSteamPanels() {
            if(!Config.TurnOffSteamPanels) return;
            SetFieldValue<WorkshopAdPanel>("dontInitialize", true);
            Log.Info("Turning off steam panels", true);
            var news = GameObject.FindObjectOfType<NewsFeedPanel>();
            var ad = GameObject.FindObjectOfType<WorkshopAdPanel>();
            var dlc = GameObject.FindObjectOfType<DLCPanelNew>();
            GameObject.Destroy(news?.gameObject);
            GameObject.Destroy(ad?.gameObject);
            GameObject.Destroy(dlc?.gameObject);
        }

        public static void ResetIsEnabledForAssets() {
            if (string.IsNullOrEmpty(PackageManager.assetStateSettingsFile))
                return;
            var assets = PackageManager.FilterAssets(new[] {
                UserAssetType.CustomAssetMetaData,
                UserAssetType.MapThemeMetaData,
                UserAssetType.ColorCorrection,
                UserAssetType.DistrictStyleMetaData,
                UserAssetType.SaveGameMetaData,
            }).ToArray();
            foreach(var asset in assets) {
                bool enabledDefault = PackageManager.GetEnabledDefault(asset);
                if (enabledDefault) {
                    ResetEnabled(asset);

                    // reset side assets.
                    if (asset.Instantiate() is CustomAssetMetaData customAssetMetaData) {
                        foreach (Package.Asset sideAsset in asset.package.FilterAssets(new [] {UserAssetType.CustomAssetMetaData})) {
                            CustomAssetMetaData customAssetMetaData2 = sideAsset.Instantiate<CustomAssetMetaData>();
                            if (customAssetMetaData2 != null &&
                                (customAssetMetaData2.type == CustomAssetMetaData.Type.PropVariation ||
                                customAssetMetaData2.type == CustomAssetMetaData.Type.SubBuilding ||
                                customAssetMetaData2.type == CustomAssetMetaData.Type.Trailer ||
                                customAssetMetaData2.type == CustomAssetMetaData.Type.Pillar ||
                                customAssetMetaData2.type == CustomAssetMetaData.Type.RoadElevation)) {
                                ResetEnabled(asset);
                            }
                        }
                    }
                }
            }
        }

        public static void ResetEnabled(Package.Asset asset) {
            SavedBool savedBool = new SavedBool(asset.checksum + ".enabled", PackageManager.assetStateSettingsFile);
            savedBool.Delete();
        }

    }
}
