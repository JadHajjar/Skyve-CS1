namespace LoadOrderMod.Util {
    using ColossalFramework;
    using ColossalFramework.Globalization;
    using ColossalFramework.Packaging;
    using ColossalFramework.PlatformServices;
    using ColossalFramework.UI;
    using HarmonyLib;
    using KianCommons;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;
    using static ColossalFramework.Plugins.PluginManager;
    using static KianCommons.ReflectionHelpers;

    public static class ContentManagerUtil {
        const string ASSET_CATEGORY_NAME = "Assets";
        const string MOD_CATEGORY_NAME = "Mods";

        public static bool IsIntroLoaded {
            get {
                try {
                    _ = ModCategory;
                    return true;
                } catch {
                    return false;
                }
            }
        }

        public static CategoryContentPanel GetCategory(string name) {
            foreach (var c in GameObject.FindObjectsOfType<UIComponent>()) {
                if (c.name != name) continue;
                CategoryContentPanel c2 = c.GetComponentInChildren<CategoryContentPanel>();
                if (c2) return c2;
            }
            throw new Exception($"CategoryContentPanel for '{name}' was not found");
        }

        private static CategoryContentPanel assetCategory_;
        private static CategoryContentPanel modCategory_;
        public static CategoryContentPanel AssetCategory => assetCategory_ ??= GetCategory(ASSET_CATEGORY_NAME);
        public static CategoryContentPanel ModCategory => modCategory_ ??= GetCategory(MOD_CATEGORY_NAME);


        private static object fieldRef_Assets_;
        public static List<EntryData> GetEntries(this CategoryContentPanel instance) {
            fieldRef_Assets_ ??= AccessTools.FieldRefAccess<CategoryContentPanel, List<EntryData>>("m_Assets");
            var assets = fieldRef_Assets_ as AccessTools.FieldRef<CategoryContentPanel, List<EntryData>>;
            return assets(instance);
        }

        public static List<EntryData> AssetEntries => AssetCategory?.GetEntries();
        public static List<EntryData> ModEntries => ModCategory?.GetEntries();
        public static IEnumerable<EntryData> EntryDatas =>
            ModEntries.EmptyIfNull()
            .Concat(AssetEntries.EmptyIfNull());

        public static EntryData GetEntryData(this Package.Asset asset) {
            return AssetEntries?.FirstOrDefault(item => item.asset == asset);
        }

        public static EntryData GetEntryData(this PluginInfo pluginInfo) {
            return ModEntries?.FirstOrDefault(item => item.pluginInfo == pluginInfo);
        }

        /// <param name="fallback">if could not get name from entry,
        /// falls back to getting name from file</param>
        public static string GetAuthor(this Package.Asset a, bool fallback) {
            string ret = a.GetEntryData()?.authorName;
            if (ret.IsAuthorNameValid())
                return ret;
            else if (fallback)
                return ResolveName(a.package.packageAuthor);
            return null;
        }

        public static string GetAuthor(this PluginInfo p) {
            return p.GetEntryData()?.authorName;
        }

        public static string ResolveName(string str) {
            string[] array = str.Split(':');
            if (array.Length == 2 &&
                array[0] == "steamid" &&
                ulong.TryParse(array[1], out ulong value) &&
                value > 0) {
                return new Friend(new UserID(value)).personaName;
            }
            return null;
        }

        public static bool IsAuthorNameValid(this string author) =>
            !author.IsNullOrWhiteSpace() && author != "[unknown]";

        public static string SafeGetAssetDesc(CustomAssetMetaData metaData, Package package) {
            string localeID;
            if (metaData.type == CustomAssetMetaData.Type.Building) {
                localeID = "BUILDING_DESC";
            } else if (metaData.type == CustomAssetMetaData.Type.Prop) {
                localeID = "PROPS_DESC";
            } else if (metaData.type == CustomAssetMetaData.Type.Tree) {
                localeID = "TREE_DESC";
            } else {
                if (metaData.type != CustomAssetMetaData.Type.Road) {
                    return metaData.name;
                }
                localeID = "NET_DESC";
            }
            return SafeGetAssetString(localeID, metaData.name, package);
        }

        private static string SafeGetAssetString(string localeID, string assetName, Package package) {
            if (package != null) {
                string key = package.packageName + "." + assetName + "_Data";
                if (Locale.Exists(localeID, key)) {
                    return Locale.Get(localeID, key);
                }
            }
            return assetName;
        }

        public static void RefreshAllEntries() {
            var contentManagerPanel = AssetCategory?.GetComponentInParent<ContentManagerPanel>();
            if (contentManagerPanel != null) {
                foreach (var cat in contentManagerPanel.GetComponentsInChildren<CategoryContentPanel>()) {
                    InvokeMethod(cat, "RefreshEntries");
                }
            }
        }
    }
}
