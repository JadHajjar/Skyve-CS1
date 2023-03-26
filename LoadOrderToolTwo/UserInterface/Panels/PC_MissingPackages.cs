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
using System.Windows.Forms;

namespace LoadOrderToolTwo.UserInterface.Panels;
public partial class PC_MissingPackages : PanelContent
{
	private readonly Dictionary<ulong, Profile.Asset> _workshopPackages = new();

	public PC_MissingPackages(List<Profile.Mod> missingMods, List<Profile.Asset> missingAssets) : base(true)
	{
		InitializeComponent();

		Text = $"{ProfileManager.CurrentProfile.Name} - {Locale.MissingPackages}";

		foreach (var package in missingAssets.Concat(missingMods).GroupBy(x => x.SteamId))
		{
			if (package.Key != 0)
			{
				LC_Items.Add(package.Last());
				_workshopPackages[package.Key] = package.Last();
			}
		}

		LC_Items.AddRange(missingMods.Where(x => x.SteamId == 0));
		LC_Items.AddRange(missingAssets.Where(x => x.SteamId == 0));

		LC_Items.CanDrawItem += LC_Items_CanDrawItem;

		CentralManager.ContentLoaded += CentralManager_ContentLoaded;

		RefreshCounts();
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

		TB_Search.Margin = L_Counts.Margin = UI.Scale(new Padding(5), UI.FontScale);
		L_Counts.Font = UI.Font(7.5F, FontStyle.Bold);
		TB_Search.Width = (int)(400 * UI.FontScale);
		OT_Workshop.Width = (int)(400 * UI.FontScale);

		B_SteamPage.Margin = B_SteamPage.Padding = UI.Scale(new Padding(7), UI.FontScale);
		TB_Search.Margin = L_Counts.Margin = UI.Scale(new Padding(5), UI.FontScale);
		T_All.Icon = ImageManager.GetIcon(nameof(Properties.Resources.I_Package));
		T_Mods.Icon = ImageManager.GetIcon(nameof(Properties.Resources.I_Mods));
		T_Assets.Icon = ImageManager.GetIcon(nameof(Properties.Resources.I_Assets));
		B_SteamPage.Image = ImageManager.GetIcon(nameof(Properties.Resources.I_Add));
	}

	private void CentralManager_ContentLoaded()
	{
		var items = new List<IGenericPackage>(LC_Items.Items);

		foreach (var item in items)
		{
			if (item is Profile.Mod mod)
			{
				var localMod = ProfileManager.GetMod(mod);

				if (localMod is null)
				{
					continue;
				}

				localMod.IsIncluded = true;
				localMod.IsEnabled = mod.Enabled;

				LC_Items.Remove(item);
			}
			else if (item is Profile.Asset asset)
			{
				var localAsset = ProfileManager.GetAsset(asset);

				if (localAsset is null)
				{
					continue;
				}

				localAsset.IsIncluded = true;

				LC_Items.Remove(item);
			}
		}

		if (LC_Items.ItemCount == 0)
		{
			this.TryInvoke(() => Form.PushBack());
		}
	}

	public override bool CanExit(bool toBeDisposed)
	{
		if (toBeDisposed && LC_Items.ItemCount > 0)
		{
			return ShowPrompt(Locale.MissingItemsRemain, PromptButtons.OKCancel, PromptIcons.Hand) == DialogResult.OK;
		}

		return true;
	}

	protected override bool LoadData()
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

		LC_Items.Invalidate();

		foreach (var item in info.Values)
		{
			ImageManager.Ensure(item.ThumbnailUrl);

			LC_Items.Invalidate();
		}

		return base.LoadData();
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
		TB_Search.Image = string.IsNullOrWhiteSpace(TB_Search.Text) ? Properties.Resources.I_Search : Properties.Resources.I_ClearSearch;
		LC_Items.FilterOrSortingChanged();
		RefreshCounts();
	}

	private async void B_SteamPage_Click(object sender, EventArgs e)
	{
		await CitiesManager.Subscribe(LC_Items.FilteredItems.Select(x => x.SteamId), false);
	}

	private void OT_Workshop_SelectedValueChanged(object sender, EventArgs e)
	{
		LC_Items.FilterOrSortingChanged();
		RefreshCounts();
	}

	private void T_Assets_TabSelected(object sender, EventArgs e)
	{
		LC_Items.FilterOrSortingChanged();
		LC_Items.ResetScroll();
		RefreshCounts();
	}

	private void LC_Items_CanDrawItem(object sender, CanDrawItemEventArgs<IGenericPackage> e)
	{
		if (T_Mods.Selected)
		{
			e.DoNotDraw = !e.Item.IsMod;
		}
		else if (T_Assets.Selected)
		{
			e.DoNotDraw = e.Item.IsMod;
		}

		if (!e.DoNotDraw && OT_Workshop.SelectedValue != Generic.ThreeOptionToggle.Value.None)
		{
			e.DoNotDraw = e.Item.SteamId == 0 != (OT_Workshop.SelectedValue == Generic.ThreeOptionToggle.Value.Option1);
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
			L_Counts.Text = $"{Locale.Showing} {totalFiltered} {Locale.OutOf.ToLower()} {total} {Locale.TotalItems.ToLower()}";
		}
	}

	private void TB_Search_IconClicked(object sender, EventArgs e)
	{
		TB_Search.Text = string.Empty;
	}
}
