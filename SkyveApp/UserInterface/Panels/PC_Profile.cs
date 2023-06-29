using Extensions;

using SkyveApp.Domain.Compatibility.Enums;
using SkyveApp.Domain.Interfaces;
using SkyveApp.Services;
using SkyveApp.Services.Interfaces;
using SkyveApp.UserInterface.Generic;
using SkyveApp.Utilities;
using SkyveApp.Utilities.IO;

using SlickControls;

using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace SkyveApp.UserInterface.Panels;
public partial class PC_Profile : PanelContent
{
	private bool loadingProfile;
	private readonly SlickCheckbox[] _launchOptions;

	private readonly IPlaysetManager _profileManager = ServiceCenter.Get<IPlaysetManager>();
	private readonly ILocationManager _locationManager = ServiceCenter.Get<ILocationManager>();
	private readonly IContentManager _contentManager = ServiceCenter.Get<IContentManager>();
	private readonly IContentUtil _contentUtil = ServiceCenter.Get<IContentUtil>();
	private readonly ISettings _settings = ServiceCenter.Get<ISettings>();
	private readonly IOUtil _iOUtil = ServiceCenter.Get<IOUtil>();

	public PC_Profile()
	{
		InitializeComponent();

		_launchOptions = new[] { CB_StartNewGame, CB_LoadSave, CB_NewAsset, CB_LoadAsset };

		SlickTip.SetTo(B_AddProfile.Controls[0], "NewProfile_Tip");
		SlickTip.SetTo(B_TempProfile.Controls[0], "TempProfile_Tip");
		SlickTip.SetTo(B_ViewProfiles, "ViewProfiles_Tip");
		SlickTip.SetTo(I_ProfileIcon, "ChangeProfileColor");
		SlickTip.SetTo(B_EditName, "EditProfileName");
		SlickTip.SetTo(B_Save, "SaveProfileChanges");

		foreach (var item in this.GetControls<SlickCheckbox>())
		{
			if (item != CB_LHT && item != CB_NoWorkshop && item.Parent != TLP_AdvancedDev)
			{
				SlickTip.SetTo(item, item.Text + "_Tip");
			}
		}

		LoadProfile(_profileManager.CurrentPlayset);

		DD_SaveFile.StartingFolder = CrossIO.Combine(_locationManager.AppDataPath, "Saves");
		DD_SaveFile.PinnedFolders = new()
		{
			["Your Save-games"] = CrossIO.Combine(_locationManager.AppDataPath, "Saves"),
			["Workshop Save-games"] = IOSelectionDialog.CustomDirectory,
		};
		DD_SaveFile.CustomFiles = _contentManager.Assets.Where(x => x.Workshop && (x.Package.WorkshopTags?.Contains("SaveGame") ?? false)).Select(x => new IOSelectionDialog.CustomFile
		{
			Name = x.Package.Name,
			Icon = x.IconImage,
			Path = x.FileName
		}).ToList();

		DD_SkipFile.StartingFolder = _locationManager.AppDataPath;
		DD_SkipFile.PinnedFolders = new() { ["App Data"] = _locationManager.AppDataPath };

		DD_NewMap.StartingFolder = _locationManager.MapsPath;
		DD_NewMap.PinnedFolders = new()
		{
			["Custom Maps"] = _locationManager.MapsPath,
			["Vanilla Maps"] = CrossIO.Combine(_locationManager.GameContentPath, "Maps"),
			["Workshop Maps"] = IOSelectionDialog.CustomDirectory,
		};
		DD_NewMap.CustomFiles = _contentManager.Assets.Where(x => x.Workshop && (x.AssetTags.Contains("Map") || (x.Package.WorkshopTags?.Contains("Map") ?? false))).Select(x => new IOSelectionDialog.CustomFile
		{
			Name = x.Package.Name,
			Icon = x.IconImage,
			Path = x.FileName
		}).ToList();

		TLP_AdvancedDev.Visible = _settings.SessionSettings.UserSettings.AdvancedLaunchOptions;

		_profileManager.ProfileChanged += ProfileManager_ProfileChanged;
	}

	private void ProfileManager_ProfileChanged(Playset p)
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

		slickIcon1.Size = slickIcon2.Size = B_EditName.Size = B_Save.Size = I_ProfileIcon.Size = I_Info.Size = I_TempProfile.Size = I_Favorite.Size = UI.Scale(new Size(24, 24), UI.FontScale) + new Size(8, 8);
		slickSpacer1.Height = (int)(1.5 * UI.FontScale);
		P_Options.Padding = UI.Scale(new Padding(5,0,5,0), UI.UIScale);
		slickSpacer1.Margin = B_TempProfile.Padding = B_AddProfile.Padding = TLP_ProfileName.Padding = P_Options.Margin = UI.Scale(new Padding(5), UI.UIScale);
		L_TempProfile.Font = UI.Font(10.5F);
		L_CurrentProfile.Font = UI.Font(12.75F, FontStyle.Bold);
		B_ViewProfiles.Font = UI.Font(9.75F);
		TLP_AdvancedDev.Margin = TLP_GeneralSettings.Margin = TLP_LaunchSettings.Margin = TLP_LSM.Margin = UI.Scale(new Padding(10), UI.UIScale);
	}

	protected override void DesignChanged(FormDesign design)
	{
		base.DesignChanged(design);

		B_TempProfile.BackColor = B_AddProfile.BackColor = FormDesign.Design.ButtonColor;
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

	internal void Ctrl_LoadProfile(Playset obj)
	{
		I_ProfileIcon.Loading = true;
		L_CurrentProfile.Text = obj.Name;
		TLP_Options.Enabled = B_EditName.Visible = B_Save.Visible = false;
		_profileManager.SetProfile(obj);
	}

	private void LoadProfile(Playset profile)
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

		SlickTip.SetTo(I_Favorite, profile.IsFavorite ? "UnFavoriteThisProfile" : "FavoriteThisProfile");

		TLP_Main.SetColumn(B_TempProfile, profile.Temporary ? 4 : 3);
		TLP_Main.SetColumn(B_AddProfile, profile.Temporary ? 3 : 4);

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
		CB_NewAsset.Checked = profile.LaunchSettings.NewAsset;
		CB_LoadAsset.Checked = profile.LaunchSettings.LoadAsset;

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

		_profileManager.CurrentPlayset.AutoSave = CB_AutoSave.Checked;
		_profileManager.CurrentPlayset.Usage = DD_ProfileUsage.SelectedItem;

		_profileManager.CurrentPlayset.LaunchSettings.NoWorkshop = CB_NoWorkshop.Checked;
		_profileManager.CurrentPlayset.LaunchSettings.NoAssets = CB_NoAssets.Checked;
		_profileManager.CurrentPlayset.LaunchSettings.NoMods = CB_NoMods.Checked;
		_profileManager.CurrentPlayset.LaunchSettings.LHT = CB_LHT.Checked;
		_profileManager.CurrentPlayset.LaunchSettings.StartNewGame = CB_StartNewGame.Checked;
		_profileManager.CurrentPlayset.LaunchSettings.MapToLoad = _iOUtil.ToRealPath(DD_NewMap.SelectedFile);
		_profileManager.CurrentPlayset.LaunchSettings.SaveToLoad = _iOUtil.ToRealPath(DD_SaveFile.SelectedFile);
		_profileManager.CurrentPlayset.LaunchSettings.LoadSaveGame = CB_LoadSave.Checked;
		_profileManager.CurrentPlayset.LaunchSettings.UseCitiesExe = CB_UseCitiesExe.Checked;
		_profileManager.CurrentPlayset.LaunchSettings.UnityProfiler = CB_UnityProfiler.Checked;
		_profileManager.CurrentPlayset.LaunchSettings.DebugMono = CB_DebugMono.Checked;
		_profileManager.CurrentPlayset.LaunchSettings.RefreshWorkshop = CB_RefreshWorkshop.Checked;
		_profileManager.CurrentPlayset.LaunchSettings.DevUi = CB_DevUI.Checked;
		_profileManager.CurrentPlayset.LaunchSettings.CustomArgs = TB_CustomArgs.Text;
		_profileManager.CurrentPlayset.LaunchSettings.NewAsset = CB_NewAsset.Checked;
		_profileManager.CurrentPlayset.LaunchSettings.LoadAsset = CB_LoadAsset.Checked;

		_profileManager.CurrentPlayset.LsmSettings.SkipFile = _iOUtil.ToRealPath(DD_SkipFile.SelectedFile);
		_profileManager.CurrentPlayset.LsmSettings.LoadEnabled = CB_LoadEnabled.Checked;
		_profileManager.CurrentPlayset.LsmSettings.LoadUsed = CB_LoadUsed.Checked;
		_profileManager.CurrentPlayset.LsmSettings.UseSkipFile = CB_SkipFile.Checked;

		_profileManager.Save(_profileManager.CurrentPlayset);
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
		Form.PushPanel<PC_ProfileAdd>();
	}

	internal void B_EditName_Click(object sender, EventArgs e)
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

		if (!_profileManager.RenameProfile(_profileManager.CurrentPlayset, TB_Name.Text))
		{
			TB_Name.SetError();
			return;
		}

		if (_profileManager.CurrentPlayset.Name != TB_Name.Text)
		{
			Notification.Create(Locale.ProfileNameChangedIllegalChars, null, PromptIcons.Info, null)
				.Show(Form, 15);
		}

		L_CurrentProfile.Text = _profileManager.CurrentPlayset.Name;
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

		var invalidPackages = _profileManager.GetInvalidPackages(DD_ProfileUsage.SelectedItem);

		if (invalidPackages.Any())
		{
			if (ShowPrompt($"{Locale.SomePackagesWillBeDisabled}\r\n{Locale.AffectedPackagesAre}\r\n• {invalidPackages.ListStrings("\r\n• ")}", PromptButtons.OKCancel, PromptIcons.Warning) == DialogResult.Cancel)
			{
				DD_ProfileUsage.SelectedItem = (PackageUsage)(-1);

				return;
			}

			_contentUtil.SetBulkIncluded(invalidPackages, false);
		}

		ValueChanged(sender, e);

		I_ProfileIcon.Image = _profileManager.CurrentPlayset.GetIcon();
	}

	private void LsmSettingsChanged(object sender, EventArgs e)
	{
		if (loadingProfile)
		{
			return;
		}

		ValueChanged(sender, e);

		_profileManager.SaveLsmSettings(_profileManager.CurrentPlayset);
	}

	private void B_Save_Click(object sender, EventArgs e)
	{
		if (_profileManager.CurrentPlayset.Save())
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
		_profileManager.SetProfile(Playset.TemporaryPlayset);
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

	private void I_ProfileIcon_Click(object sender, EventArgs e)
	{
		if (_profileManager.CurrentPlayset.Temporary)
		{
			return;
		}

		var colorDialog = new SlickColorPicker(_profileManager.CurrentPlayset.Color ?? Color.Red);

		if (colorDialog.ShowDialog() != DialogResult.OK)
		{
			return;
		}

		TLP_ProfileName.BackColor = colorDialog.Color;
		TLP_ProfileName.ForeColor = TLP_ProfileName.BackColor.GetTextColor();
		_profileManager.CurrentPlayset.Color = colorDialog.Color;
		_profileManager.Save(_profileManager.CurrentPlayset);
	}

	private void I_Favorite_Click(object sender, EventArgs e)
	{
		if (_profileManager.CurrentPlayset.Temporary)
		{
			return;
		}

		_profileManager.CurrentPlayset.IsFavorite = !_profileManager.CurrentPlayset.IsFavorite;
		_profileManager.Save(_profileManager.CurrentPlayset);

		I_Favorite.ImageName = _profileManager.CurrentPlayset.IsFavorite ? "I_StarFilled" : "I_Star";
		SlickTip.SetTo(I_Favorite, _profileManager.CurrentPlayset.IsFavorite ? "UnFavoriteThisProfile" : "FavoriteThisProfile");
	}
}
