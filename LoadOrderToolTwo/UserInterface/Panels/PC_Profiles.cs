using Extensions;

using LoadOrderToolTwo.Domain;
using LoadOrderToolTwo.UserInterface.Generic;
using LoadOrderToolTwo.Utilities;
using LoadOrderToolTwo.Utilities.Managers;

using SlickControls;

using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace LoadOrderToolTwo.UserInterface.Panels;
public partial class PC_Profiles : PanelContent
{
	private bool loadingProfile;
	public PC_Profiles()
	{
		InitializeComponent();

		LoadProfile(CentralManager.CurrentProfile);

		TB_SavePath.StartingFolder = Path.Combine(LocationManager.AppDataPath, "Saves");
		TB_SkipFile.StartingFolder = LocationManager.AppDataPath;

		CB_UseCitiesExe.Visible = CentralManager.SessionSettings.AdvancedLaunchOptions;
		CB_UnityProfiler.Visible = CentralManager.SessionSettings.AdvancedLaunchOptions;
		CB_DebugMono.Visible = CentralManager.SessionSettings.AdvancedLaunchOptions;

		ProfileManager.ProfileChanged += ProfileManager_ProfileChanged;
	}

	private void ProfileManager_ProfileChanged(Profile p)
	{
		this.TryInvoke(() => LoadProfile(p));
	}

	protected override void LocaleChanged()
	{
		Text = Locale.ProfileBubble;
		L_TempProfile.Text = Locale.TemporaryProfileCanNotBeEdited;
		TLP_LaunchSettings.Text = Locale.LaunchSettings;
		TLP_LSM.Text = Locale.LoadingScreenMod;
		TLP_GeneralSettings.Text = Locale.Settings;
		L_ProfileUsage.Text = Locale.ProfileUsage;
		L_Info.Text = Locale.ProfileSaveInfo;
	}

	protected override void OnCreateControl()
	{
		base.OnCreateControl();

		TB_SavePath.Image = TB_SkipFile.Image = Properties.Resources.I_FolderSearch;
	}

	protected override void UIChanged()
	{
		base.UIChanged();

		L_ProfileUsage.Font = UI.Font(7.5F, FontStyle.Bold);
		P_Options.Padding = P_Options.Margin = UI.Scale(new Padding(5), UI.UIScale);
		L_TempProfile.Font = UI.Font(10.5F);
		L_CurrentProfile.Font = UI.Font(12.75F, FontStyle.Bold);
		B_ViewProfiles.Font = B_NewProfile.Font = B_TempProfile.Font = B_Cancel.Font = UI.Font(9.75F);
		TLP_GeneralSettings.Margin = TLP_LaunchSettings.Margin = TLP_LSM.Margin = UI.Scale(new Padding(10), UI.UIScale);
		T_ProfileUsage.Width = (int)(300 * UI.FontScale);
	}

	protected override void DesignChanged(FormDesign design)
	{
		base.DesignChanged(design);

		L_ProfileUsage.ForeColor = design.LabelColor;
		TLP_LaunchSettings.BackColor = TLP_GeneralSettings.BackColor = TLP_LSM.BackColor = design.BackColor.Tint(Lum: design.Type.If(FormDesignType.Dark, 1, -1));
		TLP_ProfileName.BackColor = design.ButtonColor;
		L_TempProfile.ForeColor = design.YellowColor;
		P_Options.BackColor = design.AccentBackColor;
	}

	public override bool CanExit(bool toBeDisposed)
	{
		return !TB_Name.Visible && !I_ProfileIcon.Loading;
	}

	internal void Ctrl_LoadProfile(Profile obj)
	{
		I_ProfileIcon.Loading = true;
		L_CurrentProfile.Text = obj.Name;
		FLP_Options.Enabled = B_EditName.Visible = B_Save.Visible = false;
		ProfileManager.SetProfile(obj);
	}

	private void LoadProfile(Profile profile)
	{
		loadingProfile = true;

		I_ProfileIcon.Image = profile.GetIcon();
		L_TempProfile.Visible = I_TempProfile.Visible = profile.Temporary;
		B_TempProfile.Visible = !profile.Temporary;
		L_Info.Visible = I_Info.Visible = !profile.Temporary;

		TLP_GeneralSettings.Visible = !profile.Temporary;

		B_EditName.Visible = B_Save.Visible = !profile.Temporary && !TB_Name.Visible;

		I_ProfileIcon.Loading = false;
		L_CurrentProfile.Text = profile.Name;
		CB_AutoSave.Checked = profile.AutoSave;

		CB_NoWorkshop.Checked = profile.LaunchSettings.NoWorkshop;
		CB_NoAssets.Checked = profile.LaunchSettings.NoAssets;
		CB_NoMods.Checked = profile.LaunchSettings.NoMods;
		CB_LHT.Checked = profile.LaunchSettings.LHT;
		CB_UseCitiesExe.Checked = profile.LaunchSettings.UseCitiesExe;
		CB_UnityProfiler.Checked = profile.LaunchSettings.UnityProfiler;
		CB_DebugMono.Checked = profile.LaunchSettings.DebugMono;
		CB_LoadSave.Checked = profile.LaunchSettings.LoadSaveGame;
		TB_SavePath.Text = profile.LaunchSettings.SaveToLoad;

		CB_LoadUsed.Checked = profile.LsmSettings.LoadUsed;
		CB_LoadEnabled.Checked = profile.LsmSettings.LoadEnabled;
		CB_SkipFile.Checked = profile.LsmSettings.UseSkipFile;
		TB_SkipFile.Text = profile.LsmSettings.SkipFile;

		if (profile.ForAssetEditor)
		{
			T_ProfileUsage.SelectedValue = ThreeOptionToggle.Value.Option2;
		}
		else if (profile.ForGameplay)
		{
			T_ProfileUsage.SelectedValue = ThreeOptionToggle.Value.Option1;
		}
		else
		{
			T_ProfileUsage.SelectedValue = ThreeOptionToggle.Value.None;
		}

		loadingProfile = false;
	}

	private void ValueChanged(object sender, EventArgs e)
	{
		if (loadingProfile)
		{
			return;
		}

		CentralManager.CurrentProfile.AutoSave = CB_AutoSave.Checked;
		CentralManager.CurrentProfile.ForGameplay = T_ProfileUsage.SelectedValue == ThreeOptionToggle.Value.Option1;
		CentralManager.CurrentProfile.ForAssetEditor = T_ProfileUsage.SelectedValue == ThreeOptionToggle.Value.Option2;

		CentralManager.CurrentProfile.LaunchSettings.NoWorkshop = CB_NoWorkshop.Checked;
		CentralManager.CurrentProfile.LaunchSettings.NoAssets = CB_NoAssets.Checked;
		CentralManager.CurrentProfile.LaunchSettings.NoMods = CB_NoMods.Checked;
		CentralManager.CurrentProfile.LaunchSettings.LHT = CB_LHT.Checked;
		CentralManager.CurrentProfile.LaunchSettings.SaveToLoad = TB_SavePath.Text;
		CentralManager.CurrentProfile.LaunchSettings.LoadSaveGame = CB_LoadSave.Checked;
		CentralManager.CurrentProfile.LaunchSettings.UseCitiesExe = CB_UseCitiesExe.Checked;
		CentralManager.CurrentProfile.LaunchSettings.UnityProfiler = CB_UnityProfiler.Checked;
		CentralManager.CurrentProfile.LaunchSettings.DebugMono = CB_DebugMono.Checked;

		CentralManager.CurrentProfile.LsmSettings.SkipFile = TB_SkipFile.Text;
		CentralManager.CurrentProfile.LsmSettings.LoadEnabled = CB_LoadEnabled.Checked;
		CentralManager.CurrentProfile.LsmSettings.LoadUsed = CB_LoadUsed.Checked;
		CentralManager.CurrentProfile.LsmSettings.UseSkipFile = CB_SkipFile.Checked;

		ProfileManager.Save(CentralManager.CurrentProfile);
	}

	private void B_LoadProfiles_Click(object sender, EventArgs e)
	{
		if (!I_ProfileIcon.Loading)
		{
			Form.PushPanel<PC_ProfileList>(null);
		}
	}

	private void B_NewProfile_Click(object sender, EventArgs e)
	{
		TLP_New.Visible = true;
		TLP_Main.Visible = false;
	}

	private void B_EditName_Click(object sender, EventArgs e)
	{
		TB_Name.Visible = true;
		B_EditName.Visible = B_Save.Visible = false;
		L_CurrentProfile.Visible = false;
		TB_Name.Text = L_CurrentProfile.Text;

		BeginInvoke(new Action(() =>
		{
			TB_Name.Focus();
			TB_Name.SelectAll();
		}));
	}

	private void NewProfile_Click(object sender, EventArgs e)
	{
		var newProfile = new Profile() { Name = ProfileManager.GetNewProfileName(), LastEditDate = DateTime.Now };

		if (!ProfileManager.Save(newProfile))
		{
			ShowPrompt(Locale.ProfileCreationFailed, icon: PromptIcons.Error);
			return;
		}

		ProfileManager.AddProfile(newProfile);

		ProfileManager.SetProfile(newProfile);

		L_CurrentProfile.Text = newProfile.Name;
		I_ProfileIcon.Loading = true;
		FLP_Options.Enabled = false;
		TLP_Main.Visible = true;
		TLP_New.Visible = false;
		B_EditName_Click(sender, e);
	}

	private void CopyProfile_Click(object sender, EventArgs e)
	{
		var newProfile = CentralManager.CurrentProfile.Clone();
		newProfile.Name = ProfileManager.GetNewProfileName();
		newProfile.LastEditDate = DateTime.Now;

		if (!newProfile.Save())
		{
			ShowPrompt(Locale.CouldNotCreateProfile, icon: PromptIcons.Error);
			return;
		}

		ProfileManager.AddProfile(newProfile);

		ProfileManager.SetProfile(newProfile);

		L_CurrentProfile.Text = newProfile.Name;
		I_ProfileIcon.Loading = true;
		FLP_Options.Enabled = false;
		TLP_Main.Visible = true;
		TLP_New.Visible = false;
		B_EditName_Click(sender, e);
	}

	private void B_Cancel_Click(object sender, EventArgs e)
	{
		TLP_Main.Visible = true;
		TLP_New.Visible = false;
	}

	private void TB_Name_KeyDown(object sender, KeyEventArgs e)
	{
		if (e.KeyCode is Keys.Enter or Keys.Escape)
		{
			e.SuppressKeyPress = true;
			e.Handled = true;
		}
	}

	private void TB_Name_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
	{
		if (e.KeyCode == Keys.Enter)
		{
			e.IsInputKey = true;

			TB_Name.Visible = false;
		}

		if (e.KeyCode == Keys.Escape)
		{
			e.IsInputKey = true;

			TB_Name.Text = string.Empty;
			TB_Name.Visible = false;
		}
	}

	private void TB_Name_IconClicked(object sender, EventArgs e)
	{
		TB_Name.Visible = false;
	}

	private void TB_Name_Leave(object sender, EventArgs e)
	{
		if (!TB_Name.Visible)
		{
			return;
		}

		if (string.IsNullOrWhiteSpace(TB_Name.Text))
		{
			TB_Name.Visible = false;
			B_EditName.Visible = B_Save.Visible = true;
			L_CurrentProfile.Visible = true;
			return;
		}

		if (!ProfileManager.RenameProfile(ProfileManager.CurrentProfile, TB_Name.Text))
		{
			TB_Name.SetError();
			return;
		}

		if (ProfileManager.CurrentProfile.Name != TB_Name.Text)
		{
			Notification.Create(Locale.ProfileNameChangedIllegalChars, null, PromptIcons.Info, null)
				.Show(Form, 15);
		}

		L_CurrentProfile.Text = ProfileManager.CurrentProfile.Name;
		TB_Name.Visible = false;
		B_EditName.Visible = B_Save.Visible = true;
		L_CurrentProfile.Visible = true;
	}

	private void T_ProfileUsage_SelectedValueChanged(object sender, EventArgs e)
	{
		if (loadingProfile)
		{
			return;
		}

		var invalidPackages = ProfileManager.GetInvalidPackages(T_ProfileUsage.SelectedValue == ThreeOptionToggle.Value.Option1, T_ProfileUsage.SelectedValue == ThreeOptionToggle.Value.Option2);

		if (invalidPackages.Any())
		{
			if (ShowPrompt($"{Locale.SomePackagesWillBeDisabled}\r\n{Locale.AffectedPackagesAre}\r\n• {invalidPackages.ListStrings("\r\n• ")}", PromptButtons.OKCancel, PromptIcons.Warning) == DialogResult.Cancel)
			{
				T_ProfileUsage.SelectedValue = ThreeOptionToggle.Value.None;

				return;
			}
		}

		ValueChanged(sender, e);

		I_ProfileIcon.Image = CentralManager.CurrentProfile.GetIcon();
	}

	private void LsmSettingsChanged(object sender, EventArgs e)
	{
		if (loadingProfile)
		{
			return;
		}

		ValueChanged(sender, e);

		ProfileManager.SaveLsmSettings(ProfileManager.CurrentProfile);
	}

	private void B_Save_Click(object sender, EventArgs e)
	{
		if (ProfileManager.CurrentProfile.Save())
		{
			B_Save.Image = Properties.Resources.I_Check;

			new BackgroundAction(() =>
			{
				B_Save.Image = Properties.Resources.I_Save;
			}).RunIn(2000);
		}
		else
		{
			ShowPrompt(Locale.CouldNotCreateProfile, icon: PromptIcons.Error);
		}
	}

	private void B_TempProfile_Click(object sender, EventArgs e)
	{
		ProfileManager.SetProfile(Profile.TemporaryProfile);
	}
}
