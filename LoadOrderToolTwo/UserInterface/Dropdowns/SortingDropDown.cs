using Extensions;

using LoadOrderToolTwo.Domain.Enums;
using LoadOrderToolTwo.Utilities.Managers;

using SlickControls;

using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace LoadOrderToolTwo.UserInterface.Dropdowns;
internal class SortingDropDown : SlickSelectionDropDown<PackageSorting>
{
	protected override void OnHandleCreated(EventArgs e)
	{
		base.OnHandleCreated(e);

		if (Live)
		{
			Items = Enum.GetValues(typeof(PackageSorting)).Cast<PackageSorting>().Where(x => x != PackageSorting.Mod).ToArray();
			selectedItem = CentralManager.SessionSettings.UserSettings.PackageSorting;
		}
	}

	protected override void UIChanged()
	{
		Font = UI.Font(9.75F);
		Margin = UI.Scale(new Padding(5), UI.FontScale);
		Padding = UI.Scale(new Padding(5), UI.FontScale);
	}

	protected override bool SearchMatch(string searchText, PackageSorting item)
	{
		return searchText.SearchCheck(LocaleHelper.GetGlobalText($"Sorting_{item}"));
	}

	public override void ResetValue()
	{

	}

	protected override void PaintItem(PaintEventArgs e, Rectangle rectangle, Color foreColor, HoverState hoverState, PackageSorting item)
	{
		var text = LocaleHelper.GetGlobalText($"Sorting_{item}");
		var color = FormDesign.Design.ForeColor;

		using var icon = ImageManager.GetIcon(nameof(Properties.Resources.I_Sort)).Color(foreColor);

		e.Graphics.DrawImage(icon, rectangle.Align(icon.Size, ContentAlignment.MiddleLeft));

		var textSize = (int)e.Graphics.Measure(text, Font).Height;
		var textRect = new Rectangle(rectangle.X + icon.Width + Padding.Left, rectangle.Y + ((rectangle.Height - textSize) / 2), 0, textSize);

		textRect.Width = rectangle.Width - textRect.X;

		e.Graphics.DrawString(text, Font, new SolidBrush(foreColor), textRect, new StringFormat { LineAlignment = StringAlignment.Center, Trimming = StringTrimming.EllipsisCharacter });
	}
}
