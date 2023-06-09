﻿using Extensions;

using SlickControls;

using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace SkyveApp.UserInterface.Generic;
internal class ReviewRequestOptionControl : SlickImageControl
{
	public ReviewRequestOptionControl()
	{
		Cursor = Cursors.Hand;
	}

	[Browsable(true)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
	[EditorBrowsable(EditorBrowsableState.Always)]
	[Bindable(true)]
	public override string Text { get => base.Text; set { base.Text = value; UIChanged(); } }

	public bool FromScratch { get; set; }

	protected override void UIChanged()
	{
		if (Live)
		{
			Font = UI.Font(11.25F, FontStyle.Bold);
			Margin = UI.Scale(new Padding(Parent.Controls.IndexOf(this) % 2 * 100, 15, 100 - (Parent.Controls.IndexOf(this) % 2 * 100), 15), UI.FontScale);
			Padding = UI.Scale(new Padding(15), UI.FontScale);
			Size = UI.Scale(new Size(250, 90), UI.FontScale);
		}
	}

	protected override void OnMouseMove(MouseEventArgs e)
	{ }

	protected sealed override void OnPaint(PaintEventArgs e)
	{
		e.Graphics.SetUp(BackColor);

		SlickButton.GetColors(out var fore, out var back, HoverState, FromScratch ? ColorStyle.Green : ColorStyle.Active);

		if (!HoverState.HasFlag(HoverState.Pressed) && FormDesign.Design.Type == FormDesignType.Light)
		{
			back = back.Tint(Lum: 1.5F);
		}

		e.Graphics.FillRoundedRectangle(ClientRectangle.Gradient(back, 0.8F), ClientRectangle, Padding.Left);

		using var icon = ImageName.Get((int)(32 * UI.UIScale));

		e.Graphics.DrawImage(icon.Color(fore), ClientRectangle.Pad(Padding).Align(icon.Size, ContentAlignment.MiddleLeft));

		e.Graphics.DrawString(LocaleHelper.GetGlobalText(Text), Font, new SolidBrush(fore), ClientRectangle.Pad(Padding).Pad(Padding.Left + icon.Width, 0, 0, 0), new StringFormat { LineAlignment = StringAlignment.Center });
	}
}
