using SkyveApp.Domain.CS1;
using SkyveApp.Domain.CS1.Enums;
using SkyveApp.Systems.CS1.Utilities;
using SkyveApp.UserInterface.Generic;
using SkyveApp.UserInterface.Lists;

using System.Drawing;
using System.IO;
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
	private readonly DelayedAction _delayedAuthorTagsRefresh;
	protected readonly ItemListControl<T> LC_Items;
	private readonly IncludeAllButton<T> I_Actions;
	protected int UsageFilteredOut;
	private bool searchEmpty = true;
	private readonly List<string> searchTermsOr = new();
	private readonly List<string> searchTermsAnd = new();
	private readonly List<string> searchTermsExclude = new();

	private readonly ISettings _settings;
	private readonly INotifier _notifier;
	private readonly ICompatibilityManager _compatibilityManager;
	private readonly IPlaysetManager _profileManager;
	private readonly ITagsService _tagUtil;
	private readonly IPackageUtil _packageUtil;
	private readonly IDownloadService _downloadService;

	public virtual SkyvePage Page { get; }

	public PC_ContentList() : this(false) { }

	public PC_ContentList(bool load) : base(load)
	{
		ServiceCenter.Get(out _settings, out _notifier, out _compatibilityManager, out _profileManager, out _tagUtil, out _packageUtil, out _downloadService);

		LC_Items = new(Page) { Dock = DockStyle.Fill, Margin = new() };

		InitializeComponent();

		DD_Sorting.SkyvePage = Page;

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

		OT_Workshop.Visible = !((_profileManager.CurrentPlayset as Playset)?.LaunchSettings.NoWorkshop ?? false);
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
		LC_Items.SelectedItemsChanged += (_, _) => RefreshCounts();

		_delayedSearch = new(350, DelayedSearch);
		_delayedAuthorTagsRefresh = new(350, RefreshAuthorAndTags);

		if (!load)
		{
			if (!_notifier.IsContentLoaded)
			{
				LC_Items.Loading = true;
			}
			else
			{
				LC_Items.SetItems(GetItems());
			}
		}

		if (!_settings.UserSettings.AdvancedIncludeEnable || this is PC_Assets)
		{
			OT_Enabled.Hide();
			P_Filters.SetRow(OT_Workshop, 2);
			P_Filters.SetRow(OT_ModAsset, 3);
		}

		clearingFilters = false;

		RefreshCounts();

		I_SortOrder.ImageName = LC_Items.SortDescending ? "I_SortDesc" : "I_SortAsc";
		B_GridView.Selected = LC_Items.GridView;
		B_ListView.Selected = !LC_Items.GridView;

		if (!load)
		{
			_notifier.ContentLoaded += CentralManager_ContentLoaded;
		}
		else
		{
			_notifier.ContentLoaded += CentralManager_WorkshopInfoUpdated;
		}

		_notifier.WorkshopInfoUpdated += CentralManager_WorkshopInfoUpdated;
		_notifier.PackageInformationUpdated += CentralManager_WorkshopInfoUpdated;
		_notifier.CompatibilityReportProcessed += CentralManager_WorkshopInfoUpdated;

		if (!load)
		{
			new BackgroundAction("Getting tag list", RefreshAuthorAndTags).Run();
		}

		SlickTip.SetTo(B_GridView, "Switch to Grid-View");
		SlickTip.SetTo(B_ListView, "Switch to List-View");
	}

	protected void RefreshAuthorAndTags()
	{
		var items = new List<T>(LC_Items.Items);

		DD_Author.SetItems(items);
		DD_Tags.Items = _tagUtil.GetDistinctTags().ToArray();
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

	private void LC_Items_AuthorSelected(IUser obj)
	{
		DD_Author.Select(obj);

		if (P_FiltersContainer.Height == 0)
		{
			B_Filters_Click(this, null);
		}
	}

	private void LC_Items_TagSelected(ITag obj)
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
		if (DD_PackageStatus.SelectedItem == (DownloadStatusFilter)(obj + 1))
		{
			DD_PackageStatus.SelectedItem = DownloadStatusFilter.Any;
		}
		else
		{
			DD_PackageStatus.SelectedItem = (DownloadStatusFilter)(obj + 1);
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
		DD_Profile.Text = Locale.PlaysetFilter;
		DR_SubscribeTime.Text = Locale.DateSubscribed;
		DR_ServerTime.Text = Locale.DateUpdated;
		DD_Author.Text = Locale.Author;
	}

	protected override void OnCreateControl()
	{
		base.OnCreateControl();

		firstFilterPassed = true;

		if (_settings.UserSettings.AlwaysOpenFiltersAndActions)
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

	protected virtual void SetIncluded(IEnumerable<T> filteredItems, bool included)
	{
		ServiceCenter.Get<IBulkUtil>().SetBulkIncluded(filteredItems.SelectWhereNotNull(x => x.LocalPackage)!, included);
	}

	protected virtual void SetEnabled(IEnumerable<T> filteredItems, bool enabled)
	{
		ServiceCenter.Get<IBulkUtil>().SetBulkEnabled(filteredItems.SelectWhereNotNull(x => x.LocalPackage)!, enabled);
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

		B_ListView.Size = B_GridView.Size = UI.Scale(new Size(24, 24), UI.FontScale);

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
		I_Actions.Invalidate();

		this.TryInvoke(RefreshCounts);

		_delayedAuthorTagsRefresh.Run();
	}

	private void DD_Sorting_SelectedItemChanged(object sender, EventArgs e)
	{
		var settings = _settings.UserSettings.PageSettings.GetOrAdd(Page);
		settings.Sorting = (int)DD_Sorting.SelectedItem;
		_settings.SessionSettings.Save();

		LC_Items.SetSorting(DD_Sorting.SelectedItem, LC_Items.SortDescending);
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
		if ((_profileManager.CurrentPlayset as Playset)!.LaunchSettings.NoWorkshop)
		{
			if (item is ILocalPackage && !item.IsLocal)
			{
				return true;
			}
		}

		if (item.GetWorkshopInfo()?.IsInvalid == true)
		{
			return true;
		}

		if (_profileManager.CurrentPlayset.Usage > 0)
		{
			if (!(_compatibilityManager.GetPackageInfo(item)?.Usage.HasFlag(_profileManager.CurrentPlayset.Usage) ?? true))
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
			if (OT_Workshop.SelectedValue == ThreeOptionToggle.Value.Option1 == !item.IsLocal)
			{
				return true;
			}
		}

		if (OT_Included.SelectedValue != ThreeOptionToggle.Value.None)
		{
			if (OT_Included.SelectedValue == ThreeOptionToggle.Value.Option2 == item.LocalPackage?.IsIncluded())
			{
				return true;
			}
		}

		if (OT_Enabled.SelectedValue != ThreeOptionToggle.Value.None)
		{
			if (item.IsMod || OT_Enabled.SelectedValue == ThreeOptionToggle.Value.Option2 == item.LocalPackage?.IsEnabled())
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
			//if (DD_PackageStatus.SelectedItem == DownloadStatusFilter.None)
			//{
			//	if (item.Workshop)
			//	{
			//		return true;
			//	}
			//}
			//else
			if (DD_PackageStatus.SelectedItem == DownloadStatusFilter.AnyIssue)
			{
				if (item.IsLocal || _packageUtil.GetStatus(item, out _) <= DownloadStatus.OK)
				{
					return true;
				}
			}
			else
			{
				if (((int)DD_PackageStatus.SelectedItem - 1) != (int)_packageUtil.GetStatus(item, out _))
				{
					return true;
				}
			}
		}

		if (DD_ReportSeverity.SelectedItem != Dropdowns.CompatibilityNotificationFilter.Any)
		{
			if (DD_ReportSeverity.SelectedItem == Dropdowns.CompatibilityNotificationFilter.AnyIssue)
			{
				if (item.GetCompatibilityInfo().GetNotification() <= NotificationType.Info)
				{
					return true;
				}
			}
			else if (DD_ReportSeverity.SelectedItem == Dropdowns.CompatibilityNotificationFilter.NoIssues)
			{
				return item.GetCompatibilityInfo().GetNotification() > NotificationType.Info;
			}
			else if ((int)item.GetCompatibilityInfo().GetNotification() != (int)DD_ReportSeverity.SelectedItem)
			{
				return true;
			}
		}

		if (DR_SubscribeTime.Set && !DR_SubscribeTime.Match(item.LocalPackage?.LocalTime.ToLocalTime() ?? DateTime.MinValue))
		{
			return true;
		}

		if (DR_ServerTime.Set && !DR_ServerTime.Match(item.GetWorkshopInfo()?.ServerTime.ToLocalTime() ?? default))
		{
			return true;
		}

		if (DD_Author.SelectedItems.Any())
		{
			if (!DD_Author.SelectedItems.Any(x => item.GetWorkshopInfo()?.Author?.Equals(x) ?? false))
			{
				return true;
			}
		}

		if (DD_Tags.SelectedItems.Any())
		{
			if (!_tagUtil.HasAllTags(item, DD_Tags.SelectedItems))
			{
				return true;
			}
		}

		if (DD_Profile.SelectedItem is not null && !DD_Profile.SelectedItem.Temporary)
		{
			return !_profileManager.IsPackageIncludedInPlayset(item, DD_Profile.SelectedItem);
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
			|| searchTerm.SearchCheck(item.GetWorkshopInfo()?.Author?.Name)
			|| (!item.IsLocal ? item.Id.ToString() : Path.GetFileName(item.LocalParentPackage?.Folder) ?? string.Empty).IndexOf(searchTerm, StringComparison.OrdinalIgnoreCase) != -1;
	}

	private void LC_Items_CanDrawItem(object sender, CanDrawItemEventArgs<T> e)
	{
		e.DoNotDraw = IsFilteredOut(e.Item);
	}

	protected void RefreshCounts()
	{
		var countText = GetCountText();
		var format = LC_Items.SelectedItemsCount == 0 ? (UsageFilteredOut == 0 ? Locale.ShowingCount : Locale.ShowingCountWarning) : (UsageFilteredOut == 0 ? Locale.ShowingSelectedCount : Locale.ShowingSelectedCountWarning);
		var filteredText = format.FormatPlural(
			LC_Items.FilteredCount,
			GetItemText().FormatPlural(LC_Items.FilteredCount).ToLower(),
			LC_Items.SelectedItemsCount, Locale.ItemsHidden.FormatPlural(UsageFilteredOut,
			GetItemText().FormatPlural(LC_Items.FilteredCount).ToLower()));

		if (L_Counts.Text != countText)
		{
			L_Counts.Text = countText;
		}

		if (L_FilterCount.Text != filteredText)
		{
			L_FilterCount.Visible = !string.IsNullOrEmpty(filteredText);
			L_FilterCount.Text = filteredText;
		}

		I_Actions.Invalidate();
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

		LC_Items.IsTextSearchNotEmpty = !searchEmpty;

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
		LC_Items.SetSorting(DD_Sorting.SelectedItem, !LC_Items.SortDescending);

		var settings = _settings.UserSettings.PageSettings.GetOrAdd(Page);
		settings.DescendingSort = LC_Items.SortDescending;
		_settings.SessionSettings.Save();

		I_SortOrder.ImageName = LC_Items.SortDescending ? "I_SortDesc" : "I_SortAsc";
	}

	private void I_Actions_Click(object sender, EventArgs e)
	{
		var items = new SlickStripItem[]
		{
			  new (Locale.IncludeAll, "I_Check", action: () => IncludeAll(this, EventArgs.Empty))
			, new (Locale.ExcludeAll, "I_X", action: () =>ExcludeAll(this, EventArgs.Empty))
			, new (string.Empty)
			, new (Locale.EnableAll, "I_Enabled", _settings.UserSettings.AdvancedIncludeEnable, action:() => EnableAll(this, EventArgs.Empty))
			, new (Locale.DisableAll, "I_Disabled", _settings.UserSettings.AdvancedIncludeEnable, action: () => DisableAll(this, EventArgs.Empty))
			, new (string.Empty)
			, new (Locale.SelectAll, "I_DragDrop", LC_Items.SelectedItemsCount < LC_Items.FilteredItems.Count(), action: LC_Items.SelectAll)
			, new (Locale.DeselectAll, "I_Select", LC_Items.SelectedItemsCount > 0, action: LC_Items.DeselectAll)
			, new (Locale.CopyAllIds, "I_Copy", action: () => Clipboard.SetText(LC_Items.FilteredItems.ListStrings(x => x.IsLocal ? $"Local: {x.Name}" : $"{x.Id}: {x.Name}", CrossIO.NewLine)))
			, new (Locale.SubscribeAll, "I_Steam", this is PC_GenericPackageList, action: () => SubscribeAll(this, EventArgs.Empty))
			, new (Locale.DownloadAll, "I_Install", LC_Items.FilteredItems.Any(x => x.LocalPackage is null), action: () => DownloadAll(this, EventArgs.Empty))
			, new (Locale.ReDownloadAll, "I_ReDownload", LC_Items.FilteredItems.Any(x => x.LocalPackage is not null), action: () => ReDownloadAll(this, EventArgs.Empty))
			, new (string.Empty)
			, new (Locale.UnsubscribeAll, "I_RemoveSteam", action: () => UnsubscribeAll(this, EventArgs.Empty))
			, new (Locale.DeleteAll, "I_Disposable", action: () => DeleteAll(this, EventArgs.Empty))
		};

		this.TryBeginInvoke(() => SlickToolStrip.Show(Program.MainForm, I_Actions.PointToScreen(new Point(I_Actions.Width + 5, 0)), items));
	}

	private void DisableAll(object sender, EventArgs e)
	{
		SetEnabled(LC_Items.FilteredItems, false);
		LC_Items.Invalidate();
		I_Actions.Invalidate();
	}

	private void EnableAll(object sender, EventArgs e)
	{
		SetEnabled(LC_Items.FilteredItems, true);
		LC_Items.Invalidate();
		I_Actions.Invalidate();
	}

	private void ExcludeAll(object sender, EventArgs e)
	{
		SetIncluded(LC_Items.FilteredItems, false);
		LC_Items.Invalidate();
		I_Actions.Invalidate();
	}

	private void IncludeAll(object sender, EventArgs e)
	{
		SetIncluded(LC_Items.FilteredItems, true);
		LC_Items.Invalidate();
		I_Actions.Invalidate();
	}

	private void DownloadAll(object sender, EventArgs e)
	{
		_downloadService.Download(LC_Items.FilteredItems.Where(x => x.LocalPackage is null).Select(x => (IPackageIdentity)x));
		LC_Items.Invalidate();
		I_Actions.Invalidate();
	}

	private void ReDownloadAll(object sender, EventArgs e)
	{
		_downloadService.Download(LC_Items.FilteredItems.Where(x => x.LocalPackage is not null).Cast<IPackageIdentity>());
		LC_Items.Invalidate();
		I_Actions.Invalidate();
	}

	private void UnsubscribeAll(object sender, EventArgs e)
	{
		if (ShowPrompt(Locale.AreYouSure + "\r\n\r\n" + Locale.ThisUnsubscribesFrom.FormatPlural(LC_Items.FilteredItems.Count()), PromptButtons.YesNo) != DialogResult.Yes)
		{
			return;
		}

		I_Actions.Loading = true;
		ServiceCenter.Get<ISubscriptionsManager>().UnSubscribe(LC_Items.FilteredItems.Cast<IPackageIdentity>());
		I_Actions.Loading = false;
		LC_Items.Invalidate();
		I_Actions.Invalidate();
	}

	private void SubscribeAll(object sender, EventArgs e)
	{
		var removeBadPackages = false;
		var steamIds = LC_Items.SafeGetItems().AllWhere(x => x.Item.LocalPackage == null && x.Item.Id != 0);

		foreach (var item in steamIds.ToList())
		{
			var report = item.Item.GetCompatibilityInfo();

			if (report.GetNotification() >= NotificationType.Unsubscribe)
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
				LC_Items.RemoveAll(x => x.Id == item.Item.Id);
			}
		}

		if (steamIds.Count == 0 || ShowPrompt(Locale.AreYouSure + "\r\n\r\n" + Locale.ThisSubscribesTo.FormatPlural(LC_Items.FilteredItems.Count()), PromptButtons.YesNo) != DialogResult.Yes)
		{
			return;
		}

		ServiceCenter.Get<ISubscriptionsManager>().Subscribe(steamIds.Select(x => (IPackageIdentity)x.Item));
		LC_Items.Invalidate();
		I_Actions.Invalidate();
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
				if (item.IsLocal && item is IAsset asset)
				{
					CrossIO.DeleteFile(asset.FilePath);
				}
				else if (item is ILocalPackage package)
				{
					ServiceCenter.Get<IPackageManager>().DeleteAll(package.Folder);
				}
			}
		});
		I_Actions.Loading = false;
	}

	private void B_ListView_Click(object sender, EventArgs e)
	{
		B_GridView.Selected = false;
		B_ListView.Selected = true;
		LC_Items.GridView = false;

		var settings = _settings.UserSettings.PageSettings.GetOrAdd(Page);
		settings.GridView = LC_Items.GridView;
		_settings.SessionSettings.Save();
	}

	private void B_GridView_Click(object sender, EventArgs e)
	{
		B_GridView.Selected = true;
		B_ListView.Selected = false;
		LC_Items.GridView = true;

		var settings = _settings.UserSettings.PageSettings.GetOrAdd(Page);
		settings.GridView = LC_Items.GridView;
		_settings.SessionSettings.Save();
	}
}
