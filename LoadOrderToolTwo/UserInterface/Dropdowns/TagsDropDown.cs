using Extensions;

using LoadOrderToolTwo.Utilities;
using LoadOrderToolTwo.Utilities.Managers;

using SlickControls;

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

using static System.Net.Mime.MediaTypeNames;

namespace LoadOrderToolTwo.UserInterface.Dropdowns;
internal class TagsDropDown : SlickMultiSelectionDropDown<string>
{
    protected override IEnumerable<DrawableItem<string>> OrderItems(IEnumerable<DrawableItem<string>> items)
    {
        return items.OrderBy(x => x.Item != Locale.AnyTags).ThenBy(x => x.Item);
    }

    protected override void UIChanged()
    {
         Font = UI.Font(9.75F);
        if (Margin==new Padding(3))
        Margin = UI.Scale(new Padding(5), UI.FontScale);
        Padding = UI.Scale(new Padding(5), UI.FontScale);
    }

    protected override void OnSizeChanged(EventArgs e)
    {
        base.OnSizeChanged(e);

        Height = (int)(42 * UI.UIScale);
    }

    protected override void PaintItem(PaintEventArgs e, Rectangle rectangle, Color foreColor, HoverState hoverState, string text, bool selected)
    {
        if (selected && !hoverState.HasFlag(HoverState.Pressed))
            foreColor = FormDesign.Design.ActiveColor;

        using var icon = ImageManager.GetIcon(text == Locale.AnyTags ? nameof(Properties.Resources.I_Slash) : nameof(Properties.Resources.I_Tag)).Color(foreColor);

        e.Graphics.DrawImage(icon, rectangle.Align(icon.Size, ContentAlignment.MiddleLeft));

        var textSize = (int)e.Graphics.Measure(text, Font).Height;
        var textRect = new Rectangle(rectangle.X + icon.Width + Padding.Left, rectangle.Y + (rectangle.Height - textSize) / 2, 0, textSize);

        textRect.Width = rectangle.Width - textRect.X;

        e.Graphics.DrawString(text, Font, new SolidBrush(foreColor), textRect, new StringFormat { LineAlignment = StringAlignment.Center, Trimming = StringTrimming.EllipsisCharacter });
    }

	protected override void PaintSelectedItems(PaintEventArgs e, Rectangle rectangle, Color foreColor, HoverState hoverState, IEnumerable<string> items)
	{
		using var icon = ImageManager.GetIcon(!items.Any() ? nameof(Properties.Resources.I_Slash) : nameof(Properties.Resources.I_Tag)).Color(foreColor);

		e.Graphics.DrawImage(icon, rectangle.Align(icon.Size, ContentAlignment.MiddleLeft));

        var textSize = (int)e.Graphics.Measure(items.ListStrings(", ").IfEmpty(Locale.AnyTags), Font).Height;
		var textRect = new Rectangle(rectangle.X + icon.Width + Padding.Left, rectangle.Y + (rectangle.Height - textSize) / 2, 0, textSize);

		textRect.Width = rectangle.Width - textRect.X;

		e.Graphics.DrawString(items.ListStrings(", ").IfEmpty(Locale.AnyTags), Font, new SolidBrush(foreColor), textRect, new StringFormat { LineAlignment = StringAlignment.Center, Trimming = StringTrimming.EllipsisCharacter });
	}
}
