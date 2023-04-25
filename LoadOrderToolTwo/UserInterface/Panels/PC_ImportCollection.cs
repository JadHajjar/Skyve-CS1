﻿using Extensions;

using LoadOrderToolTwo.Domain;
using LoadOrderToolTwo.Domain.Interfaces;
using LoadOrderToolTwo.Utilities;
using LoadOrderToolTwo.Utilities.IO;
using LoadOrderToolTwo.Utilities.Managers;

using SlickControls;

using System.Collections.Generic;
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

		_id = collection.PublishedFileID;
		L_Title.Text = collection.Title;

		PB_Icon.Collection = true;
		PB_Icon.LoadImage(collection.PreviewURL, ImageManager.GetImage);

		LC_Items.CanDrawItem += LC_Items_CanDrawItem;

		foreach (var item in ModLogicManager.BlackList)
		{
			if (contents.ContainsKey(item))
			{
				contents.Remove(item);
			}
		}

		LC_Items.SetItems(contents.Values);

		RefreshCounts();

		new BackgroundAction(async () =>
		{
			DD_Tags.Items = contents.SelectMany(x =>
			{
				return x.Value.Tags.Select(x => new TagItem(Domain.Enums.TagSource.Workshop, x));
			}).Distinct().ToArray();

			var items = new List<IGenericPackage>(LC_Items.Items);

			foreach (var item in items)
			{
				await ImageManager.Ensure(item.ThumbnailUrl);
			}
		}).Run();

		CentralManager.ContentLoaded += CentralManager_ContentLoaded;
		CentralManager.PackageInformationUpdated += CentralManager_ContentLoaded;
	}

	private void CentralManager_ContentLoaded()
	{
		LC_Items.Invalidate();
	}

	protected override void LocaleChanged()
	{
		DD_Tags.Text = Locale.Tags;
		Text = Locale.CollectionTitle;
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

	protected override void UIChanged()
	{
		base.UIChanged();

		B_ExInclude.Width = B_UnsubSub.Width = (int)(450 * UI.FontScale);
		B_ExInclude.Font = B_UnsubSub.Font = UI.Font(8.25F);
		B_ExInclude.Margin = UI.Scale(new Padding(5, 0, 5, 5), UI.FontScale);
		B_UnsubSub.Margin = UI.Scale(new Padding(5, 5, 5, 0), UI.FontScale);
		TB_Search.Width = DD_Tags.Width = (int)(290 * UI.FontScale);
		PB_Icon.Width = TLP_Top.Height = (int)(128 * UI.FontScale);
		TLP_Top.Height += 10;
		L_Title.Font = UI.Font(14F, FontStyle.Bold);
		L_Counts.Font = UI.Font(7.5F, FontStyle.Bold);
		L_Title.Margin = UI.Scale(new Padding(7), UI.FontScale);
		L_Counts.Margin = UI.Scale(new Padding(5), UI.FontScale);
		TB_Search.Margin = DD_Tags.Margin = UI.Scale(new Padding(5, 5, 5, 0), UI.FontScale);
	}

	protected override void DesignChanged(FormDesign design)
	{
		base.DesignChanged(design);

		L_Counts.ForeColor = design.LabelColor;
		BackColor = GetTopBarColor();
		LC_Items.BackColor = design.BackColor;
	}

	public override Color GetTopBarColor()
	{
		return FormDesign.Design.BackColor.Tint(Lum: FormDesign.Design.Type.If(FormDesignType.Dark, 1, -1));
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

	private void B_SteamPage_Click(object sender, System.EventArgs e)
	{
		PlatformUtil.OpenUrl($"https://steamcommunity.com/workshop/filedetails/?id={_id}");
	}

	private void T_Assets_TabSelected(object sender, System.EventArgs e)
	{
		LC_Items.FilterChanged();
		LC_Items.ResetScroll();
		RefreshCounts();
	}

	private void TB_Search_TextChanged(object sender, System.EventArgs e)
	{
		TB_Search.ImageName = string.IsNullOrWhiteSpace(TB_Search.Text) ? "I_Search" : "I_ClearSearch";
		LC_Items.FilterChanged();
		RefreshCounts();
	}

	private async void B_UnsubSub_RightClicked(object sender, System.EventArgs e)
	{
		await CitiesManager.Subscribe(LC_Items.FilteredItems.Select(x => x.SteamId), true);
	}

	private async void B_UnsubSub_LeftClicked(object sender, System.EventArgs e)
	{
		await CitiesManager.Subscribe(LC_Items.FilteredItems.Select(x => x.SteamId), false);
	}

	private void B_ExInclude_RightClicked(object sender, System.EventArgs e)
	{
		var filteredItems = LC_Items.FilteredItems.Select(x => CentralManager.Packages.FirstOrDefault(y => y.SteamId == x.SteamId));

		ContentUtil.SetBulkIncluded(filteredItems, false);
	}

	private void B_ExInclude_LeftClicked(object sender, System.EventArgs e)
	{
		var filteredItems = LC_Items.FilteredItems.Select(x => CentralManager.Packages.FirstOrDefault(y => y.SteamId == x.SteamId));

		ContentUtil.SetBulkIncluded(filteredItems, true);
	}

	private void TB_Search_IconClicked(object sender, System.EventArgs e)
	{
		TB_Search.Text = string.Empty;
	}
}
