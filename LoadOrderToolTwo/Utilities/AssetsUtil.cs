using Extensions;

using LoadOrderShared;

using LoadOrderToolTwo.Domain;
using LoadOrderToolTwo.Domain.Utilities;
using LoadOrderToolTwo.Utilities.IO;
using LoadOrderToolTwo.Utilities.Managers;

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;

namespace LoadOrderToolTwo.Utilities;
internal class AssetsUtil
{
	private static readonly LoadOrderConfig _config;
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
		_config = LoadOrderConfig.Deserialize() ?? new();
		ExcludedHashSet = new HashSet<string>(_config.Assets.Select(x => IOUtil.ToRealPath(x.Path?.ToLower()) ?? string.Empty));

		CentralManager.ContentLoaded += CentralManager_ContentLoaded;
	}

	private static void CentralManager_ContentLoaded()
	{
		BuildAssetIndex();
	}

	public static IEnumerable<Asset> GetAssets(Package package)
	{
		if (!Directory.Exists(package.Folder))
		{
			yield break;
		}

		var files = Directory.GetFiles(package.Folder, $"*.crp", SearchOption.AllDirectories);

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

		CentralManager.InformationUpdate(asset);
		ProfileManager.TriggerAutoSave();

		SaveChanges();
	}

	internal static void SetIncluded(IEnumerable<Asset> assets, bool value)
	{
		var list = assets.ToList();

		for (var i = 0; i < list.Count; i++)
		{
			if (value)
			{
				ExcludedHashSet.Remove(list[i].FileName.ToLower());
			}
			else
			{
				ExcludedHashSet.Add(list[i].FileName.ToLower());
			}

			CentralManager.InformationUpdate(list[i]);
		}

		if (!ProfileManager.ApplyingProfile && !CitiesManager.IsRunning())
		{
			SaveChanges();
		}

		ProfileManager.TriggerAutoSave();
	}

	public static void SaveChanges()
	{
		if (ProfileManager.ApplyingProfile)
		{
			return;
		}

		_config.Assets = ExcludedHashSet
				.Select(x => new AssetInfo { Path = IOUtil.ToVirtualPath(x) })
				.ToArray();

		_config.Serialize();
	}

	internal static Asset GetAsset(string? v)
	{
		return assetIndex.TryGet(v?.Replace('/', Path.DirectorySeparatorChar).Replace('\\', Path.DirectorySeparatorChar).ToLower() ?? string.Empty);
	}

	private static void BuildAssetIndex()
	{
		assetIndex = CentralManager.Assets.ToDictionary(x => x.FileName.Replace('/', Path.DirectorySeparatorChar).Replace('\\', Path.DirectorySeparatorChar).ToLower());
	}

	internal static Bitmap? GetIcon(Asset asset)
	{
		//var fileName = Path.Combine(LocationManager.LotAppDataPath, "AssetPictures");

		//if (asset.SteamId > 0)
		//{
		//	fileName = Path.Combine(fileName, asset.SteamId.ToString());
		//}

		//fileName = Path.Combine(fileName, Path.GetFileNameWithoutExtension(asset.FileName).Trim().Replace(' ', '_') + ".png");

		//if (File.Exists(fileName))
		//{
		//	return (Bitmap)Image.FromFile(fileName);
		//}

		return null;
	}

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

	internal static IEnumerable<string> GetFindItTags(Asset asset)
	{
		var assetName = (asset.SteamId == 0 ? "" : $"{asset.SteamId}.") + Path.GetFileNameWithoutExtension(asset.FileName).RemoveDoubleSpaces().Replace(' ', '_');

		foreach (var item in _findItTags.assetTags)
		{
			if (item.Key.RemoveDoubleSpaces().Replace(' ', '_').Equals(assetName, StringComparison.CurrentCultureIgnoreCase))
			{
				return item.Value.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
			}
		}

		return new string[0];
	}

	internal static void AddFindItTag(Asset asset, string tag)
	{
		var newTags = new CustomTagsLibrary();

		newTags.Deserialize();

		var assetName = (asset.SteamId == 0 ? "" : $"{asset.SteamId}.") + Path.GetFileNameWithoutExtension(asset.FileName).RemoveDoubleSpaces().Replace(' ', '_');
		var found = false;

		foreach (var item in newTags.assetTags)
		{
			if (item.Key.RemoveDoubleSpaces().Replace(' ', '_').Equals(assetName, StringComparison.CurrentCultureIgnoreCase))
			{
				newTags.assetTags[item.Key] = item.Value + " " + tag.Trim();

				found = true;
				break;
			}
		}

		if (!found)
		{
			assetName = (asset.SteamId == 0 ? "" : $"{asset.SteamId}.") + Path.GetFileNameWithoutExtension(asset.FileName);

			newTags.assetTags[assetName] = tag.Trim();
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
