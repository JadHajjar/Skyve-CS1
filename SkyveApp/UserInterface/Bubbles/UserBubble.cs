using Extensions;

using SkyveApp.Domain.Steam;
using SkyveApp.Utilities;
using SkyveApp.Utilities.Managers;

using SlickControls;

using System;
using System.Drawing;
using System.Windows.Forms;

namespace SkyveApp.UserInterface.StatusBubbles;
internal class UserBubble : StatusBubbleBase
{
	public UserBubble()
	{
		Visible = false;
		Enabled = false;

		new BackgroundAction(async () =>
		{
			var steamId = SteamUtil.GetLoggedInSteamId();

			if (steamId != 0)
			{
				User = await SteamUtil.GetUserAsync(steamId);

				if (User is not null)
				{
					this.TryInvoke(Show);
				}
			}
		}).Run();
	}

	public SteamUser? User { get; private set; }

	protected override void CustomDraw(PaintEventArgs e, ref int targetHeight)
	{
		if (User is null)
		{
			return;
		}

		var size = UI.FontScale >= 4 ? 96 : UI.FontScale >= 2 ? 48 : 24;
		var icon = ImageManager.GetImage(User.AvatarUrl, true).Result;
		var titleHeight = Math.Max(size, (int)e.Graphics.Measure(User.Name, UI.Font(9.75F, FontStyle.Bold), Width - Padding.Horizontal).Height);
		var iconRectangle = new Rectangle(Padding.Left, Padding.Top + ((titleHeight - size) / 2), size, size);

		e.Graphics.DrawRoundImage(icon, iconRectangle);

		e.Graphics.DrawString(User.Name, UI.Font(9.75F, FontStyle.Bold), new SolidBrush(FormDesign.Design.ForeColor), new Rectangle(size + (Padding.Left * 2), Padding.Top, Width - Padding.Horizontal, titleHeight), new StringFormat { LineAlignment = StringAlignment.Center });

		targetHeight = titleHeight + Padding.Vertical;
	}
}
