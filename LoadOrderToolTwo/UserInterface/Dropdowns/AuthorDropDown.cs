using Extensions;

using LoadOrderToolTwo.Domain.Interfaces;
using LoadOrderToolTwo.Domain.Steam;
using LoadOrderToolTwo.Utilities;
using LoadOrderToolTwo.Utilities.Managers;

using SlickControls;

using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace LoadOrderToolTwo.UserInterface.Dropdowns;
internal class AuthorDropDown : SlickMultiSelectionDropDown<SteamUser>
{
	private readonly Dictionary<SteamUser, int> _counts = new();

	internal void SetItems<T>(IEnumerable<T> enumerable) where T : IPackage
	{
		foreach (var item in enumerable)
		{
			if (_counts.ContainsKey(item.Author!))
			{
				_counts[item.Author!]++;
			}
			else
			{
				_counts[item.Author!] = 1;
			}
		}

		Items = _counts.Keys.ToArray();
	}

	protected override IEnumerable<DrawableItem<SteamUser>> OrderItems(IEnumerable<DrawableItem<SteamUser>> items)
	{
		return items.OrderByDescending(x => SelectedItems.Contains(x.Item)).ThenBy(x => x.Item.Name);
	}

	protected override bool SearchMatch(string searchText, SteamUser item)
	{
		return searchText.SearchCheck(item.Name) || item.SteamId.Contains(searchText);
	}

	protected override void PaintItem(PaintEventArgs e, Rectangle rectangle, Color foreColor, HoverState hoverState, SteamUser item, bool selected)
	{
		if (item is null)
		{ return; }

		var text = item.Name;
		var icon = ImageManager.GetImage(item?.AvatarUrl, true).Result;

		if (icon != null)
		{
			e.Graphics.DrawRoundedImage(icon, rectangle.Align(new Size(rectangle.Height - 2, rectangle.Height - 2), ContentAlignment.MiddleLeft), (int)(4 * UI.FontScale));
		}

		rectangle = rectangle.Pad(rectangle.Height + Padding.Left, 0, 0, 0);

		e.Graphics.DrawString(text, Font, new SolidBrush(foreColor), rectangle.AlignToFontSize(Font), new StringFormat { LineAlignment = StringAlignment.Center, Trimming = StringTrimming.EllipsisCharacter });

		e.Graphics.DrawString(string.Format(Locale.ItemsCount, _counts[item!]), Font, new SolidBrush(Color.FromArgb(200, foreColor)), rectangle.Pad(0, 0, (int)(5 * UI.FontScale), 0).AlignToFontSize(Font), new StringFormat { Alignment = StringAlignment.Far, LineAlignment = StringAlignment.Center, Trimming = StringTrimming.EllipsisCharacter });
	}

	protected override void PaintSelectedItems(PaintEventArgs e, Rectangle rectangle, Color foreColor, HoverState hoverState, IEnumerable<SteamUser> items)
	{
		if (items.Count() == 1)
		{
			PaintItem(e, rectangle, foreColor, hoverState, items.First(), false);

			return;
		}

		if (!items.Any())
		{
			using var icon = IconManager.GetIcon("I_Slash", rectangle.Height - 2).Color(foreColor);

			e.Graphics.DrawImage(icon, rectangle.Align(icon.Size, ContentAlignment.MiddleLeft));

			e.Graphics.DrawString(Locale.AnyAuthor, Font, new SolidBrush(foreColor), rectangle.Pad(icon.Width + Padding.Left, 0, 0, 0).AlignToFontSize(Font, ContentAlignment.MiddleLeft, e.Graphics), new StringFormat { Trimming = StringTrimming.EllipsisCharacter });

			return;
		}

		var iconRect = rectangle.Align(new Size(rectangle.Height - 2, rectangle.Height - 2), ContentAlignment.MiddleLeft);
		foreach (var item in items)
		{
			var icon = ImageManager.GetImage(item?.AvatarUrl, true).Result;
			if (icon is not null)
			{
				e.Graphics.DrawRoundedImage(icon, iconRect, (int)(4 * UI.FontScale));
			}

			iconRect.X += iconRect.Width * 9 / 10;
		}

		e.Graphics.DrawString(string.Format(Locale.AuthorsSelected, items.Count()), Font, new SolidBrush(foreColor), rectangle.Pad(iconRect.Right- iconRect.Width, 0, 0, 0).AlignToFontSize(Font), new StringFormat { LineAlignment = StringAlignment.Center, Trimming = StringTrimming.EllipsisCharacter });
	}
}
