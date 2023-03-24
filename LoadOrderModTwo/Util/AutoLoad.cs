namespace LoadOrderMod.Util {
    using ColossalFramework;
    using ColossalFramework.IO;
    using ColossalFramework.Packaging;
    using ColossalFramework.PlatformServices;
    using KianCommons;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;
    using static KianCommons.Assertion;
    using static KianCommons.ReflectionHelpers;
    using static SimulationMetaData;
    using ModInfo = global::ModInfo;

    public class AutoLoad : MonoBehaviour {
        void Start() {
            LogCalled();
            base.Invoke(nameof(Do), 0.1f);
        }
        void Do() {
            LogCalled();
            bool ready =
                !SavePanel.isSaving &&
                !Data.CacheUtil.Caching &&
                Singleton<LoadingManager>.exists &&
                !Singleton<LoadingManager>.instance.m_currentlyLoading;
            if (!ready) {
                Invoke("Do", 0.5f);
                return;
            }

            Log.Debug("parsing options ...");
            try {
                ParseCommandLine("game|newGame", out string newGame);
                ParseCommandLine("save|loadSave", out string loadSave);
                bool loadAsset = ParseCommandLine("editor|loadAsset", out _);
                bool newAsset = ParseCommandLine("newAsset", out _);
                bool lsm = ParseCommandLine("LSM", out _);
                bool lht = ParseCommandLine("LHT", out _);
                Log.Debug($"options: newGame={newGame.ToSTR()} loadSave={loadSave.ToSTR()} " +
                    $"loadAsset={loadAsset} newAsset={newAsset} LSM={lsm} lht={lht}");

                var mainMenu = GameObject.FindObjectOfType<MainMenu>();

                if (loadSave != null) {
                    if (loadSave == "")
                        InvokeMethod(mainMenu, "AutoContinue");
                    else
                        LoadSavedGame(loadSave);
                } else if (newGame != null) {
                    LoadMap(newGame);
                } else if (loadAsset || newAsset) {
                    LoadAssetEditor(lht: lht, load: loadAsset, lsm:lsm);
                }
            } catch (Exception ex) {
                Log.Exception(ex);
            }
        }

        /// <summary>
        /// if no match was found, value=null and returns false.
        /// if a match is found, value="" or string after --prototype= and returns true.
        /// </summary>
        public static bool ParseCommandLine(string prototypes, out string value) {
            foreach (string arg in Environment.GetCommandLineArgs()) {
                foreach (string prototype in prototypes.Split("|")) {
                    if (MatchCommandLineArg(arg, prototype, out string val)) {
                        value = val;
                        return true;
                    }
                }
            }
            value = null;
            return false;
        }

        /// <summary>
        /// matches one arg with one prototype
        /// </summary>
        public static bool MatchCommandLineArg(string arg, string prototype, out string value) {
            if (arg == "-" + prototype) {
                value = "";
                return true;
            } else if (arg.StartsWith($"--{prototype}=")) {
                int i = prototype.Length + 3;
                if (arg.Length > i)
                    value = arg.Substring(i);
                else
                    value = "";
                return true;
            } else {
                value = null;
                return false;
            }
        }

        static bool IsCrp(string str) => str.ToLower().EndsWith(".crp");

        public void LoadSavedGame(string fileName) {
            LogCalled();
            var savedGame = IsCrp(fileName)
                ? GetAssetByPath(fileName)
                : GetSaveByName(fileName) ??
                throw new Exception(fileName + "does not exists");
            LoadSavedGame2(savedGame);
        }
        public void LoadSavedGame2(Package.Asset savedGame) {
            LogCalled();
            AssertNotNull(savedGame, "could not find save");
            var metaData = savedGame?.Instantiate<SaveGameMetaData>();
            AssertNotNull(metaData, "metadata");
            AssertNotNull(metaData.assetRef, "assetRef");
            var package = savedGame.package ?? metaData.assetRef.package;
            AssertNotNull(package, "package");

            PrintModsInfo(metaData.mods);

            SimulationMetaData ngs = new SimulationMetaData {
                m_CityName = metaData.cityName,
                m_updateMode = SimulationManager.UpdateMode.LoadGame,
            };
            if (package.GetPublishedFileID() != PublishedFileId.invalid)
                ngs.m_disableAchievements = MetaBool.True;
            UpdateTheme(metaData.mapThemeRef, ngs);

            Log.Info($"Loading new game from " +
                $"map:{ngs.m_CityName} " +
                $"assetName:{savedGame.name} " +
                $"filePath:{package.packagePath}) " +
                $"theme={(ngs.m_MapThemeMetaData?.name).ToSTR()} " +
                $"LHT:{ngs.m_invertTraffic}", true);


            LoadGame(metaData, ngs);
        }

        public void LoadMap(string str, bool lht = false) {
            LogCalled();
            Package.Asset map;
            if (str == "")
                map = GetAMap();
            else if (IsCrp(str))
                map = GetAssetByPath(str);
            else
                map = GetMapByName(str);
            LoadMap2(map, lht);
        }

        public void LoadMap2(Package.Asset map, bool lht = false) {
            LogCalled();
            AssertNotNull(map, "map not found");
            var metaData = map?.Instantiate<MapMetaData>();
            AssertNotNull(metaData, "metadata");
            AssertNotNull(metaData.assetRef, "assetRef");
            var package = map.package ?? metaData.assetRef.package;
            AssertNotNull(package, "package");

            PrintModsInfo(metaData.mods);

            SimulationMetaData ngs = new SimulationMetaData {
                m_CityName = metaData.mapName,
                m_gameInstanceIdentifier = Guid.NewGuid().ToString(),
                m_invertTraffic = lht ? MetaBool.True : MetaBool.False,
                m_disableAchievements = MetaBool.True,
                m_startingDateTime = DateTime.Now,
                m_currentDateTime = DateTime.Now,
                m_newGameAppVersion = DataLocation.productVersion,
                m_updateMode = SimulationManager.UpdateMode.NewGameFromMap,
            };
            UpdateTheme(metaData.mapThemeRef, ngs);

            Log.Info($"Loading new game from " +
                $"map:{ngs.m_CityName} " +
                $"fileName:{map.name} " +
                $"filePath:{package.packagePath}) " +
                $"theme={(ngs.m_MapThemeMetaData?.name).ToSTR()}" +
                $"LHT:{ngs.m_invertTraffic}", true);

            LoadGame(metaData, ngs);
        }

        public void LoadAssetEditor(bool load = true, bool lht = false, bool lsm = true) {
            LogCalled();

            //Patches.ForceLSMPatch.ForceLSM = lsm;

            var mode = load ?
                SimulationManager.UpdateMode.LoadAsset :
                SimulationManager.UpdateMode.NewAsset;

            Package.Asset theme = GetDefaultTheme();
            AssertNotNull(theme, "theme not found");
            var themeMetaData = theme.Instantiate<SystemMapMetaData>();
            AssertNotNull(themeMetaData, "themeMetaData");

            SimulationMetaData ngs = new SimulationMetaData {
                m_gameInstanceIdentifier = Guid.NewGuid().ToString(),
                m_WorkshopPublishedFileId = PublishedFileId.invalid,
                m_updateMode = mode,
                //m_MapThemeMetaData = themeMetaData,
                m_invertTraffic = lht ? MetaBool.True : MetaBool.False,
            };

            Singleton<LoadingManager>.instance.LoadLevel(themeMetaData.assetRef, "AssetEditor", "InAssetEditor", ngs, false);

        }

        static LoadingManager loadingMan_ => Singleton<LoadingManager>.instance;
        static void LoadGame(MetaData metadata, SimulationMetaData ngs) =>
            loadingMan_.LoadLevel(metadata.assetRef, "Game", "InGame", ngs);


        static void UpdateTheme(string mapThemeRef, SimulationMetaData ngs) {
            if (!string.IsNullOrEmpty(mapThemeRef)) {
                Package.Asset asset = PackageManager.FindAssetByName(mapThemeRef, UserAssetType.MapThemeMetaData);
                if (asset != null) {
                    ngs.m_MapThemeMetaData = asset.Instantiate<MapThemeMetaData>();
                    ngs.m_MapThemeMetaData.SetSelfRef(asset);
                }
            }
        }

        static Package.Asset GetAssetByPath(string path) {
            var package = new Package(null, path);
            return package.Find(package.packageMainAsset);
        }

        static Package.Asset GetSaveByName(string name) =>
            FindAssetByShortName(name, UserAssetType.SaveGameMetaData);

        static Package.Asset GetMapByName(string name) =>
            FindAssetByShortName(name, UserAssetType.MapMetaData);

        static Package.Asset FindAssetByShortName(string name, Package.AssetType assetType) =>
            FilterAssets(assetType).FirstOrDefault(a => string.Equals(a.name, name, StringComparison.OrdinalIgnoreCase));


        static Package.Asset GetAMap() => Maps.MinBy(m => m.Instantiate<MapMetaData>()?.mapName);
        //{
        //    Log.Debug("maps=\n"+Maps.Select(m => m.name).JoinLines());
        //    return Maps.OrderBy(m => m.name).First();
        //}

        static IEnumerable<Package.Asset> Maps => FilterAssets(UserAssetType.MapMetaData);

        internal static Package.Asset GetDefaultTheme() {
            try {
                string name = "BuiltinTerrainMap-Sunny";
                foreach (var p in PackageManager.allPackages) {
                    var asset = p.Find(name);
                    if (asset != null) {
                        var metadata = asset.Instantiate();
                        Log.Debug($"{asset.name}->{metadata}");
                        return asset;
                    }
                }
                throw new Exception(name + " theme not found");
            } catch (Exception ex) {
                Log.Exception(ex);
                throw ex;
            }
        }

        static IEnumerable<Package.Asset> FilterAssets(Package.AssetType type) =>
            PackageManager.FilterAssets(new[] { type })
            .Where(asset => asset != null);


        static void PrintModsInfo(ModInfo[] mods) {
            if (mods == null)
                Log.Info("Asset version is too old and does not contain mods info", true);
            else if (mods.Length == 0)
                Log.Info("No mods were used when this asset was created", true);
            else {
                Log.Info(
                    "The following mods were used when this asset was created:\n" +
                    mods.Select(_m => "\t" + _m.modName).JoinLines()
                    , true);
            }
        }

    }

}
