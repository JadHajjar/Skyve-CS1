using Extensions;

using LoadOrderToolTwo.Domain.Steam;
using LoadOrderToolTwo.Domain.Utilities;
using LoadOrderToolTwo.UserInterface.Lists;
using LoadOrderToolTwo.Utilities;
using LoadOrderToolTwo.Utilities.Managers;

using SlickControls;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace LoadOrderToolTwo.UserInterface.Forms;
public partial class AddPackageForm : BaseForm
{
	private readonly ItemListControl<SteamWorkshopItem> LC_Items;
	private readonly DelayedAction _delayedSearch;
	private bool searchEmpty = true;
	private readonly List<string> searchTermsOr = new();
	private readonly List<string> searchTermsAnd = new();
	private readonly List<string> searchTermsExclude = new();
	public event Action<SteamWorkshopItem>? PackageSelected;

	public AddPackageForm()
	{
		InitializeComponent();

		_delayedSearch = new(350, DelayedSearch);

		TB_Search.Placeholder = LocaleHelper.GetGlobalText("Search") + "..";

		Text = LocaleHelper.GetGlobalText("Add Packages");

		LC_Items = new()
		{
			IsSelection = true,
			IsGenericPage = true,
			Dock = DockStyle.Fill
		};

		LC_Items.PackageSelected += (p) => { Close(); PackageSelected?.Invoke(p); };

		LC_Items.SetSorting(Domain.Enums.PackageSorting.None, false);

		base_P_Content.Controls.Add(LC_Items);

		LC_Items.BringToFront();

		TB_Search.Loading = true;
		_delayedSearch.Run();
	}

	protected override void OnDeactivate(EventArgs e)
	{
		base.OnDeactivate(e);

		if (CurrentFormState != FormState.ForcedFocused)
		{
			Close();
		}
	}

	private bool DoNotDraw(SteamWorkshopItem item)
	{
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

	private bool Search(string searchTerm, SteamWorkshopItem item)
	{
		return searchTerm.SearchCheck(item.ToString())
			|| searchTerm.SearchCheck(item.Author?.Name)
			|| Path.GetFileName(item.Folder).IndexOf(searchTerm, StringComparison.OrdinalIgnoreCase) != -1;
	}

	protected override void UIChanged()
	{
		base.UIChanged();

		OT_ModAsset.Width = (int)(200 * UI.FontScale);
		TB_Search.Width = (int)(250 * UI.FontScale);
		TB_Search.Margin = OT_ModAsset.Margin = UI.Scale(new Padding(10), UI.FontScale);
	}

	protected override void DesignChanged(FormDesign design)
	{
		base.DesignChanged(design);

		tableLayoutPanel1.BackColor = design.AccentBackColor;
	}

	private void TB_Search_TextChanged(object sender, EventArgs e)
	{
		TB_Search.ImageName = (searchEmpty = string.IsNullOrWhiteSpace(TB_Search.Text)) ? "I_Search" : "I_ClearSearch";
		TB_Search.Loading = true;

		var searchText = TB_Search.Text.Trim();

		searchTermsAnd.Clear();
		searchTermsExclude.Clear();
		searchTermsOr.Clear();

		LC_Items.TextSearchEmpty = searchEmpty;

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

		_delayedSearch.Run();
	}

	private async void DelayedSearch()
	{
		var items = ulong.TryParse(TB_Search.Text.Trim(), out var steamId) 
			? await SteamUtil.GetWorkshopInfoAsync(new[] { steamId }) 
			: await SteamUtil.QueryFilesAsync(searchEmpty ? SteamQueryOrder.RankedByTrend : SteamQueryOrder.RankedByTextSearch,
				TB_Search.Text,
				OT_ModAsset.SelectedValue == Generic.ThreeOptionToggle.Value.Option1 ? new[] { "Mod" } : null,
			 	OT_ModAsset.SelectedValue == Generic.ThreeOptionToggle.Value.Option2 ? new[] { "Mod" } : null);

		LC_Items.SetItems(items.Values.Where(x => !DoNotDraw(x)));

		new BackgroundAction(() =>
		{
			Parallelism.ForEach(LC_Items.Items.ToList(), async package =>
			{
				if (!string.IsNullOrWhiteSpace(package.IconUrl))
				{
					if (await ImageManager.Ensure(package.IconUrl))
						LC_Items.Invalidate(package);
				}

				if (!string.IsNullOrWhiteSpace(package.Author?.AvatarUrl))
				{
					if (await ImageManager.Ensure(package.Author?.AvatarUrl))
						LC_Items.Invalidate(package);
				}
			}, 4);
		}).Run();

		TB_Search.Loading = false;
	}

	private void TB_Search_IconClicked(object sender, EventArgs e)
	{
		TB_Search.Text = string.Empty;
	}
}
