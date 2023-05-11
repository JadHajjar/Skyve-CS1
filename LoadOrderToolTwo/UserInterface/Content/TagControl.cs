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
			Cursor = Cursors.Hand;
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

		using (var img = Image)
		{
			return Size.Ceiling(FontMeasuring.Measure(" ", Font)) + new Size(Padding.Horizontal + img.Width * 2, Padding.Vertical);
		}
	}

	protected override void OnPaint(PaintEventArgs e)
	{
		e.Graphics.SetUp(BackColor);

		SlickButton.GetColors(out var fore, out var back, HoverState, ImageName is null ? ColorStyle.Red : ColorStyle.Active);

		using var brush = new SolidBrush(back);
		using var foreBrush = new SolidBrush(fore);

		e.Graphics.FillRoundedRectangle(brush, ClientRectangle.Pad(1), Padding.Left);

		if (ImageName is null)
		{
			if (HoverState.HasFlag(HoverState.Hovered))
			{
				using var delete = IconManager.GetIcon("I_Disposable");
				e.Graphics.DrawImage(delete.Color(FormDesign.Design.ButtonForeColor), ClientRectangle.CenterR(delete.Size));
			}
			else
			{
				e.Graphics.DrawString(Text, Font, foreBrush, ClientRectangle, new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center });
			}
		}
		else if (Live)
		{
			using (var img = Image)
			{
				e.Graphics.DrawImage(img.Color(FormDesign.Design.ButtonForeColor), ClientRectangle.CenterR(img.Size));
			}
		}

		DrawFocus(e.Graphics, ClientRectangle.Pad(1), Padding.Left, ImageName is null ? FormDesign.Design.RedColor : null);
	}
}
