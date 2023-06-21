using Extensions;

using SkyveApp.Domain;
using SkyveApp.Domain.Compatibility.Enums;
using SkyveApp.Domain.Interfaces;
using SkyveApp.Services;
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
public partial class PC_ProfileAdd : PanelContent
{
	public PC_ProfileAdd()
	{
		InitializeComponent();
		
		DAD_NewProfile.StartingFolder = LocationManager.AppDataPath;
	}

	protected override void UIChanged()
	{
		base.UIChanged();

		 B_Cancel.Font = UI.Font(9.75F);
		 DAD_NewProfile.Margin = UI.Scale(new Padding(10), UI.UIScale);
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

		var panel = new PC_Profile();

		if (Form.SetPanel(null, panel))
		{
			panel.B_EditName_Click(sender, e);
		}
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

		var panel = new PC_Profile();

		if (Form.SetPanel(null, panel))
		{
			panel.B_EditName_Click(sender, e);
		}
	}

	private void B_Cancel_Click(object sender, EventArgs e)
	{
		PushBack();
	}

	protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
	{
		if (keyData == Keys.Escape)
		{
			PushBack();
			return true;
		}

		return base.ProcessCmdKey(ref msg, keyData);
	}

	private void DAD_NewProfile_FileSelected(string obj)
	{
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
		else if (profile is null)
		{
			profile = ProfileManager.ImportProfile(obj);
		}

		try
		{
			var panel = new PC_Profile();

			if (Form.SetPanel(null, panel))
			{
				panel.Ctrl_LoadProfile(profile!);
			}
		}
		catch (Exception ex) { ShowPrompt(ex, "Failed to import your profile"); }
	}

	private async void B_ImportLink_Click(object sender, EventArgs e)
	{
		var result = ShowInputPrompt(Locale.PasteProfileId);

		if (result.DialogResult != DialogResult.OK)
			return;

		try
		{
			await ProfileManager.DownloadProfile(result.Input);
		}
		catch (Exception ex) { Program.MainForm.TryInvoke(() => MessagePrompt.Show(ex, Locale.FailedToDownloadProfile, form: Program.MainForm)); }
	}
}
