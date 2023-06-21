using Extensions;

using SkyveApp.Domain;
using SkyveApp.Domain.Interfaces;
using SkyveApp.Domain.Utilities;
using SkyveApp.Services;

using SkyveShared;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SkyveApp.Utilities;
internal class AssetsUtil
{
	private static readonly SkyveConfig _config;
	private static CustomTagsLibrary _findItTags;
	private static Dictionary<string, Asset> assetIndex = new();

	public static HashSet<string> ExcludedHashSet { get; }
	public static Dictionary<string, CSCache.Asset> AssetInfoCache { get; }

	static AssetsUtil()
	{
		AssetInfoCache = new(StringComparer.InvariantCultureIgnoreCase);

		var assets = CSCache.Deserialize()?.Assets;

		if (assets is not null)
		{
			for (var i = 0; i < assets.Length; i++)
			{
				AssetInfoCache[assets[i].IncludedPath] = assets[i];
			}
		}

		_findItTags = new();
		_findItTags.Deserialize();
		_config = SkyveConfig.Deserialize() ?? new();

		ExcludedHashSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

		foreach (var item in _config.Assets)
		{
			if (item.Path is not null)
			{
				ExcludedHashSet.Add(item.Path);
			}
		}
	}

	public static IEnumerable<Asset> GetAssets(Package package, bool withSubDirectories = true)
	{
		if (!Directory.Exists(package.Folder))
		{
			yield break;
		}

		var files = Directory.GetFiles(package.Folder, $"*.crp", withSubDirectories ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);

		foreach (var file in files)
		{
			yield return new Asset(package, file);
		}
	}

	internal static bool IsIncluded(Asset asset)
	{
		return !ExcludedHashSet.Contains(asset.FileName.ToLower());
	}

	internal static void SetIncluded(Asset asset, bool value)
	{
		if (value)
		{
			ExcludedHashSet.Remove(asset.FileName.ToLower());
		}
		else
		{
			ExcludedHashSet.Add(asset.FileName.ToLower());
		}

		CentralManager.OnInclusionUpdated();
		ProfileManager.TriggerAutoSave();

		SaveChanges();
	}

	public static void SaveChanges()
	{
		if (ProfileManager.ApplyingProfile || ContentUtil.BulkUpdating)
		{
			return;
		}

		_config.Assets = ExcludedHashSet
				.Select(x => new AssetInfo { Path = x })
				.ToArray();

		_config.Serialize();
	}

	internal static Asset GetAsset(string v)
	{
		return assetIndex.TryGet(v);
	}

	internal static void BuildAssetIndex()
	{
		assetIndex = CentralManager.Assets.ToDictionary(x => x.FileName.FormatPath(), StringComparer.OrdinalIgnoreCase);
	}

	//internal static Bitmap? GetIcon(Asset asset)
	//{
	//	//var fileName = LocationManager.Combine(LocationManager.LotAppDataPath, "AssetPictures");

	//	//if (asset.SteamId > 0)
	//	//{
	//	//	fileName = LocationManager.Combine(fileName, asset.SteamId.ToString());
	//	//}

	//	//fileName = LocationManager.Combine(fileName, Path.GetFileNameWithoutExtension(asset.FileName).Trim().Replace(' ', '_') + ".png");

	//	//if (File.Exists(fileName))
	//	//{
	//	//	return (Bitmap)Image.FromFile(fileName);
	//	//}

	//	return null;
	//}

	internal static void SetAvailableDlcs(IEnumerable<uint> dlcs)
	{
		_config.AvailableDLCs = dlcs.ToArray();
		_config.Serialize();
	}

	internal static bool IsDlcExcluded(uint dlc)
	{
		return _config.RemovedDLCs.Contains(dlc);
	}

	internal static void SetDlcsExcluded(uint[] dlc)
	{
		_config.RemovedDLCs = dlc;

		ProfileManager.TriggerAutoSave();
		SaveChanges();
	}

	internal static void SetDlcExcluded(uint dlc, bool excluded)
	{
		var list = new List<uint>(_config.RemovedDLCs);

		if (excluded)
		{
			list.Add(dlc);
		}
		else
		{
			list.Remove(dlc);
		}

		_config.RemovedDLCs = list.ToArray();

		ProfileManager.TriggerAutoSave();
		SaveChanges();
	}

	internal static IEnumerable<string> GetAllFindItTags()
	{
		foreach (var item in _findItTags.assetTags)
		{
			foreach (var tag in item.Value.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries))
			{
				yield return tag;
			}
		}
	}

	internal static IEnumerable<string> GetFindItTags(IPackage package)
	{
		var key = package is Asset asset
			? (asset.SteamId == 0 ? "" : $"{asset.SteamId}.") + Path.GetFileNameWithoutExtension(asset.FileName).RemoveDoubleSpaces().Replace(' ', '_')
			: package.Folder;

		foreach (var item in _findItTags.assetTags)
		{
			if (item.Key.RemoveDoubleSpaces().Replace(' ', '_').Equals(key.RemoveDoubleSpaces().Replace(' ', '_'), StringComparison.CurrentCultureIgnoreCase))
			{
				return item.Value.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
			}
		}

		return new string[0];
	}

	internal static void SetFindItTag(IPackage package, string tag)
	{
		var newTags = new CustomTagsLibrary();

		newTags.Deserialize();

		var found = false;
		var key = package is Asset asset
			? (asset.SteamId == 0 ? "" : $"{asset.SteamId}.") + Path.GetFileNameWithoutExtension(asset.FileName)
			: package.Folder;

		foreach (var item in newTags.assetTags)
		{
			if (item.Key.RemoveDoubleSpaces().Replace(' ', '_').Equals(key.RemoveDoubleSpaces().Replace(' ', '_'), StringComparison.CurrentCultureIgnoreCase))
			{
				newTags.assetTags[item.Key] = tag.Trim();

				found = true;
				break;
			}
		}

		if (!found)
		{
			newTags.assetTags[key] = tag.Trim();
		}

		newTags.Serialize();

		_findItTags = newTags;
	}

	internal static void RemoveFindItTag(Asset asset, string tag)
	{
		var newTags = new CustomTagsLibrary();

		newTags.Deserialize();

		var assetName = (asset.SteamId == 0 ? "" : $"{asset.SteamId}.") + Path.GetFileNameWithoutExtension(asset.FileName).RemoveDoubleSpaces().Replace(' ', '_');

		foreach (var item in newTags.assetTags)
		{
			if (item.Key.RemoveDoubleSpaces().Replace(' ', '_').Equals(assetName, StringComparison.CurrentCultureIgnoreCase))
			{
				newTags.assetTags[item.Key] = item.Value.Remove(tag).RemoveDoubleSpaces();

				break;
			}
		}

		newTags.Serialize();

		_findItTags = newTags;
	}
}
