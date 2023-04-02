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

    protected override void UIChanged()
    {
        Font = UI.Font(9.75F);
        Margin = UI.Scale(new Padding(5), UI.FontScale);
        Padding = UI.Scale(new Padding(5), UI.FontScale);
    }

    protected override void OnSizeChanged(EventArgs e)
    {
        base.OnSizeChanged(e);

        Height = (int)(42 * UI.UIScale);
    }

	protected override bool SearchMatch(string searchText, ReportSeverityFilter item)
	{
        var text = item == ReportSeverityFilter.Any ? Locale.AnyReportStatus : LocaleHelper.GetGlobalText($"CR_{item}");

		return searchText.SearchCheck(text);
	}

	protected override void PaintItem(PaintEventArgs e, Rectangle rectangle, Color foreColor, HoverState hoverState, ReportSeverityFilter item)
    {
        var text = item == ReportSeverityFilter.Any ? Locale.AnyReportStatus : LocaleHelper.GetGlobalText($"CR_{item}");
        var color = item switch
        {
            ReportSeverityFilter.MinorIssues => FormDesign.Design.YellowColor,
            ReportSeverityFilter.MajorIssues => FormDesign.Design.YellowColor.MergeColor(FormDesign.Design.RedColor),
            ReportSeverityFilter.Unsubscribe => FormDesign.Design.RedColor,
            ReportSeverityFilter.Remarks or ReportSeverityFilter.Any => FormDesign.Design.ForeColor,
            _ => FormDesign.Design.GreenColor
        };

        using var icon = item == ReportSeverityFilter.Any ? ImageManager.GetIcon("I_Slash") : ((ReportSeverity)((int)item - 1)).GetSeverityIcon(true);

        e.Graphics.DrawImage(icon.Color(color), rectangle.Align(icon.Size, ContentAlignment.MiddleLeft));

        var textSize = (int)e.Graphics.Measure(text, Font).Height;
        var textRect = new Rectangle(rectangle.X + icon.Width + Padding.Left, rectangle.Y + (rectangle.Height - textSize) / 2, 0, textSize);

        textRect.Width = rectangle.Width - textRect.X;

        e.Graphics.DrawString(text, Font, new SolidBrush(foreColor), textRect, new StringFormat { LineAlignment = StringAlignment.Center, Trimming = StringTrimming.EllipsisCharacter });
    }
}
