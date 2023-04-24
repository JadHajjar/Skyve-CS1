using CompatibilityReport.CatalogData;

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
public partial class PC_MissingPackages : PanelContent
{
	private readonly Dictionary<ulong, Profile.Asset> _workshopPackages = new();
	private bool allowExit;

	public PC_MissingPackages(List<Profile.Mod> missingMods, List<Profile.Asset> missingAssets) : base(true)
	{
		InitializeComponent();

		foreach (var package in missingAssets.Concat(missingMods).GroupBy(x => x.SteamId))
		{
			if (package.Key != 0)
			{
				if (ModLogicManager.IsBlackListed(package.Key))
				{
					continue;
				}

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

	protected override void LocaleChanged()
	{
		Text = $"{ProfileManager.CurrentProfile.Name} - {Locale.MissingPackages}";
		DD_Tags.Text = Locale.Tags;
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
		TB_Search.Width =
		DD_Tags.Width =
		OT_Workshop.Width = (int)(400 * UI.FontScale);

		B_SteamPage.Margin = B_SteamPage.Padding = UI.Scale(new Padding(7), UI.FontScale);
		TB_Search.Margin = DD_Tags.Margin = OT_Workshop.Margin = L_Counts.Margin = UI.Scale(new Padding(5), UI.FontScale);
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
		if (toBeDisposed && !allowExit && LC_Items.ItemCount > 0)
		{
			if (ShowPrompt(Locale.MissingItemsRemain, PromptButtons.OKCancel, PromptIcons.Hand) == DialogResult.OK)
			{
				allowExit = true;
				Form.PushBack();
			}

			return false;
		}

		return true;
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
			return x.Value.Tags.Select(x => new TagItem(Domain.Enums.TagSource.Workshop, x));
		}).Distinct().ToArray();

		LC_Items.Invalidate();

		foreach (var item in info.Values)
		{
			await ImageManager.Ensure(item.ThumbnailUrl);

			LC_Items.Invalidate();
		}

		return true;
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
		var removeBadPackages = false;
		var steamIds = LC_Items.FilteredItems.ToList(x => x.SteamId);

		steamIds.Remove(0);

		foreach (var item in steamIds.ToList())
		{
			var report = CompatibilityManager.GetCompatibilityReport(item);

			if (report?.Severity == Enums.ReportSeverity.Unsubscribe)
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
				LC_Items.RemoveAll(x => x.SteamId == item);
			}
		}

		await CitiesManager.Subscribe(LC_Items.FilteredItems.Select(x => x.SteamId), false);
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

		if (DD_Tags.SelectedItems.Any())
		{
			foreach (var tag in DD_Tags.SelectedItems)
			{
				if (!(e.Item.Tags?.Any(tag.Value) ?? false))
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
}
