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

		Text = $"{Locale.Mod.Plural} - {ProfileManager.CurrentProfile.Name}";
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

		if (!CentralManager.SessionSettings.UserSettings.AdvancedIncludeEnable)
		{
			return string.Format(Locale.ModIncludedTotal, modsIncluded, total);
		}

		if (modsIncluded == modsEnabled)
		{
			return string.Format(Locale.ModIncludedAndEnabledTotal, modsIncluded, total);
		}

		return string.Format(Locale.ModIncludedEnabledTotal, modsIncluded, modsEnabled, total);
	}

	protected override string GetFilteredCountText(int filteredCount)
	{
		return Locale.ShowingMods.FormatPlural(filteredCount);
	}

	protected override void SetIncluded(IEnumerable<Mod> filteredItems, bool included)
	{
		ContentUtil.SetBulkIncluded(filteredItems, included);
	}

	protected override void SetEnabled(IEnumerable<Mod> filteredItems, bool enabled)
	{
		ContentUtil.SetBulkEnabled(filteredItems, enabled);
	}
}
