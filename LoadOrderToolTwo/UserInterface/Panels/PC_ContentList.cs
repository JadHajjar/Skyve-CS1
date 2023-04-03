using Extensions;

using LoadOrderToolTwo.Domain;
using LoadOrderToolTwo.Domain.Enums;
using LoadOrderToolTwo.Domain.Interfaces;
using LoadOrderToolTwo.Domain.Utilities;
using LoadOrderToolTwo.UserInterface.Generic;
using LoadOrderToolTwo.UserInterface.Lists;
using LoadOrderToolTwo.Utilities;
using LoadOrderToolTwo.Utilities.Managers;

using SlickControls;

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

using static CompatibilityReport.CatalogData.Enums;

namespace LoadOrderToolTwo.UserInterface.Panels;
internal partial class PC_ContentList<T> : PanelContent where T : IPackage
{
	private bool clearingFilters = true;
	private bool firstFilterPassed;
	private readonly DelayedAction _delayedSearch;
	protected readonly ItemListControl<T> LC_Items;

	public PC_ContentList()
	{
		LC_Items = new() { Dock = DockStyle.Fill, Margin = new() };

		InitializeComponent();

		B_Filters.Height = B_Actions.Height = DD_Sorting.Height = TB_Search.Height = 0;

		TLP_Main.Controls.Add(LC_Items, 0, 6);
		TLP_Main.SetColumnSpan(LC_Items, 5);

		OT_Workshop.Visible = !CentralManager.CurrentProfile.LaunchSettings.NoWorkshop;

		LC_Items.CanDrawItem += LC_Items_CanDrawItem;

		_delayedSearch = new(350, DelayedSearch);

		if (!CentralManager.IsContentLoaded)
		{
			LC_Items.Loading = true;
		}
		else
		{
			LC_Items.SetItems(GetItems());
		}

		if (!CentralManager.SessionSettings.UserSettings.AdvancedIncludeEnable || this is PC_Assets)
		{
			OT_Enabled.Hide();
			B_DisEnable.Dispose();
			TLP_Main.SetColumnSpan(B_ExInclude, 2);
		}

		clearingFilters = false;

		RefreshCounts();

		CentralManager.ContentLoaded += CentralManager_ContentLoaded;
		CentralManager.WorkshopInfoUpdated += CentralManager_WorkshopInfoUpdated;
		CentralManager.PackageInformationUpdated += CentralManager_WorkshopInfoUpdated;

		new BackgroundAction("Getting tag list", () =>
		{
			var items = new List<T>(LC_Items.Items);

			DD_Tags.Items = items.SelectMany(x =>
			{
				var tags = new List<string>(x.Tags ?? new string[0]);

				if (x is Asset asset)
				{
					tags.AddRange(asset.AssetTags);
				}
				else if (x.Package.Assets is not null)
				{
					foreach (var item in x.Package.Assets)
					{
						tags.AddRange(item.AssetTags);
					}
				}

				return tags;
			}).Distinct().ToArray();
		}).Run();
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

	protected override void DesignChanged(FormDesign design)
	{
		base.DesignChanged(design);

		tableLayoutPanel3.BackColor = design.AccentBackColor;
		P_Filters.BackColor = P_Actions.BackColor = design.BackColor.Tint(Lum: design.Type.If(FormDesignType.Dark, 1, -1));
		LC_Items.BackColor = design.BackColor;
		L_Counts.ForeColor = L_FilterCount.ForeColor = design.InfoColor;
		L_Duplicates.ForeColor = design.RedColor;
	}

	protected override void LocaleChanged()
	{
		L_Duplicates.Text = Locale.MultipleModsIncluded;
		DD_PackageStatus.Text = Locale.PackageStatus;
		DD_ReportSeverity.Text = Locale.ReportSeverity;
		DD_Tags.Text = Locale.Tags;
		DD_Profile.Text = Locale.Profiles;
		slickDateRange1.Text = Locale.DateSubscribed;
		slickDateRange2.Text = Locale.DateUpdated;
	}

	protected override void OnCreateControl()
	{
		base.OnCreateControl();

		firstFilterPassed = true;
		P_FiltersContainer.Height = P_ActionsContainer.Height = 0;
		P_FiltersContainer.Visible = P_ActionsContainer.Visible = true;
	}

	protected virtual IEnumerable<T> GetItems()
	{
		throw new NotImplementedException();
	}

	protected virtual void SetEnabled(IEnumerable<T> filteredItems, bool included)
	{
		throw new NotImplementedException();
	}

	protected virtual void SetIncluded(IEnumerable<T> filteredItems, bool enabled)
	{
		throw new NotImplementedException();
	}

	protected virtual string GetFilteredCountText(int filteredCount)
	{
		throw new NotImplementedException();
	}

	protected virtual string GetCountText()
	{
		throw new NotImplementedException();
	}

	protected override void UIChanged()
	{
		base.UIChanged();

		P_FiltersContainer.Padding = P_ActionsContainer.Padding = TB_Search.Margin
			= L_Duplicates.Margin = L_Counts.Margin = L_FilterCount.Margin
			= B_ExInclude.Margin = B_DisEnable.Margin = B_Filters.Margin
			= B_Actions.Margin = slickDateRange1.Margin = slickDateRange2.Margin 
			= B_Refresh.Margin = UI.Scale(new Padding(5), UI.FontScale);
		B_Filters.Image = P_Filters.Image = ImageManager.GetIcon(nameof(Properties.Resources.I_Filter));
		B_Actions.Image = P_Actions.Image = ImageManager.GetIcon(nameof(Properties.Resources.I_Actions));
		B_Refresh.Image = ImageManager.GetIcon(nameof(Properties.Resources.I_Refresh));
		I_ClearFilters.Image = ImageManager.GetIcon(nameof(Properties.Resources.I_ClearFilter));
		I_ClearFilters.Size = UI.FontScale >= 1.25 ? new(32, 32) : new(24, 24);
		I_ClearFilters.Location = new(P_Filters.Width - P_Filters.Padding.Right - I_ClearFilters.Width, P_Filters.Padding.Bottom);
		L_Duplicates.Font = L_Counts.Font = L_FilterCount.Font = UI.Font(7.5F, FontStyle.Bold);
		DD_Sorting.Width = (int)(180 * UI.FontScale);
		TB_Search.Width = (int)(400 * UI.FontScale);
	}

	private void B_Actions_Click(object sender, EventArgs e)
	{
		B_Actions.Text = P_ActionsContainer.Height == 0 ? "HideActions" : "ShowActions";
		AnimationHandler.Animate(P_ActionsContainer, P_ActionsContainer.Height == 0 ? new Size(0, P_ActionsContainer.Padding.Vertical + P_Actions.Height) : Size.Empty, 3, AnimationOption.IgnoreWidth);
	}

	private void B_DisEnable_LeftClicked(object sender, EventArgs e)
	{
		SetEnabled(LC_Items.FilteredItems, false);
	}

	private void B_DisEnable_RightClicked(object sender, EventArgs e)
	{
		SetEnabled(LC_Items.FilteredItems, true);
	}

	private void B_ExInclude_LeftClicked(object sender, EventArgs e)
	{
		SetIncluded(LC_Items.FilteredItems, false);
	}

	private void B_ExInclude_RightClicked(object sender, EventArgs e)
	{
		SetIncluded(LC_Items.FilteredItems, true);
	}

	private void B_Filters_Click(object sender, EventArgs e)
	{
		B_Filters.Text = P_FiltersContainer.Height == 0 ? "HideFilters" : "ShowFilters";
		AnimationHandler.Animate(P_FiltersContainer, P_FiltersContainer.Height == 0 ? new Size(0, P_FiltersContainer.Padding.Vertical + P_Filters.Height) : Size.Empty, 3, AnimationOption.IgnoreWidth);
	}

	private void CentralManager_ContentLoaded()
	{
		if (LC_Items.Loading)
		{
			LC_Items.Loading = false;
		}

		LC_Items.SetItems(GetItems());

		this.TryInvoke(RefreshCounts);
	}

	private void CentralManager_WorkshopInfoUpdated()
	{
		LC_Items.Invalidate();

		this.TryInvoke(RefreshCounts);
	}

	private void DD_Sorting_SelectedItemChanged(object sender, EventArgs e)
	{
		LC_Items.SetSorting(DD_Sorting.SelectedItem);
	}

	private void DelayedSearch()
	{
		LC_Items.FilterOrSortingChanged();
		this.TryInvoke(RefreshCounts);
		TB_Search.Loading = false;
	}

	private void FilterChanged(object sender, EventArgs e)
	{
		if (!clearingFilters)
		{
			TB_Search.Loading = true;
			_delayedSearch.Run();
		}
	}

	private void I_ClearFilters_Click(object sender, EventArgs e)
	{
		clearingFilters = true;
		this.ClearForm();
		clearingFilters = false;
		FilterChanged(sender, e);
	}

	private bool IsFilteredOut(T item)
	{
		if (CentralManager.CurrentProfile.LaunchSettings.NoWorkshop)
		{
			if (item.Workshop)
			{
				return true;
			}
		}

		if (CentralManager.CurrentProfile.ForAssetEditor)
		{
			if (item.Package.ForNormalGame == true)
			{
				return true;
			}
		}

		if (CentralManager.CurrentProfile.ForGameplay)
		{
			if (item.Package.ForAssetEditor == true)
			{
				return true;
			}
		}

		if (!firstFilterPassed)
		{
			return false;
		}

		if (OT_Workshop.SelectedValue != ThreeOptionToggle.Value.None)
		{
			if (OT_Workshop.SelectedValue == ThreeOptionToggle.Value.Option1 == item.Workshop)
			{
				return true;
			}
		}

		if (OT_Included.SelectedValue != ThreeOptionToggle.Value.None)
		{
			if (OT_Included.SelectedValue == ThreeOptionToggle.Value.Option1 == item.IsIncluded)
			{
				return true;
			}
		}

		if (OT_Enabled.SelectedValue != ThreeOptionToggle.Value.None)
		{
			if (item.Package.Mod is null || OT_Enabled.SelectedValue == ThreeOptionToggle.Value.Option1 == item.Package.Mod.IsEnabled)
			{
				return true;
			}
		}

		if (DD_PackageStatus.SelectedItem != DownloadStatusFilter.Any)
		{
			if (DD_PackageStatus.SelectedItem == DownloadStatusFilter.None)
			{
				if (item.Workshop)
				{
					return true;
				}
			}
			else
			{
				if (((int)DD_PackageStatus.SelectedItem - 1) != (int)item.Status)
				{
					return true;
				}
			}
		}

		if (DD_ReportSeverity.SelectedItem != ReportSeverityFilter.Any)
		{
			if (((int)DD_ReportSeverity.SelectedItem - 1) != (int)(item.Package.CompatibilityReport?.Severity ?? 0))
			{
				return true;
			}
		}

		if (!slickDateRange1.Match(item.LocalTime))
		{
			return true;
		}

		if (!slickDateRange2.Match(item.ServerTime))
		{
			return true;
		}

		if (DD_Tags.SelectedItems.Any())
		{
			foreach (var tag in DD_Tags.SelectedItems)
			{
				if (item is Asset asset)
				{
					if (!(item.Tags?.Any(tag) ?? false) && !asset.AssetTags.Any(tag))
					{
						return true;
					}
				}
				else
				{
					if (!(item.Tags?.Any(tag) ?? false) && !item.Package.Assets.Any(x => x.AssetTags.Any(tag)))
					{
						return true;
					}
				}
			}
		}

		if (DD_Profile.SelectedItem is not null && !DD_Profile.SelectedItem.Temporary)
		{
			if (item is Asset asset)
			{
				if (!DD_Profile.SelectedItem.Assets.Any(x => ProfileManager.ToLocalPath(x.RelativePath).PathEquals(asset.FileName)))
				{
					return true;
				}
			}
			else if (item is Mod mod)
			{
				if (!DD_Profile.SelectedItem.Mods.Any(x => ProfileManager.ToLocalPath(x.RelativePath).PathEquals(mod.Folder)))
				{
					return true;
				}
			}
			else
			{
				if (!DD_Profile.SelectedItem.Assets.Any(x => ProfileManager.ToLocalPath(x.RelativePath).PathContains(item.Folder)) && !DD_Profile.SelectedItem.Mods.Any(x => ProfileManager.ToLocalPath(x.RelativePath).PathContains(item.Folder)))
				{
					return true;
				}
			}
		}

		if (!string.IsNullOrWhiteSpace(TB_Search.Text))
		{
			if (!(TB_Search.Text.SearchCheck(item.ToString()) || TB_Search.Text.SearchCheck(item.Author?.Name) || TB_Search.Text.SearchCheck(item.SteamId.ToString())))
			{
				return true;
			}
		}

		return false;
	}

	private void LC_Items_CanDrawItem(object sender, CanDrawItemEventArgs<T> e)
	{
		e.DoNotDraw = IsFilteredOut(e.Item);
	}

	private void RefreshCounts()
	{
		var countText = GetCountText();
		var filteredText = GetFilteredCountText(LC_Items.FilteredCount);

		if (L_Counts.Text != countText)
		{
			L_Counts.Text = countText;
		}

		if (L_FilterCount.Text != filteredText)
		{
			L_FilterCount.Visible = !string.IsNullOrEmpty(filteredText);
			L_FilterCount.Text = filteredText;
		}

		L_Duplicates.Visible = typeof(T) != typeof(Asset) && ModsUtil.GetDuplicateMods().Any();
	}

	private void TB_Search_IconClicked(object sender, EventArgs e)
	{
		TB_Search.Text = string.Empty;
	}

	private void TB_Search_TextChanged(object sender, EventArgs e)
	{
		TB_Search.Image = string.IsNullOrWhiteSpace(TB_Search.Text) ? Properties.Resources.I_Search : Properties.Resources.I_ClearSearch;
		FilterChanged(sender, e);
	}

	private void L_Duplicates_Click(object sender, EventArgs e)
	{
		Form.PushPanel<PC_ModUtilities>(null);
	}
}
