using ColossalFramework.IO;
using ColossalFramework.Packaging;
using ColossalFramework.PlatformServices;
using ColossalFramework.Plugins;

using KianCommons;
using KianCommons.Plugins;

using SkyveMod.Settings;
using SkyveMod.Util;

using SkyveShared;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;

using static ColossalFramework.Plugins.PluginManager;
using static KianCommons.ReflectionHelpers;

namespace SkyveMod.Data;
extern alias Injections;

using SteamUtilities = Injections.SkyveInjections.SteamUtilities;
public class CacheUtil
{
	public CSCache Cache;
	public Package.Asset[] CachedAssets;
	public PluginInfo[] CachedPlugins;
	public CacheUtil(Package.Asset[] assets, PluginManager.PluginInfo[] plugins)
	{
		CachedAssets = assets;
		CachedPlugins = plugins;
		Load();
	}

	public void Load()
	{
		Cache = CSCache.Deserialize() ?? new CSCache();
	}

	public void Save()
	{
		Cache.Serialize();
	}

	internal CSCache.Mod GetOrCreateModCache(PluginInfo p)
	{
		if (Cache.GetItem(p.modPath) is not CSCache.Mod ret)
		{
			ret = new CSCache.Mod { IncludedPath = p.modPath };
			Cache.AddItem(ret);
		}
		return ret;
	}

	internal CSCache.Asset GetOrCreateAssetCache(Package.Asset a)
	{
		if (Cache.GetItem(a.GetPath()) is not CSCache.Asset ret)
		{
			ret = new CSCache.Asset { IncludedPath = a.GetPath() };
			Cache.AddItem(ret);
		}
		return ret;
	}

	public void AquirePathDetails()
	{
		try
		{
			LogCalled();
			Cache.GamePath = DataLocation.applicationBase;
			Log.Info("Config.GamePath=" + Cache.GamePath, true);
			foreach (var pluginInfo in CachedPlugins)
			{
				if (pluginInfo.publishedFileID != PublishedFileId.invalid)
				{
					Cache.WorkShopContentPath = Path.GetDirectoryName(pluginInfo.modPath);
					Log.Info("Config.WorkShopContentPath=" + Cache.WorkShopContentPath, true);
					break;
				}
			}
		}
		catch (Exception ex)
		{
			Log.Exception(ex);
		}
	}

	public void AquireModsDetails()
	{
		LogCalled();
		foreach (var pluginInfo in CachedPlugins)
		{
			try
			{
				if (pluginInfo?.userModInstance == null)
				{
					continue;
				}

				var cache = GetOrCreateModCache(pluginInfo);
				Assertion.NotNull(cache);

				cache.Description = pluginInfo.GetUserModInstance()?.Description;
				cache.Name = pluginInfo.GetModName();
			}
			catch (Exception ex)
			{
				ex.Log("pluginInfo=" + pluginInfo);
			}
		}
		LogSucceeded();
	}

	public void AquireAssetsDetails()
	{
		LogCalled();
		var timerInstantiate = new Stopwatch();
		var assetCount = 0;


		foreach (var asset in CachedAssets)
		{
			try
			{
				Assertion.NotNull(asset, "asset");
				if (!asset.isMainAsset)
				{
					continue;
				}

				if (asset.GetPath().IsNullorEmpty())
				{
					continue; // TODO support LUT .png
				}

				var cache = GetOrCreateAssetCache(asset);
				Assertion.NotNull(cache, "assetInfo");
				Assertion.NotNull(asset.package, "asset.package");

				cache.Name = asset.name;

				var tags = new List<string>(asset.type.Tags());
				tags.AddRange(asset.package.Tags());

				timerInstantiate.Start();
				var metaData = asset.Instantiate() as MetaData;
				timerInstantiate.Stop();
				assetCount++;

				if (metaData is CustomAssetMetaData customAssetMetaData)
				{
					cache.Description = ContentManagerUtil.SafeGetAssetDesc(customAssetMetaData, asset.package);
					tags.AddRange(customAssetMetaData.Tags());
				}
				cache.Tags = tags.ToArray();

			}
			catch (Exception ex)
			{
				ex.Log($"asset: {asset}");
			}
		}

		double ms = timerInstantiate.ElapsedMilliseconds;
		Log.Debug("average asset instantiation time = " + (ms / assetCount));

		LogSucceeded();
	}

	public void AquireMissingItems()
	{
		Cache.MissingDir =
			SteamUtilities.GetMissingItems()
			.Select(id => id.AsUInt64)
			.ToArray();
	}

	public static void CacheData()
	{
		try
		{
			if (!ConfigUtil.Config.UGCCache)
			{
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
		}
		catch (Exception ex) { ex.Log(); }
	}

	static void CacheDataImpl(Package.Asset[] assets, PluginManager.PluginInfo[] plugins)
	{
		try
		{
			new CacheUtil(assets, plugins).CacheAll();
		}
		catch (Exception ex) { ex.Log(); }
	}

	public static bool Caching;
	public void CacheAll()
	{
		LogCalled();
		if (!ConfigUtil.Config.UGCCache)
		{
			Log.Info("Skipping UGCCache ...");
			return;
		}

		try
		{
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
		}
		catch (Exception ex) { Log.Exception(ex); }
		finally { Caching = false; }
	}

	private void AquireDlcs()
	{
		var dic = new List<uint>();

		foreach (var dlc in new uint[] { 2342310,2313324,2313323,2313322,2313321,2313320,2225941,2225940,2224691,2224690,2148904,2148903,2148902,2148901,2148900,2144483,2144482,2144481,2144480,2008400,1992293,1992292,1992291,1992290,1726384,1726383,1726382,1726381,1726380,1531473,1531472,1531471,1531470,1196100,1148022,1148021,1148020,1146930,1065491,1065490,1059820,944071,944070,815380,715194,715193,715192,715191,715190,614582,614581,614580,563850,547502,547501,547500,536610,526612,526611,526610,525940,515191,515190,470930,470680,456200,420610,369150,365040,355600,352512,352511,352510,346791,346790,340160, })
		{
			if (PlatformService.IsAppOwned(dlc))
			{
				dic.Add(dlc);
			}
		}

		Cache.Dlcs = dic.ToArray();
	}
}
