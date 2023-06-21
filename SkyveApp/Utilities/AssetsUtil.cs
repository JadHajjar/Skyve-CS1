using Extensions;

using SkyveApp.Domain;
using SkyveApp.Domain.Interfaces;
using SkyveApp.Domain.Utilities;
using SkyveApp.Services;
using SkyveApp.Services.Interfaces;

using SkyveShared;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SkyveApp.Utilities;
public class AssetsUtil : IAssetUtil
{
	private readonly SkyveConfig _config;
	private CustomTagsLibrary _findItTags;
	private Dictionary<string, Asset> assetIndex = new();

	public HashSet<string> ExcludedHashSet { get; }
	public Dictionary<string, CSCache.Asset> AssetInfoCache { get; }

	private readonly IContentManager _contentManager;
	private readonly IProfileManager _profileManager;
	private readonly INotifier _notifier;

	public AssetsUtil(IContentManager contentManager, IProfileManager profileManager, INotifier notifier)
	{
		_contentManager = contentManager;
		_profileManager = profileManager;
		_notifier = notifier;

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

		_notifier.ContentLoaded += BuildAssetIndex;
	}

	public IEnumerable<Asset> GetAssets(Package package, bool withSubDirectories = true)
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

	public bool IsIncluded(Asset asset)
	{
		return !ExcludedHashSet.Contains(asset.FileName.ToLower());
	}

	public void SetIncluded(Asset asset, bool value)
	{
		if (value)
		{
			ExcludedHashSet.Remove(asset.FileName.ToLower());
		}
		else
		{
			ExcludedHashSet.Add(asset.FileName.ToLower());
		}

		_notifier.OnInclusionUpdated();
		_profileManager.TriggerAutoSave();

		SaveChanges();
	}

	public void SaveChanges()
	{
		if (_profileManager.ApplyingProfile || _notifier.BulkUpdating)
		{
			return;
		}

		_config.Assets = ExcludedHashSet
				.Select(x => new AssetInfo { Path = x })
				.ToArray();

		_config.Serialize();
	}

	public Asset GetAsset(string v)
	{
		return assetIndex.TryGet(v);
	}

	public void BuildAssetIndex()
	{
		assetIndex = _contentManager.Assets.ToDictionary(x => x.FileName.FormatPath(), StringComparer.OrdinalIgnoreCase);
	}

	//public Bitmap? GetIcon(Asset asset)
	//{
	//	//var fileName = CrossIO.Combine(LocationManager.LotAppDataPath, "AssetPictures");

	//	//if (asset.SteamId > 0)
	//	//{
	//	//	fileName = CrossIO.Combine(fileName, asset.SteamId.ToString());
	//	//}

	//	//fileName = CrossIO.Combine(fileName, Path.GetFileNameWithoutExtension(asset.FileName).Trim().Replace(' ', '_') + ".png");

	//	//if (File.Exists(fileName))
	//	//{
	//	//	return (Bitmap)Image.FromFile(fileName);
	//	//}

	//	return null;
	//}

	public void SetAvailableDlcs(IEnumerable<uint> dlcs)
	{
		_config.AvailableDLCs = dlcs.ToArray();
		_config.Serialize();
	}

	public bool IsDlcExcluded(uint dlc)
	{
		return _config.RemovedDLCs.Contains(dlc);
	}

	public void SetDlcsExcluded(uint[] dlc)
	{
		_config.RemovedDLCs = dlc;

		_profileManager.TriggerAutoSave();
		SaveChanges();
	}

	public void SetDlcExcluded(uint dlc, bool excluded)
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

		_profileManager.TriggerAutoSave();
		SaveChanges();
	}

	public IEnumerable<string> GetAllFindItTags()
	{
		foreach (var item in _findItTags.assetTags)
		{
			foreach (var tag in item.Value.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries))
			{
				yield return tag;
			}
		}
	}

	public IEnumerable<string> GetFindItTags(IPackage package)
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

	public void SetFindItTag(IPackage package, string tag)
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

	public void RemoveFindItTag(Asset asset, string tag)
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
