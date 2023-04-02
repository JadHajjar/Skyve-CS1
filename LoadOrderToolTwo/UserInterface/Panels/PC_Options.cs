using Extensions;

using LoadOrderToolTwo.Domain.Utilities;
using LoadOrderToolTwo.Utilities;
using LoadOrderToolTwo.Utilities.Managers;

using SlickControls;

using System;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;

namespace LoadOrderToolTwo.UserInterface.Panels;
public partial class PC_Options : PanelContent
{
	private bool folderPathsChanged;

	public PC_Options()
	{
		InitializeComponent();

		foreach (var cb in this.GetControls<SlickCheckbox>())
		{
			if (!string.IsNullOrWhiteSpace(cb.Tag?.ToString()))
			{
				cb.Checked = (bool)typeof(SessionSettings)
					.GetProperty(cb.Tag!.ToString(), BindingFlags.Instance | BindingFlags.Public)
					.GetValue(CentralManager.SessionSettings);
			}
		}

		TB_VirtualAppDataPath.Visible = TB_VirtualGamePath.Visible = LocationManager.Platform is not Platform.Windows;

		TB_GamePath.Text = LocationManager.GamePath;
		TB_AppDataPath.Text = LocationManager.AppDataPath;
		TB_SteamPath.Text = LocationManager.SteamPath;
		TB_VirtualAppDataPath.Text = LocationManager.VirtualAppDataPath;
		TB_VirtualGamePath.Text = LocationManager.VirtualGamePath;

		folderPathsChanged = false;

		DD_Language.Items = LocaleHelper.GetAvailableLanguages().Select(lang => new CultureInfo(lang)).ToArray();
		DD_Language.SelectedItem = DD_Language.Items.FirstOrDefault(x => x.IetfLanguageTag == LocaleHelper.CurrentCulture.IetfLanguageTag);
		DD_Language.SelectedItemChanged += DD_Language_SelectedItemChanged;
	}

	protected override void LocaleChanged()
	{
		Text = Locale.Options;
	}

	protected override void UIChanged()
	{
		base.UIChanged();

		TLP_UI.Image = ImageManager.GetIcon(nameof(Properties.Resources.I_UserInterface));
		TLP_Folders.Image = ImageManager.GetIcon(nameof(Properties.Resources.I_Folder));
		TLP_Preferences.Image = ImageManager.GetIcon(nameof(Properties.Resources.I_Preferences));
		TLP_Settings.Image = ImageManager.GetIcon(nameof(Properties.Resources.I_Cog));
		TLP_HelpLogs.Image = ImageManager.GetIcon(nameof(Properties.Resources.I_AskHelp));
		TLP_Advanced.Image = ImageManager.GetIcon(nameof(Properties.Resources.I_Hazard));
		DD_Language.Width = (int)(220 * UI.FontScale);
		TLP_Main.Padding = UI.Scale(new Padding(3, 0, 7, 0), UI.FontScale);
		B_Theme.Margin = DD_Language.Margin = TLP_UI.Margin = TLP_Settings.Margin = TLP_Advanced.Margin = B_HelpTranslate.Margin = TLP_HelpLogs.Margin =
		TLP_Preferences.Margin = TLP_Folders.Margin = UI.Scale(new Padding(10), UI.UIScale);
		slickSpacer1.Height = (int)(1.5 * UI.FontScale);
		slickSpacer1.Margin = UI.Scale(new Padding(5), UI.UIScale);
	}
	
	protected override void DesignChanged(FormDesign design)
	{
		base.DesignChanged(design);

		TLP_UI.BackColor = TLP_Preferences.BackColor = TLP_Folders.BackColor = design.BackColor.Tint(Lum: design.Type.If(FormDesignType.Dark, 1, -1));
	}

	public override bool CanExit(bool toBeDisposed)
	{
		if (folderPathsChanged)
		{
			if (ShowPrompt(Locale.ChangingFoldersRequiresRestart, PromptButtons.OKCancel, PromptIcons.Hand) == System.Windows.Forms.DialogResult.OK)
			{
				LocationManager.SetPaths(TB_GamePath.Text, TB_AppDataPath.Text, TB_SteamPath.Text, TB_VirtualAppDataPath.Text, TB_VirtualGamePath.Text);

				Process.Start(Application.ExecutablePath);

				folderPathsChanged = false;

				Application.Exit();

				return false;
			}
		}

		return true;
	}

	protected override void OnCreateControl()
	{
		base.OnCreateControl();

		foreach (var tb in TLP_Folders.GetControls<SlickPathTextBox>())
		{
			tb.Image = Properties.Resources.I_FolderSearch;
		}
	}

	private void CB_CheckChanged(object sender, EventArgs e)
	{
		var cb = (sender as SlickCheckbox)!;

		typeof(SessionSettings)
			.GetProperty(cb.Tag!.ToString(), BindingFlags.Instance | BindingFlags.Public)
			.SetValue(CentralManager.SessionSettings, cb.Checked);

		CentralManager.SessionSettings.Save();
	}

	private void TB_FolderPath_TextChanged(object sender, EventArgs e)
	{
		folderPathsChanged = true;
	}

	private void DD_Language_SelectedItemChanged(object sender, EventArgs e)
	{
		LocaleHelper.SetLanguage(DD_Language.SelectedItem);
	}

	private void B_Theme_Click(object sender, EventArgs e)
	{
		Form.PushPanel<PC_ThemeChanger>(null);
	}

	private void B_HelpTranslate_Click(object sender, EventArgs e)
	{
		try
		{
			Process.Start("https://crowdin.com/project/load-order-mod-2");
		}
		catch { }
	}
}
