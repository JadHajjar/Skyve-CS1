using Extensions;
using SkyveApp.Domain.Compatibility.Enums;
using SkyveApp.Utilities;

using SlickControls;

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;


namespace SkyveApp.UserInterface.Dropdowns;

internal class PackageUsageDropDown : SlickMultiSelectionDropDown<PackageUsage>
{
	protected override void OnHandleCreated(EventArgs e)
	{
		base.OnHandleCreated(e);

		if (Live)
		{
			Items = Enum.GetValues(typeof(PackageUsage)).Cast<PackageUsage>().ToArray();
			SelectedItems = Items;
		}
	}

	public override void ResetValue()
	{
		SelectedItems = Items;
	}

	protected override void UIChanged()
	{
		base.UIChanged();

		Width = (int)(200 * UI.FontScale);
	}

	protected override bool SearchMatch(string searchText, PackageUsage item)
	{
		var text = LocaleCR.Get($"{item}");

		return searchText.SearchCheck(text);
	}

	protected override void PaintItem(PaintEventArgs e, Rectangle rectangle, Color foreColor, HoverState hoverState, PackageUsage item, bool selected)
	{
		var text = LocaleCR.Get($"{item}");

		using var icon = IconManager.GetIcon("I_City", rectangle.Height - 2).Color(foreColor);

		e.Graphics.DrawImage(icon, rectangle.Align(icon.Size, ContentAlignment.MiddleLeft));

		e.Graphics.DrawString(text, Font, new SolidBrush(foreColor), rectangle.Pad(icon.Width + Padding.Left, 0, 0, 0).AlignToFontSize(Font, ContentAlignment.MiddleLeft, e.Graphics), new StringFormat { Trimming = StringTrimming.EllipsisCharacter });

	}

	protected override void PaintSelectedItems(PaintEventArgs e, Rectangle rectangle, Color foreColor, HoverState hoverState, IEnumerable<PackageUsage> items)
	{
		var text = !items.Any() ? "Invalid" : items.Count() == Items.Length ? "All" : items.ListStrings(x => LocaleCR.Get($"{x}"), ", ");

		using var icon = IconManager.GetIcon("I_City", rectangle.Height - 2).Color(foreColor);

		e.Graphics.DrawImage(icon, rectangle.Align(icon.Size, ContentAlignment.MiddleLeft));

		e.Graphics.DrawString(text, Font, new SolidBrush(foreColor), rectangle.Pad(icon.Width + Padding.Left, 0, 0, 0).AlignToFontSize(Font, ContentAlignment.MiddleLeft, e.Graphics), new StringFormat { Trimming = StringTrimming.EllipsisCharacter });
	}
}
