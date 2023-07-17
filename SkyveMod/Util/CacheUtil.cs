using ColossalFramework.Packaging;
using ColossalFramework.PlatformServices;
using ColossalFramework.Plugins;

using KianCommons;

using SkyveMod.Settings;
using SkyveMod.Util;

using SkyveShared;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;

using static ColossalFramework.Plugins.PluginManager;
using static KianCommons.ReflectionHelpers;

namespace SkyveMod.Data;
extern alias Injections;
public class CacheUtil
{
	public AssetInfoCache Cache;
	public Package.Asset[] CachedAssets;
	public PluginInfo[] CachedPlugins;

	public CacheUtil(Package.Asset[] assets, PluginManager.PluginInfo[] plugins)
	{
		CachedAssets = assets;
		CachedPlugins = plugins;
		Cache = AssetInfoCache.Deserialize();
	}

	public void Save()
	{
		Cache.Serialize();
	}

	internal AssetInfoCache.Asset GetOrCreateAssetCache(Package.Asset a)
	{
		if (Cache.GetAsset(a.GetPath()) is not AssetInfoCache.Asset ret)
		{
			ret = new AssetInfoCache.Asset { Path = a.GetPath() };

			Cache.AddAsset(ret);
		}

		return ret;
	}

	public void AquireAssetsDetails()
	{
		LogCalled();

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

				cache.FullName = asset.fullName;
				cache.Name = asset.name;

				var tags = new List<string>(asset.type.Tags());
				tags.AddRange(asset.package.Tags());

				var metaData = asset.Instantiate() as MetaData;

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

		LogSucceeded();
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

		foreach (var dlc in new uint[] { 2342310, 2313324, 2313323, 2313322, 2313321, 2313320, 2225941, 2225940, 2224691, 2224690, 2148904, 2148903, 2148902, 2148901, 2148900, 2144483, 2144482, 2144481, 2144480, 2008400, 1992293, 1992292, 1992291, 1992290, 1726384, 1726383, 1726382, 1726381, 1726380, 1531473, 1531472, 1531471, 1531470, 1196100, 1148022, 1148021, 1148020, 1146930, 1065491, 1065490, 1059820, 944071, 944070, 815380, 715194, 715193, 715192, 715191, 715190, 614582, 614581, 614580, 563850, 547502, 547501, 547500, 536610, 526612, 526611, 526610, 525940, 515191, 515190, 470930, 470680, 456200, 420610, 369150, 365040, 355600, 352512, 352511, 352510, 346791, 346790, 340160, })
		{
			if (PlatformService.IsAppOwned(dlc))
			{
				dic.Add(dlc);
			}
		}

		Cache.AvailableDLCs = dic.ToArray();
	}
}
