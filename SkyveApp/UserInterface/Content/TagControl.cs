﻿using Extensions;

using SkyveApp.Domain;

using SlickControls;

using System;
using System.Drawing;
using System.Windows.Forms;

namespace SkyveApp.UserInterface.Content;
internal class TagControl : SlickImageControl
{
	public TagItem TagInfo { get; set; }
	public bool Display { get; set; }

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

	protected override void OnMouseClick(MouseEventArgs e)
	{
		base.OnMouseClick(e);

		if (Display && e.Button == MouseButtons.Left)
		{
			Clipboard.SetText(TagInfo.Value);
		}
	}

	public override Size GetPreferredSize(Size proposedSize)
	{
		if (Live && ImageName is not null)
		{
			using var img = Image;
			return Size.Ceiling(FontMeasuring.Measure(" ", Font)) + new Size(Padding.Horizontal + (img.Width * 2), Padding.Vertical);
		}

		using (var img = TagInfo.Icon.Default)
		{
			return Size.Ceiling(FontMeasuring.Measure(TagInfo.Value, Font)) + new Size(Padding.Horizontal + img.Width, Padding.Vertical);
		}
	}

	protected override void OnPaint(PaintEventArgs e)
	{
		e.Graphics.SetUp(BackColor);

		SlickButton.GetColors(out var fore, out var back, HoverState, !Display && ImageName is null ? ColorStyle.Red : ColorStyle.Active);

		using var brush = new SolidBrush(back);
		using var foreBrush = new SolidBrush(fore);

		e.Graphics.FillRoundedRectangle(brush, ClientRectangle.Pad(1), Padding.Left);

		if (Live && ImageName is not null)
		{
			using var img = Image;
			e.Graphics.DrawImage(img.Color(FormDesign.Design.ButtonForeColor), ClientRectangle.CenterR(img.Size));
		}
		else
		{
			using var img = (HoverState.HasFlag(HoverState.Hovered) ? (Display && TagInfo.Source is not Domain.Enums.TagSource.FindIt ? "I_Copy" : "I_Disposable") : TagInfo.Icon).Default;
			e.Graphics.DrawImage(img.Color(FormDesign.Design.ButtonForeColor), ClientRectangle.Pad(Padding).Align(img.Size, ContentAlignment.MiddleLeft));

			e.Graphics.DrawString(TagInfo.Value, Font, foreBrush, ClientRectangle.Pad(Padding.Horizontal + img.Width, 0, 0, 0), new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center });
		}

		DrawFocus(e.Graphics, ClientRectangle.Pad(1), Padding.Left, !Display && ImageName is null ? FormDesign.Design.RedColor : null);
	}
}
