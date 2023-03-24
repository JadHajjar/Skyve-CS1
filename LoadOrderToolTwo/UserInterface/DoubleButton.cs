using Extensions;

using LoadOrderToolTwo.Utilities.Managers;

using SlickControls;

using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace LoadOrderToolTwo.UserInterface;
public class DoubleButton : SlickControl
{
	public event EventHandler? LeftClicked;
	public event EventHandler? RightClicked;

	[Category("Appearance"), DefaultValue("")]
	public string Option1 { get; set; } = string.Empty;
	[Category("Appearance"), DefaultValue("")]
	public string Option2 { get; set; } = string.Empty;
	[Category("Appearance"), DefaultValue(ColorStyle.Red)]
	public ColorStyle OptionStyle1 { get; set; } = ColorStyle.Red;
	[Category("Appearance"), DefaultValue(ColorStyle.Green)]
	public ColorStyle OptionStyle2 { get; set; } = ColorStyle.Green;
	[Category("Appearance"), DefaultValue(null)]
	public string? Image1 { get; set; }
	[Category("Appearance"), DefaultValue(null)]
	public string? Image2 { get; set; }

	public DoubleButton()
	{
		Cursor = Cursors.Hand;
	}

	protected override void UIChanged()
	{
		Font = UI.Font(9.75F);
		Margin = UI.Scale(new Padding(5), UI.FontScale);
		Padding = UI.Scale(new Padding(5), UI.FontScale);
	}

	protected override void OnSizeChanged(EventArgs e)
	{
		base.OnSizeChanged(e);

		Height = (int)(28 * UI.UIScale);
	}

	protected override void OnMouseClick(MouseEventArgs e)
	{
		base.OnMouseClick(e);

		if (e.Button == MouseButtons.Left)
		if (e.Location.X < Width / 2)
		{
			LeftClicked?.Invoke(this, e);	
		}
		else
				RightClicked?.Invoke(this, e);
	}

	protected override void OnPaint(PaintEventArgs e)
	{
		e.Graphics.Clear(BackColor);
		e.Graphics.SmoothingMode = SmoothingMode.HighQuality;
		e.Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

		var iconOnly = Width < 200 * UI.FontScale;
		var iconSize = UI.FontScale >= 1.25 ? 24 : 16;
		var cursorLocation = PointToClient(Cursor.Position);
		var rectangle1 = new Rectangle(0, 0, (Width) / 2, Height - 1);
		var rectangle2 = new Rectangle((Width) / 2, 0, (Width) / 2, Height - 1);
		var option1Hovered = rectangle1.Contains(cursorLocation) && HoverState.HasFlag(HoverState.Hovered);
		var option2Hovered = rectangle2.Contains(cursorLocation) && HoverState.HasFlag(HoverState.Hovered);
		var textColor1 = (option1Hovered && HoverState.HasFlag(HoverState.Pressed)) ? FormDesign.Design.ActiveForeColor : FormDesign.Design.ForeColor;
		var textColor2 = (option2Hovered && HoverState.HasFlag(HoverState.Pressed)) ? FormDesign.Design.ActiveForeColor : FormDesign.Design.ForeColor;

		e.Graphics.FillRoundedRectangle(ClientRectangle.Gradient(FormDesign.Design.ButtonColor, 0.5F), ClientRectangle.Pad(1, 1, 2, 2), Padding.Left);
		
		// Option 1
		if (option1Hovered)
		{
			e.Graphics.FillRoundedRectangle(rectangle1.Gradient(Color.FromArgb(HoverState.HasFlag(HoverState.Pressed) ? 255 : 100, OptionStyle1.GetColor()), 0.5F), rectangle1, Padding.Left, topRight: false, botRight: false);
		}

		if (Image1 != null)
		{
			using var img1 = ImageManager.GetIcon(Image1)?.Color(textColor1);

			if (img1 != null)
			{
				e.Graphics.DrawImage(img1, new Rectangle(Padding.Left, (Height - iconSize) / 2, iconSize, iconSize));
			}
		}

		if (!iconOnly)
		{
			e.Graphics.DrawString(LocaleHelper.GetGlobalText(Option1), Font, new SolidBrush(textColor1), rectangle1.Pad(Padding).Pad(Image1 != null ? iconSize : 0, 0, 0, 0), new StringFormat { LineAlignment = StringAlignment.Center, Alignment = StringAlignment.Center });
		}

		// Option 2
		if (option2Hovered)
		{
			e.Graphics.FillRoundedRectangle(rectangle2.Gradient(Color.FromArgb(HoverState.HasFlag(HoverState.Pressed) ? 255 : 100, OptionStyle2.GetColor()), 0.5F), rectangle2, Padding.Left, topLeft: false, botLeft: false);
		}

		if (Image2 != null)
		{
			using var img2 = ImageManager.GetIcon(Image2)?.Color(textColor2);

			if (img2 != null)
			{
				e.Graphics.DrawImage(img2, new Rectangle(Width - iconSize - Padding.Right, (Height - iconSize) / 2, iconSize, iconSize));
			}
		}

		if (!iconOnly)
		{
			e.Graphics.DrawString(LocaleHelper.GetGlobalText(Option2), Font, new SolidBrush(textColor2), rectangle2.Pad(Padding).Pad(0, 0, Image2 != null ? iconSize : 0, 0), new StringFormat { LineAlignment = StringAlignment.Center, Alignment = StringAlignment.Center });
		}

		e.Graphics.DrawLine(new Pen(FormDesign.Design.AccentBackColor, 1.5F), Width / 2, -1, Width / 2, Height + 1);
	}
}
