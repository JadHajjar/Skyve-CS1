using Extensions;

using LoadOrderToolTwo.Domain.Enums;
using LoadOrderToolTwo.Utilities.Managers;

using SlickControls;

using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace LoadOrderToolTwo.UserInterface.Dropdowns;
internal class ProfileSortingDropDown : SlickSelectionDropDown<ProfileSorting>
{
	protected override void OnHandleCreated(EventArgs e)
	{
		base.OnHandleCreated(e);

		if (Live)
		{
			Items = Enum.GetValues(typeof(ProfileSorting)).Cast<ProfileSorting>().ToArray();

			SelectedItem = CentralManager.SessionSettings.UserSettings.ProfileSorting;
		}
	}

	protected override bool SearchMatch(string searchText, ProfileSorting item)
	{
		return searchText.SearchCheck(LocaleHelper.GetGlobalText($"Sorting_{item}"));
	}

	protected override void PaintItem(PaintEventArgs e, Rectangle rectangle, Color foreColor, HoverState hoverState, ProfileSorting item)
	{
		var text = LocaleHelper.GetGlobalText($"Sorting_{item}");
		var color = FormDesign.Design.ForeColor;

		using var icon = IconManager.GetIcon("I_Sort").Color(foreColor);

		e.Graphics.DrawImage(icon, rectangle.Align(icon.Size, ContentAlignment.MiddleLeft));

		var textSize = (int)e.Graphics.Measure(text, Font).Height;
		var textRect = new Rectangle(rectangle.X + icon.Width + Padding.Left, rectangle.Y + ((rectangle.Height - textSize) / 2), 0, textSize);

		textRect.Width = rectangle.Width - textRect.X;

		e.Graphics.DrawString(text, Font, new SolidBrush(foreColor), textRect, new StringFormat { LineAlignment = StringAlignment.Center, Trimming = StringTrimming.EllipsisCharacter });
	}
}
