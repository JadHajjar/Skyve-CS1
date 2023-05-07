using Extensions;

using LoadOrderToolTwo.Domain.Compatibility;
using LoadOrderToolTwo.Utilities;
using LoadOrderToolTwo.Utilities.Managers;

using SlickControls;

using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace LoadOrderToolTwo.UserInterface.Dropdowns;

public enum CompatibilityNotificationFilter // second hex is the id group of the notification, 0 means no notification | first hex is an id
{
	Any = 0x00,
	AnyIssue = 0x20,
	Info = 0x10,
	MissingDependency = 0x01,
	Caution = 0x11,
	Warning = 0x21,
	AttentionRequired = 0x02,
	Unsubscribe = 0x12,
	Switch = 0x22,
}

internal class ReportSeverityDropDown : SlickSelectionDropDown<CompatibilityNotificationFilter>
{
	protected override void OnHandleCreated(EventArgs e)
	{
		base.OnHandleCreated(e);

		if (Live)
		{
			Items = Enum.GetValues(typeof(CompatibilityNotificationFilter)).Cast<CompatibilityNotificationFilter>().ToArray();
		}
	}

	protected override bool SearchMatch(string searchText, CompatibilityNotificationFilter item)
	{
		var text = item == CompatibilityNotificationFilter.Any ? Locale.AnyReportStatus : LocaleHelper.GetGlobalText($"CR_{item}");

		return searchText.SearchCheck(text);
	}

	protected override void PaintItem(PaintEventArgs e, Rectangle rectangle, Color foreColor, HoverState hoverState, CompatibilityNotificationFilter item)
	{
		var text = item switch { CompatibilityNotificationFilter.Any => Locale.AnyReportStatus, CompatibilityNotificationFilter.AnyIssue => Locale.AnyIssues, _ => LocaleHelper.GetGlobalText($"CR_{item}") };
		var color = ((NotificationType)(int)item).GetColor();

		using var icon = (item switch { CompatibilityNotificationFilter.Any => new DynamicIcon("I_Stability"), CompatibilityNotificationFilter.AnyIssue => new DynamicIcon("I_Warning"), _ => ((NotificationType)(int)item).GetIcon(true) }).Get(rectangle.Height - 2).Color(color);

		e.Graphics.DrawImage(icon, rectangle.Align(icon.Size, ContentAlignment.MiddleLeft));

		e.Graphics.DrawString(text, Font, new SolidBrush(foreColor), rectangle.Pad(icon.Width + Padding.Left, 0, 0, 0).AlignToFontSize(Font, ContentAlignment.MiddleLeft, e.Graphics), new StringFormat { Trimming = StringTrimming.EllipsisCharacter });
	}
}
