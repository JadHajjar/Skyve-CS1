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

	protected override Color? TintColor => CentralManager.CurrentProfile.Color;

	protected override void OnHandleCreated(EventArgs e)
	{
		base.OnHandleCreated(e);

		if (!Live)
		{
			return;
		}

		Text = Locale.ProfileBubble;
		Image = CentralManager.CurrentProfile.GetIcon();

		ProfileManager.ProfileChanged += ProfileManager_ProfileChanged;
	}

	protected override void Dispose(bool disposing)
	{
		base.Dispose(disposing);
		
		ProfileManager.ProfileChanged -= ProfileManager_ProfileChanged;
	}

	private void ProfileManager_ProfileChanged(Domain.Profile obj)
	{
		Image = CentralManager.CurrentProfile.GetIcon();
	}

	protected override void CustomDraw(PaintEventArgs e, ref int targetHeight)
	{
		DrawText(e, ref targetHeight, CentralManager.CurrentProfile.Name ?? "");

		if (ProfileManager.ProfilesLoaded)
		{
			DrawValue(e, ref targetHeight, (ProfileManager.Profiles.Count() - 1).ToString(), Locale.ProfilesLoaded);
		}

		if (CentralManager.CurrentProfile.Temporary)
		{
			DrawText(e, ref targetHeight, Locale.CreateProfileHere, FormDesign.Design.YellowColor);
		}
		else
		{
			DrawText(e, ref targetHeight, CentralManager.CurrentProfile.AutoSave ? Locale.AutoProfileSaveOn : Locale.AutoProfileSaveOff, CentralManager.CurrentProfile.AutoSave ? FormDesign.Design.GreenColor : FormDesign.Design.YellowColor);
		}
	}
}
