using Extensions;

using LoadOrderToolTwo.Domain;
using LoadOrderToolTwo.Domain.Enums;
using LoadOrderToolTwo.UserInterface.Lists;
using LoadOrderToolTwo.Utilities;
using LoadOrderToolTwo.Utilities.Managers;

using SlickControls;

using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace LoadOrderToolTwo.UserInterface.Panels;
public partial class PC_ModUtilities : PanelContent
{
	private readonly ItemListControl<Mod> LC_Duplicates;

	public PC_ModUtilities()
	{
		LC_Duplicates = new ItemListControl<Mod>() { Dock = DockStyle.Top, DoubleSizeOnHover = false };
		InitializeComponent();

		P_DuplicateMods.Controls.Add(LC_Duplicates);
		LC_Duplicates.SizeChanged += LC_Duplicates_SizeChanged;
		RefreshModIssues();

		CentralManager.ModInformationUpdated += RefreshModIssues;

		DD_Missing.StartingFolder = DD_Unused.StartingFolder = Path.Combine(LocationManager.AppDataPath, "Report", "LoadingScreenMod");
		DD_Missing.ValidExtensions = DD_Unused.ValidExtensions = new[] { ".htm", ".html" };

		SlickTip.SetTo(B_LoadCollection, "LoadCollectionTip");
		SlickTip.SetTo(DD_Missing, "LsmMissingTip");
		SlickTip.SetTo(DD_Unused, "LsmUnusedTip");
		SlickTip.SetTo(B_ReDownload, "FixAllTip");
	}

	private void RefreshModIssues()
	{
		var duplicates = ModsUtil.GetDuplicateMods();
		var modsOutOfDate = CentralManager.Mods.AllWhere(x => x.IsIncluded && x.Status == DownloadStatus.OutOfDate);
		var modsIncomplete = CentralManager.Mods.AllWhere(x => x.IsIncluded && x.Status == DownloadStatus.PartiallyDownloaded);

		LC_Duplicates.SetItems(duplicates.SelectMany(x => x));
		LC_Duplicates.SetSorting(PackageSorting.Mod);

		B_ReDownload.Loading = false;

		this.TryInvoke(() =>
		{
			P_DuplicateMods.Visible = duplicates.Any();

			L_OutOfDate.Text = $"{modsOutOfDate.Count} {(modsOutOfDate.Count == 1 ? Locale.ModOutOfDate : Locale.ModOutOfDatePlural)}:\r\n{modsOutOfDate.ListStrings(x => $"    • {x}", "\r\n")}";
			L_Incomplete.Text = $"{modsIncomplete.Count} {(modsIncomplete.Count == 1 ? Locale.ModIncomplete : Locale.ModIncompletePlural)}:\r\n{modsIncomplete.ListStrings(x => $"    • {x}", "\r\n")}";

			L_OutOfDate.Visible = modsOutOfDate.Count > 0;
			L_Incomplete.Visible = modsIncomplete.Count > 0;
			P_ModIssues.Visible = modsOutOfDate.Count > 0 || modsIncomplete.Count > 0;
		});
	}

	protected override void LocaleChanged()
	{
		Text = Locale.Utilities;
	}

	protected override void UIChanged()
	{
		base.UIChanged();

		B_ReDownload.Image = ImageManager.GetIcon(nameof(Properties.Resources.I_Tools));
		P_Filters.Image = ImageManager.GetIcon(nameof(Properties.Resources.I_Steam));
		P_ModIssues.Image = ImageManager.GetIcon(nameof(Properties.Resources.I_ModWarning));
		P_DuplicateMods.Image = ImageManager.GetIcon(nameof(Properties.Resources.I_Broken));
		P_LsmReport.Image = ImageManager.GetIcon(nameof(Properties.Resources.I_LSM));
		B_ReDownload.Margin = UI.Scale(new Padding(5), UI.FontScale);
		P_Filters.Margin = P_LsmReport.Margin = P_ModIssues.Margin = P_DuplicateMods.Margin = UI.Scale(new Padding(10, 0, 10, 10), UI.FontScale);
		TB_CollectionLink.Margin = B_LoadCollection.Margin = UI.Scale(new Padding(5), UI.FontScale);
		L_OutOfDate.Font = L_Incomplete.Font = UI.Font(9.75F);
	}

	protected override void DesignChanged(FormDesign design)
	{
		base.DesignChanged(design);

		L_OutOfDate.ForeColor = design.YellowColor;
		L_Incomplete.ForeColor = design.RedColor;
	}

	private void LC_Duplicates_SizeChanged(object sender, EventArgs e)
	{
		var height = LC_Duplicates.GetTotalHeight(LC_Duplicates.SafeGetItems());

		if (height != LC_Duplicates.Height)
			LC_Duplicates.Height = height;
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
		B_ReDownload.Loading = true;
		SteamUtil.ReDownload(CentralManager.Mods.Where(x => x.Status is DownloadStatus.OutOfDate or DownloadStatus.PartiallyDownloaded).ToArray());
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

		Form.PushPanel(null, new PC_MissingLsmPackages(assets.ToList()));
	}

	private bool LSMDragDrop_ValidFile(string arg)
	{
		return LsmUtil.IsValidLsmReportFile(arg);
	}

	private void LSM_UnusedDrop_FileSelected(string obj)
	{
		var assets = LsmUtil.LoadUnusedAssets(obj);

		Form.PushPanel(null, new PC_UnusedLsmPackages(assets.ToList()));
	}
}
