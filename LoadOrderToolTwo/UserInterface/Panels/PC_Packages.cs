using Extensions;

using LoadOrderToolTwo.Domain;
using LoadOrderToolTwo.Utilities;
using LoadOrderToolTwo.Utilities.Managers;

using System.Collections.Generic;
using System.Linq;

namespace LoadOrderToolTwo.UserInterface.Panels;
internal class PC_Packages : PC_ContentList<Package>
{
	public PC_Packages()
	{

	}

	protected override void LocaleChanged()
	{
		base.LocaleChanged();

		Text = $"{Locale.Package.Plural} - {ProfileManager.CurrentProfile.Name}";
	}

	protected override IEnumerable<Package> GetItems()
	{
		if (CentralManager.SessionSettings.UserSettings.FilterOutPackagesWithOneAsset || CentralManager.SessionSettings.UserSettings.FilterOutPackagesWithMods)
		{
			return CentralManager.Packages.Where(x =>
			{
				if (CentralManager.SessionSettings.UserSettings.FilterOutPackagesWithOneAsset && (x.Assets?.Count() ?? 0) == 1)
				{
					return false;
				}

				if (CentralManager.SessionSettings.UserSettings.FilterOutPackagesWithMods && x.Mod is not null)
				{
					return false;
				}

				return true;
			});
		}

		return CentralManager.Packages;
	}

	protected override string GetCountText()
	{
		int packagesIncluded = 0, modsIncluded = 0, modsEnabled = 0;

		foreach (var item in CentralManager.Packages)
		{
			if (item.IsIncluded)
			{
				packagesIncluded++;

				if (item.Mod is not null)
				{
					modsIncluded++;

					if (item.Mod.IsEnabled)
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
		return Locale.ShowingPackages.FormatPlural(filteredCount);
	}

	protected override void SetIncluded(IEnumerable<Package> filteredItems, bool included)
	{
		ContentUtil.SetBulkIncluded(filteredItems, included);
	}

	protected override void SetEnabled(IEnumerable<Package> filteredItems, bool enabled)
	{
		ContentUtil.SetBulkIncluded(filteredItems.Where(x => x.Mod is not null).Select(x => x.Mod!), enabled);
	}
}
