using Extensions;

using LoadOrderToolTwo.Utilities;
using LoadOrderToolTwo.Utilities.Managers;

using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace LoadOrderToolTwo.UserInterface.StatusBubbles;
internal class ProfileBubble : StatusBubbleBase
{
	public ProfileBubble()
	{ }

	public override Color? TintColor { get => CentralManager.CurrentProfile.Color; set { } }

	protected override void OnHandleCreated(EventArgs e)
	{
		base.OnHandleCreated(e);

		if (!Live)
		{
			return;
		}

		Text = Locale.ProfileBubble;
		ImageName = CentralManager.CurrentProfile.GetIcon();

		ProfileManager.ProfileChanged += ProfileManager_ProfileChanged;
	}

	protected override void Dispose(bool disposing)
	{
		base.Dispose(disposing);
		
		ProfileManager.ProfileChanged -= ProfileManager_ProfileChanged;
	}

	private void ProfileManager_ProfileChanged(Domain.Profile obj)
	{
		ImageName = CentralManager.CurrentProfile.GetIcon();
	}

	protected override void CustomDraw(PaintEventArgs e, ref int targetHeight)
	{
		DrawText(e, ref targetHeight, CentralManager.CurrentProfile.Name ?? "");

		if (CentralManager.CurrentProfile.Temporary)
		{
			DrawText(e, ref targetHeight, Locale.CreateProfileHere, FormDesign.Design.YellowColor);
		}
		else
		{
			DrawText(e, ref targetHeight, CentralManager.CurrentProfile.AutoSave ? Locale.AutoProfileSaveOn : Locale.AutoProfileSaveOff, CentralManager.CurrentProfile.AutoSave ? FormDesign.Design.GreenColor : FormDesign.Design.YellowColor);
		}

		if (ProfileManager.ProfilesLoaded)
		{
			DrawText(e, ref targetHeight, Locale.LoadedCount.FormatPlural(ProfileManager.Profiles.Count() - 1, Locale.Profile.FormatPlural(ProfileManager.Profiles.Count() - 1).ToLower()));
		}
	}
}
