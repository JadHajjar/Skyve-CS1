using Extensions;

using SkyveApp.Domain;
using SkyveApp.Services;
using SkyveApp.Services.Interfaces;
using SkyveApp.Utilities;

using System.Collections.Generic;
using System.Linq;

namespace SkyveApp.UserInterface.Panels;
internal class PC_Packages : PC_ContentList<Package>
{
	private readonly ISettings _settings = Program.Services.GetService<ISettings>();
	private readonly IContentManager _contentManager = Program.Services.GetService<IContentManager>();

	public PC_Packages()
	{
	}

	protected override void LocaleChanged()
	{
		base.LocaleChanged();

		Text = $"{Locale.Package.Plural} - {Program.Services.GetService<IPlaysetManager>().CurrentPlayset.Name}";
	}

	protected override IEnumerable<Package> GetItems()
	{
		if (_settings.SessionSettings.UserSettings.FilterOutPackagesWithOneAsset || _settings.SessionSettings.UserSettings.FilterOutPackagesWithMods)
		{
			return _contentManager.Packages.Where(x =>
			{
				if (_settings.SessionSettings.UserSettings.FilterOutPackagesWithOneAsset && (x.Assets?.Count() ?? 0) == 1)
				{
					return false;
				}

				if (_settings.SessionSettings.UserSettings.FilterOutPackagesWithMods && x.Mod is not null)
				{
					return false;
				}

				return true;
			});
		}

		return _contentManager.Packages;
	}

	protected override string GetCountText()
	{
		int packagesIncluded = 0, modsIncluded = 0, modsEnabled = 0;

		foreach (var item in _contentManager.Packages)
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

		if (!_settings.SessionSettings.UserSettings.AdvancedIncludeEnable)
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

	protected override void SetIncluded(IEnumerable<Package> filteredItems, bool included)
	{
		Program.Services.GetService<IContentUtil>().SetBulkIncluded(filteredItems, included);
	}

	protected override void SetEnabled(IEnumerable<Package> filteredItems, bool enabled)
	{
		Program.Services.GetService<IContentUtil>().SetBulkIncluded(filteredItems.Where(x => x.Mod is not null).Select(x => x.Mod!), enabled);
	}
}
