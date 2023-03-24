using Extensions;

using LoadOrderToolTwo.Utilities;

using SlickControls;

using System;
using System.Drawing;
using System.Windows.Forms;

namespace LoadOrderToolTwo.UserInterface;
internal class NewProfileOptionControl : SlickImageControl
{
	public NewProfileOptionControl()
	{
		Cursor = Cursors.Hand;
	}

    public bool FromScratch { get; set; }

    protected override void UIChanged()
	{
		Font = UI.Font(12.75F, FontStyle.Bold);
		Margin = UI.Scale(new Padding(FromScratch ? 0 : 100, 20, FromScratch ? 100 : 0, 20), UI.FontScale);
		Padding = UI.Scale(new Padding(15), UI.FontScale);
		Size = UI.Scale(new Size(330, 120), UI.FontScale);
	}

	protected override void OnMouseMove(MouseEventArgs e)
	{ }

	protected sealed override void OnPaint(PaintEventArgs e)
	{
		e.Graphics.Clear(BackColor);
		e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
		e.Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

		SlickButton.GetColors(out var fore, out var back, HoverState, FromScratch ? ColorStyle.Green : ColorStyle.Active);

		if (!HoverState.HasFlag(HoverState.Pressed) && FormDesign.Design.Type == FormDesignType.Light)
		{
			back = back.Tint(Lum: 1.5F);
		}

		e.Graphics.FillRoundedRectangle(ClientRectangle.Gradient(back, 0.8F), ClientRectangle, Padding.Left);

		e.Graphics.DrawImage((FromScratch ? Properties.Resources.I_New_48 : Properties.Resources.I_Copy_48).Color(fore), ClientRectangle.Pad(Padding).Align(new Size(48, 48), ContentAlignment.MiddleLeft));

		e.Graphics.DrawString(FromScratch ? Locale.StartScratch : Locale.ContinueFromCurrent, Font, new SolidBrush(fore), ClientRectangle.Pad(Padding).Pad(Padding.Left + 48, 0, 0, 0), new StringFormat { LineAlignment = StringAlignment.Center });
	}
}
