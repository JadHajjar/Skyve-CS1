using Extensions;

using SlickControls;

using System;
using System.Drawing;
using System.Windows.Forms;

namespace LoadOrderToolTwo.UserInterface.Content;
internal class TagControl : SlickImageControl
{
	public TagControl()
	{
	}

	protected override void OnHandleCreated(EventArgs e)
	{
		base.OnHandleCreated(e);

		if (Live)
		{
			AutoSize = true;
		}
	}

	protected override void UIChanged()
	{
		base.UIChanged();

		Padding = UI.Scale(new Padding(3, 2, 3, 2), UI.FontScale);
	}

	public override Size GetPreferredSize(Size proposedSize)
	{
		if (ImageName is null)
		{
			return Size.Ceiling(FontMeasuring.Measure(Text, Font)) + new Size(Padding.Horizontal, Padding.Vertical);
		}

		using (Image)
		{
			return Size.Ceiling(FontMeasuring.Measure(" ", Font)) + new Size(Padding.Horizontal + Image.Width * 2, Padding.Vertical);
		}
	}

	protected override void OnPaint(PaintEventArgs e)
	{
		e.Graphics.Clear(BackColor);

		using var brush = new SolidBrush(FormDesign.Design.ButtonColor);
		using var foreBrush = new SolidBrush(FormDesign.Design.ButtonForeColor);

		e.Graphics.FillRoundedRectangle(brush, ClientRectangle.Pad(1), Padding.Left);


		if (ImageName is null)
		{
			e.Graphics.DrawString(Text, Font, foreBrush, ClientRectangle, new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center });
		}
		else if (Live)
		{
			using (Image)
			{
				e.Graphics.DrawImage(Image.Color(FormDesign.Design.ButtonForeColor), ClientSize.CenterR(Image.Size));
			}
		}
	}
}
