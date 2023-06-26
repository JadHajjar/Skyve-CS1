using Extensions;

using SkyveApp.Domain;
using SkyveApp.Services;
using SkyveApp.Services.Interfaces;
using SkyveApp.Utilities;

using SlickControls;

using System;
using System.Drawing;
using System.Windows.Forms;

namespace SkyveApp.UserInterface.StatusBubbles;
internal class FavoriteProfileBubble : StatusBubbleBase
{
	private readonly IPlaysetManager _profileManager;

	public Playset Profile { get; }

	public FavoriteProfileBubble(Playset profile)
	{
		_profileManager = Program.Services.GetService<IPlaysetManager>();
		Profile = profile;
	}

	public override Color? TintColor { get => Profile.Color; set { } }

	protected override void OnHandleCreated(EventArgs e)
	{
		base.OnHandleCreated(e);

		if (!Live)
		{
			return;
		}

		Text = Profile.Name;
		ImageName = Profile.GetIcon();

		_profileManager.ProfileChanged += ProfileManager_ProfileChanged;
	}

	protected override void UIChanged()
	{
		base.UIChanged();

		Margin = UI.Scale(new Padding(10, 0, 5, 10), UI.FontScale);
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing)
		{
			_profileManager.ProfileChanged -= ProfileManager_ProfileChanged;
		}

		base.Dispose(disposing);
	}

	protected override void OnMouseClick(MouseEventArgs e)
	{
		base.OnMouseClick(e);

		if (e.Button == MouseButtons.Left)
		{
			Loading = true;
			_profileManager.SetProfile(Profile);
		}
	}

	private void ProfileManager_ProfileChanged(Domain.Playset obj)
	{
		Loading = false;
		Text = Profile.Name;
		ImageName = Profile.GetIcon();
		Invalidate();
	}

	protected override void CustomDraw(PaintEventArgs e, ref int targetHeight)
	{
		if (_profileManager.CurrentPlayset == Profile)
		{
			e.Graphics.DrawRoundedRectangle(new Pen(FormDesign.Design.ActiveColor, (float)(1.5 * UI.FontScale)), ClientRectangle.Pad(1 + (int)Math.Floor(1.5 * UI.FontScale)), Padding.Left);
		}
	}
}
