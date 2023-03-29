using Extensions;

using LoadOrderShared;

using LoadOrderToolTwo.Domain;
using LoadOrderToolTwo.Domain.Utilities;
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

	public static HashSet<string> ExcludedHashSet { get; }
	public static Dictionary<string, CSCache.Asset> AssetInfoCache { get; }

	static AssetsUtil()
	{
		_config = LoadOrderConfig.Deserialize() ?? new();
		var cache = CSCache.Deserialize()?.Assets.ToDictionary(x => x.IncludedPath, x => x, StringComparer.InvariantCultureIgnoreCase);

		_findItTags = new();
		AssetInfoCache = cache ?? new();
		ExcludedHashSet = new HashSet<string>(_config.Assets.Select(x => x.Path?.ToLower() ?? string.Empty));

		_findItTags.Deserialize();
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
				.Select(x => new AssetInfo { Path = x })
				.ToArray();

		_config.Serialize();
	}

	internal static Asset GetAsset(string? v)
	{
		return CentralManager.Assets.FirstOrDefault(x => x.FileName.Equals(v, StringComparison.InvariantCultureIgnoreCase));
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

	internal static bool IsDlcExcluded(uint dlc)
	{
		return _config.RemovedDLCs.Contains(dlc);
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

	internal static IEnumerable<string> GetFindItTags(Asset asset)
	{
		var assetName = (asset.SteamId == 0?"":$"{asset.SteamId}.")+ Path.GetFileNameWithoutExtension(asset.FileName).RemoveDoubleSpaces().Replace(' ', '_');

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
