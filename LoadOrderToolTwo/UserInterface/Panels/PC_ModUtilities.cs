using Extensions;

using LoadOrderToolTwo.Domain;
using LoadOrderToolTwo.Domain.Enums;
using LoadOrderToolTwo.UserInterface.Lists;
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
	private readonly ItemListControl<Mod> LC_Duplicates;

	public PC_ModUtilities()
	{
		LC_Duplicates = new ItemListControl<Mod>() { Dock = DockStyle.Top };
		InitializeComponent();

		P_DuplicateMods.Controls.Add(LC_Duplicates);

		RefreshModIssues();

		CentralManager.ModInformationUpdated += RefreshModIssues;
	}

	private void RefreshModIssues()
	{
		var duplicates = ModsUtil.GetDuplicateMods();
		var modsOutOfDate = CentralManager.Mods.Count(x => x.IsIncluded && x.Status == DownloadStatus.OutOfDate);
		var modsIncomplete = CentralManager.Mods.Count(x => x.IsIncluded && x.Status == DownloadStatus.PartiallyDownloaded);

		LC_Duplicates.SetItems(duplicates.SelectMany(x => x));
		LC_Duplicates.SetSorting(PackageSorting.Mod);

		this.TryInvoke(() =>
		{
			LC_Duplicates.Height = LC_Duplicates.GetTotalHeight(LC_Duplicates.SafeGetItems());
			P_DuplicateMods.Visible = duplicates.Any();

			L_OutOfDate.Text = $"{modsOutOfDate} {(modsOutOfDate == 1 ? Locale.ModOutOfDate : Locale.ModOutOfDatePlural)}";
			L_Incomplete.Text = $"{modsIncomplete} {(modsIncomplete == 1 ? Locale.ModIncomplete : Locale.ModIncompletePlural)}";

			L_OutOfDate.Visible = modsOutOfDate > 0;
			L_Incomplete.Visible = modsIncomplete > 0;
			P_ModIssues.Visible = modsOutOfDate > 0 || modsIncomplete > 0;
		});
	}

	protected override void LocaleChanged()
	{
		Text = Locale.ModUtilities;
	}

	protected override void UIChanged()
	{
		base.UIChanged();

		B_ReDownload.Image = ImageManager.GetIcon(nameof(Properties.Resources.I_Tools));
		P_Filters.Image = ImageManager.GetIcon(nameof(Properties.Resources.I_Steam));
		P_ModIssues.Image = ImageManager.GetIcon(nameof(Properties.Resources.I_ModWarning));
		P_DuplicateMods.Image = ImageManager.GetIcon(nameof(Properties.Resources.I_Broken));
		B_ReDownload.Margin = UI.Scale(new Padding(5), UI.FontScale);
		P_Filters.Margin = P_ModIssues.Margin = P_DuplicateMods.Margin = UI.Scale(new Padding(10, 0, 10, 10), UI.FontScale);
		TB_CollectionLink.Margin = B_LoadCollection.Margin = UI.Scale(new Padding(5), UI.FontScale);
		LC_Duplicates.Height = LC_Duplicates.GetTotalHeight(LC_Duplicates.SafeGetItems());
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
				var collection = contents[ulong.Parse(collectionId)];
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

	private void TB_CollectionLink_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
	{
		if (e.KeyData == Keys.Enter)
		{
			B_LoadCollection_Click(this, EventArgs.Empty);

			e.IsInputKey = false;
		}
	}

	private void LSMDragDrop_FileSelected(string obj)
	{
		var assets = LsmUtil.LoadMissingAssets(obj);

		Form.PushPanel(null, new PC_MissingPackages(new(), assets.ToList()));
	}
}
