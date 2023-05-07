using Extensions;

using LoadOrderToolTwo.Domain;
using LoadOrderToolTwo.Domain.Interfaces;
using LoadOrderToolTwo.Utilities;
using LoadOrderToolTwo.Utilities.Managers;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LoadOrderToolTwo.UserInterface.Panels;
internal class PC_GenericPackageList : PC_ContentList<IPackage>
{
	private readonly Dictionary<ulong, Profile.Asset> _workshopPackages = new();
	private readonly List<IPackage> _items = new();

	public PC_GenericPackageList(IEnumerable<IPackage> items) : base(true)
	{
		LC_Items.IsGenericPage = true;

		TB_Search.Placeholder = "SearchGenericPackages";

		var cachedSteamInfo = SteamUtil.GetCachedInfo() ?? new();

		foreach (var package in items.GroupBy(x => x.SteamId))
		{
			if (package.Key != 0)
			{
				if (CompatibilityManager.IsBlacklisted(package.First()))
				{
					continue;
				}

				var steamPackage = package.Last();

				if (steamPackage.RemovedFromSteam)
				{
					continue;
				}

				_items.Add(steamPackage);

				if (steamPackage is Profile.Asset profileAsset)
				{
					profileAsset.WorkshopInfo = steamPackage.Package?.WorkshopInfo;

					if (profileAsset.WorkshopInfo == null)
					{
						if (cachedSteamInfo.ContainsKey(package.Key) && DateTime.UtcNow - cachedSteamInfo[package.Key].Timestamp < TimeSpan.FromDays(2))
						{
							profileAsset.WorkshopInfo = cachedSteamInfo[package.Key];
						}
						else
						{
							_workshopPackages[package.Key] = profileAsset;
						}
					}
				}
			}
			else
			{
				_items.AddRange(package);
			}
		}

		LC_Items.SetItems(_items);

		RefreshCounts();

		new BackgroundAction("Getting tag list", RefreshAuthorAndTags).Run();
	}

	protected override async Task<bool> LoadDataAsync()
	{
		var steamIds = _workshopPackages.Keys.Distinct().ToArray();

		var info = await SteamUtil.GetWorkshopInfoAsync(steamIds);

		foreach (var item in info)
		{
			if (_workshopPackages.ContainsKey(item.Key))
			{
				_workshopPackages[item.Key].WorkshopInfo = item.Value;
			}
		}

		RefreshAuthorAndTags();

		LC_Items.Invalidate();

		Parallelism.ForEach(LC_Items.Items.ToList(), async package =>
		{
			if (!string.IsNullOrWhiteSpace(package.IconUrl))
			{
				if (await ImageManager.Ensure(package.IconUrl))
					LC_Items.Invalidate(package);
			}

			if (!string.IsNullOrWhiteSpace(package.Author?.AvatarUrl))
			{
				if (await ImageManager.Ensure(package.Author?.AvatarUrl))
					LC_Items.Invalidate(package);
			}
		}, 4);

		return true;
	}

	protected override IEnumerable<IPackage> GetItems()
	{
		return _items;
	}

	protected override string GetCountText()
	{
		int packagesIncluded = 0, modsIncluded = 0, modsEnabled = 0;

		foreach (var item in _items)
		{
			var package = item.Package;

			if (package is null)
			{
				continue;
			}

			if (package.IsIncluded)
			{
				packagesIncluded++;

				if (package.Mod is not null)
				{
					modsIncluded++;

					if (package.Mod.IsEnabled)
					{
						modsEnabled++;
					}
				}
			}
		}

		var total = LC_Items.ItemCount;

		if (!CentralManager.SessionSettings.UserSettings.AdvancedIncludeEnable)
		{
			return string.Format(Locale.PackageIncludedTotal, packagesIncluded, total);
		}

		if (modsIncluded == modsEnabled)
		{
			return string.Format(Locale.PackageIncludedAndEnabledTotal, packagesIncluded, total);
		}

		return string.Format(Locale.PackageIncludedEnabledTotal, packagesIncluded, modsIncluded, modsEnabled, total);
	}

	protected override string GetFilteredCountText(int filteredCount)
	{
		return string.Format(Locale.ShowingPackages, filteredCount);
	}

	protected override void SetIncluded(IEnumerable<IPackage> filteredItems, bool included)
	{
		ContentUtil.SetBulkIncluded(filteredItems.SelectWhereNotNull(x => x.Package)!, included);
	}

	protected override void SetEnabled(IEnumerable<IPackage> filteredItems, bool enabled)
	{
		ContentUtil.SetBulkIncluded(filteredItems.Where(x => x.Package?.Mod is not null).Select(x => x.Package!.Mod!), enabled);
	}
}
