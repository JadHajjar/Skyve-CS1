using Extensions;

using LoadOrderToolTwo.Utilities;
using LoadOrderToolTwo.Utilities.Managers;

using SlickControls;
using SlickControls.Controls.Form;

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using static CompatibilityReport.CatalogData.Enums;

namespace LoadOrderToolTwo.UserInterface;
internal class LanguageDropDown : SlickSelectionDropDown<CultureInfo>
{
	protected override void UIChanged()
	{
		Font = UI.Font("Segoe UI", 9.75F);
		Margin = UI.Scale(new Padding(5), UI.FontScale);
		Padding = UI.Scale(new Padding(5), UI.FontScale);
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

	protected override void PaintItem(PaintEventArgs e, Rectangle rectangle, Color foreColor, HoverState hoverState, CultureInfo item)
	{
		if (item == null)
			return;

		var text = $"{item.EnglishName} / {item.NativeName.ToCapital()}";
		using var icon = (Bitmap)Properties.Resources.ResourceManager.GetObject("Lang_" + item.IetfLanguageTag.ToUpper(), Properties.Resources.Culture);

		if (icon != null)
		{
			e.Graphics.DrawImage(icon, rectangle.Align(icon.Size, ContentAlignment.MiddleLeft));

			var textRect = new Rectangle(rectangle.X + icon.Width + Padding.Left, rectangle.Y + (rectangle.Height - Font.Height) / 2, 0, Font.Height);

			textRect.Width = rectangle.Width - textRect.X;

			e.Graphics.DrawString(text, Font, new SolidBrush(foreColor), textRect, new StringFormat { LineAlignment = StringAlignment.Center, Trimming = StringTrimming.EllipsisCharacter });
		}
	}
}
