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
public partial class PC_Mods : PanelContent
{
	private readonly ItemListControl<Mod> LC_Mods;

	public PC_Mods()
	{
		LC_Mods = new() { Dock = DockStyle.Fill, Margin = new() };

		InitializeComponent();

		B_Filters.Height = B_Actions.Height = DD_Sorting.Height = TB_Search.Height = 0;

		TLP_Main.Controls.Add(LC_Mods, 0, 6);
		TLP_Main.SetColumnSpan(LC_Mods, 4);

		OT_Workshop.Visible = !CentralManager.CurrentProfile.LaunchSettings.NoWorkshop;

		DT_SubFrom.Value = DT_UpdateFrom.Value = DT_UpdateFrom.MinDate;
		DT_SubTo.Value = DT_UpdateTo.Value = DT_UpdateTo.MaxDate;

		LC_Mods.CanDrawItem += LC_Mods_CanDrawItem;

		if (!CentralManager.IsContentLoaded)
		{
			LC_Mods.Loading = true;
		}
		else
		{
			LC_Mods.SetItems(CentralManager.Mods);
		}

		if (!CentralManager.SessionSettings.AdvancedIncludeEnable)
		{
			B_DisEnable.Dispose();
			TLP_Main.SetColumnSpan(B_ExInclude, 2);
		}

		RefreshCounts();

		CentralManager.ContentLoaded += CentralManager_ContentLoaded;
		CentralManager.WorkshopInfoUpdated += CentralManager_WorkshopInfoUpdated;
		CentralManager.ModInformationUpdated += CentralManager_WorkshopInfoUpdated;
	}

	protected override void OnCreateControl()
	{
		base.OnCreateControl();

		P_FiltersContainer.Height = P_ActionsContainer.Height = 0;
		P_FiltersContainer.Visible = P_ActionsContainer.Visible = true;
	}

	private void CentralManager_WorkshopInfoUpdated()
	{
		LC_Mods.Invalidate();

		this.TryInvoke(RefreshCounts);
	}

	private void RefreshCounts()
	{
		var modsIncluded = CentralManager.Mods.Count(x => x.IsIncluded);
		var modsEnabled = CentralManager.Mods.Count(x => x.IsEnabled && x.IsIncluded);
		var total = CentralManager.Mods.Count();
		var text = string.Empty;

		if (!CentralManager.SessionSettings.AdvancedIncludeEnable)
		{
			text = $"{modsIncluded} {(modsIncluded == 1 ? Locale.ModIncluded : Locale.ModIncludedPlural)}, {total} {Locale.Total.ToLower()}";
		}
		if (modsIncluded == modsEnabled)
		{
			text = $"{modsIncluded} {(modsIncluded == 1 ? Locale.ModIncludedAndEnabled : Locale.ModIncludedAndEnabledPlural)}, {total} {Locale.Total.ToLower()}";
		}
		else
		{
			text = $"{modsIncluded} {(modsIncluded == 1 ? Locale.ModIncluded : Locale.ModIncludedPlural)} && {modsEnabled} {(modsEnabled == 1 ? Locale.ModEnabled : Locale.ModEnabledPlural)}, {total} {Locale.Total.ToLower()}";
		}

		if (L_Counts.Text != text)
		{
			L_Counts.Text = text;
		}

		if (L_FilterCount.Visible = total != LC_Mods.FilteredCount)
		{
			L_FilterCount.Text = $"{Locale.Showing} {LC_Mods.FilteredCount} {Locale.Mods.ToLower()}";
		}

		L_Duplicates.Visible = ModsUtil.GetDuplicateMods().Any();
	}

	protected override void LocaleChanged()
	{
		Text = $"{Locale.Mods} - {ProfileManager.CurrentProfile.Name}";
		DD_PackageStatus.Text = Locale.ModStatus;
		DD_ReportSeverity.Text = Locale.ReportSeverity;
		L_Duplicates.Text = Locale.MultipleModsIncluded;
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
		P_Filters.BackColor =P_Actions.BackColor= design.BackColor.Tint(Lum: design.Type.If(FormDesignType.Dark, 1, -1));
		LC_Mods.BackColor = design.BackColor;
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

	private void LC_Mods_CanDrawItem(object sender, CanDrawItemEventArgs<Domain.Mod> e)
	{
		e.DoNotDraw = IsFilteredOut(e.Item);
	}

	private bool IsFilteredOut(Mod mod)
	{
		var doNotDraw = false;

		if (CentralManager.CurrentProfile.LaunchSettings.NoWorkshop)
		{
			doNotDraw = mod.Workshop;
		}

		if (!doNotDraw && CentralManager.CurrentProfile.ForAssetEditor)
		{
			doNotDraw = mod.Package.ForNormalGame == true;
		}

		if (!doNotDraw && CentralManager.CurrentProfile.ForGameplay)
		{
			doNotDraw = mod.Package.ForAssetEditor == true;
		}

		if (!doNotDraw && OT_Workshop.SelectedValue != ThreeOptionToggle.Value.None)
		{
			doNotDraw = OT_Workshop.SelectedValue == ThreeOptionToggle.Value.Option1 == mod.Workshop;
		}

		if (!doNotDraw && OT_Included.SelectedValue != ThreeOptionToggle.Value.None)
		{
			doNotDraw = OT_Included.SelectedValue == ThreeOptionToggle.Value.Option1 == mod.IsIncluded;
		}

		if (!doNotDraw && OT_Enabled.SelectedValue != ThreeOptionToggle.Value.None)
		{
			doNotDraw = OT_Enabled.SelectedValue == ThreeOptionToggle.Value.Option1 == mod.IsEnabled;
		}

		if (!doNotDraw && DD_PackageStatus.SelectedItem != DownloadStatusFilter.Any)
		{
			if (DD_PackageStatus.SelectedItem == DownloadStatusFilter.None)
			{
				doNotDraw = mod.Workshop;
			}
			else
			{
				doNotDraw = ((int)DD_PackageStatus.SelectedItem - 1) != (int)mod.Status;
			}
		}

		if (!doNotDraw && DD_ReportSeverity.SelectedItem != ReportSeverityFilter.Any)
		{
			doNotDraw = ((int)DD_ReportSeverity.SelectedItem - 1) != (int)(mod.Package.CompatibilityReport?.Severity ?? 0);
		}

		if (!doNotDraw && (DT_SubFrom.Value != DT_SubFrom.MinDate || DT_SubTo.Value != DT_SubTo.MaxDate))
		{
			doNotDraw = mod.LocalTime < DT_SubFrom.Value || mod.LocalTime > DT_SubTo.Value;
		}

		if (!doNotDraw && (DT_UpdateFrom.Value != DT_UpdateFrom.MinDate || DT_UpdateTo.Value != DT_UpdateTo.MaxDate))
		{
			doNotDraw = mod.ServerTime < DT_UpdateFrom.Value || mod.ServerTime > DT_UpdateTo.Value;
		}

		if (!doNotDraw && !string.IsNullOrWhiteSpace(TB_Search.Text))
		{
			doNotDraw = !(
				TB_Search.Text.SearchCheck(mod.Name) ||
				TB_Search.Text.SearchCheck(mod.Author?.Name) ||
				TB_Search.Text.SearchCheck(mod.SteamId.ToString()));
		}

		return doNotDraw;
	}

	private void CentralManager_ContentLoaded()
	{
		if (LC_Mods.Loading)
		{
			LC_Mods.Loading = false;
		}

		LC_Mods.SetItems(CentralManager.Mods);

		this.TryInvoke(RefreshCounts);
	}

	private void FilterChanged(object sender, EventArgs e)
	{
		LC_Mods.FilterOrSortingChanged();
		RefreshCounts();
	}

	private void I_ClearFilters_Click(object sender, EventArgs e)
	{
		this.ClearForm();
	}

	private void B_ExInclude_LeftClicked(object sender, EventArgs e)
	{
		ModsUtil.SetIncluded(LC_Mods.FilteredItems, false);
	}

	private void B_ExInclude_RightClicked(object sender, EventArgs e)
	{
		ModsUtil.SetIncluded(LC_Mods.FilteredItems, true);
	}

	private void B_DisEnable_LeftClicked(object sender, EventArgs e)
	{
		ModsUtil.SetEnabled(LC_Mods.FilteredItems, false);
	}

	private void B_DisEnable_RightClicked(object sender, EventArgs e)
	{
		ModsUtil.SetEnabled(LC_Mods.FilteredItems, true);
	}

	private void DD_Sorting_SelectedItemChanged(object sender, EventArgs e)
	{
		LC_Mods.SetSorting(DD_Sorting.SelectedItem);
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
