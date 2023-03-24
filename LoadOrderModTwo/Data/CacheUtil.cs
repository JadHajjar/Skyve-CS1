namespace LoadOrderMod.Data {
    extern alias Injections;
    using SteamUtilities = Injections.LoadOrderInjections.SteamUtilities;
    using ColossalFramework.IO;
    using ColossalFramework.Packaging;
    using ColossalFramework.PlatformServices;
    using ColossalFramework.Plugins;
    using KianCommons;
    using KianCommons.Plugins;
    using LoadOrderMod.Settings;
    using LoadOrderMod.Util;
    using LoadOrderShared;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using static ColossalFramework.Plugins.PluginManager;
    using static KianCommons.ReflectionHelpers;
    using System.Threading;
	using LoadOrderToolTwo.ColossalOrder;

	public class CacheUtil {
        public CSCache Cache;
        public Package.Asset[] CachedAssets;
        public PluginInfo[] CachedPlugins;
        public CacheUtil(Package.Asset[] assets, PluginManager.PluginInfo[] plugins) {
            CachedAssets = assets;
            CachedPlugins = plugins;
            Load();
        }

        public void Load() => Cache = CSCache.Deserialize() ?? new CSCache();
        public void Save() => Cache.Serialize();

        internal CSCache.Mod GetOrCreateModCache(PluginInfo p) {
            var ret = Cache.GetItem(p.modPath) as CSCache.Mod;
            if (ret == null) {
                ret = new CSCache.Mod { IncludedPath = p.modPath };
                Cache.AddItem(ret);
            }
            return ret;
        }

        internal CSCache.Asset GetOrCreateAssetCache(Package.Asset a) {
            var ret = Cache.GetItem(a.GetPath()) as CSCache.Asset;
            if (ret == null) {
                ret = new CSCache.Asset { IncludedPath = a.GetPath() };
                Cache.AddItem(ret);
            }
            return ret;
        }

        public void AquirePathDetails() {
            try {
                LogCalled();
                Cache.GamePath = DataLocation.applicationBase;
                Log.Info("Config.GamePath=" + Cache.GamePath, true);
                foreach (var pluginInfo in CachedPlugins) {
                    if (pluginInfo.publishedFileID != PublishedFileId.invalid) {
                        Cache.WorkShopContentPath = Path.GetDirectoryName(pluginInfo.modPath);
                        Log.Info("Config.WorkShopContentPath=" + Cache.WorkShopContentPath, true);
                        break;
                    }
                }

            } catch (Exception ex) {
                Log.Exception(ex);
            }
        }

        public void AquireModsDetails() {
            LogCalled();
            foreach (var pluginInfo in CachedPlugins) {
                try {
                    if (pluginInfo?.userModInstance == null) continue;
                    var cache = GetOrCreateModCache(pluginInfo);
                    Assertion.NotNull(cache);

                    cache.Description = pluginInfo.GetUserModInstance()?.Description;
                    cache.Name = pluginInfo.GetModName();
                } catch (Exception ex) {
                    ex.Log("pluginInfo=" + pluginInfo);
                }
            }
            LogSucceeded();
        }

        public void AquireAssetsDetails() {
            LogCalled();
            var timerInstantiate = new Stopwatch();
            int assetCount = 0;


            foreach (var asset in CachedAssets) {
                try {
                    Assertion.NotNull(asset, "asset");
                    if (!asset.isMainAsset) continue;
                    if (asset.GetPath().IsNullorEmpty())
                        continue; // TODO support LUT .png

                    var cache = GetOrCreateAssetCache(asset);
                    Assertion.NotNull(cache, "assetInfo");
                    Assertion.NotNull(asset.package, "asset.package");

                    cache.Name = asset.name;

                    List<string> tags = new List<string>(asset.type.Tags());
                    tags.AddRange(asset.package.Tags());

                    timerInstantiate.Start();
                    MetaData metaData = asset.Instantiate() as MetaData;
                    timerInstantiate.Stop();
                    assetCount++;

                    if (metaData is CustomAssetMetaData customAssetMetaData) {
                        cache.Description = ContentManagerUtil.SafeGetAssetDesc(customAssetMetaData, asset.package);
                        tags.AddRange(customAssetMetaData.Tags());
                    }
                    cache.Tags = tags.ToArray();

                } catch (Exception ex) {
                    ex.Log($"asset: {asset}");
                }
            }

            double ms = timerInstantiate.ElapsedMilliseconds;
            Log.Debug("average asset instantiation time = " + ms / assetCount);

            LogSucceeded();
        }

        public void AquireMissingItems() {
            Cache.MissingDir =
                SteamUtilities.GetMissingItems()
                .Select(id => id.AsUInt64)
                .ToArray();
        }

        public static void CacheData() {
            try {
                if (!ConfigUtil.Config.UGCCache) {
                    Log.Info("Skipping UGCCache ...");
                    return;
                }
                // evaluate this to avoid race condition.
                
                var assets = PackageManager.FilterAssets(new[] {
                    UserAssetType.CustomAssetMetaData,
                    UserAssetType.MapThemeMetaData,
                    UserAssetType.ColorCorrection,
                    UserAssetType.DistrictStyleMetaData,
                }).ToArray();
                var plugins = PluginManager.instance.GetPluginsInfo().ToArray();
                new Thread(() => CacheDataImpl(assets, plugins)).Start();
            } catch (Exception ex) { ex.Log(); }
        }

        static void CacheDataImpl(Package.Asset[] assets, PluginManager.PluginInfo []plugins) {
            try {
                new CacheUtil(assets, plugins).CacheAll();
            } catch(Exception ex) { ex.Log(); }
        }

        public static bool Caching;
        public void CacheAll() {
            LogCalled();
            if (!ConfigUtil.Config.UGCCache) {
                Log.Info("Skipping UGCCache ...");
                return;
            }

            try {
                Caching = true;
                //AquirePathDetails();
                //Save();
                AquireModsDetails();
                Save();
                AquireMissingItems();
                Save();
                AquireAssetsDetails();
                Save();
                AquireDlcs();
                Save();
            } catch (Exception ex) { Log.Exception(ex); }
            finally { Caching = false; }
        }

		private void AquireDlcs()
		{
            var dic = new List<uint>();

            foreach (DLC dlc in Enum.GetValues(typeof(DLC)))
            {
                if( PlatformService.IsDlcInstalled((uint)dlc))
                    dic.Add((uint)dlc);
			}

            Cache.Dlcs = dic.ToArray();
		}
	}
}
