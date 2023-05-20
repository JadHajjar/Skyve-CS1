using Extensions;

using SkyveApp.Domain.Compatibility;
using SkyveApp.Utilities;
using SkyveApp.Utilities.Managers;

using SlickControls;

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace SkyveApp.UserInterface.Dropdowns;

public enum CompatibilityNotificationFilter // second hex is the id group of the notification, 0 means no notification | first hex is an id
{
	Any = -2,
	AnyIssue = -1,
	None,
	Info,
	MissingDependency,
	Caution,
	Warning,
	AttentionRequired,
	Exclude,
	Unsubscribe,
	Switch,
}

internal class ReportSeverityDropDown : SlickSelectionDropDown<CompatibilityNotificationFilter>
{
	protected override void OnHandleCreated(EventArgs e)
	{
		base.OnHandleCreated(e);

		if (Live)
		{
			Items = Enum.GetValues(typeof(CompatibilityNotificationFilter)).Cast<CompatibilityNotificationFilter>().ToArray();

			selectedItem = CompatibilityNotificationFilter.Any;
		}
	}

	protected override IEnumerable<DrawableItem<CompatibilityNotificationFilter>> OrderItems(IEnumerable<DrawableItem<CompatibilityNotificationFilter>> items)
	{
		return items.OrderBy(x => (int)x.Item);
	}

	protected override bool SearchMatch(string searchText, CompatibilityNotificationFilter item)
	{
		var text = item == CompatibilityNotificationFilter.Any ? Locale.AnyCompatibilityStatus : LocaleCR.Get($"{item}");

		return searchText.SearchCheck(text);
	}

	protected override void PaintItem(PaintEventArgs e, Rectangle rectangle, Color foreColor, HoverState hoverState, CompatibilityNotificationFilter item)
	{
		var text = item switch { CompatibilityNotificationFilter.Any => Locale.AnyCompatibilityStatus, CompatibilityNotificationFilter.AnyIssue => Locale.AnyIssue, _ => LocaleCR.Get($"{item}") };
		var color = item switch { CompatibilityNotificationFilter.Any => foreColor, CompatibilityNotificationFilter.AnyIssue => FormDesign.Design.RedColor, _ => ((NotificationType)(int)item).GetColor() };
		using var icon = (item switch { CompatibilityNotificationFilter.Any => new DynamicIcon("I_Slash"), CompatibilityNotificationFilter.AnyIssue => new DynamicIcon("I_Warning"), _ => ((NotificationType)(int)item).GetIcon(true) }).Get(rectangle.Height - 2).Color(color);

		e.Graphics.DrawImage(icon, rectangle.Align(icon.Size, ContentAlignment.MiddleLeft));

		e.Graphics.DrawString(text, Font, new SolidBrush(foreColor), rectangle.Pad(icon.Width + Padding.Left, 0, 0, 0).AlignToFontSize(Font, ContentAlignment.MiddleLeft, e.Graphics), new StringFormat { Trimming = StringTrimming.EllipsisCharacter });
	}
}
