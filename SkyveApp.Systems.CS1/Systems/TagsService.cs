using Extensions;

using SkyveApp.Domain;
using SkyveApp.Domain.CS1;
using SkyveApp.Domain.CS1.Steam;
using SkyveApp.Domain.CS1.Utilities;
using SkyveApp.Domain.Systems;

using SkyveShared;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkyveApp.Systems.CS1.Systems;
internal class TagsService : ITagsService
{
	private readonly HashSet<string> _assetTags;
	private readonly HashSet<string> _workshopTags;
	private readonly Dictionary<string, string[]> _assetTagsDictionary;
	private readonly Dictionary<string, string[]> _customTagsDictionary;

	private readonly IWorkshopService _workshopService;

	private CustomTagsLibrary _findItTags;

	public TagsService(INotifier notifier, IWorkshopService workshopService)
	{
		_assetTagsDictionary = new(new PathEqualityComparer());
		_customTagsDictionary = new(new PathEqualityComparer());
		_workshopService = workshopService;
		_assetTags = new HashSet<string>();
		_workshopTags = new HashSet<string>();
		_findItTags = new();
		_findItTags.Deserialize();

		var csCache = CSCache.Deserialize();

		if (csCache != null )
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

		notifier.WorkshopInfoUpdated += UpdateWorkshopTags;
		UpdateWorkshopTags();
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
		foreach (var item in _workshopTags)
		{
			yield return new TagItem(Domain.CS1.Enums.TagSource.Workshop, item);
		}

		foreach (var item in _assetTags)
		{
			yield return new TagItem(Domain.CS1.Enums.TagSource.InGame, item);
		}

		foreach (var item in _findItTags.assetTags)
		{
			foreach (var tag in item.Value.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries))
			{
				yield return new TagItem(Domain.CS1.Enums.TagSource.FindIt, tag);
			}
		}
	}

	public IEnumerable<ITag> GetTags(IPackage package, bool ignoreParent = false)
	{
		if (package is IAsset asset)
		{
			if (_assetTagsDictionary.TryGetValue(asset.FilePath, out var assetTags))
			{
				foreach (var item in assetTags)
				{
					yield return new TagItem(Domain.CS1.Enums.TagSource.InGame, item);
				}
			}

			var key = (asset.IsLocal ? "" : $"{asset.Id}.") + Path.GetFileNameWithoutExtension(asset.FilePath).RemoveDoubleSpaces().Replace(' ', '_');

			foreach (var item in _findItTags.assetTags)
			{
				if (item.Key.RemoveDoubleSpaces().Replace(' ', '_').Equals(key.RemoveDoubleSpaces().Replace(' ', '_'), StringComparison.CurrentCultureIgnoreCase))
				{
					foreach (var tag in item.Value.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries))
					{
						yield return new TagItem(Domain.CS1.Enums.TagSource.FindIt, tag);
					}
				}
			}
		}
		else if (package.LocalParentPackage is ILocalPackageWithContents lp && _customTagsDictionary.TryGetValue(lp.Folder, out var customTags))
		{
			foreach (var item in customTags)
			{
				yield return new TagItem(Domain.CS1.Enums.TagSource.Custom, item);
			}
		}

		if (package.GetWorkshopInfo()?.Tags is string[] workshopTags)
		{
			foreach (var item in workshopTags)
			{
				yield return new TagItem(Domain.CS1.Enums.TagSource.Workshop, item);
			}
		}
	}

	public void SetTags(IPackage package, IEnumerable< string> value)
	{
		if (package is IAsset asset)
		{
			var newTags = new CustomTagsLibrary();

			newTags.Deserialize();

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

			_findItTags = newTags;
			_findItTags.Serialize();
		}
		else if (package.LocalParentPackage is ILocalPackageWithContents lp)
		{
			_customTagsDictionary[lp.Folder] = value.WhereNotEmpty().ToArray();


		}
	}
}
