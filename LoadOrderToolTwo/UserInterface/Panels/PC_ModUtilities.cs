using LoadOrderToolTwo.Domain;
using LoadOrderToolTwo.Utilities;
using LoadOrderToolTwo.Utilities.Managers;

using SlickControls;

using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace LoadOrderToolTwo.UserInterface.Panels;
public partial class PC_ModUtilities : PanelContent
{
	public PC_ModUtilities()
	{
		InitializeComponent();
	}

	protected override void LocaleChanged()
	{
		Text = Locale.ModUtilities;
	}

	protected override void UIChanged()
	{
		base.UIChanged();

		B_ReDownload.Image = ImageManager.GetIcon(nameof(Properties.Resources.I_ReDownload));
		B_ReDownload.Margin = UI.Scale(new Padding(5), UI.FontScale);
		P_Filters.Margin = roundedGroupPanel1.Margin = UI.Scale(new Padding(10, 0, 10, 10), UI.FontScale);
		TB_CollectionLink.Margin = B_LoadCollection.Margin = UI.Scale(new Padding(5), UI.FontScale);
	}

	private async void B_LoadCollection_Click(object sender, EventArgs e)
	{
		if (!B_LoadCollection.Loading && this.CheckValidation())
		{
			B_LoadCollection.Loading = true;

			var collectionId = Regex.Match(TB_CollectionLink.Text, TB_CollectionLink.ValidationRegex).Groups[1].Value;
			var contents = await SteamUtil.GetCollectionContentsAsync(collectionId);

			if (contents?.Any() ?? false)
			{
				var collection= contents[ulong.Parse(collectionId)];
				contents.Remove(ulong.Parse(collectionId));
				Form.PushPanel(null, new PC_ImportCollection(collection, contents));
				TB_CollectionLink.Text = string.Empty;
			}

			B_LoadCollection.Loading = false;
		}
	}

	private void B_ReDownload_Click(object sender, EventArgs e)
	{
		SteamUtil.ReDownload(CentralManager.Mods.Where(x => x.Status is DownloadStatus.OutOfDate or DownloadStatus.PartiallyDownloaded).Select(x => x.SteamId).ToArray());
	}
}
