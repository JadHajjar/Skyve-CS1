using Extensions;

using LoadOrderToolTwo.Domain;
using LoadOrderToolTwo.Utilities;
using LoadOrderToolTwo.Utilities.Managers;

using SlickControls;

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace LoadOrderToolTwo.UserInterface.Dropdowns;
internal class ProfilesDropDown : SlickSelectionDropDown<Profile>
{
	protected override void OnHandleCreated(EventArgs e)
	{
		base.OnHandleCreated(e);

		if (Live)
		{
			Items = ProfileManager.Profiles.ToArray();
		}
	}

	protected override IEnumerable<DrawableItem<Profile>> OrderItems(IEnumerable<DrawableItem<Profile>> items)
	{
		return items.OrderByDescending(x => x.Item.Temporary).ThenByDescending(x => x.Item.LastEditDate);
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

	protected override void PaintItem(PaintEventArgs e, Rectangle rectangle, Color foreColor, HoverState hoverState, Profile item)
	{
		if (item is null)
		{ return; }

		var text = item.Name;

		if (item.Temporary)
		{
			text = Locale.Unfiltered;
		}

		using var icon = ImageManager.GetIcon(item.Temporary ? nameof(Properties.Resources.I_Slash) : item.ForGameplay ? nameof(Properties.Resources.I_City) : item.ForAssetEditor ? nameof(Properties.Resources.I_Tools) : nameof(Properties.Resources.I_ProfileSettings)).Color(foreColor);

		e.Graphics.DrawImage(icon, rectangle.Align(icon.Size, ContentAlignment.MiddleLeft));

		var textSize = (int)e.Graphics.Measure(text, Font).Height;
		var textRect = new Rectangle(rectangle.X + icon.Width + Padding.Left, rectangle.Y + ((rectangle.Height - textSize) / 2), 0, textSize);

		textRect.Width = rectangle.Width - textRect.X;

		e.Graphics.DrawString(text, Font, new SolidBrush(foreColor), textRect, new StringFormat { LineAlignment = StringAlignment.Center, Trimming = StringTrimming.EllipsisCharacter });
	}
}
