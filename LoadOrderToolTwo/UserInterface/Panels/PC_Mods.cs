using LoadOrderToolTwo.Domain;
using LoadOrderToolTwo.Utilities;
using LoadOrderToolTwo.Utilities.Managers;

using System.Collections.Generic;
using System.Linq;

namespace LoadOrderToolTwo.UserInterface.Panels;
internal class PC_Mods : PC_ContentList<Mod>
{
	public PC_Mods()
	{

	}

	protected override void LocaleChanged()
	{
		base.LocaleChanged();

		Text = $"{Locale.Mods} - {ProfileManager.CurrentProfile.Name}";
	}

	protected override IEnumerable<Mod> GetItems()
	{
		return CentralManager.Mods;
	}

	protected override string GetCountText()
	{
		var modsIncluded = CentralManager.Mods.Count(x => x.IsIncluded);
		var modsEnabled = CentralManager.Mods.Count(x => x.IsEnabled && x.IsIncluded);
		var total = LC_Items.ItemCount;

		if (!CentralManager.SessionSettings.AdvancedIncludeEnable)
		{
			return $"{modsIncluded} {(modsIncluded == 1 ? Locale.ModIncluded : Locale.ModIncludedPlural)}, {total} {Locale.Total.ToLower()}";
		}

		if (modsIncluded == modsEnabled)
		{
			return $"{modsIncluded} {(modsIncluded == 1 ? Locale.ModIncludedAndEnabled : Locale.ModIncludedAndEnabledPlural)}, {total} {Locale.Total.ToLower()}";
		}

		return $"{modsIncluded} {(modsIncluded == 1 ? Locale.ModIncluded : Locale.ModIncludedPlural)} && {modsEnabled} {(modsEnabled == 1 ? Locale.ModEnabled : Locale.ModEnabledPlural)}, {total} {Locale.Total.ToLower()}";
	}

	protected override string GetFilteredCountText(int filteredCount)
	{
		return $"{Locale.Showing} {filteredCount} {Locale.Mods.ToLower()}";
	}

	protected override void SetIncluded(IEnumerable<Mod> filteredItems, bool included)
	{
		ModsUtil.SetIncluded(filteredItems, included);
	}

	protected override void SetEnabled(IEnumerable<Mod> filteredItems, bool enabled)
	{
		ModsUtil.SetEnabled(filteredItems, enabled);
	}
}
