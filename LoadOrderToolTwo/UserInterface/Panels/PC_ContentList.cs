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
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;

using static CompatibilityReport.CatalogData.Enums;

namespace LoadOrderToolTwo.UserInterface.Panels;
internal partial class PC_ContentList<T> : PanelContent where T : IPackage
{
	private bool clearingFilters = true;
	private bool firstFilterPassed;
	private readonly DelayedAction _delayedSearch;
	protected readonly ItemListControl<T> LC_Items;
	private bool searchEmpty = true;
	private List<string> searchTermsOr = new();
	private List<string> searchTermsAnd = new();
	private List<string> searchTermsExclude = new();
	private string? searchAuthor;

	public PC_ContentList()
	{
		LC_Items = new() { Dock = DockStyle.Fill, Margin = new() };

		InitializeComponent();

		B_Filters.Height = B_Actions.Height = DD_Sorting.Height = TB_Search.Height = 0;

		TLP_Main.Controls.Add(LC_Items, 0, 6);
		TLP_Main.SetColumnSpan(LC_Items, 5);

		OT_Workshop.Visible = !CentralManager.CurrentProfile.LaunchSettings.NoWorkshop;

		LC_Items.FilterRequested += FilterChanged;
		LC_Items.CanDrawItem += LC_Items_CanDrawItem;
		LC_Items.DownloadStatusSelected += LC_Items_DownloadStatusSelected;
		LC_Items.CompatibilityReportSelected += LC_Items_CompatibilityReportSelected;
		LC_Items.DateSelected += LC_Items_DateSelected;
		LC_Items.TagSelected += LC_Items_TagSelected;
		LC_Items.AddToSearch += LC_Items_AddToSearch;

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
			P_Filters.SetRow(OT_Workshop, 2);
		}

		clearingFilters = false;

		RefreshCounts();

		CentralManager.ContentLoaded += CentralManager_ContentLoaded;
		CentralManager.WorkshopInfoUpdated += CentralManager_WorkshopInfoUpdated;
		CentralManager.PackageInformationUpdated += CentralManager_WorkshopInfoUpdated;

		new BackgroundAction("Getting tag list", () =>
		{
			var items = new List<T>(LC_Items.Items);

			DD_Tags.Items = items.SelectMany(x => x.Tags.Select(x => x.Value)).Distinct().ToArray();
		}).Run();
	}

	private void LC_Items_AddToSearch(string obj)
	{
		TB_Search.Text = (TB_Search.Text + " " + obj).Trim();
	}

	private void LC_Items_TagSelected(string obj)
	{
		DD_Tags.Select(obj);

		if (P_FiltersContainer.Height == 0)
		{
			B_Filters_Click(this, EventArgs.Empty);
		}
	}

	private void LC_Items_DateSelected(DateTime obj)
	{
		DR_ServerTime.SetValue(DateRangeType.After, obj);

		if (P_FiltersContainer.Height == 0)
		{
			B_Filters_Click(this, EventArgs.Empty);
		}
	}

	private void LC_Items_CompatibilityReportSelected(ReportSeverity obj)
	{
		DD_ReportSeverity.SelectedItem = (ReportSeverityFilter)(obj + 1);

		if (P_FiltersContainer.Height == 0)
		{
			B_Filters_Click(this, EventArgs.Empty);
		}
	}

	private void LC_Items_DownloadStatusSelected(DownloadStatus obj)
	{
		DD_PackageStatus.SelectedItem = (DownloadStatusFilter)(obj + 1);

		if (P_FiltersContainer.Height == 0)
		{
			B_Filters_Click(this, EventArgs.Empty);
		}
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
		DD_Profile.Text = Locale.ProfileFilter;
		DR_SubscribeTime.Text = Locale.DateSubscribed;
		DR_ServerTime.Text = Locale.DateUpdated;
		DD_Author.Text = Locale.Author;
	}

	protected override void OnCreateControl()
	{
		base.OnCreateControl();

		firstFilterPassed = true;

		if (CentralManager.SessionSettings.UserSettings.AlwaysOpenFiltersAndActions)
		{
			P_FiltersContainer.Visible = P_ActionsContainer.Visible = true;
			P_FiltersContainer.AutoSize = P_ActionsContainer.AutoSize = true;
			B_Filters.Text = "HideFilters";
			B_Actions.Text = "HideActions";
		}
		else
		{
			P_FiltersContainer.Height = P_ActionsContainer.Height = 0;
			P_FiltersContainer.Visible = P_ActionsContainer.Visible = true;
		}
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
			= B_Actions.Margin = DR_SubscribeTime.Margin = DR_ServerTime.Margin
			= DD_ReportSeverity.Margin = DD_PackageStatus.Margin = DD_Profile.Margin = DD_Tags.Margin
			= B_Refresh.Margin = B_UnsubscribeAll.Margin = DD_Sorting.Margin = UI.Scale(new Padding(5), UI.FontScale);
		B_UnsubscribeAll.Padding = UI.Scale(new Padding(7), UI.FontScale);
		I_ClearFilters.Size = UI.Scale(new Size(16, 16), UI.FontScale);
		L_Duplicates.Font = L_Counts.Font = L_FilterCount.Font = UI.Font(7.5F, FontStyle.Bold);
		DD_Sorting.Width = (int)(180 * UI.FontScale);
		TB_Search.Width = (int)(400 * UI.FontScale);
	}

	private void B_Actions_Click(object sender, EventArgs e)
	{
		B_Actions.Text = P_ActionsContainer.Height == 0 ? "HideActions" : "ShowActions";
		AnimationHandler.Animate(P_ActionsContainer, P_ActionsContainer.Height == 0 ? new Size(0, P_ActionsContainer.Padding.Vertical + P_Actions.Height) : Size.Empty, 3, AnimationOption.IgnoreWidth);
		P_ActionsContainer.AutoSize = false;
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
		P_FiltersContainer.AutoSize = false;
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
		CentralManager.SessionSettings.UserSettings.PackageSorting = DD_Sorting.SelectedItem;
		CentralManager.SessionSettings.Save();

		LC_Items.SetSorting(DD_Sorting.SelectedItem);
	}

	private void DelayedSearch()
	{
		LC_Items.DoFilterChanged();
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

		if (DR_SubscribeTime.Set && !DR_SubscribeTime.Match(item.SubscribeTime.ToLocalTime()))
		{
			return true;
		}

		if (DR_ServerTime.Set && !DR_ServerTime.Match(item.ServerTime.ToLocalTime()))
		{
			return true;
		}

		if (DD_Tags.SelectedItems.Any())
		{
			foreach (var tag in DD_Tags.SelectedItems)
			{
				if (!(item.Tags?.Any(x => x.Value == tag) ?? false))
				{
					return true;
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

		if (!string.IsNullOrEmpty(searchAuthor))
		{
			if (searchAuthor != item.Author?.Name)
			{
				return true;
			}
		}

		if (!searchEmpty)
		{
			for (var i = 0; i < searchTermsExclude.Count; i++)
			{
				if (Search(searchTermsExclude[i], item))
					return true;
			}

			var orMatched = searchTermsOr.Count == 0;

			for (var i = 0; i < searchTermsOr.Count; i++)
			{
				if (Search(searchTermsOr[i], item))
				{
					orMatched = true;
					break;
				}
			}

			if (!orMatched)
				return true;

			for (var i = 0; i < searchTermsAnd.Count; i++)
			{
				if (!Search(searchTermsAnd[i], item))
				{
					return true;
				}
			}

			return false;
		}

		return false;
	}

	private bool Search(string searchTerm, T item)
	{
		return searchTerm.SearchCheck(item.ToString())
			|| searchTerm.SearchCheck(item.Author?.Name)
			|| Path.GetFileName(item.Folder).IndexOf(searchTerm, StringComparison.OrdinalIgnoreCase) != -1;
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
		TB_Search.ImageName = string.IsNullOrWhiteSpace(TB_Search.Text) ? "I_Search" : "I_ClearSearch";

		var searchText = TB_Search.Text.RegexRemove(@"\[\w+:(.+)?\]").Trim();

		searchTermsAnd.Clear();
		searchTermsExclude.Clear();
		searchTermsOr.Clear();

		if (!(searchEmpty = string.IsNullOrWhiteSpace(searchText)))
		{
			foreach (var item in searchText.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
			{
				switch (item.TrimStart()[0])
				{
					case '+':
						if (item.Trim().Length > 2)
							searchTermsAnd.Add(item.Trim().Substring(1));
						break;
					case '-':
						if (item.Trim().Length > 2)
							searchTermsExclude.Add(item.Trim().Substring(1));
						break;
					default:
						searchTermsOr.Add(item.Trim());
						break;
				}
			}
		}

		searchAuthor = null;

		foreach (Match match in Regex.Matches(TB_Search.Text, @"\[(\w+):(.+)?\]"))
		{
			switch (match.Groups[1].Value.ToLower())
			{
				case "author":
					searchAuthor = match.Groups[2].Value;
					break;
			}
		}

		FilterChanged(sender, e);
	}

	private void L_Duplicates_Click(object sender, EventArgs e)
	{
		Form.PushPanel<PC_ModUtilities>(null);
	}

	private async void B_UnsubscribeAll_Click(object sender, EventArgs e)
	{
		if (ShowPrompt(Locale.AreYouSure, PromptButtons.YesNo) != DialogResult.Yes)
		{
			return;
		}

		B_UnsubscribeAll.Loading = true;
		await CitiesManager.Subscribe(LC_Items.FilteredItems.Select(x => x.SteamId), true);
		B_UnsubscribeAll.Loading = false;
	}
}
