using ColossalFramework.Packaging;
using ColossalFramework.PlatformServices;
using ColossalFramework.Plugins;

using KianCommons;

using SkyveMod.Settings;
using SkyveMod.Util;

using SkyveShared;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

using static ColossalFramework.Plugins.PluginManager;
using static KianCommons.ReflectionHelpers;

namespace SkyveMod.Data;
extern alias Injections;
public class CacheUtil(Package.Asset[] assets, PluginManager.PluginInfo[] plugins)
{
	public AssetInfoCache Cache = AssetInfoCache.Deserialize();
	public Package.Asset[] CachedAssets = assets;
	public PluginInfo[] CachedPlugins = plugins;

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

				cache.Tags = [.. tags];

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

			var assets = PackageManager.FilterAssets([
					UserAssetType.CustomAssetMetaData,
					UserAssetType.MapThemeMetaData,
					UserAssetType.ColorCorrection,
					UserAssetType.DistrictStyleMetaData,
				]).ToArray();
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
        uint[] dlcs = [346791, 369150, 420610, 456200, 515190, 515191, 547500, 547501, 547502, 563850, 614580, 614581, 614582, 715190, 715191, 715192, 715193, 715194, 815380, 944070, 944071, 1059820, 1065490, 1065491, 1146930, 1148020, 1148021, 1148022, 1196100, 1531470, 1531471, 1531472, 1531473, 1726380, 1726381, 1726382, 1726383, 1726384, 1992290, 1992291, 1992292, 1992293, 2008400, 2144480, 2144481, 2144482, 2144483, 2148900, 2148901, 2148902, 2148903, 2148904, 2224690, 2224691, 2225940, 2225941];

        uint[] extars = [340160, 346790, 352510, 352511, 352512, 355600, 365040, 470680, 470930, 525940, 526610, 526611, 526612, 536610];

        uint[] AvailableDLCs = [.. dlcs, .. extars];

        var dic = new List<uint>();

		foreach (var dlc in AvailableDLCs)
		{
			if (PlatformService.IsAppOwned(dlc))
			{
				dic.Add(dlc);
			}
		}

		Cache.AvailableDLCs = [.. dic];
	}
}
