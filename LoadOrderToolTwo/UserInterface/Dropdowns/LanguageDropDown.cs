using Extensions;

using SlickControls;

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;

using static LoadOrderToolTwo.UserInterface.Generic.ThreeOptionToggle;

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
		Font = UI.Font("Segoe UI", 7.5F);
		Padding = UI.Scale(new Padding(3), UI.FontScale);
	}

	protected override void OnSizeChanged(EventArgs e)
	{
		base.OnSizeChanged(e);

		Height = (int)(42 * UI.UIScale);
	}

	protected override IEnumerable<DrawableItem<CultureInfo>> OrderItems(IEnumerable<DrawableItem<CultureInfo>> items)
	{
		return items.OrderByDescending(x => x.Item.IetfLanguageTag == "en").ThenBy(x => x.Item.EnglishName);
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

			var textHeight = (int)e.Graphics.Measure(" ", Font).Height + 1;
			var text1 = (item.Parent.EnglishName, Regex.Match(item.EnglishName, @"\((.+?)\)").Groups[1].Value);
			var text2 = (item.Parent.NativeName, Regex.Match(item.NativeName, @"\((.+?)\)").Groups[1].Value);
			var textRect = rectangle.Pad( iconSize + Padding.Horizontal, -Padding.Top, Padding.Right, -Padding.Bottom);
			var textRect1 = textRect.Align(new Size(textRect.Width, textHeight), ContentAlignment.TopLeft);
			var textRect2 = textRect.Align(new Size(textRect.Width, textHeight), ContentAlignment.BottomLeft);

			textRect.Width = rectangle.Width - textRect.X;

			e.Graphics.DrawString(text1.EnglishName, Font, new SolidBrush(foreColor), textRect1, new StringFormat { Trimming = StringTrimming.EllipsisCharacter });
			e.Graphics.DrawString(text2.NativeName, Font, new SolidBrush(foreColor), textRect2, new StringFormat { Trimming = StringTrimming.EllipsisCharacter });

			e.Graphics.DrawString(" / " + text1.Value, Font, new SolidBrush(Color.FromArgb(175,foreColor)), textRect1, new StringFormat { Alignment = StringAlignment.Far, Trimming = StringTrimming.EllipsisCharacter });
			e.Graphics.DrawString(" / " + text2.Value, Font, new SolidBrush(Color.FromArgb(175,foreColor)), textRect2, new StringFormat { Alignment = StringAlignment.Far, Trimming = StringTrimming.EllipsisCharacter });
		}
	}
}
