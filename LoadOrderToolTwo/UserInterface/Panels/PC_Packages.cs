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

		Text = $"{Locale.Packages} - {ProfileManager.CurrentProfile.Name}";
	}

	protected override IEnumerable<Package> GetItems()
	{
		if (CentralManager.SessionSettings.FilterOutPackagesWithOneAsset || CentralManager.SessionSettings.FilterOutPackagesWithMods)
		{
			return CentralManager.Packages.Where(x => !(x.Mod is not null && CentralManager.SessionSettings.FilterOutPackagesWithMods) && !(CentralManager.SessionSettings.FilterOutPackagesWithOneAsset && x.Assets?.Count() > 1));
		}

		return CentralManager.Packages;
	}

	protected override string GetCountText()
	{
		var packagesIncluded = CentralManager.Packages.Count(x => x.IsIncluded);
		var total = LC_Items.ItemCount;
		var text = string.Empty;

		return $"{packagesIncluded} {(packagesIncluded == 1 ? Locale.PackageIncluded : Locale.PackageIncludedPlural)}, {total} {Locale.Total.ToLower()}";
	}

	protected override string GetFilteredCountText(int filteredCount)
	{
		return $"{Locale.Showing} {filteredCount} {Locale.Packages.ToLower()}";
	}

	protected override void SetIncluded(IEnumerable<Package> filteredItems, bool included)
	{
		ModsUtil.SetIncluded(filteredItems.Where(x => x.Mod is not null).Select(x => x.Mod!), included);
		AssetsUtil.SetIncluded(filteredItems.Where(x => x.Assets is not null).SelectMany(x => x.Assets!), included);
	}

	protected override void SetEnabled(IEnumerable<Package> filteredItems, bool enabled)
	{
		ModsUtil.SetEnabled(filteredItems.Where(x => x.Mod is not null).Select(x => x.Mod!), enabled);
	}
}
