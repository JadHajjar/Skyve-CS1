using LoadOrderToolTwo.Domain;
using LoadOrderToolTwo.Utilities;
using LoadOrderToolTwo.Utilities.Managers;

using System.Collections.Generic;
using System.Linq;

namespace LoadOrderToolTwo.UserInterface.Panels;
internal class PC_Assets : PC_ContentList<Asset>
{
	public PC_Assets()
	{

	}

	protected override void LocaleChanged()
	{
		base.LocaleChanged();

		Text = $"{Locale.Assets} - {ProfileManager.CurrentProfile.Name}";
	}

	protected override IEnumerable<Asset> GetItems()
	{
		if (CentralManager.SessionSettings.UserSettings.LinkModAssets)
		{
			return CentralManager.Assets.Where(x => x.Package.Mod is null);
		}

		return CentralManager.Assets;
	}

	protected override string GetCountText()
	{
		var assetsIncluded = CentralManager.Assets.Count(x => x.IsIncluded);
		var total = LC_Items.ItemCount;
		var text = string.Empty;

		return string.Format(Locale.AssetIncludedTotal, assetsIncluded, total);
	}

	protected override string GetFilteredCountText(int filteredCount)
	{
		return string.Format(Locale.ShowingAssets, filteredCount);
	}

	protected override void SetIncluded(IEnumerable<Asset> filteredItems, bool included)
	{
		AssetsUtil.SetIncluded(filteredItems, included);
	}

	protected override void SetEnabled(IEnumerable<Asset> filteredItems, bool enabled)
	{
	}
}
