using Extensions;

using Skyve.Domain;
using Skyve.Domain.CS1;
using Skyve.Domain.Systems;

using SkyveShared;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace Skyve.Systems.CS1.Utilities;
internal class AssetsUtil : IAssetUtil
{
	private readonly AssetConfig _config;
	private Dictionary<string, IAsset> assetIndex = new();

	public HashSet<string> ExcludedHashSet { get; }
	public Dictionary<string, AssetInfoCache.Asset> AssetInfoCache { get; }

	private readonly IPackageManager _contentManager;
	private readonly INotifier _notifier;

	public AssetsUtil(IPackageManager contentManager, INotifier notifier)
	{
		_contentManager = contentManager;
		_notifier = notifier;

		AssetInfoCache = new(new PathEqualityComparer());

		var assets = SkyveShared.AssetInfoCache.Deserialize().Assets;

		if (assets is not null)
		{
			foreach (var item in assets)
			{
				AssetInfoCache[item.Path] = item;
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
#if DEBUG
			ServiceCenter.Get<ILogger>().Debug("Getting assets failed, directory not found: " + package.Folder);
#endif
			yield break;
		}

		var files = Directory.GetFiles(package.Folder, $"*.crp", withSubDirectories ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);

		foreach (var file in files)
		{
			if (Regex.IsMatch(Path.GetFileName(file), ExtensionClass.CharBlackListPattern))
				continue;

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

		if (_notifier.ApplyingPlayset || _notifier.BulkUpdating)
		{
			return;
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
