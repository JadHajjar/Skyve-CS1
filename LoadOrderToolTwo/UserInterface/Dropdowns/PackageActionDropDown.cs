using Extensions;

using LoadOrderToolTwo.Domain.Compatibility;
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
internal class PackageActionDropDown : SlickSelectionDropDown<InteractionAction>
{
	protected override void OnHandleCreated(EventArgs e)
	{
		base.OnHandleCreated(e);

		if (Live)
		{
			Items = Enum.GetValues(typeof(InteractionAction)).Cast<InteractionAction>().Where(x => CRNAttribute.GetAttribute(x).Browsable).ToArray();
		}
	}

	protected override void UIChanged()
	{
		base.UIChanged();

		Width = (int)(150 * UI.FontScale);
	}

	protected override bool SearchMatch(string searchText, InteractionAction item)
	{
		var text = LocaleHelper.GetGlobalText($"CR_{item}");

		return searchText.SearchCheck(text);
	}

	protected override void PaintItem(PaintEventArgs e, Rectangle rectangle, Color foreColor, HoverState hoverState, InteractionAction item)
	{
		var text = LocaleHelper.GetGlobalText($"CR_{item}");
		var color = CRNAttribute.GetNotification(item).GetColor();

		using var icon = IconManager.GetIcon("I_Actions", rectangle.Height - 2).Color(color);

		e.Graphics.DrawImage(icon, rectangle.Align(icon.Size, ContentAlignment.MiddleLeft));

		e.Graphics.DrawString(text, Font, new SolidBrush(foreColor), rectangle.Pad(icon.Width + Padding.Left, 0, 0, 0).AlignToFontSize(Font, ContentAlignment.MiddleLeft, e.Graphics), new StringFormat { Trimming = StringTrimming.EllipsisCharacter });
	}
}
