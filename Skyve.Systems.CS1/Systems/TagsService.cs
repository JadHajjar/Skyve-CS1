using Extensions;

using Skyve.Domain;
using Skyve.Domain.CS1;
using Skyve.Domain.CS1.Enums;
using Skyve.Domain.CS1.Utilities;
using Skyve.Domain.Systems;
using Skyve.Systems.CS1.Utilities;

using SkyveShared;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Skyve.Systems.CS1.Systems;
internal class TagsService : ITagsService
{
	private readonly HashSet<string> _assetTags;
	private readonly Dictionary<string, int> _workshopTags;
	private readonly Dictionary<string, string[]> _assetTagsDictionary;
	private readonly Dictionary<string, string[]> _customTagsDictionary;
	private readonly Dictionary<string, HashSet<string>> _tagsCache;

	private readonly INotifier _notifier;
	private readonly ILogger _logger;
	private readonly IWorkshopService _workshopService;

	public TagsService(INotifier notifier, IWorkshopService workshopService, IAssetUtil assetUtil, ILogger logger)
	{
		_assetTagsDictionary = new(new PathEqualityComparer());
		_customTagsDictionary = new(new PathEqualityComparer());
		_tagsCache = new(StringComparer.InvariantCultureIgnoreCase);
		_notifier = notifier;
		_logger = logger;
		_workshopService = workshopService;
		_assetTags = new HashSet<string>();
		_workshopTags = new Dictionary<string, int>();
		var findItTags = CustomTagsLibrary.Deserialize() ?? new();

		var csCache = AssetInfoCache.Deserialize() ?? new();

		if (csCache is not null)
		{
			foreach (var asset in (assetUtil as AssetsUtil)!.AssetInfoCache ?? new())
			{
				if (asset.Value.Tags is not null)
				{
					_assetTagsDictionary[asset.Key] = asset.Value.Tags;

					foreach (var tag in asset.Value.Tags)
					{
						_assetTags.Add(tag);
					}
				}
			}
		}

		ISave.Load(out Dictionary<string, string[]> customTags, "CustomTags.json");

		if (customTags is not null)
		{
			foreach (var tag in customTags)
			{
				_customTagsDictionary[tag.Key] = tag.Value;
			}
		}

		if (findItTags is not null)
		{
			foreach (var tag in findItTags.assetTags)
			{
				if (!_customTagsDictionary.ContainsKey(tag.Key))
				{
					_customTagsDictionary[tag.Key] = tag.Value.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
				}
				else
				{
					_customTagsDictionary[tag.Key] = _customTagsDictionary[tag.Key].Concat(tag.Value.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)).Distinct().ToArray();
				}
			}
		}

		_notifier.WorkshopInfoUpdated += UpdateWorkshopTags;
		_notifier.ContentLoaded += GenerateCache;

		Task.Run(UpdateWorkshopTags);
		Task.Run(GenerateCache);
	}

	private void GenerateCache()
	{
		_tagsCache.Clear();

		foreach (var kvp in _assetTagsDictionary)
		{
			foreach (var item in kvp.Value)
			{
				if (!_tagsCache.ContainsKey(item))
				{
					_tagsCache[item] = new(new PathEqualityComparer());
				}

				_tagsCache[item].Add(kvp.Key);
			}
		}

		foreach (var kvp in _customTagsDictionary)
		{
			foreach (var item in kvp.Value)
			{
				if (!_tagsCache.ContainsKey(item))
				{
					_tagsCache[item] = new(new PathEqualityComparer());
				}

				_tagsCache[item].Add(kvp.Key);
			}
		}

		var assetDictionary = new Dictionary<string, IAsset>(StringComparer.CurrentCultureIgnoreCase);

		foreach (var asset in ServiceCenter.Get<IPackageManager>().Assets)
		{
			assetDictionary[(asset as Asset)!.FullName] = asset;
		}

		foreach (var kvp in CustomTagsLibrary.Deserialize().assetTags)
		{
			if (assetDictionary.TryGetValue(kvp.Key, out var asset))
			{
				_customTagsDictionary[asset.FilePath] = (_customTagsDictionary.TryGet(asset.FilePath) ?? new string[0]).Concat(kvp.Value.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)).Distinct().ToArray();

				foreach (var item in kvp.Value.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries))
				{
					if (!_tagsCache.ContainsKey(item))
					{
						_tagsCache[item] = new(new PathEqualityComparer());
					}

					_tagsCache[item].Add(asset.FilePath);

				}
			}
		}
	}

	private void UpdateWorkshopTags()
	{
		try
		{
			var packages = _workshopService.GetAllPackages().ToList();

			lock (_workshopTags)
			{
				_workshopTags.Clear();

				foreach (var package in packages)
				{
					foreach (var tag in package.Tags)
					{
						_workshopTags[tag] = _workshopTags.GetOrAdd(tag) + 1;
					}
				}
			}
		}
		catch (Exception ex) { _logger.Warning("Failed to update workshop tags: " + ex.Message); }
	}

	public IEnumerable<ITag> GetDistinctTags()
	{
		var returned = new List<string>();

		lock (_workshopTags)
		{
			foreach (var item in _workshopTags)
			{
				if (!returned.Contains(item.Key))
				{
					returned.Add(item.Key);
					yield return new TagItem(TagSource.Workshop, item.Key);
				}
			}
		}

		foreach (var item in _assetTags)
		{
			if (!returned.Contains(item))
			{
				returned.Add(item);
				yield return new TagItem(TagSource.InGame, item);
			}
		}

		foreach (var kvp in _customTagsDictionary)
		{
			foreach (var item in kvp.Value)
			{
				if (!returned.Contains(item))
				{
					returned.Add(item);

					yield return new TagItem(TagSource.Custom, item);
				}
			}
		}
	}

	public IEnumerable<ITag> GetTags(IPackage package, bool ignoreParent = false)
	{
		var returned = new List<string>();

		if (!ignoreParent && package.GetWorkshopInfo()?.Tags is string[] workshopTags)
		{
			foreach (var item in workshopTags)
			{
				if (!returned.Contains(item))
				{
					returned.Add(item);
					yield return new TagItem(TagSource.Workshop, item);
				}
			}
		}

		if (package is IAsset asset)
		{
			if (_assetTagsDictionary.TryGetValue(asset.FilePath, out var assetTags))
			{
				foreach (var item in assetTags)
				{
					if (!returned.Contains(item))
					{
						returned.Add(item);
						yield return new TagItem(TagSource.InGame, item);
					}
				}
			}
		}

		if (package.LocalPackage is ILocalPackage localPackage)
		{
			if (_customTagsDictionary.TryGetValue(localPackage.FilePath, out var customTags))
			{
				foreach (var item in customTags)
				{
					if (!returned.Contains(item))
					{
						returned.Add(item);
						yield return new TagItem(TagSource.Custom, item);
					}
				}
			}
		}

		if (!ignoreParent && package.LocalParentPackage is ILocalPackageWithContents lp && _customTagsDictionary.TryGetValue(lp.Folder, out var customParentTags))
		{
			foreach (var item in customParentTags)
			{
				if (!returned.Contains(item))
				{
					returned.Add(item);
					yield return new TagItem(TagSource.Custom, item);
				}
			}
		}
	}

	public void SetTags(IPackage package, IEnumerable<string> value)
	{
		if (package is IAsset asset)
		{
			var newTags = CustomTagsLibrary.Deserialize();

			var found = false;
			var tag = value.WhereNotEmpty().ListStrings(" ");
			var key = (asset.IsLocal ? "" : $"{asset.Id}.") + Path.GetFileNameWithoutExtension(asset.FilePath).RemoveDoubleSpaces().Replace(' ', '_');

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

			_customTagsDictionary[asset.FilePath] = value.WhereNotEmpty().ToArray();

			ISave.Save(_customTagsDictionary, "CustomTags.json");
		}
		else if (package.LocalParentPackage is ILocalPackageWithContents lp)
		{
			_customTagsDictionary[lp.Folder] = value.WhereNotEmpty().ToArray();

			ISave.Save(_customTagsDictionary, "CustomTags.json");
		}

		_notifier.OnRefreshUI(true);
	}

	public bool HasAllTags(IPackage package, IEnumerable<ITag> tags)
	{
		var workshopTags = package.GetWorkshopInfo()?.Tags;

		if (package is ILocalPackage localPackage)
		{
			foreach (var tag in tags)
			{
				if (_tagsCache.TryGetValue(tag.Value, out var hash) && hash.Contains(localPackage.FilePath))
				{
					continue;
				}

				if (workshopTags?.Any(x => x.Equals(tag.Value, StringComparison.InvariantCultureIgnoreCase)) ?? false)
				{
					continue;
				}

				return false;
			}

			return true;
		}

		foreach (var tag in tags)
		{
			if (workshopTags?.Any(x => x.Equals(tag.Value, StringComparison.InvariantCultureIgnoreCase)) ?? false)
			{
				continue;
			}

			return false;
		}

		return true;
	}

	public int GetTagUsage(ITag tag)
	{
		lock (_workshopTags)
		{
			return
				(_workshopTags.TryGetValue(tag.Value, out var count) ? count : 0) +
				(_tagsCache.TryGetValue(tag.Value, out var hash) ? hash.Count : 0);
		}
	}

	public ITag CreateGlobalTag(string text)
	{
		return new TagItem(TagSource.Global, text);
	}

	public ITag CreateCustomTag(string text)
	{
		return new TagItem(TagSource.Custom, text);
	}

	public ITag CreateWorkshopTag(string text)
	{
		return new TagItem(TagSource.Workshop, text);
	}
}
