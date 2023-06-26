using Extensions;

using SkyveApp.Domain;
using SkyveApp.Services;
using SkyveApp.Services.Interfaces;
using SkyveApp.Utilities;

using System.Collections.Generic;
using System.Linq;

namespace SkyveApp.UserInterface.Panels;
internal class PC_Mods : PC_ContentList<Mod>
{
	public PC_Mods()
	{

	}

	protected override void LocaleChanged()
	{
		base.LocaleChanged();

		Text = $"{Locale.Mod.Plural} - {Program.Services.GetService<IPlaysetManager>().CurrentPlayset.Name}";
	}

	protected override IEnumerable<Mod> GetItems()
	{
		return Program.Services.GetService<IContentManager>().Mods;
	}

	protected override string GetCountText()
	{
		var mods = Program.Services.GetService<IContentManager>().Mods;
		var modsIncluded = mods.Count(x => x.IsIncluded);
		var modsEnabled = mods.Count(x => x.IsEnabled && x.IsIncluded);
		var total = LC_Items.ItemCount;

		if (!Program.Services.GetService<ISettings>().SessionSettings.UserSettings.AdvancedIncludeEnable)
		{
			return string.Format(Locale.ModIncludedTotal, modsIncluded, total);
		}

		if (modsIncluded == modsEnabled)
		{
			return string.Format(Locale.ModIncludedAndEnabledTotal, modsIncluded, total);
		}

		return string.Format(Locale.ModIncludedEnabledTotal, modsIncluded, modsEnabled, total);
	}

	protected override LocaleHelper.Translation GetItemText()
	{
		return Locale.Mod;
	}

	protected override void SetIncluded(IEnumerable<Mod> filteredItems, bool included)
	{
		Program.Services.GetService<IContentUtil>().SetBulkIncluded(filteredItems, included);
	}

	protected override void SetEnabled(IEnumerable<Mod> filteredItems, bool enabled)
	{
		Program.Services.GetService<IContentUtil>().SetBulkEnabled(filteredItems, enabled);
	}
}
