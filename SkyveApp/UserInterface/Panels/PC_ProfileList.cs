using Extensions;

using SkyveApp.Domain;
using SkyveApp.UserInterface.Lists;
using SkyveApp.Utilities;
using SkyveApp.Utilities.Managers;

using SlickControls;

using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace SkyveApp.UserInterface.Panels;
public partial class PC_ProfileList : PanelContent
{
	private readonly ProfileListControl LC_Items;

	public PC_ProfileList()
	{
		InitializeComponent();

		DD_Sorting.Height = TB_Search.Height = 0;

		LC_Items = new ProfileListControl() { Dock = DockStyle.Fill };
		LC_Items.CanDrawItem += Ctrl_CanDrawItem;
		LC_Items.LoadProfile += Ctrl_LoadProfile;
		LC_Items.MergeProfile += Ctrl_MergeProfile;
		LC_Items.ExcludeProfile += Ctrl_ExcludeProfile;
		LC_Items.DisposeProfile += Ctrl_DisposeProfile;
		panel1.Controls.Add(LC_Items);

		RefreshCounts();
	}

	private void Ctrl_CanDrawItem(object sender, CanDrawItemEventArgs<Profile> e)
	{
		var valid = true;

		if (e.Item.Usage > 0)
		{
			valid &= DD_Usage.SelectedItems.Contains(e.Item.Usage);
		}

		if (!string.IsNullOrWhiteSpace(TB_Search.Text))
		{
			valid &= TB_Search.Text.SearchCheck(e.Item.Name);
		}

		e.DoNotDraw = !valid;
	}

	private void Ctrl_DisposeProfile(Profile obj)
	{
		ProfileManager.DeleteProfile(obj);
	}

	private void Ctrl_ExcludeProfile(Profile obj)
	{
		FLP_Profiles.Enabled = false;
		ProfileManager.ExcludeProfile(obj);
	}

	private void Ctrl_MergeProfile(Profile obj)
	{
		FLP_Profiles.Enabled = false;
		ProfileManager.MergeProfile(obj);
	}

	private void Ctrl_LoadProfile(Profile obj)
	{
		Form.PushBack();

		(Form.CurrentPanel as PC_Profiles)?.Ctrl_LoadProfile(obj);
	}

	private void RefreshCounts()
	{
		var favorites = ProfileManager.Profiles.Count(x => x.IsFavorite);
		var total = ProfileManager.Profiles.Count(x => !x.Temporary);
		var filteredCount = LC_Items.FilteredItems.Count();
		var text = string.Empty;

		if (favorites == 0)
		{
			text = string.Format(Locale.FavoriteTotal, total);
		}
		else
		{
			text = string.Format(Locale.FavoriteProfileTotal, favorites, total);
		}

		if (L_Counts.Text != text)
		{
			L_Counts.Text = text;
		}

		if (L_FilterCount.Visible = total != filteredCount)
		{
			L_FilterCount.Text = Locale.ShowingCount.FormatPlural(filteredCount, Locale.Profile.FormatPlural(filteredCount));
		}
	}

	protected override void LocaleChanged()
	{
		Text = Locale.YourProfiles;
	}

	protected override void UIChanged()
	{
		base.UIChanged();

		TB_Search.Margin = L_Counts.Margin = L_FilterCount.Margin = DD_Usage.Margin = UI.Scale(new Padding(5), UI.FontScale);
		L_Counts.Font = L_FilterCount.Font = UI.Font(7.5F, FontStyle.Bold);
		DD_Sorting.Width = (int)(180 * UI.FontScale);
		TB_Search.Width = (int)(400 * UI.FontScale);
	}

	protected override void DesignChanged(FormDesign design)
	{
		base.DesignChanged(design);

		tableLayoutPanel3.BackColor = design.AccentBackColor;
		L_Counts.ForeColor = L_FilterCount.ForeColor = design.InfoColor;
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

	private void FilterChanged(object sender, EventArgs e)
	{
		TB_Search.ImageName = string.IsNullOrWhiteSpace(TB_Search.Text) ? "I_Search" : "I_ClearSearch";

		LC_Items.FilterChanged();
		RefreshCounts();
	}

	private void DD_Sorting_SelectedItemChanged(object sender, EventArgs e)
	{
		if (IsHandleCreated)
		{
			LC_Items.SetSorting(DD_Sorting.SelectedItem);
		}
	}

	private void TB_Search_IconClicked(object sender, EventArgs e)
	{
		TB_Search.Text = string.Empty;
	}
}
