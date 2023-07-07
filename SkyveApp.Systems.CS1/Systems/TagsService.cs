﻿using Extensions;

using SkyveApp.Domain;
using SkyveApp.Domain.CS1;
using SkyveApp.Domain.CS1.Utilities;
using SkyveApp.Domain.Systems;

using SkyveShared;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace SkyveApp.Systems.CS1.Systems;
internal class TagsService : ITagsService
{
	private readonly HashSet<string> _assetTags;
	private readonly HashSet<string> _workshopTags;
	private readonly Dictionary<string, string[]> _assetTagsDictionary;
	private readonly Dictionary<string, string[]> _customTagsDictionary;
	private readonly Dictionary<string, HashSet<string>> _tagsCache;

	private readonly IWorkshopService _workshopService;

	public TagsService(INotifier notifier, IWorkshopService workshopService)
	{
		_assetTagsDictionary = new(new PathEqualityComparer());
		_customTagsDictionary = new(new PathEqualityComparer());
		_tagsCache = new(StringComparer.InvariantCultureIgnoreCase);
		_workshopService = workshopService;
		_assetTags = new HashSet<string>();
		_workshopTags = new HashSet<string>();
		var findItTags =  CustomTagsLibrary.Deserialize();

		var csCache = CSCache.Deserialize();

		if (csCache is not null)
		{
			foreach (var asset in csCache.Assets)
			{
				_assetTagsDictionary[asset.IncludedPath] = asset.Tags;

				foreach (var tag in asset.Tags)
				{
					_assetTags.Add(tag);
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

		notifier.WorkshopInfoUpdated += UpdateWorkshopTags;
		notifier.ContentLoaded += GenerateCache;

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
			assetDictionary[(asset.IsLocal ? "" : $"{asset.Id}.") + Path.GetFileNameWithoutExtension(asset.FilePath).RemoveDoubleSpaces().Replace(' ', '_')] = asset;
		}

		foreach (var kvp in CustomTagsLibrary.Deserialize().assetTags)
		{
			if (assetDictionary.TryGetValue(kvp.Key.RemoveDoubleSpaces().Replace(' ', '_'), out var asset))
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
		var packages = _workshopService.GetAllPackages().ToList();

		foreach (var package in packages)
		{
			foreach (var tag in package.Tags)
			{
				_workshopTags.Add(tag);
			}
		}
	}

	public IEnumerable<ITag> GetDistinctTags()
	{
		var returned = new List<string>();

		foreach (var item in _workshopTags)
		{
			if (!returned.Contains(item))
			{
				returned.Add(item);
				yield return new TagItem(Domain.CS1.Enums.TagSource.Workshop, item);
			}
		}

		foreach (var item in _assetTags)
		{
			if (!returned.Contains(item))
			{
				returned.Add(item);
				yield return new TagItem(Domain.CS1.Enums.TagSource.InGame, item);
			}
		}

		foreach (var kvp in _customTagsDictionary)
		{
			foreach (var item in kvp.Value)
			{
				if (!returned.Contains(item))
				{
					returned.Add(item);

					yield return new TagItem(Domain.CS1.Enums.TagSource.Custom, item);
				}
			}
		}
	}

	public IEnumerable<ITag> GetTags(IPackage package, bool ignoreParent = false)
	{
		var returned = new List<string>();
	
		if (package.GetWorkshopInfo()?.Tags is string[] workshopTags)
		{
			foreach (var item in workshopTags)
			{
				if (!returned.Contains(item))
				{
					returned.Add(item);
					yield return new TagItem(Domain.CS1.Enums.TagSource.Workshop, item);
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
						yield return new TagItem(Domain.CS1.Enums.TagSource.InGame, item);
					}
				}
			}

			if (_customTagsDictionary.TryGetValue(asset.FilePath, out var customTags))
			{
				foreach (var item in customTags)
				{
					if (!returned.Contains(item))
					{
						returned.Add(item);
						yield return new TagItem(Domain.CS1.Enums.TagSource.Custom, item);
					}
				}
			}
		}
		else if (package.LocalParentPackage is ILocalPackageWithContents lp && _customTagsDictionary.TryGetValue(lp.Folder, out var customTags))
		{
			foreach (var item in customTags)
			{
				if (!returned.Contains(item))
				{
					returned.Add(item);
					yield return new TagItem(Domain.CS1.Enums.TagSource.Custom, item);
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
}