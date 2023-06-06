using Extensions;

using SkyveApp.Domain;
using SkyveApp.Domain.Compatibility.Enums;
using SkyveApp.UserInterface.Generic;
using SkyveApp.Utilities;
using SkyveApp.Utilities.IO;
using SkyveApp.Utilities.Managers;

using SlickControls;

using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace SkyveApp.UserInterface.Panels;
public partial class PC_Profiles : PanelContent
{
	private bool buttonStateRunning;
	private bool loadingProfile;
	private readonly SlickCheckbox[] _launchOptions;
	public PC_Profiles()
	{
		InitializeComponent();

		B_StartStop.Enabled = CitiesManager.CitiesAvailable();

		SlickTip.SetTo(B_StartStop, string.Format(Locale.LaunchTooltip, "[F5]"));

		_launchOptions = new[] { CB_StartNewGame, CB_LoadSave, CB_NewAsset, CB_LoadAsset };

		SlickTip.SetTo(B_NewProfile, "NewProfile_Tip");
		SlickTip.SetTo(B_TempProfile, "TempProfile_Tip");
		SlickTip.SetTo(B_ViewProfiles, "ViewProfiles_Tip");

		foreach (var item in this.GetControls<SlickCheckbox>())
		{
			if (item != CB_LHT && item != CB_NoWorkshop && item.Parent != TLP_AdvancedDev)
			{
				SlickTip.SetTo(item, item.Text + "_Tip");
			}
		}

		LoadProfile(CentralManager.CurrentProfile);

		DD_SaveFile.StartingFolder = LocationManager.Combine(LocationManager.AppDataPath, "Saves");
		DD_SaveFile.PinnedFolders = new()
		{
			["Your Save-games"] = LocationManager.Combine(LocationManager.AppDataPath, "Saves"),
			["Workshop Save-games"] = IOSelectionDialog.CustomDirectory,
		};
		DD_SaveFile.CustomFiles = CentralManager.Assets.Where(x => x.Workshop && (x.Package.WorkshopTags?.Contains("SaveGame") ?? false)).Select(x => new IOSelectionDialog.CustomFile
		{
			Name = x.Package.Name,
			Icon = x.IconImage,
			Path = x.FileName
		}).ToList();

		DD_SkipFile.StartingFolder = LocationManager.AppDataPath;
		DD_SkipFile.PinnedFolders = new() { ["App Data"] = LocationManager.AppDataPath };

		DD_NewMap.StartingFolder = LocationManager.MapsPath;
		DD_NewMap.PinnedFolders = new()
		{
			["Custom Maps"] = LocationManager.MapsPath,
			["Vanilla Maps"] = LocationManager.Combine(LocationManager.GameContentPath, "Maps"),
			["Workshop Maps"] = IOSelectionDialog.CustomDirectory,
		};
		DD_NewMap.CustomFiles = CentralManager.Assets.Where(x => x.Workshop && (x.AssetTags.Contains("Map") || (x.Package.WorkshopTags?.Contains("Map") ?? false))).Select(x => new IOSelectionDialog.CustomFile
		{
			Name = x.Package.Name,
			Icon = x.IconImage,
			Path = x.FileName
		}).ToList();

		TLP_AdvancedDev.Visible = CentralManager.SessionSettings.UserSettings.AdvancedLaunchOptions;

		DAD_NewProfile.StartingFolder = LocationManager.AppDataPath;

		ProfileManager.ProfileChanged += ProfileManager_ProfileChanged;

		CitiesManager.MonitorTick += CitiesManager_MonitorTick;

		RefreshButtonState(CitiesManager.IsRunning(), true);
	}

	private void ProfileManager_ProfileChanged(Profile p)
	{
		this.TryInvoke(() => LoadProfile(p));
	}

	protected override void LocaleChanged()
	{
		Text = Locale.ProfileBubble;
		L_TempProfile.Text = Locale.TemporaryProfileCanNotBeEdited;
		DD_ProfileUsage.Text = Locale.ProfileUsage;
		L_Info.Text = Locale.ProfileSaveInfo;
	}

	protected override void UIChanged()
	{
		base.UIChanged();

		B_EditName.Size = B_Save.Size = I_ProfileIcon.Size = I_Info.Size = I_TempProfile.Size = I_Favorite.Size = UI.Scale(new Size(24, 24), UI.FontScale) + new Size(8, 8);
		slickSpacer1.Height = (int)(1.5 * UI.FontScale);
		slickSpacer1.Margin = UI.Scale(new Padding(5), UI.UIScale);
		B_Discover.Margin =	B_StartStop.Margin = P_Options.Padding = P_Options.Margin = UI.Scale(new Padding(5), UI.UIScale);
		L_TempProfile.Font = UI.Font(10.5F);
		B_Discover.Font = B_StartStop.Font = UI.Font(9.75F, FontStyle.Bold);
		L_CurrentProfile.Font = UI.Font(12.75F, FontStyle.Bold);
		B_ViewProfiles.Font = B_NewProfile.Font = B_TempProfile.Font = B_Cancel.Font = UI.Font(9.75F);
		TLP_AdvancedDev.Margin = TLP_GeneralSettings.Margin = TLP_LaunchSettings.Margin = TLP_LSM.Margin = DAD_NewProfile.Margin = UI.Scale(new Padding(10), UI.UIScale);
	}

	protected override void DesignChanged(FormDesign design)
	{
		base.DesignChanged(design);

		TLP_LaunchSettings.BackColor = TLP_AdvancedDev.BackColor = TLP_GeneralSettings.BackColor = TLP_LSM.BackColor = design.BackColor.Tint(Lum: design.Type.If(FormDesignType.Dark, 1, -1));
		L_TempProfile.ForeColor = design.YellowColor;
		P_Options.BackColor = design.AccentBackColor;
	}

	public override bool CanExit(bool toBeDisposed)
	{
		if (I_ProfileIcon.Loading)
		{
			if (toBeDisposed)
			{
				Notification.Create(Locale.ProfileStillLoading, null, PromptIcons.Hand, null).Show(Form, 10);
			}

			return false;
		}

		if (TB_Name.Visible)
		{
			if (toBeDisposed)
			{
				Notification.Create(Locale.ApplyProfileNameBeforeExit, null, PromptIcons.Hand, null).Show(Form, 10);
			}

			return false;
		}

		return true;
	}

	internal void Ctrl_LoadProfile(Profile obj)
	{
		I_ProfileIcon.Loading = true;
		L_CurrentProfile.Text = obj.Name;
		TLP_Options.Enabled = B_EditName.Visible = B_Save.Visible = false;
		ProfileManager.SetProfile(obj);
	}

	private void LoadProfile(Profile profile)
	{
		loadingProfile = true;

		TLP_ProfileName.BackColor = profile.Color ?? FormDesign.Design.ButtonColor;
		TLP_ProfileName.ForeColor = TLP_ProfileName.BackColor.GetTextColor();
		I_ProfileIcon.ImageName = profile.GetIcon();
		I_Favorite.ImageName = profile.IsFavorite ? "I_StarFilled" : "I_Star";
		L_TempProfile.Visible = I_TempProfile.Visible = profile.Temporary;
		B_TempProfile.Visible = !profile.Temporary;
		I_Favorite.Visible = I_ProfileIcon.Enabled = L_Info.Visible = I_Info.Visible = !profile.Temporary;
		TLP_Options.Enabled = true;
		TLP_GeneralSettings.Visible = !profile.Temporary;

		TLP_Main.SetColumn(B_TempProfile, profile.Temporary ? 2 : 3);
		TLP_Main.SetColumn(B_NewProfile, profile.Temporary ? 3 : 2);

		TLP_Options.SetRow(TLP_GeneralSettings, profile.Temporary ? 2 : 0);
		TLP_Options.SetRow(TLP_LSM, profile.Temporary ? 0 : 1);

		B_EditName.Visible = B_Save.Visible = !profile.Temporary && !TB_Name.Visible;

		I_ProfileIcon.Loading = false;
		L_CurrentProfile.Text = profile.Name;
		CB_AutoSave.Checked = profile.AutoSave;
		DD_ProfileUsage.SelectedItem = profile.Usage > 0 ? profile.Usage : (PackageUsage)(-1);

		CB_NoWorkshop.Checked = profile.LaunchSettings.NoWorkshop;
		CB_NoAssets.Checked = profile.LaunchSettings.NoAssets;
		CB_NoMods.Checked = profile.LaunchSettings.NoMods;
		CB_LHT.Checked = profile.LaunchSettings.LHT;
		CB_UseCitiesExe.Checked = profile.LaunchSettings.UseCitiesExe;
		CB_UnityProfiler.Checked = profile.LaunchSettings.UnityProfiler;
		CB_DebugMono.Checked = profile.LaunchSettings.DebugMono;
		CB_LoadSave.Checked = profile.LaunchSettings.LoadSaveGame;
		CB_StartNewGame.Checked = profile.LaunchSettings.StartNewGame;
		CB_DevUI.Checked = profile.LaunchSettings.DevUi;
		CB_RefreshWorkshop.Checked = profile.LaunchSettings.RefreshWorkshop;
		DD_NewMap.SelectedFile = profile.LaunchSettings.MapToLoad;
		DD_SaveFile.SelectedFile = profile.LaunchSettings.SaveToLoad;
		TB_CustomArgs.Text = profile.LaunchSettings.CustomArgs;

		CB_LoadUsed.Checked = profile.LsmSettings.LoadUsed;
		CB_LoadEnabled.Checked = profile.LsmSettings.LoadEnabled;
		CB_SkipFile.Checked = profile.LsmSettings.UseSkipFile;
		DD_SkipFile.SelectedFile = profile.LsmSettings.SkipFile;

		DD_SaveFile.Enabled = CB_LoadSave.Checked;
		DD_SkipFile.Enabled = CB_SkipFile.Checked;
		DD_NewMap.Enabled = CB_StartNewGame.Checked;

		loadingProfile = false;
	}

	private void ValueChanged(object sender, EventArgs e)
	{
		DD_SaveFile.Enabled = CB_LoadSave.Checked;
		DD_SkipFile.Enabled = CB_SkipFile.Checked;
		DD_NewMap.Enabled = CB_StartNewGame.Checked;

		if (loadingProfile)
		{
			return;
		}

		if (_launchOptions.Contains(sender) && (sender as SlickCheckbox)!.Checked)
		{
			foreach (var item in _launchOptions)
			{
				if (item == sender)
				{
					continue;
				}

				item.Checked = false;
			}
		}

		CentralManager.CurrentProfile.AutoSave = CB_AutoSave.Checked;
		CentralManager.CurrentProfile.Usage = DD_ProfileUsage.SelectedItem;

		CentralManager.CurrentProfile.LaunchSettings.NoWorkshop = CB_NoWorkshop.Checked;
		CentralManager.CurrentProfile.LaunchSettings.NoAssets = CB_NoAssets.Checked;
		CentralManager.CurrentProfile.LaunchSettings.NoMods = CB_NoMods.Checked;
		CentralManager.CurrentProfile.LaunchSettings.LHT = CB_LHT.Checked;
		CentralManager.CurrentProfile.LaunchSettings.StartNewGame = CB_StartNewGame.Checked;
		CentralManager.CurrentProfile.LaunchSettings.MapToLoad = IOUtil.ToRealPath(DD_NewMap.SelectedFile);
		CentralManager.CurrentProfile.LaunchSettings.SaveToLoad = IOUtil.ToRealPath(DD_SaveFile.SelectedFile);
		CentralManager.CurrentProfile.LaunchSettings.LoadSaveGame = CB_LoadSave.Checked;
		CentralManager.CurrentProfile.LaunchSettings.UseCitiesExe = CB_UseCitiesExe.Checked;
		CentralManager.CurrentProfile.LaunchSettings.UnityProfiler = CB_UnityProfiler.Checked;
		CentralManager.CurrentProfile.LaunchSettings.DebugMono = CB_DebugMono.Checked;
		CentralManager.CurrentProfile.LaunchSettings.RefreshWorkshop = CB_RefreshWorkshop.Checked;
		CentralManager.CurrentProfile.LaunchSettings.DevUi = CB_DevUI.Checked;
		CentralManager.CurrentProfile.LaunchSettings.CustomArgs = TB_CustomArgs.Text;
		CentralManager.CurrentProfile.LaunchSettings.NewAsset = CB_NewAsset.Checked;
		CentralManager.CurrentProfile.LaunchSettings.LoadAsset = CB_LoadAsset.Checked;

		CentralManager.CurrentProfile.LsmSettings.SkipFile = IOUtil.ToRealPath(DD_SkipFile.SelectedFile);
		CentralManager.CurrentProfile.LsmSettings.LoadEnabled = CB_LoadEnabled.Checked;
		CentralManager.CurrentProfile.LsmSettings.LoadUsed = CB_LoadUsed.Checked;
		CentralManager.CurrentProfile.LsmSettings.UseSkipFile = CB_SkipFile.Checked;

		ProfileManager.Save(CentralManager.CurrentProfile);
	}

	private void B_LoadProfiles_Click(object sender, EventArgs e)
	{
		if (!I_ProfileIcon.Loading)
		{
			Form.PushPanel(new PC_ProfileList());
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

		this.TryBeginInvoke(() =>
		{
			TB_Name.Focus();
			TB_Name.SelectAll();
		});
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
		TLP_Options.Enabled = false;
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
		TLP_Options.Enabled = false;
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

		var invalidPackages = ProfileManager.GetInvalidPackages(DD_ProfileUsage.SelectedItem);

		if (invalidPackages.Any())
		{
			if (ShowPrompt($"{Locale.SomePackagesWillBeDisabled}\r\n{Locale.AffectedPackagesAre}\r\n• {invalidPackages.ListStrings("\r\n• ")}", PromptButtons.OKCancel, PromptIcons.Warning) == DialogResult.Cancel)
			{
				DD_ProfileUsage.SelectedItem = (PackageUsage)(-1);

				return;
			}

			ContentUtil.SetBulkIncluded(invalidPackages, false);
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
			B_Save.ImageName = "I_Check";

			new BackgroundAction(() =>
			{
				B_Save.ImageName = "I_Save";
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

	private void DD_SaveFile_FileSelected(string obj)
	{
		DD_SaveFile.SelectedFile = obj;
		ValueChanged(DD_SaveFile, EventArgs.Empty);
	}

	private void DD_SkipFile_FileSelected(string obj)
	{
		DD_SkipFile.SelectedFile = obj;
		LsmSettingsChanged(DD_SkipFile, EventArgs.Empty);
	}

	private void DD_NewMap_FileSelected(string obj)
	{
		DD_NewMap.SelectedFile = obj;
		ValueChanged(DD_NewMap, EventArgs.Empty);
	}

	private bool DD_ValidFile(object sender, string arg)
	{
		return (sender as DragAndDropControl)!.ValidExtensions.Any(x => x.Equals(Path.GetExtension(arg), StringComparison.CurrentCultureIgnoreCase));
	}

	private void DAD_NewProfile_FileSelected(string obj)
	{
		TLP_Main.Visible = true;
		TLP_New.Visible = false;

		var profile = ProfileManager.Profiles.FirstOrDefault(x => x.Name!.Equals(Path.GetFileNameWithoutExtension(obj), StringComparison.InvariantCultureIgnoreCase));

		if (obj.EndsWith(".xml", StringComparison.OrdinalIgnoreCase))
		{
			if (profile is not null)
			{
				ShowPrompt(Locale.ProfileNameUsed, icon: PromptIcons.Hand);
				return;
			}

			profile = ProfileManager.ConvertLegacyProfile(obj, false);

			if (profile is null)
			{
				ShowPrompt(Locale.FailedToImportLegacyProfile, icon: PromptIcons.Error);
				return;
			}
		}
		else if (profile is not null)
		{
			Ctrl_LoadProfile(profile);
			return;
		}
		else
		{
			profile = ProfileManager.ImportProfile(obj);
		}

		try
		{ Ctrl_LoadProfile(profile!); }
		catch (Exception ex) { ShowPrompt(ex, "Failed to import your profile"); }
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
	}

	private void CitiesManager_MonitorTick(bool isAvailable, bool isRunning)
	{
		this.TryInvoke(() => B_StartStop.Enabled = isAvailable);

		RefreshButtonState(isRunning);
	}

	private void RefreshButtonState(bool running, bool firstTime = false)
	{
		if (!running)
		{
			if (buttonStateRunning || firstTime)
			{
				this.TryInvoke(() =>
				{
					B_StartStop.ImageName = "I_CS";
					B_StartStop.Text = Locale.StartCities;
					buttonStateRunning = false;
				});
			}

			return;
		}

		if (!buttonStateRunning || firstTime)
		{
			this.TryInvoke(() =>
			{
				B_StartStop.ImageName = "I_Stop";
				B_StartStop.Text = Locale.StopCities;
				buttonStateRunning = true;
			});
		}
	}

	private void B_StartStop_Click(object sender, EventArgs e)
	{
		Program.MainForm.LaunchStopCities();
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
