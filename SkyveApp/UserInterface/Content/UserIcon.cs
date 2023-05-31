using Extensions;

using SkyveApp.Utilities.Managers;

using SlickControls;

using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace SkyveApp.UserInterface.Content;

internal class UserIcon : SlickImageControl
{
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public ulong UserId { get; set; }
	[Category("Appearance"), DefaultValue(true)]
	public bool HalfColor { get; set; } = true;

	protected override void OnPaint(PaintEventArgs e)
	{
		if (HalfColor)
		{
			e.Graphics.Clear(FormDesign.Design.BackColor);

			e.Graphics.FillRectangle(new SolidBrush(FormDesign.Design.AccentBackColor), new Rectangle(0, 0, Width, Height / 2));
		}
		else
		{
			e.Graphics.Clear(BackColor);
		}

		if (Loading)
		{
			DrawLoader(e.Graphics, ClientRectangle.CenterR(UI.Scale(new Size(32, 32), UI.UIScale)));
			return;
		}

		e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;

		if (Image == null)
		{
			using var image = Properties.Resources.I_AssetIcon.Color(FormDesign.Design.IconColor);

			e.Graphics.FillRoundedRectangle(new SolidBrush(FormDesign.Design.IconColor), ClientRectangle, (int)(10 * UI.FontScale));
			e.Graphics.FillRoundedRectangle(new SolidBrush(FormDesign.Design.BackColor), ClientRectangle.Pad(1), (int)(10 * UI.FontScale));

			e.Graphics.DrawRoundedImage(image, ClientRectangle, (int)(10 * UI.FontScale), FormDesign.Design.AccentBackColor);
		}
		else
		{
			e.Graphics.DrawRoundedImage(Image, ClientRectangle, (int)(10 * UI.FontScale), FormDesign.Design.AccentBackColor);

			if (CompatibilityManager.CompatibilityData.Authors.TryGet(UserId)?.Verified ?? false)
			{
				var checkRect = ClientRectangle.Pad((int)(5 * UI.FontScale)).Align(UI.Scale(new Size(24, 24), UI.FontScale), ContentAlignment.BottomRight);

				e.Graphics.FillEllipse(new SolidBrush(FormDesign.Design.GreenColor), checkRect);

				checkRect = checkRect.Pad((int)(5 * UI.FontScale));

				using var img = IconManager.GetIcon("I_Check", checkRect.Height);
				e.Graphics.DrawImage(img.Color(Color.White), checkRect.Pad(0, 0, -1, -1));
			}
		}
	}
}
