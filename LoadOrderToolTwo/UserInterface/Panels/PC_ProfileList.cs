using Extensions;

using LoadOrderToolTwo.Domain;
using LoadOrderToolTwo.Domain.Enums;
using LoadOrderToolTwo.UserInterface.Generic;
using LoadOrderToolTwo.UserInterface.Lists;
using LoadOrderToolTwo.UserInterface.Profiles;
using LoadOrderToolTwo.Utilities;
using LoadOrderToolTwo.Utilities.Managers;

using SlickControls;

using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace LoadOrderToolTwo.UserInterface.Panels;
public partial class PC_ProfileList : PanelContent
{
	private readonly ProfileListControl LC_Items;

	public PC_ProfileList()
	{
		InitializeComponent();

		DD_Sorting.Height = TB_Search.Height = 0;

		LC_Items= new ProfileListControl() { Dock = DockStyle.Fill };
		LC_Items.CanDrawItem += Ctrl_CanDrawItem;
		LC_Items.LoadProfile += Ctrl_LoadProfile;
		LC_Items.MergeProfile += Ctrl_MergeProfile;
		LC_Items.ExcludeProfile += Ctrl_ExcludeProfile;
		LC_Items.DisposeProfile += Ctrl_DisposeProfile;
		panel1.Controls.Add(LC_Items);

		//foreach (var profile in ProfileManager.Profiles)
		//{
		//	if (!profile.Temporary)
		//	{
		//		AddProfile(profile);
		//	}
		//}

		ProfileManager.ProfileChanged += ProfileManager_ProfileChanged;

		RefreshCounts();

		DD_Sorting.SelectedItem = ProfileSorting.LastEdit;
	}

	private void Ctrl_CanDrawItem(object sender, CanDrawItemEventArgs<Profile> e)
	{
		var valid = true;

		if (T_ProfileUsage.SelectedValue != ThreeOptionToggle.Value.None)
		{
			valid &= (T_ProfileUsage.SelectedValue == ThreeOptionToggle.Value.Option1 && e.Item.ForGameplay) || (T_ProfileUsage.SelectedValue == ThreeOptionToggle.Value.Option2 && e.Item.ForAssetEditor);
		}

		if (!string.IsNullOrWhiteSpace(TB_Search.Text))
		{
			valid &= TB_Search.Text.SearchCheck(e.Item.Name);
		}

		e.DoNotDraw = !valid;
	}

	private void ProfileManager_ProfileChanged(Profile obj)
	{
		FLP_Profiles.Enabled = true;

		foreach (ProfilePreviewControl item in FLP_Profiles.Controls)
		{
			item.Loading = false;
		}
	}

	private ProfilePreviewControl AddProfile(Profile profile)
	{
		var ctrl = new ProfilePreviewControl(profile);

		FLP_Profiles.Controls.Add(ctrl);

		ctrl.LoadProfile += Ctrl_LoadProfile;
		ctrl.MergeProfile += Ctrl_MergeProfile;
		ctrl.ExcludeProfile += Ctrl_ExcludeProfile;
		ctrl.DisposeProfile += Ctrl_DisposeProfile;

		return ctrl;
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
			L_FilterCount.Text = string.Format(Locale.ShowingProfiles, filteredCount);
		}
	}

	protected override void LocaleChanged()
	{
		Text = Locale.YourProfiles;
	}

	protected override void UIChanged()
	{
		base.UIChanged();

		TB_Search.Margin = L_Counts.Margin = L_FilterCount.Margin = T_ProfileUsage.Margin = UI.Scale(new Padding(5), UI.FontScale);
		L_Counts.Font = L_FilterCount.Font = UI.Font(7.5F, FontStyle.Bold);
		DD_Sorting.Width = (int)(180 * UI.FontScale);
		TB_Search.Width = (int)(400 * UI.FontScale);
		T_ProfileUsage.Width = (int)(300 * UI.FontScale);
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
		TB_Search.Image = string.IsNullOrWhiteSpace(TB_Search.Text) ? Properties.Resources.I_Search : Properties.Resources.I_ClearSearch;

		LC_Items.FilterOrSortingChanged();
		RefreshCounts();
	}

	private void DD_Sorting_SelectedItemChanged(object sender, EventArgs e)
	{
		LC_Items.SetSorting(DD_Sorting.SelectedItem);
		var sorted = (DD_Sorting.SelectedItem switch
		{
			ProfileSorting.Name => ProfileManager.Profiles.OrderByDescending(x => x.IsFavorite).ThenBy(x => x.Name),
			ProfileSorting.DateCreated => ProfileManager.Profiles.OrderByDescending(x => x.IsFavorite).ThenByDescending(x => x.DateCreated),
			ProfileSorting.Usage => ProfileManager.Profiles.OrderByDescending(x => x.IsFavorite).ThenByDescending(x => x.ForGameplay).ThenByDescending(x => x.ForAssetEditor).ThenBy(x => x.LastEditDate),
			ProfileSorting.LastEdit or _ => ProfileManager.Profiles.OrderByDescending(x => x.IsFavorite).ThenByDescending(x => x.LastEditDate)
		}).ToList();

		var lastFavorited = sorted.LastOrDefault(x => x.IsFavorite);
		FLP_Profiles.SuspendDrawing();
		foreach (ProfilePreviewControl profile in FLP_Profiles.Controls)
		{
			FLP_Profiles.SetFlowBreak(profile, lastFavorited == profile.Profile);
		}
		FLP_Profiles.OrderBy(x => sorted.IndexOf((x as ProfilePreviewControl)!.Profile), false);
		FLP_Profiles.ResumeDrawing();
	}

	private void TB_Search_IconClicked(object sender, EventArgs e)
	{
		TB_Search.Text = string.Empty;
	}
}
