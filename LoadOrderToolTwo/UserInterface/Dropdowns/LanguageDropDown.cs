using Extensions;

using SlickControls;

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace LoadOrderToolTwo.UserInterface.Dropdowns;
internal class LanguageDropDown : SlickSelectionDropDown<CultureInfo>
{
	protected override void OnCreateControl()
	{
		base.OnCreateControl();

		if (Live)
		{
			ItemHeight = 22;
		}
	}

	protected override void UIChanged()
	{
		Font = UI.Font("Segoe UI", 8.25F);
		Padding = UI.Scale(new Padding(5), UI.FontScale);
	}

	protected override void OnSizeChanged(EventArgs e)
	{
		base.OnSizeChanged(e);

		Height = (int)(42 * UI.UIScale);
	}

	protected override IEnumerable<DrawableItem<CultureInfo>> OrderItems(IEnumerable<DrawableItem<CultureInfo>> items)
	{
		return items.OrderByDescending(x => x.Item.IetfLanguageTag == "en-US").ThenBy(x => x.Item.EnglishName);
	}

	protected override bool SearchMatch(string searchText, CultureInfo item)
	{
		return searchText.SearchCheck(item.EnglishName) || searchText.SearchCheck(item.NativeName);
	}

	protected override void PaintItem(PaintEventArgs e, Rectangle rectangle, Color foreColor, HoverState hoverState, CultureInfo item)
	{
		if (item == null)
		{
			return;
		}

		using var icon = (Bitmap)Properties.Resources.ResourceManager.GetObject("Lang_" + item.IetfLanguageTag.ToUpper(), Properties.Resources.Culture);

		if (icon != null)
		{
			var iconSize = Math.Min(48, (int)(16 * UI.UIScale));

			e.Graphics.DrawImage(icon, rectangle.Pad(Padding).Align(new Size(iconSize, iconSize), ContentAlignment.MiddleLeft));

			var text = hoverState.HasFlag(HoverState.Hovered) ? (item.NativeName.RegexRemove(@"\((.+?)\)").Trim().UpperFirstLetter(), Regex.Match(item.NativeName, @"\((.+?)\)").Groups[1].Value) : (item.EnglishName.RegexRemove(@"\((.+?)\)").Trim(), Regex.Match(item.EnglishName, @"\((.+?)\)").Groups[1].Value);
			var textSize1 = Size.Ceiling(e.Graphics.Measure(text.Item1, Font));
			var textSize2 = Size.Ceiling(e.Graphics.Measure(" / " + text.Value, UI.Font(7F)));
			var textRect = rectangle.Pad(iconSize + Padding.Horizontal, (int)(ItemHeight * UI.FontScale) + 1 != rectangle.Height ? -1 : (-Padding.Top + 1), Padding.Right * 3 / 2, (int)(ItemHeight * UI.FontScale) + 1 != rectangle.Height ? -3 : (-Padding.Bottom + 1));
			var textRect1 = textRect.Align(new Size(textRect.Width, textSize1.Height), textSize1.Width + textSize2.Width > textRect.Width ? ContentAlignment.TopCenter : ContentAlignment.MiddleCenter);
			var textRect2 = textRect.Align(new Size(textRect.Width, textSize2.Height), textSize1.Width + textSize2.Width > textRect.Width ? ContentAlignment.BottomCenter : ContentAlignment.MiddleCenter);

			textRect.Width = rectangle.Width - textRect.X;

			e.Graphics.DrawString(text.Item1, Font, new SolidBrush(foreColor), textRect1, new StringFormat { Trimming = StringTrimming.EllipsisCharacter });

			e.Graphics.DrawString(" / " + text.Value, UI.Font(7F), new SolidBrush(Color.FromArgb(175, foreColor)), textRect2, new StringFormat { Alignment = StringAlignment.Far, Trimming = StringTrimming.EllipsisCharacter });
		}
	}
}
