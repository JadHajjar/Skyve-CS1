using Extensions;

using SkyveApp.Domain;
using SkyveApp.Domain.Interfaces;
using SkyveApp.UserInterface.Lists;
using SkyveApp.Utilities;
using SkyveApp.Utilities.Managers;

using SlickControls;

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SkyveApp.UserInterface.Panels;
public partial class PC_ProfileList : PanelContent
{
	private readonly ProfileListControl LC_Items;

	public PC_ProfileList() : this(null) { }

	public PC_ProfileList(IEnumerable<IProfile>? profiles)
	{
		InitializeComponent();

		DD_Sorting.Height = TB_Search.Height = 0;

		LC_Items = new ProfileListControl(false) { Dock = DockStyle.Fill, GridView = true };
		LC_Items.CanDrawItem += Ctrl_CanDrawItem;
		panel1.Controls.Add(LC_Items);

		if (profiles is null)
		{
			LC_Items.LoadProfile += Ctrl_LoadProfile;
			LC_Items.MergeProfile += Ctrl_MergeProfile;
			LC_Items.ExcludeProfile += Ctrl_ExcludeProfile;
			LC_Items.DisposeProfile += Ctrl_DisposeProfile;
			LC_Items.Loading = !ProfileManager.ProfilesLoaded;

			if (!LC_Items.Loading)
			{
				LC_Items.SetItems(ProfileManager.Profiles.Skip(1));
			}

			ProfileManager.ProfileChanged += LoadProfile;
		}
		else
		{
			L_Counts.Visible = TLP_ProfileName.Visible = B_AddProfile.Visible = B_TempProfile.Visible = B_Discover.Visible = false;

			DD_Sorting.Parent = null;
			TLP_Main.SetColumn(DD_Usage, 2);
			TLP_Main.SetColumnSpan(TB_Search, 2);

			LC_Items.ReadOnly = true;
			LC_Items.SetItems(profiles);
			LC_Items.SetSorting(Domain.Enums.ProfileSorting.Downloads);
		}

		SlickTip.SetTo(B_AddProfile.Controls[0], "NewProfile_Tip");
		SlickTip.SetTo(B_TempProfile.Controls[0], "TempProfile_Tip");
		SlickTip.SetTo(B_GridView, "Switch to Grid-View");
		SlickTip.SetTo(B_ListView, "Switch to List-View");

		RefreshCounts();
	}

	private void Ctrl_CanDrawItem(object sender, CanDrawItemEventArgs<IProfile> e)
	{
		var valid = true;

		if (e.Item.Usage > 0 && DD_Usage.SelectedItems.Count() > 0)
		{
			valid &= DD_Usage.SelectedItems.Contains(e.Item.Usage);
		}

		if (!string.IsNullOrWhiteSpace(TB_Search.Text))
		{
			var author = SteamUtil.GetUser(e.Item.Author);

			valid &= TB_Search.Text.SearchCheck(e.Item.Name) || (author is not null && TB_Search.Text.SearchCheck(author.Name));
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
		if (!I_ProfileIcon.Loading)
		{
			I_ProfileIcon.Loading = true;
			L_CurrentProfile.Text = obj.Name;
			I_Favorite.Visible = B_Save.Visible = false;
			ProfileManager.SetProfile(obj);
		}
	}

	protected override void OnCreateControl()
	{
		base.OnCreateControl();

		if (!LC_Items.ReadOnly)
		{
			LoadProfile(ProfileManager.CurrentProfile);
		}
	}

	private void RefreshCounts()
	{
		if (L_Counts.Visible)
		{
			var favorites = ProfileManager.Profiles.Count(x => x.IsFavorite);
			var total = ProfileManager.Profiles.Count(x => !x.Temporary);
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
		}

		var filteredCount = LC_Items.FilteredItems.Count();

		L_FilterCount.Text = Locale.ShowingCount.FormatPlural(filteredCount, Locale.Profile.FormatPlural(filteredCount));
	}

	protected override void LocaleChanged()
	{
		Text = LC_Items.ReadOnly ? Locale.DiscoverProfiles : Locale.YourProfiles;
	}

	protected override void UIChanged()
	{
		base.UIChanged();

		slickIcon1.Size = slickIcon2.Size = B_EditName.Size = B_Save.Size = I_ProfileIcon.Size = I_Favorite.Size = UI.Scale(new Size(24, 24), UI.FontScale) + new Size(8, 8);
		L_CurrentProfile.Font = UI.Font(12.75F, FontStyle.Bold);
		roundedPanel.Margin = TB_Search.Margin = L_Counts.Margin = L_FilterCount.Margin = DD_Sorting.Margin = DD_Usage.Margin = UI.Scale(new Padding(7), UI.FontScale);
		B_TempProfile.Padding = B_AddProfile.Padding = TLP_ProfileName.Padding = B_ListView.Padding = B_GridView.Padding = UI.Scale(new Padding(5), UI.FontScale);
		B_ListView.Size = B_GridView.Size = UI.Scale(new Size(24, 24), UI.FontScale);
		L_Counts.Font = L_FilterCount.Font = UI.Font(7.5F, FontStyle.Bold);
		B_Discover.Font = UI.Font(9.75F, FontStyle.Bold);
		DD_Usage.Width = DD_Sorting.Width = (int)(180 * UI.FontScale);
		TB_Search.Width = (int)(250 * UI.FontScale);
		roundedPanel.Padding = new Padding((int)(2.5 * UI.FontScale) + 1, (int)(5 * UI.FontScale), (int)(2.5 * UI.FontScale), (int)(5 * UI.FontScale));

		var size = (int)(30 * UI.FontScale) - 6;
		TB_Search.MaximumSize = new Size(9999, size);
		TB_Search.MinimumSize = new Size(0, size);
	}

	protected override void DesignChanged(FormDesign design)
	{
		base.DesignChanged(design);

		TLP_ProfileName.Invalidate();
		B_TempProfile.BackColor = B_AddProfile.BackColor = FormDesign.Design.ButtonColor;
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

	private void LoadProfile(Profile profile)
	{
		this.TryInvoke(() =>
	{
		TLP_ProfileName.BackColor = profile.Color ?? FormDesign.Design.ButtonColor;
		TLP_ProfileName.ForeColor = TLP_ProfileName.BackColor.GetTextColor();
		I_ProfileIcon.ImageName = profile.GetIcon();
		I_Favorite.ImageName = profile.IsFavorite ? "I_StarFilled" : "I_Star";
		B_TempProfile.Visible = !profile.Temporary;
		I_ProfileIcon.Enabled = !profile.Temporary;

		tableLayoutPanel1.SetColumn(B_TempProfile, profile.Temporary ? 2 : 1);
		tableLayoutPanel1.SetColumn(B_AddProfile, profile.Temporary ? 1 : 2);

		SlickTip.SetTo(I_Favorite, profile.IsFavorite ? "UnFavoriteThisProfile" : "FavoriteThisProfile");

		I_Favorite.Visible = B_Save.Visible = !profile.Temporary;

		I_ProfileIcon.Loading = false;
		L_CurrentProfile.Text = profile.Name;

		LC_Items.Invalidate();
	});
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

	private void B_ListView_Click(object sender, EventArgs e)
	{
		B_GridView.Selected = false;
		B_ListView.Selected = true;
		LC_Items.GridView = false;
	}

	private void B_GridView_Click(object sender, EventArgs e)
	{
		B_GridView.Selected = true;
		B_ListView.Selected = false;
		LC_Items.GridView = true;
	}

	private void B_AddProfile_Click(object sender, EventArgs e)
	{
		Form.PushPanel<PC_ProfileAdd>();
	}

	private void B_TempProfile_Click(object sender, EventArgs e)
	{
		ProfileManager.SetProfile(Profile.TemporaryProfile);
	}

	private async void B_Save_Click(object sender, EventArgs e)
	{
		if (ProfileManager.CurrentProfile.Save())
		{
			B_Save.ImageName = "I_Check";

			await Task.Delay(1500);

			B_Save.ImageName = "I_Save";
		}
		else
		{
			ShowPrompt(Locale.CouldNotCreateProfile, icon: PromptIcons.Error);
		}
	}

	private void B_EditName_Click(object sender, EventArgs e)
	{
		Form.PushPanel<PC_Profile>();
	}

	private void I_ProfileIcon_Click(object sender, EventArgs e)
	{
		if (ProfileManager.CurrentProfile.Temporary)
		{
			return;
		}

		var colorDialog = new SlickColorPicker(ProfileManager.CurrentProfile.Color ?? Color.Red);

		if (colorDialog.ShowDialog() != DialogResult.OK)
		{
			return;
		}

		TLP_ProfileName.BackColor = colorDialog.Color;
		TLP_ProfileName.ForeColor = TLP_ProfileName.BackColor.GetTextColor();
		ProfileManager.CurrentProfile.Color = colorDialog.Color;
		ProfileManager.Save(ProfileManager.CurrentProfile);
	}

	private void I_Favorite_Click(object sender, EventArgs e)
	{
		if (ProfileManager.CurrentProfile.Temporary)
		{
			return;
		}

		ProfileManager.CurrentProfile.IsFavorite = !ProfileManager.CurrentProfile.IsFavorite;
		ProfileManager.Save(ProfileManager.CurrentProfile);

		I_Favorite.ImageName = ProfileManager.CurrentProfile.IsFavorite ? "I_StarFilled" : "I_Star";
		SlickTip.SetTo(I_Favorite, ProfileManager.CurrentProfile.IsFavorite ? "UnFavoriteThisProfile" : "FavoriteThisProfile");
	}

	private async void B_Discover_Click(object sender, EventArgs e)
	{
		try
		{
			B_Discover.Loading = true;

			var profiles = await SkyveApiUtil.GetPublicProfiles();

			Invoke(() => Form.PushPanel(new PC_ProfileList(profiles)));
		}
		catch (Exception ex)
		{
			ShowPrompt(ex, Locale.FailedToRetrieveProfiles);
		}

		B_Discover.Loading = false;
	}
}
