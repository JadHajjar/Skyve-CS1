using Extensions;

using SkyveApp.Domain;
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
public partial class PC_ProfileAdd : PanelContent
{
	private readonly IProfileManager _profileManager = Program.Services.GetService<IProfileManager>();

	public PC_ProfileAdd()
	{
		InitializeComponent();
		
		DAD_NewProfile.StartingFolder = Program.Services.GetService<ILocationManager>().AppDataPath;
	}

	protected override void UIChanged()
	{
		base.UIChanged();

		 B_Cancel.Font = UI.Font(9.75F);
		 DAD_NewProfile.Margin = UI.Scale(new Padding(10), UI.UIScale);
	}

	private void NewProfile_Click(object sender, EventArgs e)
	{
		var newProfile = new Profile() { Name = _profileManager.GetNewProfileName(), LastEditDate = DateTime.Now };

		if (!_profileManager.Save(newProfile))
		{
			ShowPrompt(Locale.ProfileCreationFailed, icon: PromptIcons.Error);
			return;
		}

		_profileManager.AddProfile(newProfile);

		_profileManager.SetProfile(newProfile);

		var panel = new PC_Profile();

		if (Form.SetPanel(null, panel))
		{
			panel.B_EditName_Click(sender, e);
		}
	}

	private void CopyProfile_Click(object sender, EventArgs e)
	{
		var newProfile = _profileManager.CurrentProfile.Clone();
		newProfile.Name = _profileManager.GetNewProfileName();
		newProfile.LastEditDate = DateTime.Now;

		if (!newProfile.Save())
		{
			ShowPrompt(Locale.CouldNotCreateProfile, icon: PromptIcons.Error);
			return;
		}

		_profileManager.AddProfile(newProfile);

		_profileManager.SetProfile(newProfile);

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
		var profile = _profileManager.Profiles.FirstOrDefault(x => x.Name!.Equals(Path.GetFileNameWithoutExtension(obj), StringComparison.InvariantCultureIgnoreCase));

		if (obj.EndsWith(".xml", StringComparison.OrdinalIgnoreCase))
		{
			if (profile is not null)
			{
				ShowPrompt(Locale.ProfileNameUsed, icon: PromptIcons.Hand);
				return;
			}

			profile = _profileManager.ConvertLegacyProfile(obj, false);

			if (profile is null)
			{
				ShowPrompt(Locale.FailedToImportLegacyProfile, icon: PromptIcons.Error);
				return;
			}
		}
		else if (profile is null)
		{
			profile = _profileManager.ImportProfile(obj);
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
			await _profileManager.DownloadProfile(result.Input);
		}
		catch (Exception ex) { Program.MainForm.TryInvoke(() => MessagePrompt.Show(ex, Locale.FailedToDownloadProfile, form: Program.MainForm)); }
	}
}
