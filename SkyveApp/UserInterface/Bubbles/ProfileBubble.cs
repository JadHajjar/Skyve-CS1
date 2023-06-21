using Extensions;
using SkyveApp.Services;
using SkyveApp.Services.Interfaces;
using SkyveApp.Utilities;

using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace SkyveApp.UserInterface.StatusBubbles;
internal class ProfileBubble : StatusBubbleBase
{
	private readonly IProfileManager _profileManager;

	public ProfileBubble()
	{ 
		_profileManager = Program.Services.GetService<IProfileManager>();
	}

	public override Color? TintColor { get => _profileManager.CurrentProfile.Color; set { } }

	protected override void OnHandleCreated(EventArgs e)
	{
		base.OnHandleCreated(e);

		if (!Live)
		{
			return;
		}

		Text = Locale.ProfileBubble;
		ImageName = _profileManager.CurrentProfile.GetIcon();

		_profileManager.ProfileChanged += ProfileManager_ProfileChanged;
	}

	protected override void Dispose(bool disposing)
	{
		base.Dispose(disposing);

		_profileManager.ProfileChanged -= ProfileManager_ProfileChanged;
	}

	private void ProfileManager_ProfileChanged(Domain.Profile obj)
	{
		ImageName = _profileManager.CurrentProfile.GetIcon();
	}

	protected override void CustomDraw(PaintEventArgs e, ref int targetHeight)
	{
		DrawText(e, ref targetHeight, _profileManager.CurrentProfile.Name ?? "");

		if (_profileManager.CurrentProfile.Temporary)
		{
			DrawText(e, ref targetHeight, Locale.CreateProfileHere, FormDesign.Design.YellowColor);
		}
		else
		{
			DrawText(e, ref targetHeight, _profileManager.CurrentProfile.AutoSave ? Locale.AutoProfileSaveOn : Locale.AutoProfileSaveOff, _profileManager.CurrentProfile.AutoSave ? FormDesign.Design.GreenColor : FormDesign.Design.YellowColor);
		}

		if (_profileManager.ProfilesLoaded)
		{
			DrawText(e, ref targetHeight, Locale.LoadedCount.FormatPlural(_profileManager.Profiles.Count() - 1, Locale.Profile.FormatPlural(_profileManager.Profiles.Count() - 1).ToLower()));
		}
	}
}
