using Extensions;

using LoadOrderToolTwo.Domain;
using LoadOrderToolTwo.Domain.Interfaces;
using LoadOrderToolTwo.Utilities;
using LoadOrderToolTwo.Utilities.Managers;

using SlickControls;

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LoadOrderToolTwo.UserInterface.Panels;
public partial class PC_UnusedLsmPackages : PanelContent
{
	private readonly Dictionary<ulong, Profile.Asset> _workshopPackages = new();

	public PC_UnusedLsmPackages(List<Profile.Asset> missingAssets) : base(true)
	{
		InitializeComponent();

		_workshopPackages = missingAssets.Distinct(x => x.SteamId).ToDictionary(x => x.SteamId, x => x);
		_workshopPackages.Remove(0);

		LC_Items.AddRange(_workshopPackages.Values);
		LC_Items.AddRange(missingAssets.Where(x => x.SteamId == 0));

		LC_Items.CanDrawItem += LC_Items_CanDrawItem;

		CentralManager.ContentLoaded += CentralManager_ContentLoaded;
		CentralManager.AssetInformationUpdated += CentralManager_ContentLoaded;

		RefreshCounts();
	}

	protected override void LocaleChanged()
	{
		DD_Tags.Text = Locale.Tags;
		Text = Locale.UnusedPackages;
	}

	protected override void DesignChanged(FormDesign design)
	{
		base.DesignChanged(design);

		LC_Items.BackColor = design.AccentBackColor;
		L_Counts.ForeColor = design.InfoColor;
	}

	protected override void UIChanged()
	{
		base.UIChanged();

		TB_Search.Margin = DD_Tags.Margin = L_Counts.Margin = UI.Scale(new Padding(5), UI.FontScale);
		L_Counts.Font = UI.Font(7.5F, FontStyle.Bold);
		TB_Search.Width = DD_Tags.Width = (int)(400 * UI.FontScale);

		B_SubscribeAll.Margin = B_ExcludeAll.Margin = B_IncludeAll.Margin = B_SubscribeAll.Padding = UI.Scale(new Padding(7), UI.FontScale);
		TB_Search.Margin = L_Counts.Margin = UI.Scale(new Padding(5), UI.FontScale);
	}

	private void CentralManager_ContentLoaded()
	{
		var items = new List<IGenericPackage>(LC_Items.Items);

		foreach (var item in items)
		{
			if (item is Profile.Asset asset)
			{
				var localAsset = ProfileManager.GetAsset(asset);

				if (localAsset is null || !localAsset.IsIncluded)
				{
					continue;
				}

				LC_Items.Remove(item);
			}
		}

		if (LC_Items.ItemCount == 0)
		{
			this.TryInvoke(() => Form.PushBack());
		}

		LC_Items.Invalidate();
	}

	protected override async Task<bool> LoadDataAsync()
	{
		var steamIds = _workshopPackages.Keys.Distinct().ToArray();

		var info = SteamUtil.GetWorkshopInfoAsync(steamIds).Result;

		foreach (var item in info)
		{
			if (_workshopPackages.ContainsKey(item.Key))
			{
				_workshopPackages[item.Key].WorkshopInfo = item.Value;
			}
		}

		DD_Tags.Items = info.SelectMany(x =>
		{
			return x.Value.Tags;
		}).Distinct().ToArray();

		LC_Items.Invalidate();

		foreach (var item in info.Values)
		{
			await ImageManager.Ensure(item.ThumbnailUrl);

			LC_Items.Invalidate();
		}

		return true;
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

	internal static void PromptMissingPackages(BasePanelForm form, List<Profile.Mod> missingMods, List<Profile.Asset> missingAssets)
	{
		var pauseEvent = new AutoResetEvent(false);

		form.TryInvoke(() =>
		{
			var panel = new PC_MissingPackages(missingMods, missingAssets);

			form.PushPanel(null, panel);

			panel.Disposed += (s, e) =>
			{
				pauseEvent.Set();
			};
		});

		pauseEvent.WaitOne();
	}

	private void TB_Search_TextChanged(object sender, EventArgs e)
	{
		TB_Search.ImageName = string.IsNullOrWhiteSpace(TB_Search.Text) ? "I_Search" : "I_ClearSearch";
		LC_Items.FilterChanged();
		RefreshCounts();
	}

	private async void B_SteamPage_Click(object sender, EventArgs e)
	{
		await CitiesManager.Subscribe(LC_Items.FilteredItems.Select(x => x.SteamId), true);
	}

	private void OT_Workshop_SelectedValueChanged(object sender, EventArgs e)
	{
		LC_Items.FilterChanged();
		RefreshCounts();
	}

	private void T_Assets_TabSelected(object sender, EventArgs e)
	{
		LC_Items.FilterChanged();
		LC_Items.ResetScroll();
		RefreshCounts();
	}

	private void LC_Items_CanDrawItem(object sender, CanDrawItemEventArgs<IGenericPackage> e)
	{
		if (DD_Tags.SelectedItems.Any())
		{
			foreach (var tag in DD_Tags.SelectedItems)
			{
				if (!(e.Item.Tags?.Any(tag) ?? false))
				{
					e.DoNotDraw = true;
				}
			}
		}

		if (!e.DoNotDraw && !string.IsNullOrWhiteSpace(TB_Search.Text))
		{
			e.DoNotDraw = !(
				TB_Search.Text.SearchCheck(e.Item.Name) ||
				TB_Search.Text.SearchCheck(e.Item.Author?.Name) ||
				TB_Search.Text.SearchCheck(e.Item.SteamId.ToString()) ||
				(e.Item.Tags?.Any(x => TB_Search.Text.SearchCheck(x)) ?? false));
		}
	}

	private void RefreshCounts()
	{
		var total = LC_Items.ItemCount;
		var totalFiltered = LC_Items.FilteredCount;

		if (totalFiltered == total)
		{
			L_Counts.Text = $"{total} {Locale.TotalItems.ToLower()}";
		}
		else
		{
			L_Counts.Text = string.Format(Locale.ShowingFilteredItems, totalFiltered, total);
		}
	}

	private void TB_Search_IconClicked(object sender, EventArgs e)
	{
		TB_Search.Text = string.Empty;
	}

	private void B_ExcludeAll_Click(object sender, EventArgs e)
	{
		var items = LC_Items.FilteredItems.ToList();

		var packages = items.Select(x => { ContentUtil.GetGenericPackageState(x, out var p); return p; }).Where(x => x != null).ToList();

		ContentUtil.SetBulkIncluded(packages.SelectMany(x => x!.Assets), false);
	}

	private void B_IncludeAll_Click(object sender, EventArgs e)
	{
		var items = LC_Items.FilteredItems.ToList();

		var packages = items.Select(x => { ContentUtil.GetGenericPackageState(x, out var p); return p; }).Where(x => x != null).ToList();

		ContentUtil.SetBulkIncluded(packages.SelectMany(x => x!.Assets), true);
	}
}
