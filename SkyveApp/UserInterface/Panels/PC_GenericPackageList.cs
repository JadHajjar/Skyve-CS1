using Extensions;

using SkyveApp.Domain;
using SkyveApp.Domain.Interfaces;
using SkyveApp.Utilities;
using SkyveApp.Utilities.Managers;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SkyveApp.UserInterface.Panels;
internal class PC_GenericPackageList : PC_ContentList<IPackage>
{
	private readonly Dictionary<ulong, Profile.Asset> _workshopPackages = new();
	private readonly List<IPackage> _items = new();

	public PC_GenericPackageList(IEnumerable<IPackage> items) : base(true)
	{
		LC_Items.IsGenericPage = true;

		TB_Search.Placeholder = "SearchGenericPackages";

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
					profileAsset.WorkshopInfo = SteamUtil.GetItem(package.Key);
				}
			}
			else
			{
				_items.AddRange(package);
			}
		}

		SteamUtil.WorkshopItemsLoaded += () =>
		{
			LC_Items.Invalidate();

			new BackgroundAction("Getting tag list", RefreshAuthorAndTags).Run();
		};

		LC_Items.SetItems(_items);

		RefreshCounts();

		new BackgroundAction("Getting tag list", RefreshAuthorAndTags).Run();
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

	protected override LocaleHelper.Translation GetItemText()
	{
		return Locale.Package;
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
