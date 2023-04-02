using Extensions;
using LoadOrderToolTwo.Domain.Enums;
using LoadOrderToolTwo.Utilities;
using LoadOrderToolTwo.Utilities.Managers;

using SlickControls;

using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace LoadOrderToolTwo.UserInterface.Dropdowns;
internal class PackageStatusDropDown : SlickSelectionDropDown<DownloadStatusFilter>
{
    protected override void OnHandleCreated(EventArgs e)
    {
        base.OnHandleCreated(e);

        if (Live)
        {
            Items = Enum.GetValues(typeof(DownloadStatusFilter)).Cast<DownloadStatusFilter>().ToArray();
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

	protected override bool SearchMatch(string searchText, DownloadStatusFilter item)
	{
        GetStatusDescriptors(item, out var text, out _, out _);

		return searchText.SearchCheck(text);
	}

	protected override void PaintItem(PaintEventArgs e, Rectangle rectangle, Color foreColor, HoverState hoverState, DownloadStatusFilter item)
    {
        GetStatusDescriptors(item, out var text, out var icon, out var color);

        using (icon.Color(hoverState.HasFlag(HoverState.Pressed) ? foreColor : color))
        {
            e.Graphics.DrawImage(icon, rectangle.Align(icon.Size, ContentAlignment.MiddleLeft));

        var textSize = (int)e.Graphics.Measure(text, Font).Height;
            var textRect = new Rectangle(rectangle.X + icon.Width + Padding.Left, rectangle.Y + (rectangle.Height - textSize) / 2, 0, textSize);

            textRect.Width = rectangle.Width - textRect.X;

            e.Graphics.DrawString(text, Font, new SolidBrush(foreColor), textRect, new StringFormat { LineAlignment = StringAlignment.Center, Trimming = StringTrimming.EllipsisCharacter });
        }
    }

    private void GetStatusDescriptors(DownloadStatusFilter status, out string text, out Bitmap icon, out Color color)
    {
        switch (status)
        {
            case DownloadStatusFilter.Any:
                text = Locale.AnyStatus;
                icon = ImageManager.GetIcon("I_Slash");
                color = FormDesign.Design.ForeColor;
                return;

            case DownloadStatusFilter.OK:
                text = Locale.UpToDate;
                icon = ImageManager.GetIcon("I_Ok");
                color = FormDesign.Design.GreenColor;
                return;
            case DownloadStatusFilter.Unknown:
                text = Locale.StatusUnknown;
                icon = ImageManager.GetIcon("I_Question");
                color = FormDesign.Design.YellowColor;
                return;
            case DownloadStatusFilter.OutOfDate:
                text = Locale.OutOfDate;
                icon = ImageManager.GetIcon("I_OutOfDate");
                color = FormDesign.Design.YellowColor;
                return;
            case DownloadStatusFilter.NotDownloaded:
                text = Locale.ModIsNotDownloaded;
                icon = ImageManager.GetIcon("I_Question");
                color = FormDesign.Design.RedColor;
                return;
            case DownloadStatusFilter.PartiallyDownloaded:
                text = Locale.PartiallyDownloaded;
                icon = ImageManager.GetIcon("I_Broken");
                color = FormDesign.Design.RedColor;
                return;
            case DownloadStatusFilter.Removed:
                text = Locale.ModIsRemoved;
                icon = ImageManager.GetIcon("I_ContentRemoved");
                color = FormDesign.Design.RedColor;
                return;
        }

        text = Locale.Local;
        icon = ImageManager.GetIcon("I_Local");
        color = FormDesign.Design.YellowColor;
    }
}
