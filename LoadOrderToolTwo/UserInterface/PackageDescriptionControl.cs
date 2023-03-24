using Extensions;

using LoadOrderToolTwo.Domain;
using LoadOrderToolTwo.Utilities;
using LoadOrderToolTwo.Utilities.Managers;

using SlickControls;

using System.Drawing;
using System.Windows.Forms;

using static CompatibilityReport.CatalogData.Enums;

namespace LoadOrderToolTwo.UserInterface;
internal class PackageDescriptionControl : SlickImageControl
{
	public Package? Package { get; private set; }

	public void SetPackage(Package package)
	{
		Package = package;

		if (!string.IsNullOrWhiteSpace(Package.Author?.AvatarUrl))
		{
			LoadImage(Package.Author?.AvatarUrl, ImageManager.GetImage);
		}
	}

	protected override void UIChanged()
	{
		Padding = UI.Scale(new Padding(4), UI.FontScale);
	}

	protected override void OnPaint(PaintEventArgs e)
	{
		e.Graphics.Clear(FormDesign.Design.BackColor);
		e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
		e.Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

		if (Package == null)
		{
			return;
		}

		var versionRect = Package.Mod == null ? new Rectangle(-Padding.Left, 0, 0, 0) : DrawLabel(e, Package.BuiltIn ? Locale.Vanilla : "v" + Package.Mod?.Version.GetString(), null, FormDesign.Design.YellowColor.MergeColor(FormDesign.Design.BackColor, 40), new Rectangle(Padding.Left, Padding.Top, (int)(100 * UI.FontScale), e.ClipRectangle.Height), ContentAlignment.TopLeft);
		var timeRect = DrawLabel(e, Package.LocalTime.ToLocalTime().ToString("g"), Properties.Resources.I_UpdateTime, FormDesign.Design.AccentColor.MergeColor(FormDesign.Design.BackColor, 75), new Rectangle(versionRect.Right + Padding.Left, Padding.Top, (int)(100 * UI.FontScale), e.ClipRectangle.Height), ContentAlignment.TopLeft);

		GetStatusDescriptors(Package, out var text, out var icon, out var color);
		var statusRect = string.IsNullOrEmpty(text) ? timeRect : DrawLabel(e, text, icon, color.MergeColor(FormDesign.Design.BackColor, 60), new Rectangle(timeRect.Right + Padding.Left, Padding.Top, (int)(100 * UI.FontScale), e.ClipRectangle.Height), ContentAlignment.TopLeft);

		if (Package.Workshop)
		{
			DrawLabel(e, Package.SteamId.ToString(), Properties.Resources.I_Steam_16, FormDesign.Design.AccentColor.MergeColor(FormDesign.Design.ActiveColor, 75).MergeColor(FormDesign.Design.BackColor, 40), new Rectangle(0, Padding.Top, e.ClipRectangle.Width, e.ClipRectangle.Height), ContentAlignment.TopRight);
		}

		var report = Package.CompatibilityReport;
		if (report is not null)
		{
			DrawLabel(e, LocaleHelper.GetGlobalText($"CR_{report.Severity}"), Properties.Resources.I_CompatibilityReport_16, (report.Severity switch
			{
				ReportSeverity.MinorIssues => FormDesign.Design.YellowColor,
				ReportSeverity.MajorIssues => FormDesign.Design.YellowColor.MergeColor(FormDesign.Design.RedColor),
				ReportSeverity.Unsubscribe => FormDesign.Design.RedColor,
				ReportSeverity.Remarks => FormDesign.Design.ForeColor,
				_ => FormDesign.Design.GreenColor
			}).MergeColor(FormDesign.Design.BackColor, 60), new Rectangle(statusRect.Right + Padding.Left, Padding.Top, (int)(100 * UI.FontScale), e.ClipRectangle.Height), ContentAlignment.TopLeft);
		}

		if (Package.Author is not null)
		{
			var size = e.Graphics.Measure("by " + Package.Author.Name, UI.Font(9.75F)).ToSize();
			var authorRect = ClientRectangle.Pad(Padding).Align(new Size(size.Width + Padding.Horizontal + Padding.Right + size.Height, size.Height + Padding.Vertical), ContentAlignment.BottomLeft);
			authorRect.X += Padding.Left;
			var avatarRect = authorRect.Pad(Padding).Align(new(size.Height, size.Height), ContentAlignment.MiddleLeft);

			e.Graphics.FillRoundedRectangle(new SolidBrush(FormDesign.Design.BackColor), authorRect, (int)(6 * UI.FontScale));

			e.Graphics.DrawString("by " + Package.Author.Name, UI.Font(9.75F), new SolidBrush(FormDesign.Design.ForeColor), authorRect.Pad(avatarRect.Right - Padding.Left, 0, 0, 0), new StringFormat { LineAlignment = StringAlignment.Center });

			if (Loading)
			{
				DrawLoader(e.Graphics, avatarRect);
			}
			else if (Image != null)
			{
				e.Graphics.DrawRoundImage(Image, avatarRect);
			}
		}
	}

	private Rectangle DrawLabel(PaintEventArgs e, string? text, Bitmap? icon, Color color, Rectangle rectangle, ContentAlignment alignment)
	{
		if (text == null)
		{
			return Rectangle.Empty;
		}

		var size = e.Graphics.Measure(text, UI.Font(8.25F)).ToSize();

		if (icon is not null)
		{
			size.Width += icon.Width + Padding.Left;
		}

		size.Width += Padding.Left;

		rectangle = rectangle.Pad(Padding).Align(size, alignment);

		using var backBrush = rectangle.Gradient(color);
		using var foreBrush = new SolidBrush(color.GetTextColor());

		e.Graphics.FillRoundedRectangle(backBrush, rectangle, (int)(3 * UI.FontScale));
		e.Graphics.DrawString(text, UI.Font(8.25F), foreBrush, icon is null ? rectangle : rectangle.Pad(icon.Width + (Padding.Left * 2) - 3, 0, 0, 0), new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center });

		if (icon is not null)
		{
			e.Graphics.DrawImage(icon.Color(color.GetTextColor()), rectangle.Pad(Padding.Left, 0, 0, 0).Align(icon.Size, ContentAlignment.MiddleLeft));
		}

		return rectangle;
	}

	private void GetStatusDescriptors(Package package, out string text, out Bitmap? icon, out Color color)
	{
		if (!package.Workshop && !package.BuiltIn)
		{
			text = Locale.Local;
			icon = Properties.Resources.I_Local_16;
			color = FormDesign.Design.YellowColor;
			return;
		}

		switch (package.Status)
		{
			case DownloadStatus.OK:
				text = Locale.UpToDate;
				icon = Properties.Resources.I_Ok_16;
				color = FormDesign.Design.GreenColor;
				return;
			case DownloadStatus.Unknown:
				text = Locale.StatusUnknown;
				icon = Properties.Resources.I_Question_16;
				color = FormDesign.Design.YellowColor;
				return;
			case DownloadStatus.OutOfDate:
				text = Locale.OutOfDate;
				icon = Properties.Resources.I_OutOfDate_16;
				color = FormDesign.Design.YellowColor;
				return;
			case DownloadStatus.NotDownloaded:
				text = Locale.ModIsNotDownloaded;
				icon = Properties.Resources.I_Question_16;
				color = FormDesign.Design.RedColor;
				return;
			case DownloadStatus.PartiallyDownloaded:
				text = Locale.PartiallyDownloaded;
				icon = Properties.Resources.I_Broken_16;
				color = FormDesign.Design.RedColor;
				return;
			case DownloadStatus.Removed:
				text = Locale.ModIsRemoved;
				icon = Properties.Resources.I_ContentRemoved_16;
				color = FormDesign.Design.RedColor;
				return;
		}

		text = string.Empty;
		icon = null;
		color = Color.White;
	}
}
