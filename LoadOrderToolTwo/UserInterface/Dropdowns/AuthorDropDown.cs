using Extensions;

using LoadOrderToolTwo.Domain.Steam;
using LoadOrderToolTwo.Utilities;
using LoadOrderToolTwo.Utilities.Managers;

using SlickControls;

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace LoadOrderToolTwo.UserInterface.Dropdowns;
internal class AuthorDropDown : SlickMultiSelectionDropDown<SteamUser>
{
	protected override void OnHandleCreated(EventArgs e)
	{
		base.OnHandleCreated(e);

		if (Live)
		{
			Items = CentralManager.Packages.Where(x => x.Author is not null).Select(x => x.Author).Distinct(x => x!.SteamId).ToArray()!;
		}
	}

	protected override IEnumerable<DrawableItem<SteamUser>> OrderItems(IEnumerable<DrawableItem<SteamUser>> items)
	{
		return items.OrderBy(x => x.Item.Name);
	}

	protected override void PaintItem(PaintEventArgs e, Rectangle rectangle, Color foreColor, HoverState hoverState, SteamUser item, bool selected)
	{
		if (item is null)
		{ return; }

		var text = item.Name;
		var icon = ImageManager.GetImage(item?.AvatarUrl, true).Result;

		if (icon != null)
		{
			e.Graphics.DrawRoundedImage(icon, rectangle.Align(new Size(rectangle.Height, rectangle.Height), ContentAlignment.MiddleLeft), (int)(4 * UI.FontScale));
		}

		var textSize = (int)e.Graphics.Measure(text, Font).Height;
		var textRect = new Rectangle(rectangle.X + (icon == null ? 0 : (icon.Width + Padding.Left)), rectangle.Y + ((rectangle.Height - textSize) / 2), 0, textSize);

		textRect.Width = rectangle.Width - textRect.X;

		e.Graphics.DrawString(text, Font, new SolidBrush(foreColor), textRect, new StringFormat { LineAlignment = StringAlignment.Center, Trimming = StringTrimming.EllipsisCharacter });
	}

	protected override void PaintSelectedItems(PaintEventArgs e, Rectangle rectangle, Color foreColor, HoverState hoverState, IEnumerable<SteamUser> items)
	{
		using var icon = IconManager.GetIcon(!items.Any() ? "I_Slash" : "I_Developer").Color(foreColor);

		e.Graphics.DrawImage(icon, rectangle.Align(icon.Size, ContentAlignment.MiddleLeft));

		var textSize = (int)e.Graphics.Measure(items.ListStrings(", ").IfEmpty(Locale.AnyTags), Font).Height;
		var textRect = new Rectangle(rectangle.X + icon.Width + Padding.Left, rectangle.Y + ((rectangle.Height - textSize) / 2), 0, textSize);

		textRect.Width = rectangle.Width - textRect.X;

		e.Graphics.DrawString(items.ListStrings(", ").IfEmpty(Locale.AnyTags), Font, new SolidBrush(foreColor), textRect, new StringFormat { LineAlignment = StringAlignment.Center, Trimming = StringTrimming.EllipsisCharacter });

	}
}
