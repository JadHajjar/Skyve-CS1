using Extensions;

using SkyveApp.Domain;
using SkyveApp.Domain.CS1;
using SkyveApp.Domain.CS1.Utilities;
using SkyveApp.Domain.Systems;

using SkyveShared;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SkyveApp.Systems.CS1.Utilities;
internal class AssetsUtil : IAssetUtil
{
	private readonly AssetConfig _config;
	private Dictionary<string, IAsset> assetIndex = new();

	public HashSet<string> ExcludedHashSet { get; }
	public Dictionary<string, CSCache.Asset> AssetInfoCache { get; }

	private readonly IPackageManager _contentManager;
	private readonly INotifier _notifier;

	public AssetsUtil(IPackageManager contentManager, INotifier notifier)
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

		_config = AssetConfig.Deserialize() ?? new();

		ExcludedHashSet = new HashSet<string>(_config.ExcludedAssets, new PathEqualityComparer());

		_notifier.ContentLoaded += BuildAssetIndex;
	}

	public IEnumerable<IAsset> GetAssets(ILocalPackageWithContents package, bool withSubDirectories = true)
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
		if (_notifier.ApplyingPlayset || _notifier.BulkUpdating)
		{
			return;
		}

		_config.ExcludedAssets = ExcludedHashSet.ToList();

		_config.Serialize();
	}

	internal void SetExcludedAssets(IEnumerable<string> excludedAssets)
	{
		ExcludedHashSet.Clear();

		foreach (var item in excludedAssets)
		{
			ExcludedHashSet.Add(item);
		}

		SaveChanges();
	}

	public IAsset? GetAssetByFile(string? v)
	{
		return assetIndex.TryGet(v ?? string.Empty);
	}

	public void BuildAssetIndex()
	{
		assetIndex = _contentManager.Assets.ToDictionary(x => x.FilePath.FormatPath(), StringComparer.OrdinalIgnoreCase);
	}
}
