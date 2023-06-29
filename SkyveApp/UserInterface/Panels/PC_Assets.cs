using SkyveApp.Domain;
using SkyveApp.Services;
using SkyveApp.Services.Interfaces;
using SkyveApp.Utilities;

using System.Collections.Generic;
using System.Linq;

namespace SkyveApp.UserInterface.Panels;
internal class PC_Assets : PC_ContentList<Asset>
{
	private readonly IPlaysetManager _profileManager = ServiceCenter.Get<IPlaysetManager>();
	private readonly ISettings _settings = ServiceCenter.Get<ISettings>();
	private readonly IContentManager _contentManager = ServiceCenter.Get<IContentManager>();
	public PC_Assets()
	{
	}

	protected override void LocaleChanged()
	{
		base.LocaleChanged();

		Text = $"{Locale.Asset.Plural} - {_profileManager.CurrentPlayset.Name}";
	}

	protected override IEnumerable<Asset> GetItems()
	{
		if (_settings.SessionSettings.UserSettings.LinkModAssets)
		{
			return _contentManager.Assets.Where(x => x.Package.Mod is null);
		}

		return _contentManager.Assets;
	}

	protected override string GetCountText()
	{
		var assetsIncluded = _contentManager.Assets.Count(x => x.IsIncluded);
		var total = LC_Items.ItemCount;
		var text = string.Empty;

		return string.Format(Locale.AssetIncludedTotal, assetsIncluded, total);
	}

	protected override Extensions.LocaleHelper.Translation GetItemText()
	{
		return Locale.Asset;
	}

	protected override void SetIncluded(IEnumerable<Asset> filteredItems, bool included)
	{
		ServiceCenter.Get<IContentUtil>().SetBulkIncluded(filteredItems, included);
	}

	protected override void SetEnabled(IEnumerable<Asset> filteredItems, bool enabled)
	{
	}
}
