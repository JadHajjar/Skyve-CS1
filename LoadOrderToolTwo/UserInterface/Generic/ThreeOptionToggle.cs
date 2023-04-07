using Extensions;

using LoadOrderToolTwo.Utilities.Managers;

using SlickControls;

using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace LoadOrderToolTwo.UserInterface.Generic;
public class ThreeOptionToggle : SlickControl, ISupportsReset
{
    private Value _selectedValue;

    public enum Value
    {
        None = 0,
        Option1 = 1,
        Option2 = 2
    }

    public event EventHandler? SelectedValueChanged;

    [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public Value SelectedValue { get => _selectedValue; set { _selectedValue = value; Invalidate(); SelectedValueChanged?.Invoke(this, EventArgs.Empty); } }
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

    public ThreeOptionToggle()
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

        if (!Anchor.HasFlag(AnchorStyles.Top | AnchorStyles.Bottom))
        {
            Height = (int)(28 * UI.UIScale);
        }
    }

    protected override void OnMouseClick(MouseEventArgs e)
    {
        base.OnMouseClick(e);

		var centerWidth = Math.Max(Width / 5, (int)(40 * UI.FontScale));
		var option1Hovered = e.Location.X < (Width - centerWidth) / 2;
        var option2Hovered = e.Location.X > (Width + centerWidth) / 2;

        if (option1Hovered && SelectedValue != Value.Option1)
        {
            SelectedValue = Value.Option1;
        }
        else if (option2Hovered && SelectedValue != Value.Option2)
        {
            SelectedValue = Value.Option2;
        }
        else
        {
            SelectedValue = Value.None;
        }
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        e.Graphics.SetUp(BackColor);

        var iconOnly = Width < 200 * UI.FontScale;
        var iconSize = UI.FontScale >= 1.25 ? 24 : 16;
		var centerWidth = Math.Max(Width / 5, (int)(40 * UI.FontScale));
		var cursorLocation = PointToClient(Cursor.Position);
        var rectangle1 = new Rectangle(0, 0, (Width - centerWidth) / 2, Height - 1);
        var rectangle2 = new Rectangle((Width + centerWidth) / 2, 0, (Width - centerWidth) / 2, Height - 1);
        var rectangleNone = new Rectangle((Width - centerWidth) / 2, 0, centerWidth, Height - 1);
        var option1Hovered = rectangle1.Contains(cursorLocation) && HoverState.HasFlag(HoverState.Hovered);
        var option2Hovered = rectangle2.Contains(cursorLocation) && HoverState.HasFlag(HoverState.Hovered);
        var noneHovered = rectangleNone.Contains(cursorLocation) && HoverState.HasFlag(HoverState.Hovered);
        var textColor1 = SelectedValue == Value.Option1 || option1Hovered && HoverState.HasFlag(HoverState.Pressed) ? FormDesign.Design.ActiveForeColor : FormDesign.Design.ForeColor;
        var textColor2 = SelectedValue == Value.Option2 || option2Hovered && HoverState.HasFlag(HoverState.Pressed) ? FormDesign.Design.ActiveForeColor : FormDesign.Design.ForeColor;
        var textColorNone = SelectedValue == Value.None || noneHovered && HoverState.HasFlag(HoverState.Pressed) ? FormDesign.Design.ActiveForeColor : FormDesign.Design.ForeColor;

        e.Graphics.FillRoundedRectangle(ClientRectangle.Gradient(FormDesign.Design.ButtonColor, 0.5F), ClientRectangle, Padding.Left);

        // Option 1
        if (option1Hovered || SelectedValue == Value.Option1)
        {
            e.Graphics.FillRoundedRectangle(rectangle1.Gradient(Color.FromArgb(HoverState.HasFlag(HoverState.Pressed) || SelectedValue == Value.Option1 ? 255 : 100, OptionStyle1.GetColor()), 0.5F), rectangle1, Padding.Left, topRight: false, botRight: false);
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
        if (option2Hovered || SelectedValue == Value.Option2)
        {
            e.Graphics.FillRoundedRectangle(rectangle2.Gradient(Color.FromArgb(HoverState.HasFlag(HoverState.Pressed) || SelectedValue == Value.Option2 ? 255 : 100, OptionStyle2.GetColor()), 0.5F), rectangle2, Padding.Left, topLeft: false, botLeft: false);
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

        // Center
        if (noneHovered || SelectedValue == Value.None)
        {
            e.Graphics.FillRectangle(rectangleNone.Gradient(Color.FromArgb(HoverState.HasFlag(HoverState.Pressed) || SelectedValue == Value.None ? 255 : 100, FormDesign.Design.ActiveColor), 0.5F), rectangleNone);
        }

        using var slash = Properties.Resources.I_Slash.Color(textColorNone);
        e.Graphics.DrawImage(slash, ClientRectangle.CenterR(iconSize, iconSize));

        if (!Enabled)
        {
            e.Graphics.FillRectangle(new SolidBrush(Color.FromArgb(100, BackColor)), ClientRectangle);
        }
    }

    public void ResetValue()
    {
        SelectedValue = Value.None;
    }
}
