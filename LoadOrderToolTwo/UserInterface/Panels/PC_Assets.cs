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
		return CentralManager.Assets;
	}

	protected override string GetCountText()
	{
		var assetsIncluded = CentralManager.Assets.Count(x => x.IsIncluded);
		var total = LC_Items.ItemCount;
		var text = string.Empty;

		return $"{assetsIncluded} {(assetsIncluded == 1 ? Locale.AssetIncluded : Locale.AssetIncludedPlural)}, {total} {Locale.Total.ToLower()}";
	}

	protected override string GetFilteredCountText(int filteredCount)
	{
		return $"{Locale.Showing} {filteredCount} {Locale.Assets.ToLower()}";
	}

	protected override void SetIncluded(IEnumerable<Asset> filteredItems, bool included)
	{
		AssetsUtil.SetIncluded(filteredItems, included);
	}

	protected override void SetEnabled(IEnumerable<Asset> filteredItems, bool enabled)
	{
	}
}
