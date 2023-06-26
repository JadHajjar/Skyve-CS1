using Extensions;

using SkyveApp.Domain.Steam;
using SkyveApp.Services;
using SkyveApp.Services.Interfaces;
using SkyveApp.Utilities;
using SkyveApp.Utilities.IO;

using SlickControls;

using System;
using System.Drawing;
using System.Windows.Forms;

namespace SkyveApp.UserInterface.Generic;
internal class SteamUserControl : SlickControl
{
	public SteamUserControl()
	{
		Visible = false;
		Enabled = false;
		Cursor = Cursors.Hand;

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

	public SteamUserControl(ulong steamId)
	{
		Visible = false;
		Enabled = false;
		Cursor = Cursors.Hand;

		new BackgroundAction(async () =>
		{
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

	protected override void UIChanged()
	{
		Padding = UI.Scale(new Padding(3), UI.FontScale);
	}

	protected override void OnMouseClick(MouseEventArgs e)
	{
		base.OnMouseClick(e);

		if (e.Button == MouseButtons.Left && User is not null)
		{
			PlatformUtil.OpenUrl(User.ProfileUrl);
		}
	}

	public SteamUser? User { get; private set; }
	public string? InfoText { get; set; }

	protected override void OnPaint(PaintEventArgs e)
	{
		if (User is null)
		{
			return;
		}

		e.Graphics.SetUp(BackColor);

		e.Graphics.FillRoundedRectangle(new SolidBrush(FormDesign.Design.AccentBackColor), ClientRectangle, Padding.Horizontal);

		var size = Height - (Padding.Vertical * 2);
		var image = Program.Services.GetService<IImageService>().GetImage(User.AvatarUrl, true).Result;
		var textRectangle = ClientRectangle.Pad(size + (Padding.Left * 2), 0, 0, 0);
		var avatarRect = ClientRectangle.Pad(Padding).Align(new Size(size, size), ContentAlignment.MiddleLeft);
		var infoSize = e.Graphics.Measure(InfoText.IfEmpty(Locale.LoggedInAs), UI.Font(6.75F, FontStyle.Bold));
		var nameSize = e.Graphics.Measure(User.Name, UI.Font(9F, FontStyle.Bold));

		if (image is not null)
		{
			e.Graphics.DrawRoundImage(image, avatarRect);
		}
		else
		{
			using var generic = Properties.Resources.I_GenericUser.Color(FormDesign.Design.IconColor);

			e.Graphics.DrawRoundImage(generic, avatarRect);
		}

		e.Graphics.DrawString(InfoText.IfEmpty(Locale.LoggedInAs), UI.Font(6.75F, FontStyle.Bold), new SolidBrush(FormDesign.Design.InfoColor), textRectangle);
		e.Graphics.DrawString(User.Name, UI.Font(9F, FontStyle.Bold), new SolidBrush(FormDesign.Design.ForeColor), textRectangle, new StringFormat { LineAlignment = StringAlignment.Far });

		var width = (int)Math.Max(infoSize.Width, nameSize.Width) + Padding.Left + Padding.Horizontal + size;
		var height = (int)(infoSize.Height + nameSize.Height);

		if ((width != Width && Dock == DockStyle.None) || height != Height)
		{
			Size = new(width, height);
		}

		if (Program.Services.GetService<ICompatibilityManager>().CompatibilityData.Authors.TryGet(User.SteamId)?.Verified ?? false)
		{
			var checkRect = avatarRect.Align(new Size(avatarRect.Height / 3, avatarRect.Height / 3), ContentAlignment.BottomRight);

			e.Graphics.FillEllipse(new SolidBrush(FormDesign.Design.GreenColor), checkRect.Pad(-(int)(2 * UI.FontScale)));

			using var img = IconManager.GetIcon("I_Check", checkRect.Height);
			e.Graphics.DrawImage(img.Color(Color.White), checkRect.Pad(0, 0, -1, -1));
		}
	}
}
