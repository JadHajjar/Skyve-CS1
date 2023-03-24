namespace LoadOrderMod.Util {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using ColossalFramework.Packaging;
    using ColossalFramework.Plugins;
    using ColossalFramework.PlatformServices;

    public static class TagsUtil {
        internal static Dictionary<string, string> Tag2Category;
        internal static Dictionary<Package.AssetType, string> Type2Category;

        static TagsUtil() {
            Tag2Category = new Dictionary<string, string>() {
                {SteamHelper.kSteamTagRoad, "Network"},
            };

            Type2Category = new Dictionary<Package.AssetType, string>() {
                { UserAssetType.ColorCorrection, "LUT"},
                { UserAssetType.MapThemeMetaData, "Theme"},
                { UserAssetType.DistrictStyleMetaData, "Style"},
                { UserAssetType.MapMetaData, "Map"},
                { UserAssetType.SaveGameMetaData, "Save"},
                { UserAssetType.ScenarioMetaData, "Scenario"},
            };
        }

        public static string[] Tags(this Package.AssetType assetType) {
            if (assetType != null && Type2Category.TryGetValue(assetType, out string cat2))
                return new string[] { cat2 };
            else
                return new string[0];
        }

        public static string[] Tags(this CustomAssetMetaData metadata) {
            var tags = new List<string>();
            foreach(var tag in metadata.steamTags) {
                if(Tag2Category.TryGetValue(tag, out var cat))
                    tags.Add(cat);
                else
                    tags.Add(tag);
            }

            return tags.ToArray();
        }

        public static string [] Tags(this Package package) {
            if(package.HasMod())
                return new[] { "Mod" };
            else
                return new string[0];
        }

        public static bool HasMod(this Package package) {
            PublishedFileId publishedFileId = package.GetPublishedFileID();
            var plugins = PluginManager.instance.GetPluginsInfo();
            if(publishedFileId != PublishedFileId.invalid) {
                return plugins.Any(item => item.publishedFileID == publishedFileId);
            } else {
                return false;
                // local mod assets are not loaded
                // return plugins.Any(item => item.name == package.packageName);
            }
        }
    }
}
