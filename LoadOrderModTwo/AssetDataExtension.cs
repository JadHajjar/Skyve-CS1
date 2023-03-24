namespace LoadOrderMod {
    using System.Collections.Generic;
    using ICities;
    using System;
    using KianCommons;

    /// <summary>
    /// record user data to aid hot reload.
    /// </summary>
    public class LOMAssetDataExtension : AssetDataExtensionBase {
        public static Dictionary<PrefabInfo, Dictionary<string, byte[]>> Assets2UserData =
            new Dictionary<PrefabInfo, Dictionary<string, byte[]>>();

        internal static void Init() => Assets2UserData.Clear();
        internal static void Release() => Assets2UserData.Clear();

        public override void OnAssetLoaded(string name, object asset, Dictionary<string, byte[]> userData) =>
            OnAssetLoadedImpl(name, asset as PrefabInfo, userData);

        internal static void OnAssetLoadedImpl(string name, PrefabInfo asset, Dictionary<string, byte[]> userData) {
            if(asset)
                Assets2UserData.Add(asset, userData);
        }

        // code to be used by other mods
        // might need to copy ReadTypeMetadataPatch (search github)
        public static void HotReload() {
            var assets2UserData = Type.GetType("LoadOrderMod.LOMAssetDataExtension, LoadOrderMod", throwOnError: false)
                ?.GetField("Assets2UserData")
                ?.GetValue(null)
                as Dictionary<PrefabInfo, Dictionary<string, byte[]>>;

            if (assets2UserData == null) {
                Log.Warning("Could not hot reload assets because LoadOrderMod was not found");
                return;
            }

            var editPrefabInfo = ToolsModifierControl.toolController.m_editPrefabInfo;
            foreach (var asset2UserData in assets2UserData) {
                var asset = asset2UserData.Key;
                var userData = asset2UserData.Value;
                if (asset) {
                    if (editPrefabInfo) {
                        // asset editor work around
                        asset = FindLoadedCounterPart<NetInfo>(asset);
                    }
                    OnAssetLoadedImpl(asset.name, asset, userData);
                }
            }
        }

        /// <summary>
        /// OnLoad() calls IntializePrefab() which can create duplicates. Therefore we should match by name.
        /// </summary>
        private static PrefabInfo FindLoadedCounterPart<T>(PrefabInfo source)
            where T : PrefabInfo {
            int n = PrefabCollection<T>.LoadedCount();
            for (uint i = 0; i < n; ++i) {
                T prefab = PrefabCollection<T>.GetLoaded(i);
                if (prefab?.name == source.name) {
                    return prefab;
                }
            }
            return source;
        }
    }
}
