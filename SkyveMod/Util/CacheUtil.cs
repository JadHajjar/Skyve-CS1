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
					cache.MetaName = customAssetMetaData.name;
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
			AquireAssetsDetails();
			Save();
		}
		catch (Exception ex) { Log.Exception(ex); }
		finally { Caching = false; }
	}

	public static void CacheOwnedDlcs()
	{
		try
		{
			var dic = new List<uint>();
			var config = DlcConfig.Deserialize();

			Log.Info($"Checking owned DLCs...");

			foreach (var dlc in config.AvailableDLCs)
			{
				if (PlatformService.IsAppOwned(dlc))
				{
					dic.Add(dlc);
				}
			}

			config.OwnedDLCs = dic;
			config.Serialize();

			Log.Info($"Owned DLCs: {string.Join(", ", config.OwnedDLCs.Select(x => x.ToString()).ToArray())}");
		}
		catch (Exception ex) { Log.Exception(ex); }
	}
}
