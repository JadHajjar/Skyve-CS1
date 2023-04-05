using Extensions;

using LoadOrderToolTwo.Utilities;
using LoadOrderToolTwo.Utilities.Managers;

using System;
using System.Windows.Forms;

namespace LoadOrderToolTwo.UserInterface.StatusBubbles;
internal class ProfileBubble : StatusBubbleBase
{
	public ProfileBubble()
	{ }

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
		if (disposing)
		{
			ProfileManager.ProfileChanged -= ProfileManager_ProfileChanged;
		}

		base.Dispose(disposing);
	}

	private void ProfileManager_ProfileChanged(Domain.Profile obj)
	{
		Image = CentralManager.CurrentProfile.GetIcon();
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
	}
}
