using Extensions;

using LoadOrderToolTwo.Utilities;
using LoadOrderToolTwo.Utilities.Managers;

using SlickControls;

using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

using static CompatibilityReport.CatalogData.Enums;

namespace LoadOrderToolTwo.UserInterface.Dropdowns;
internal class ReportSeverityDropDown : SlickSelectionDropDown<ReportSeverityFilter>
{
	protected override void OnHandleCreated(EventArgs e)
	{
		base.OnHandleCreated(e);

		if (Live)
		{
			Items = Enum.GetValues(typeof(ReportSeverityFilter)).Cast<ReportSeverityFilter>().ToArray();
		}
	}

	protected override bool SearchMatch(string searchText, ReportSeverityFilter item)
	{
		var text = item == ReportSeverityFilter.Any ? Locale.AnyReportStatus : LocaleHelper.GetGlobalText($"CR_{item}");

		return searchText.SearchCheck(text);
	}

	protected override void PaintItem(PaintEventArgs e, Rectangle rectangle, Color foreColor, HoverState hoverState, ReportSeverityFilter item)
	{
		var text = item switch { ReportSeverityFilter.Any => Locale.AnyReportStatus, ReportSeverityFilter.AnyIssue => Locale.AnyIssues, _ => LocaleHelper.GetGlobalText($"CR_{item}") };
		var color = item switch
		{
			ReportSeverityFilter.MinorIssues => FormDesign.Design.YellowColor,
			ReportSeverityFilter.MajorIssues => FormDesign.Design.YellowColor.MergeColor(FormDesign.Design.RedColor),
			ReportSeverityFilter.Unsubscribe or ReportSeverityFilter.AnyIssue => FormDesign.Design.RedColor,
			ReportSeverityFilter.Remarks or ReportSeverityFilter.Any => foreColor,
			_ => FormDesign.Design.GreenColor
		};

		using var icon = (item switch { ReportSeverityFilter.Any => new DynamicIcon("I_Slash"), ReportSeverityFilter.AnyIssue => new DynamicIcon("I_Warning"), _ => ((ReportSeverity)((int)item - 2)).GetSeverityIcon(true) }).Get(rectangle.Height - 2).Color(color);

		e.Graphics.DrawImage(icon, rectangle.Align(icon.Size, ContentAlignment.MiddleLeft));

		e.Graphics.DrawString(text, Font, new SolidBrush(foreColor), rectangle.Pad(icon.Width + Padding.Left, 0, 0, 0).AlignToFontSize(Font, ContentAlignment.MiddleLeft, e.Graphics), new StringFormat { Trimming = StringTrimming.EllipsisCharacter });
	}
}
