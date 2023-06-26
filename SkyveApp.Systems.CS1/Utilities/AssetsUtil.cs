using Extensions;

using SkyveApp.Domain;
using SkyveApp.Domain.Interfaces;
using SkyveApp.Domain.Systems;
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
	private Dictionary<string, IAsset> assetIndex = new();

	public HashSet<string> ExcludedHashSet { get; }
	public Dictionary<string, CSCache.Asset> AssetInfoCache { get; }

	private readonly IContentManager _contentManager;
	private readonly INotifier _notifier;

	public AssetsUtil(IContentManager contentManager, INotifier notifier)
	{
		_contentManager = contentManager;
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

	public IEnumerable<IAsset> GetAssets(ILocalPackage package, bool withSubDirectories = true)
	{
		if (!Directory.Exists(package.Folder))
		{
			yield break;
		}

		var files = Directory.GetFiles(package.Folder, $"*.crp", withSubDirectories ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);

		foreach (var file in files)
		{
			var fileName = file.FormatPath();

			AssetInfoCache.TryGetValue(fileName, out var asset);

			yield return new Asset(package, fileName, asset);
		}
	}

	public bool IsIncluded(IAsset asset)
	{
		return !ExcludedHashSet.Contains(asset.FilePath.ToLower());
	}

	public void SetIncluded(IAsset asset, bool value)
	{
		if (value)
		{
			ExcludedHashSet.Remove(asset.FilePath.ToLower());
		}
		else
		{
			ExcludedHashSet.Add(asset.FilePath.ToLower());
		}

		_notifier.OnInclusionUpdated();
		_notifier.TriggerAutoSave();

		SaveChanges();
	}

	public void SaveChanges()
	{
		if (_notifier.ApplyingProfile || _notifier.BulkUpdating)
		{
			return;
		}

		_config.Assets = ExcludedHashSet
				.Select(x => new AssetInfo { Path = x })
				.ToArray();

		_config.Serialize();
	}

	public IAsset? GetAssetByFile(string? v)
	{
		return assetIndex.TryGet(v??string.Empty);
	}

	public void BuildAssetIndex()
	{
		assetIndex = _contentManager.Assets.ToDictionary(x => x.FilePath.FormatPath(), StringComparer.OrdinalIgnoreCase);
	}

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

		_notifier.TriggerAutoSave();
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

		_notifier.TriggerAutoSave();
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

	public IEnumerable<string> GetFindItTags(ILocalPackage package)
	{
		var key = package is Asset asset
			? (asset.Id == 0 ? "" : $"{asset.Id}.") + Path.GetFileNameWithoutExtension(asset.FilePath).RemoveDoubleSpaces().Replace(' ', '_')
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

	public void SetFindItTag(ILocalPackage package, string tag)
	{
		var newTags = new CustomTagsLibrary();

		newTags.Deserialize();

		var found = false;
		var key = package is Asset asset
			? (asset.Id == 0 ? "" : $"{asset.Id}.") + Path.GetFileNameWithoutExtension(asset.FilePath)
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

		var assetName = (asset.Id == 0 ? "" : $"{asset.Id}.") + Path.GetFileNameWithoutExtension(asset.FilePath).RemoveDoubleSpaces().Replace(' ', '_');

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
