﻿using Extensions;

using SkyveApp.Domain;
using SkyveApp.Domain.Enums;
using SkyveApp.Utilities;

using SlickControls;

using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace SkyveApp.UserInterface.Dropdowns;
internal class TagsDropDown : SlickMultiSelectionDropDown<TagItem>
{
	protected override IEnumerable<TagItem> OrderItems(IEnumerable<TagItem> items)
	{
		return items.OrderByDescending(x => SelectedItems.Contains(x)).ThenBy(x => x.Value);
	}

	protected override bool SearchMatch(string searchText, TagItem item)
	{
		return searchText.SearchCheck(item.Value);
	}

	protected override void PaintItem(PaintEventArgs e, Rectangle rectangle, Color foreColor, HoverState hoverState, TagItem item, bool selected)
	{
		var text = item.Value;

		using var icon = IconManager.GetIcon(text == Locale.AnyTags ? "I_Slash" : item.Source switch { TagSource.Workshop => "I_Steam", TagSource.FindIt => "I_Search", _ => "I_Tag" }, rectangle.Height - 2).Color(foreColor);

		e.Graphics.DrawImage(icon, rectangle.Align(icon.Size, ContentAlignment.MiddleLeft));

		e.Graphics.DrawString(text, Font, new SolidBrush(foreColor), rectangle.Pad(icon.Width + Padding.Left, 0, 0, 0).AlignToFontSize(Font, ContentAlignment.MiddleLeft, e.Graphics), new StringFormat { Trimming = StringTrimming.EllipsisCharacter });
	}

	protected override void PaintSelectedItems(PaintEventArgs e, Rectangle rectangle, Color foreColor, HoverState hoverState, IEnumerable<TagItem> items)
	{
		var text = items.Count() switch { 0 => Locale.AnyTags.ToString(), <= 2 => items.ListStrings(", "), _ => $"{items.Take(2).ListStrings(", ")} +{items.Count() - 2}" };

		using var icon = IconManager.GetIcon(!items.Any() ? "I_Slash" : "I_Tag", rectangle.Height - 2).Color(foreColor);

		e.Graphics.DrawImage(icon, rectangle.Align(icon.Size, ContentAlignment.MiddleLeft));

		e.Graphics.DrawString(text, Font, new SolidBrush(foreColor), rectangle.Pad(icon.Width + Padding.Left, 0, 0, 0).AlignToFontSize(Font, ContentAlignment.MiddleLeft, e.Graphics), new StringFormat { Trimming = StringTrimming.EllipsisCharacter });
	}
}
