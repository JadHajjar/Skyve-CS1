using Extensions;

using LoadOrderToolTwo.Domain;
using LoadOrderToolTwo.Utilities;
using LoadOrderToolTwo.Utilities.Managers;

using SlickControls;

using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

using static CompatibilityReport.CatalogData.Enums;

namespace LoadOrderToolTwo.UserInterface.Panels;
public partial class PC_Packages : PanelContent
{
	private readonly ItemListControl<Package> LC_Packages;

	public PC_Packages()
	{
		LC_Packages = new() { Dock = DockStyle.Fill, Margin = new() };

		InitializeComponent();

		B_Filters.Height = B_Actions.Height = DD_Sorting.Height = TB_Search.Height = 0;

		TLP_Main.Controls.Add(LC_Packages, 0, 6);
		TLP_Main.SetColumnSpan(LC_Packages, 4);

		OT_Workshop.Visible = !CentralManager.CurrentProfile.LaunchSettings.NoWorkshop;

		DT_SubFrom.Value = DT_UpdateFrom.Value = DT_UpdateFrom.MinDate;
		DT_SubTo.Value = DT_UpdateTo.Value = DT_UpdateTo.MaxDate;

		LC_Packages.CanDrawItem += LC_Packages_CanDrawItem;

		if (!CentralManager.IsContentLoaded)
		{
			LC_Packages.Loading = true;
		}
		else
		{
			if (CentralManager.SessionSettings.FilterOutPackagesWithOneAsset || CentralManager.SessionSettings.FilterOutPackagesWithMods)
			{
				LC_Packages.SetItems(CentralManager.Packages.Where(x => !(x.Mod is not null && CentralManager.SessionSettings.FilterOutPackagesWithMods) && !(CentralManager.SessionSettings.FilterOutPackagesWithOneAsset && x.Assets?.Count() > 1)));
			}
			else
			{
				LC_Packages.SetItems(CentralManager.Packages);
			}
		}

		if (!CentralManager.SessionSettings.AdvancedIncludeEnable)
		{
			B_DisEnable.Dispose();
			TLP_Main.SetColumnSpan(B_ExInclude, 2);
		}

		RefreshCounts();

		CentralManager.ContentLoaded += CentralManager_ContentLoaded;
		CentralManager.WorkshopInfoUpdated += CentralManager_WorkshopInfoUpdated;
		CentralManager.PackageInformationUpdated += CentralManager_WorkshopInfoUpdated;
	}

	protected override void OnCreateControl()
	{
		base.OnCreateControl();

		P_FiltersContainer.Height = P_ActionsContainer.Height = 0;
		P_FiltersContainer.Visible = P_ActionsContainer.Visible = true;
	}

	private void CentralManager_WorkshopInfoUpdated()
	{
		LC_Packages.Invalidate();

		this.TryInvoke(RefreshCounts);
	}

	private void RefreshCounts()
	{
		var modsIncluded = CentralManager.Packages.Count(x => x.IsIncluded);
		var total = LC_Packages.ItemCount;
		var text = string.Empty;

		text = $"{modsIncluded} {(modsIncluded == 1 ? Locale.PackageIncluded : Locale.PackageIncludedPlural)}, {total} {Locale.Total.ToLower()}";

		if (L_Counts.Text != text)
		{
			L_Counts.Text = text;
		}

		if (L_FilterCount.Visible = total != LC_Packages.FilteredCount)
		{
			L_FilterCount.Text = $"{Locale.Showing} {LC_Packages.FilteredCount} {Locale.Packages.ToLower()}";
		}

		L_Duplicates.Visible = ModsUtil.GetDuplicateMods().Any();
	}

	protected override void LocaleChanged()
	{
		Text = $"{Locale.Packages} - {ProfileManager.CurrentProfile.Name}";
		DD_PackageStatus.Text = Locale.PackageStatus;
		DD_ReportSeverity.Text = Locale.ReportSeverity;
		L_Duplicates.Text = Locale.MultiplePackagesIncluded;
		L_From1.Text = L_From2.Text = Locale.From;
		L_To1.Text = L_To2.Text = Locale.To;
		L_DateSubscribed.Text = Locale.DateSubscribed;
		L_DateUpdated.Text = Locale.DateUpdated;
	}

	protected override void UIChanged()
	{
		base.UIChanged();

		P_FiltersContainer.Padding = P_ActionsContainer.Padding = TB_Search.Margin = L_Duplicates.Margin = L_Counts.Margin = L_FilterCount.Margin
			= B_ExInclude.Margin = B_DisEnable.Margin = B_Filters.Margin = B_Actions.Margin = TLP_Dates.Margin = UI.Scale(new Padding(5), UI.FontScale);
		B_Filters.Image = P_Filters.Image = ImageManager.GetIcon(nameof(Properties.Resources.I_Filter));
		B_Actions.Image = P_Actions.Image = ImageManager.GetIcon(nameof(Properties.Resources.I_Actions));
		I_ClearFilters.Image = ImageManager.GetIcon(nameof(Properties.Resources.I_ClearFilter));
		I_ClearFilters.Size = UI.FontScale >= 1.25 ? new(32, 32) : new(24, 24);
		I_ClearFilters.Location = new(P_Filters.Width - P_Filters.Padding.Right - I_ClearFilters.Width, P_Filters.Padding.Bottom);
		L_Duplicates.Font = L_Counts.Font = L_FilterCount.Font = UI.Font(7.5F, FontStyle.Bold);
		L_DateSubscribed.Font = L_DateUpdated.Font = UI.Font(8.25F, FontStyle.Bold);
		DD_Sorting.Width = (int)(180 * UI.FontScale);
		TB_Search.Width = (int)(400 * UI.FontScale);
	}

	protected override void DesignChanged(FormDesign design)
	{
		base.DesignChanged(design);

		tableLayoutPanel3.BackColor = design.AccentBackColor;
		P_Filters.BackColor = P_Actions.BackColor = design.BackColor.Tint(Lum: design.Type.If(FormDesignType.Dark, 1, -1));
		LC_Packages.BackColor = design.BackColor;
		L_Counts.ForeColor = L_FilterCount.ForeColor = design.InfoColor;
		L_Duplicates.ForeColor = design.RedColor;
	}

	public override bool KeyPressed(ref Message msg, Keys keyData)
	{
		if (keyData is (Keys.Control | Keys.F))
		{
			TB_Search.Focus();
			TB_Search.SelectAll();

			return true;
		}

		return false;
	}

	private void LC_Packages_CanDrawItem(object sender, CanDrawItemEventArgs<Domain.Package> e)
	{
		e.DoNotDraw = IsFilteredOut(e.Item);
	}

	private bool IsFilteredOut(Package package)
	{
		var doNotDraw = false;

		if (CentralManager.CurrentProfile.LaunchSettings.NoWorkshop)
		{
			doNotDraw = package.Workshop;
		}

		if (!doNotDraw && CentralManager.CurrentProfile.ForAssetEditor)
		{
			doNotDraw = package.ForNormalGame == true;
		}

		if (!doNotDraw && CentralManager.CurrentProfile.ForGameplay)
		{
			doNotDraw = package.ForAssetEditor == true;
		}

		if (!doNotDraw && OT_Workshop.SelectedValue != ThreeOptionToggle.Value.None)
		{
			doNotDraw = OT_Workshop.SelectedValue == ThreeOptionToggle.Value.Option1 == package.Workshop;
		}

		if (!doNotDraw && OT_Included.SelectedValue != ThreeOptionToggle.Value.None)
		{
			doNotDraw = OT_Included.SelectedValue == ThreeOptionToggle.Value.Option1 == package.IsIncluded;
		}

		if (!doNotDraw && OT_Enabled.SelectedValue != ThreeOptionToggle.Value.None)
		{
			doNotDraw = package.Mod is Mod mod &&
				OT_Enabled.SelectedValue == ThreeOptionToggle.Value.Option1 == mod.IsEnabled;
		}

		if (!doNotDraw && DD_PackageStatus.SelectedItem != DownloadStatusFilter.Any)
		{
			if (DD_PackageStatus.SelectedItem == DownloadStatusFilter.None)
			{
				doNotDraw = package.Workshop;
			}
			else
			{
				doNotDraw = ((int)DD_PackageStatus.SelectedItem - 1) != (int)package.Status;
			}
		}

		if (!doNotDraw && DD_ReportSeverity.SelectedItem != ReportSeverityFilter.Any)
		{
			doNotDraw = ((int)DD_ReportSeverity.SelectedItem - 1) != (int)(package.CompatibilityReport?.Severity ?? 0);
		}

		if (!doNotDraw && (DT_SubFrom.Value != DT_SubFrom.MinDate || DT_SubTo.Value != DT_SubTo.MaxDate))
		{
			doNotDraw = package.LocalTime < DT_SubFrom.Value || package.LocalTime > DT_SubTo.Value;
		}

		if (!doNotDraw && (DT_UpdateFrom.Value != DT_UpdateFrom.MinDate || DT_UpdateTo.Value != DT_UpdateTo.MaxDate))
		{
			doNotDraw = package.ServerTime < DT_UpdateFrom.Value || package.ServerTime > DT_UpdateTo.Value;
		}

		if (!doNotDraw && !string.IsNullOrWhiteSpace(TB_Search.Text))
		{
			doNotDraw = !(
				TB_Search.Text.SearchCheck(package.Name) ||
				TB_Search.Text.SearchCheck(package.Author?.Name) ||
				TB_Search.Text.SearchCheck(package.SteamId.ToString()));
		}

		return doNotDraw;
	}

	private void CentralManager_ContentLoaded()
	{
		if (LC_Packages.Loading)
		{
			LC_Packages.Loading = false;
		}

		if (CentralManager.SessionSettings.FilterOutPackagesWithOneAsset)
		{
			LC_Packages.SetItems(CentralManager.Packages.Where(x => x.Mod is not null || x.Assets?.Count() > 1));
		}
		else
		{
			LC_Packages.SetItems(CentralManager.Packages);
		}

		this.TryInvoke(RefreshCounts);
	}

	private void FilterChanged(object sender, EventArgs e)
	{
		LC_Packages.FilterOrSortingChanged();
		RefreshCounts();
	}

	private void I_ClearFilters_Click(object sender, EventArgs e)
	{
		this.ClearForm();
	}

	private void B_ExInclude_LeftClicked(object sender, EventArgs e)
	{
		ModsUtil.SetIncluded(LC_Packages.FilteredItems.Where(x => x.Mod is not null).Select(x => x.Mod!), false);
		AssetsUtil.SetIncluded(LC_Packages.FilteredItems.Where(x => x.Assets is not null).SelectMany(x => x.Assets!), false);
	}

	private void B_ExInclude_RightClicked(object sender, EventArgs e)
	{
		ModsUtil.SetIncluded(LC_Packages.FilteredItems.Where(x => x.Mod is not null).Select(x => x.Mod!), true);
		AssetsUtil.SetIncluded(LC_Packages.FilteredItems.Where(x => x.Assets is not null).SelectMany(x => x.Assets!), true);
	}

	private void B_DisEnable_LeftClicked(object sender, EventArgs e)
	{
		ModsUtil.SetEnabled(LC_Packages.FilteredItems.Where(x => x.Mod is not null).Select(x => x.Mod!), false);
	}

	private void B_DisEnable_RightClicked(object sender, EventArgs e)
	{
		ModsUtil.SetEnabled(LC_Packages.FilteredItems.Where(x => x.Mod is not null).Select(x => x.Mod!), true);
	}

	private void DD_Sorting_SelectedItemChanged(object sender, EventArgs e)
	{
		LC_Packages.SetSorting(DD_Sorting.SelectedItem);
	}

	private void B_Filters_Click(object sender, EventArgs e)
	{
		B_Filters.Text = P_FiltersContainer.Height == 0 ? "HideFilters" : "ShowFilters";
		AnimationHandler.Animate(P_FiltersContainer, P_FiltersContainer.Height == 0 ? new Size(0, P_FiltersContainer.Padding.Vertical + P_Filters.Height) : Size.Empty, 3, AnimationOption.IgnoreWidth);
	}

	private void B_Actions_Click(object sender, EventArgs e)
	{
		B_Actions.Text = P_ActionsContainer.Height == 0 ? "HideActions" : "ShowActions";
		AnimationHandler.Animate(P_ActionsContainer, P_ActionsContainer.Height == 0 ? new Size(0, P_ActionsContainer.Padding.Vertical + P_Actions.Height) : Size.Empty, 3, AnimationOption.IgnoreWidth);
	}
}
