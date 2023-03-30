using Extensions;

using LoadOrderToolTwo.Utilities;
using LoadOrderToolTwo.Utilities.Managers;

using SlickControls;
using SlickControls.Controls.Form;

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace LoadOrderToolTwo.UserInterface.Dropdowns;
internal class TagsDropDown : SlickSelectionDropDown<string>
{
    protected override void OnHandleCreated(EventArgs e)
    {
        base.OnHandleCreated(e);

        if (Live)
        {
            new BackgroundAction("Getting tag list", () =>
            {
                Items = new[] { Locale.AnyTags }
                .Concat(AssetsUtil.AssetInfoCache.Values.SelectMany(x => x.Tags))
                .Concat(SteamUtil.GetCachedInfo()?.Values.SelectMany(x => x.Tags ?? new string[0]) ?? new string[0])
                .Concat(AssetsUtil.GetAllFindItTags())
                .Distinct().ToArray();
            }).Run();
        }
    }

    protected override IEnumerable<DrawableItem<string>> OrderItems(IEnumerable<DrawableItem<string>> items)
    {
        return items.OrderBy(x => x.Item != Locale.AnyTags).ThenBy(x => x.Item);
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

        Height = (int)(42 * UI.UIScale);
    }

    protected override void PaintItem(PaintEventArgs e, Rectangle rectangle, Color foreColor, HoverState hoverState, string text)
    {
        using var icon = ImageManager.GetIcon(text == Locale.AnyTags ? nameof(Properties.Resources.I_Slash) : nameof(Properties.Resources.I_Tag)).Color(foreColor);

        e.Graphics.DrawImage(icon, rectangle.Align(icon.Size, ContentAlignment.MiddleLeft));

        var textRect = new Rectangle(rectangle.X + icon.Width + Padding.Left, rectangle.Y + (rectangle.Height - Font.Height) / 2, 0, Font.Height);

        textRect.Width = rectangle.Width - textRect.X;

        e.Graphics.DrawString(text, Font, new SolidBrush(foreColor), textRect, new StringFormat { LineAlignment = StringAlignment.Center, Trimming = StringTrimming.EllipsisCharacter });
    }
}
