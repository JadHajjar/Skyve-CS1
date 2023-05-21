using Extensions;

using SkyveApp.Domain;
using SkyveApp.Domain.Compatibility;
using SkyveApp.Domain.Enums;
using SkyveApp.Domain.Interfaces;
using SkyveApp.Domain.Steam;
using SkyveApp.Domain.Utilities;
using SkyveApp.UserInterface.Generic;
using SkyveApp.UserInterface.Lists;
using SkyveApp.Utilities;
using SkyveApp.Utilities.IO;
using SkyveApp.Utilities.Managers;

using SlickControls;

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SkyveApp.UserInterface.Panels;
internal partial class PC_ContentList<T> : PanelContent where T : IPackage
{
	private bool clearingFilters = true;
	private bool firstFilterPassed;
	private readonly DelayedAction _delayedSearch;
	protected readonly ItemListControl<T> LC_Items;
	private readonly IncludeAllButton<T> I_Actions;
	protected int UsageFilteredOut;
	private bool searchEmpty = true;
	private readonly List<string> searchTermsOr = new();
	private readonly List<string> searchTermsAnd = new();
	private readonly List<string> searchTermsExclude = new();

	public PC_ContentList() : this(false) { }

	public PC_ContentList(bool load) : base(load)
	{
		LC_Items = new() { Dock = DockStyle.Fill, Margin = new() };

		InitializeComponent();

		I_Actions = new IncludeAllButton<T>(LC_Items);
		I_Actions.ActionClicked += I_Actions_Click;
		I_Actions.IncludeAllClicked += IncludeAll;
		I_Actions.ExcludeAllClicked += ExcludeAll;
		I_Actions.EnableAllClicked += EnableAll;
		I_Actions.DisableAllClicked += DisableAll;
		I_Actions.SubscribeAllClicked += SubscribeAll;

		TLP_Main.Controls.Add(LC_Items, 0, TLP_Main.RowCount - 1);
		TLP_Main.SetColumnSpan(LC_Items, TLP_Main.ColumnCount);
		TLP_MiddleBar.Controls.Add(I_Actions, 0, 0);

		if (this is not PC_GenericPackageList)
		{
			TLP_Main.SetColumn(FLP_Search, 0);
			TLP_Main.SetColumnSpan(FLP_Search, 2);
			TLP_Main.SetColumn(P_FiltersContainer, 0);
			TLP_Main.SetColumnSpan(P_FiltersContainer, 4);
		}

		OT_Workshop.Visible = !CentralManager.CurrentProfile.LaunchSettings.NoWorkshop;
		OT_ModAsset.Visible = this is not PC_Assets and not PC_Mods;

		LC_Items.FilterRequested += FilterChanged;
		LC_Items.CanDrawItem += LC_Items_CanDrawItem;
		LC_Items.DownloadStatusSelected += LC_Items_DownloadStatusSelected;
		LC_Items.CompatibilityReportSelected += LC_Items_CompatibilityReportSelected;
		LC_Items.DateSelected += LC_Items_DateSelected;
		LC_Items.TagSelected += LC_Items_TagSelected;
		LC_Items.AuthorSelected += LC_Items_AuthorSelected;
		LC_Items.FilterByEnabled += LC_Items_FilterByEnabled;
		LC_Items.FilterByIncluded += LC_Items_FilterByIncluded;
		LC_Items.AddToSearch += LC_Items_AddToSearch;
		LC_Items.OpenWorkshopSearch += LC_Items_OpenWorkshopSearch;
		LC_Items.OpenWorkshopSearchInBrowser += LC_Items_OpenWorkshopSearchInBrowser;

		_delayedSearch = new(350, DelayedSearch);

		if (!load)
		{
			if (!CentralManager.IsContentLoaded)
			{
				LC_Items.Loading = true;
			}
			else
			{
				LC_Items.SetItems(GetItems());
			}
		}

		if (!CentralManager.SessionSettings.UserSettings.AdvancedIncludeEnable || this is PC_Assets)
		{
			OT_Enabled.Hide();
			P_Filters.SetRow(OT_Workshop, 2);
			P_Filters.SetRow(OT_ModAsset, 3);
		}

		clearingFilters = false;

		RefreshCounts();

		I_SortOrder.ImageName = LC_Items.SortDesc ? "I_SortDesc" : "I_SortAsc";

		if (!load)
		{
			CentralManager.ContentLoaded += CentralManager_ContentLoaded;
		}
		else
		{
			CentralManager.ContentLoaded += CentralManager_WorkshopInfoUpdated;
		}

		CentralManager.WorkshopInfoUpdated += CentralManager_WorkshopInfoUpdated;
		CentralManager.PackageInformationUpdated += CentralManager_WorkshopInfoUpdated;
		CompatibilityManager.ReportProcessed += CentralManager_WorkshopInfoUpdated;

		if (!load)
		{
			new BackgroundAction("Getting tag list", RefreshAuthorAndTags).Run();
		}
	}

	protected void RefreshAuthorAndTags()
	{
		var items = new List<T>(LC_Items.Items);

		DD_Author.SetItems(items.Where(x => x.Author is not null));
		DD_Tags.Items = items.SelectMany(x => x.Tags).Distinct(x => x.Value.ToLower()).ToArray();
	}

	private void LC_Items_OpenWorkshopSearch()
	{
		Form.PushPanel(null, new PC_SelectPackage(TB_Search.Text, this is PC_Mods ? ThreeOptionToggle.Value.Option1 : this is PC_Assets ? ThreeOptionToggle.Value.Option2 : ThreeOptionToggle.Value.None));
	}

	private void LC_Items_OpenWorkshopSearchInBrowser()
	{
		PlatformUtil.OpenUrl($"https://steamcommunity.com/workshop/browse/?appid=255710&searchtext={WebUtility.UrlEncode(TB_Search.Text)}&browsesort=trend&section=readytouseitems&actualsort=trend&p=1&days=365" + (this is PC_Mods ? "&requiredtags%5B0%5D=Mod" : ""));
	}

	private void LC_Items_AddToSearch(string obj)
	{
		if (TB_Search.Text.Length == 0)
		{
			TB_Search.Text = obj;
		}
		else
		{
			TB_Search.Text += $",+{obj}";
		}
	}

	private void LC_Items_FilterByIncluded(bool obj)
	{
		OT_Included.SelectedValue = obj ? ThreeOptionToggle.Value.Option1 : ThreeOptionToggle.Value.Option2;

		if (P_FiltersContainer.Height == 0)
		{
			B_Filters_Click(this, null);
		}
	}

	private void LC_Items_FilterByEnabled(bool obj)
	{
		OT_Enabled.SelectedValue = obj ? ThreeOptionToggle.Value.Option1 : ThreeOptionToggle.Value.Option2;

		if (P_FiltersContainer.Height == 0)
		{
			B_Filters_Click(this, null);
		}
	}

	private void LC_Items_AuthorSelected(SteamUser obj)
	{
		DD_Author.Select(obj);

		if (P_FiltersContainer.Height == 0)
		{
			B_Filters_Click(this, null);
		}
	}

	private void LC_Items_TagSelected(TagItem obj)
	{
		DD_Tags.Select(obj);

		if (P_FiltersContainer.Height == 0)
		{
			B_Filters_Click(this, null);
		}
	}

	private void LC_Items_DateSelected(DateTime obj)
	{
		DR_ServerTime.SetValue(DateRangeType.After, obj);

		if (P_FiltersContainer.Height == 0)
		{
			B_Filters_Click(this, null);
		}
	}

	private void LC_Items_CompatibilityReportSelected(NotificationType obj)
	{
		if ((int)DD_ReportSeverity.SelectedItem == (int)obj)
		{
			DD_ReportSeverity.SelectedItem = Dropdowns.CompatibilityNotificationFilter.Any;
		}
		else
		{
			DD_ReportSeverity.SelectedItem = (Dropdowns.CompatibilityNotificationFilter)obj;
		}

		if (P_FiltersContainer.Height == 0)
		{
			B_Filters_Click(this, null);
		}
	}

	private void LC_Items_DownloadStatusSelected(DownloadStatus obj)
	{
		if (DD_PackageStatus.SelectedItem == (DownloadStatusFilter)(obj + 2))
		{
			DD_PackageStatus.SelectedItem = DownloadStatusFilter.Any;
		}
		else
		{
			DD_PackageStatus.SelectedItem = (DownloadStatusFilter)(obj + 2);
		}

		if (P_FiltersContainer.Height == 0)
		{
			B_Filters_Click(this, null);
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

		TLP_MiddleBar.BackColor = design.AccentBackColor;
		P_Filters.BackColor = design.BackColor.Tint(Lum: design.Type.If(FormDesignType.Dark, -1, 1));
		LC_Items.BackColor = design.BackColor;
		L_Counts.ForeColor = L_FilterCount.ForeColor = design.InfoColor;
	}

	protected override void LocaleChanged()
	{
		DD_PackageStatus.Text = Locale.PackageStatus;
		DD_ReportSeverity.Text = Locale.CompatibilityStatus;
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
			P_FiltersContainer.Visible = true;
			P_FiltersContainer.AutoSize = true;
			B_Filters.Text = "HideFilters";
		}
		else
		{
			P_FiltersContainer.Height = 0;
			P_FiltersContainer.Visible = true;
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

	protected virtual LocaleHelper.Translation GetItemText()
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

		P_FiltersContainer.Padding = TB_Search.Margin = I_Refresh.Padding = B_Filters.Padding
			= L_Counts.Margin = L_FilterCount.Margin = I_SortOrder.Padding
			= B_Filters.Margin = I_SortOrder.Margin = I_Refresh.Margin = DD_Sorting.Margin = UI.Scale(new Padding(5), UI.FontScale);

		B_Filters.Size = B_Filters.GetAutoSize(true);

		OT_Enabled.Margin = OT_Included.Margin = OT_Workshop.Margin = OT_ModAsset.Margin
			= DD_ReportSeverity.Margin = DR_SubscribeTime.Margin = DR_ServerTime.Margin
			= DD_Author.Margin = DD_PackageStatus.Margin = DD_Profile.Margin = DD_Tags.Margin = UI.Scale(new Padding(4, 2, 4, 2), UI.FontScale);

		I_ClearFilters.Size = UI.Scale(new Size(16, 16), UI.FontScale);
		L_Counts.Font = L_FilterCount.Font = UI.Font(7.5F, FontStyle.Bold);
		DD_Sorting.Width = (int)(175 * UI.FontScale);
		TB_Search.Width = (int)(250 * UI.FontScale);

		var size = (int)(30 * UI.FontScale) - 6;

		TB_Search.MaximumSize = I_Refresh.MaximumSize = B_Filters.MaximumSize = I_SortOrder.MaximumSize = DD_Sorting.MaximumSize = new Size(9999, size);
		TB_Search.MinimumSize = I_Refresh.MinimumSize = B_Filters.MinimumSize = I_SortOrder.MinimumSize = DD_Sorting.MinimumSize = new Size(0, size);
	}

	private void B_Filters_Click(object sender, MouseEventArgs? e)
	{
		if (e is null || e.Button is MouseButtons.Left or MouseButtons.None)
		{
			B_Filters.Text = P_FiltersContainer.Height == 0 ? "HideFilters" : "ShowFilters";
			AnimationHandler.Animate(P_FiltersContainer, P_FiltersContainer.Height == 0 ? new Size(0, P_FiltersContainer.Padding.Vertical + P_Filters.Height) : Size.Empty, 3, AnimationOption.IgnoreWidth);
			P_FiltersContainer.AutoSize = false;
		}
		else if (e?.Button == MouseButtons.Middle)
		{
			I_ClearFilters_Click(sender, e);
		}
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

		LC_Items.SetSorting(DD_Sorting.SelectedItem, LC_Items.SortDesc);
	}

	private void DelayedSearch()
	{
		UsageFilteredOut = 0;
		LC_Items.DoFilterChanged();
		this.TryInvoke(RefreshCounts);
		I_Refresh.Loading = false;
	}

	private void FilterChanged(object sender, EventArgs e)
	{
		if (!clearingFilters)
		{
			I_Refresh.Loading = true;
			_delayedSearch.Run();

			if (sender == I_Refresh)
			{
				LC_Items.SortingChanged();
			}
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

		if (CentralManager.CurrentProfile.Usage > 0)
		{
			if (!(item.GetCompatibilityInfo().Data?.Package.Usage.HasFlag(CentralManager.CurrentProfile.Usage) ?? true))
			{
				UsageFilteredOut++;
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
			if (OT_Included.SelectedValue == ThreeOptionToggle.Value.Option2 == item.IsIncluded)
			{
				return true;
			}
		}

		if (OT_Enabled.SelectedValue != ThreeOptionToggle.Value.None)
		{
			if (item.Package?.Mod is null || OT_Enabled.SelectedValue == ThreeOptionToggle.Value.Option2 == item.Package.Mod.IsEnabled)
			{
				return true;
			}
		}

		if (OT_ModAsset.SelectedValue != ThreeOptionToggle.Value.None)
		{
			if (OT_ModAsset.SelectedValue == ThreeOptionToggle.Value.Option2 == item.IsMod)
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
			else if (DD_PackageStatus.SelectedItem == DownloadStatusFilter.AnyIssue)
			{
				if (!item.Workshop || item.Package?.Status <= DownloadStatus.OK)
				{
					return true;
				}
			}
			else
			{
				if (((int)DD_PackageStatus.SelectedItem - 2) != (int)(item.Package?.Status ?? DownloadStatus.None))
				{
					return true;
				}
			}
		}

		if (DD_ReportSeverity.SelectedItem != Dropdowns.CompatibilityNotificationFilter.Any)
		{
			if (DD_ReportSeverity.SelectedItem == Dropdowns.CompatibilityNotificationFilter.AnyIssue)
			{
				if (item.GetCompatibilityInfo().Notification > NotificationType.Info)
				{
					return true;
				}
			}
			else if ((int)item.GetCompatibilityInfo().Notification != (int)DD_ReportSeverity.SelectedItem)
			{
				return true;
			}
		}

		if (DR_SubscribeTime.Set && !DR_SubscribeTime.Match(item.Package?.LocalTime.ToLocalTime() ?? DateTime.MinValue))
		{
			return true;
		}

		if (DR_ServerTime.Set && !DR_ServerTime.Match(item.ServerTime.ToLocalTime()))
		{
			return true;
		}

		if (DD_Author.SelectedItems.Any())
		{
			if (!DD_Author.SelectedItems.Any(x => item.Author?.Equals(x) ?? false))
			{
				return true;
			}
		}

		if (DD_Tags.SelectedItems.Any())
		{
			foreach (var tag in DD_Tags.SelectedItems)
			{
				if (!(item.Tags?.Any(x => x.Value == tag.Value) ?? false))
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
				if (string.IsNullOrEmpty(item.Folder))
				{
					if (!DD_Profile.SelectedItem.Assets.Any(x => x.SteamId == item.SteamId) && !DD_Profile.SelectedItem.Mods.Any(x => x.SteamId == item.SteamId))
					{
						return true;
					}
				}
				else if (!DD_Profile.SelectedItem.Assets.Any(x => ProfileManager.ToLocalPath(x.RelativePath).PathContains(item.Folder)) && !DD_Profile.SelectedItem.Mods.Any(x => ProfileManager.ToLocalPath(x.RelativePath).PathContains(item.Folder)))
				{
					return true;
				}
			}
		}

		if (!searchEmpty)
		{
			for (var i = 0; i < searchTermsExclude.Count; i++)
			{
				if (Search(searchTermsExclude[i], item))
				{
					return true;
				}
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
			{
				return true;
			}

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
			|| (item.Workshop ? item.SteamId.ToString() : Path.GetFileName(item.Folder)).IndexOf(searchTerm, StringComparison.OrdinalIgnoreCase) != -1;
	}

	private void LC_Items_CanDrawItem(object sender, CanDrawItemEventArgs<T> e)
	{
		e.DoNotDraw = IsFilteredOut(e.Item);
	}

	protected void RefreshCounts()
	{
		var countText = GetCountText();
		var filteredText = (UsageFilteredOut == 0 ? Locale.ShowingCount : Locale.ShowingCountWarning).FormatPlural(LC_Items.FilteredCount, GetItemText().FormatPlural(LC_Items.FilteredCount).ToLower(), Locale.ItemsHidden.FormatPlural(UsageFilteredOut, GetItemText().FormatPlural(LC_Items.FilteredCount).ToLower()));

		if (L_Counts.Text != countText)
		{
			L_Counts.Text = countText;
		}

		if (L_FilterCount.Text != filteredText)
		{
			L_FilterCount.Visible = !string.IsNullOrEmpty(filteredText);
			L_FilterCount.Text = filteredText;
		}
	}

	private void TB_Search_IconClicked(object sender, EventArgs e)
	{
		TB_Search.Text = string.Empty;
	}

	private void TB_Search_TextChanged(object sender, EventArgs e)
	{
		TB_Search.ImageName = (searchEmpty = string.IsNullOrWhiteSpace(TB_Search.Text)) ? "I_Search" : "I_ClearSearch";

		var searchText = TB_Search.Text.Trim();

		searchTermsAnd.Clear();
		searchTermsExclude.Clear();
		searchTermsOr.Clear();

		LC_Items.TextSearchNotEmpty = !searchEmpty;

		if (!searchEmpty)
		{
			var matches = Regex.Matches(searchText, @"(?:^|,)?\s*([+-]?)\s*([^,\-\+]+)");
			foreach (Match item in matches)
			{
				switch (item.Groups[1].Value)
				{
					case "+":
						if (!string.IsNullOrWhiteSpace(item.Groups[2].Value))
						{
							searchTermsAnd.Add(item.Groups[2].Value.Trim());
						}

						break;
					case "-":
						if (!string.IsNullOrWhiteSpace(item.Groups[2].Value))
						{
							searchTermsExclude.Add(item.Groups[2].Value.Trim());
						}

						break;
					default:
						searchTermsOr.Add(item.Groups[2].Value.Trim());
						break;
				}
			}
		}

		FilterChanged(sender, e);
	}

	private void Icon_SizeChanged(object sender, EventArgs e)
	{
		(sender as Control)!.Width = (sender as Control)!.Height;
	}

	private void I_SortOrder_Click(object sender, EventArgs e)
	{
		LC_Items.SetSorting(DD_Sorting.SelectedItem, !LC_Items.SortDesc);

		CentralManager.SessionSettings.UserSettings.PackageSortingDesc = LC_Items.SortDesc;
		CentralManager.SessionSettings.Save();

		I_SortOrder.ImageName = LC_Items.SortDesc ? "I_SortDesc" : "I_SortAsc";
	}

	private void I_Actions_Click(object sender, EventArgs e)
	{
		var items = new SlickStripItem[]
		{
			  new (Locale.IncludeAll, "I_Check", action: () => IncludeAll(this, EventArgs.Empty))
			, new (Locale.ExcludeAll, "I_X", action: () =>ExcludeAll(this, EventArgs.Empty))
			, new (string.Empty)
			, new (Locale.EnableAll, "I_Enabled", CentralManager.SessionSettings.UserSettings.AdvancedIncludeEnable, action:() => EnableAll(this, EventArgs.Empty))
			, new (Locale.DisableAll, "I_Disabled", CentralManager.SessionSettings.UserSettings.AdvancedIncludeEnable, action: () =>DisableAll(this, EventArgs.Empty))
			, new (string.Empty)
			, new (Locale.CopyAllIds, "I_Copy", action: () => Clipboard.SetText(LC_Items.FilteredItems.Where(x => x.SteamId != 0).Select(x => x.SteamId).ListStrings(" ")))
			, new (string.Empty)
			, new (Locale.SubscribeAll, "I_Steam", this is PC_GenericPackageList, action: () => SubscribeAll(this, EventArgs.Empty))
			, new (string.Empty, show: this is PC_GenericPackageList)
			, new (Locale.UnsubscribeAll, "I_RemoveSteam", action: () => UnsubscribeAll(this, EventArgs.Empty))
			, new (Locale.DeleteAll, "I_Disposable", action: () => DeleteAll(this, EventArgs.Empty))
		};

		this.TryBeginInvoke(() => SlickToolStrip.Show(Program.MainForm, I_Actions.PointToScreen(new Point(I_Actions.Width + 5, 0)), items));
	}

	private void DisableAll(object sender, EventArgs e)
	{
		SetEnabled(LC_Items.FilteredItems, false);
		LC_Items.Invalidate();
	}

	private void EnableAll(object sender, EventArgs e)
	{
		SetEnabled(LC_Items.FilteredItems, true);
		LC_Items.Invalidate();
	}

	private void ExcludeAll(object sender, EventArgs e)
	{
		SetIncluded(LC_Items.FilteredItems, false);
		LC_Items.Invalidate();
	}

	private void IncludeAll(object sender, EventArgs e)
	{
		SetIncluded(LC_Items.FilteredItems, true);
		LC_Items.Invalidate();
	}

	private async void UnsubscribeAll(object sender, EventArgs e)
	{
		if (ShowPrompt(Locale.AreYouSure, PromptButtons.YesNo) != DialogResult.Yes)
		{
			return;
		}

		I_Actions.Loading = true;
		await CitiesManager.UnSubscribe(LC_Items.FilteredItems.Select(x => x.SteamId));
		I_Actions.Loading = false;
	}

	private async void SubscribeAll(object sender, EventArgs e)
	{
		var removeBadPackages = false;
		var steamIds = LC_Items.SafeGetItems().AllWhere(x => x.Item.Package == null && x.Item.SteamId != 0);

		if (steamIds.Count == 0 || ShowPrompt(Locale.AreYouSure, PromptButtons.YesNo) != DialogResult.Yes)
		{
			return;
		}

		foreach (var item in steamIds.ToList())
		{
			var report = item.Item.GetCompatibilityInfo();

			if (report.Notification >= NotificationType.Unsubscribe)
			{
				if (!removeBadPackages && ShowPrompt(Locale.ItemsShouldNotBeSubscribedInfo + "\r\n\r\n" + Locale.WouldYouLikeToSkipThose, PromptButtons.YesNo, PromptIcons.Hand) == DialogResult.No)
				{
					break;
				}
				else
				{
					removeBadPackages = true;
				}

				steamIds.Remove(item);
				LC_Items.RemoveAll(x => x.SteamId == item.Item.SteamId);
			}
		}

		await CitiesManager.Subscribe(LC_Items.FilteredItems.Select(x => x.SteamId));
	}

	private async void DeleteAll(object sender, EventArgs e)
	{
		if (ShowPrompt(Locale.AreYouSure, PromptButtons.YesNo) != DialogResult.Yes)
		{
			return;
		}

		I_Actions.Loading = true;
		await Task.Run(() =>
		{
			var items = LC_Items.FilteredItems.ToList();
			foreach (var item in items)
			{
				if (!item.Workshop && item is Asset asset)
				{
					ExtensionClass.DeleteFile(asset.FileName);
				}
				else
				{
					ContentUtil.DeleteAll(item.Folder);
				}
			}
		});
		I_Actions.Loading = false;
	}
}
