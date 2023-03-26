using Extensions;

using LoadOrderToolTwo.Domain.Interfaces;
using LoadOrderToolTwo.Utilities;
using LoadOrderToolTwo.Utilities.Managers;

using SlickControls;

using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace LoadOrderToolTwo.UserInterface.Panels;
public partial class PC_ImportCollection : PanelContent
{
	private readonly string? _id;

	public PC_ImportCollection(Domain.Steam.SteamWorkshopItem collection, Dictionary<ulong, Domain.Steam.SteamWorkshopItem> contents)
	{
		InitializeComponent();

		Text = Locale.CollectionTitle;
		_id = collection.PublishedFileID;
		L_Title.Text = collection.Title;

		PB_Icon.Collection = true;
		PB_Icon.LoadImage(collection.PreviewURL, ImageManager.GetImage);

		LC_Items.CanDrawItem += LC_Items_CanDrawItem;

		LC_Items.SetItems(contents.Values);

		RefreshCounts();

		new BackgroundAction(() =>
		{
			foreach (var item in LC_Items.Items)
			{
				ImageManager.Ensure(item.ThumbnailUrl);
			}
		}).Run();
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

		if (!e.DoNotDraw && !string.IsNullOrWhiteSpace(TB_Search.Text))
		{
			e.DoNotDraw = !(
				TB_Search.Text.SearchCheck(e.Item.Name) ||
				TB_Search.Text.SearchCheck(e.Item.Author?.Name) ||
				TB_Search.Text.SearchCheck(e.Item.SteamId.ToString()) ||
				(e.Item.Tags?.Any(x => TB_Search.Text.SearchCheck(x)) ?? false));
		}
	}

	protected override void UIChanged()
	{
		base.UIChanged();

		B_ExInclude.Width = B_UnsubSub.Width = (int)(550 * UI.FontScale);
		TB_Search.Width = (int)(290 * UI.FontScale);
		PB_Icon.Width = TLP_Top.Height = (int)(128 * UI.FontScale);
		TLP_Top.Height += 10;
		L_Title.Font = UI.Font(14F, FontStyle.Bold);
		L_Counts.Font = UI.Font(7.5F, FontStyle.Bold);
		L_Title.Margin = B_SteamPage.Margin = B_SteamPage.Padding = UI.Scale(new Padding(7), UI.FontScale);
		TB_Search.Margin = L_Counts.Margin = UI.Scale(new Padding(5), UI.FontScale);
		T_All.Icon = ImageManager.GetIcon(nameof(Properties.Resources.I_Package));
		T_Mods.Icon = ImageManager.GetIcon(nameof(Properties.Resources.I_Mods));
		T_Assets.Icon = ImageManager.GetIcon(nameof(Properties.Resources.I_Assets));
	}

	protected override void DesignChanged(FormDesign design)
	{
		base.DesignChanged(design);

		L_Counts.ForeColor = design.LabelColor;
		BackColor = design.AccentBackColor;
		tableLayoutPanel1.BackColor = panel1.BackColor = panel2.BackColor = P_Back.BackColor = design.BackColor;
	}

	public override Color GetTopBarColor()
	{
		return FormDesign.Design.AccentBackColor;
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

	private void B_SteamPage_Click(object sender, System.EventArgs e)
	{
		try
		{ Process.Start($"https://steamcommunity.com/workshop/filedetails/?id={_id}"); }
		catch { }
	}

	private void T_Assets_TabSelected(object sender, System.EventArgs e)
	{
		LC_Items.FilterOrSortingChanged();
		LC_Items.ResetScroll();
		RefreshCounts();
	}

	private void TB_Search_TextChanged(object sender, System.EventArgs e)
	{
		TB_Search.Image = string.IsNullOrWhiteSpace(TB_Search.Text) ? Properties.Resources.I_Search : Properties.Resources.I_ClearSearch;
		LC_Items.FilterOrSortingChanged();
		RefreshCounts();
	}

	private async void B_UnsubSub_LeftClicked(object sender, System.EventArgs e)
	{
		await CitiesManager.Subscribe(LC_Items.FilteredItems.Select(x => x.SteamId), true);
	}

	private async void B_UnsubSub_RightClicked(object sender, System.EventArgs e)
	{
		await CitiesManager.Subscribe(LC_Items.FilteredItems.Select(x => x.SteamId), false);
	}

	private void B_ExInclude_LeftClicked(object sender, System.EventArgs e)
	{
		var filteredItems = LC_Items.FilteredItems.Select(x => CentralManager.Packages.FirstOrDefault(y => y.SteamId == x.SteamId)).ToList();
		
		ModsUtil.SetIncluded(filteredItems.Where(x => x?.Mod is not null).Select(x => x.Mod!), false);
		AssetsUtil.SetIncluded(filteredItems.Where(x => x?.Assets is not null).SelectMany(x => x.Assets!), false);
	}

	private void B_ExInclude_RightClicked(object sender, System.EventArgs e)
	{
		var filteredItems = LC_Items.FilteredItems.Select(x => CentralManager.Packages.FirstOrDefault(y => y.SteamId == x.SteamId)).ToList();
		
		ModsUtil.SetIncluded(filteredItems.Where(x => x?.Mod is not null).Select(x => x.Mod!), true);
		AssetsUtil.SetIncluded(filteredItems.Where(x => x?.Assets is not null).SelectMany(x => x.Assets!), true);
	}

	private void TB_Search_IconClicked(object sender, System.EventArgs e)
	{
		TB_Search.Text = string.Empty;
	}
}
