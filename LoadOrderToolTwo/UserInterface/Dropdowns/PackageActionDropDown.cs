using Extensions;

using LoadOrderToolTwo.Domain.Compatibility;
using LoadOrderToolTwo.Utilities;
using LoadOrderToolTwo.Utilities.Managers;

using SlickControls;

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LoadOrderToolTwo.UserInterface.Dropdowns;
internal class PackageActionDropDown : SlickSelectionDropDown<StatusAction>
{
	protected override void UIChanged()
	{
		base.UIChanged();

		Width = (int)(175 * UI.FontScale);
	}

	protected override bool SearchMatch(string searchText, StatusAction item)
	{
		var text = LocaleCR.Get($"{item}");

		return searchText.SearchCheck(text);
	}

	protected override void PaintItem(PaintEventArgs e, Rectangle rectangle, Color foreColor, HoverState hoverState, StatusAction item)
	{
		var text = LocaleCR.Get($"{item}");
		var color = CRNAttribute.GetNotification(item).GetColor();

		using var icon = IconManager.GetIcon("I_Actions", rectangle.Height - 2).Color(color);

		e.Graphics.DrawImage(icon, rectangle.Align(icon.Size, ContentAlignment.MiddleLeft));

		e.Graphics.DrawString(text, Font, new SolidBrush(foreColor), rectangle.Pad(icon.Width + Padding.Left, 0, 0, 0).AlignToFontSize(Font, ContentAlignment.MiddleLeft, e.Graphics), new StringFormat { Trimming = StringTrimming.EllipsisCharacter });
	}
}
