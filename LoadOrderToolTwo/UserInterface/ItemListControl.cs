using Extensions;

using LoadOrderToolTwo.Domain;
using LoadOrderToolTwo.Domain.Interfaces;
using LoadOrderToolTwo.UserInterface.Panels;
using LoadOrderToolTwo.Utilities;
using LoadOrderToolTwo.Utilities.Managers;

using SlickControls;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

using static CompatibilityReport.CatalogData.Enums;

namespace LoadOrderToolTwo.UserInterface;
internal class ItemListControl<T> : SlickStackedListControl<T> where T : IPackage
{
	private PackageSorting sorting;
	private Rectangle versionRect;

	public ItemListControl()
	{
		DoubleSizeOnHover = CentralManager.SessionSettings.LargeItemOnHover;
		HighlightOnHover = true;
		SeparateWithLines = true;

		if (!CentralManager.IsContentLoaded)
		{
			Loading = true;

			CentralManager.ContentLoaded += () => Loading = false;
		}
	}

	public IEnumerable<T> FilteredItems => SafeGetItems().Select(x => x.Item);

	public int FilteredCount => SafeGetItems().Count;

	protected override void UIChanged()
	{
		ItemHeight = 36;

		base.UIChanged();

		Padding = UI.Scale(new Padding(3, 2, 3, 2), UI.FontScale);
	}

	public void SetSorting(PackageSorting packageSorting)
	{
		sorting = packageSorting;

		FilterOrSortingChanged();
	}

	protected override IEnumerable<DrawableItem<T>> OrderItems(IEnumerable<DrawableItem<T>> items)
	{
		return sorting switch
		{
			PackageSorting.FileSize => items
				.OrderByDescending(x => x.Item.FileSize),

			PackageSorting.Name => items
				.OrderBy(x => x.Item.ToString()),

			PackageSorting.Author => items
				.OrderBy(x => x.Item.Author?.Name ?? string.Empty),

			PackageSorting.Status => items
				.OrderByDescending(x => x.Item.Status),

			PackageSorting.UpdateTime => items
				.OrderByDescending(x => x.Item.ServerTime.If(DateTime.MinValue, x.Item.LocalTime)),

			PackageSorting.CompatibilityReport => items
				.OrderByDescending(x => x.Item.Package.CompatibilityReport?.Severity ?? default),

			_ => items
				.OrderByDescending(x => x.Item.IsIncluded)
				.ThenByDescending(x => x.Item.Workshop)
				.ThenBy(x => x.Item.ToString())
		};
	}

	protected override bool IsItemActionHovered(DrawableItem<T> item, Point location)
	{
		var rects = GetActionRectangles(item.Bounds, item.Item, true);
		
		if (item.Item.Package.Mod is Mod)
		{
			if (rects.IncludedRect.Contains(location))
			{
				setTip(Locale.ExcludeInclude);
			}

			if (rects.EnabledRect.Contains(location))
			{
				setTip(Locale.EnableDisable);
			}
		}
		else
		{
			if (rects.IncludedRect.Contains(location))
			{
				setTip(Locale.ExcludeInclude);
			}
		}

		if (rects.CenterRect.Contains(location))
		{
			setTip(Locale.OpenPackagePage);
		}

		if (rects.FolderRect.Contains(location))
		{
			setTip(Locale.OpenLocalFolder);
		}

		if (item.Item.Workshop)
		{
			if (rects.SteamRect.Contains(location))
			{
				setTip(Locale.ViewOnSteam);
			}

			if (rects.SteamIdRect.Contains(location))
			{
				setTip(Locale.CopySteamId);
			}

			if (rects.AuthorRect.Contains(location))
			{
				setTip(Locale.OpenAuthorPage);
			}
		}

		void setTip(string text) => SlickTip.SetTo(this, text, timeout: 20000);

		return rects.Contain(location);
	}

	protected override void OnItemMouseClick(DrawableItem<T> item, MouseEventArgs e)
	{
		base.OnItemMouseClick(item, e);

		if (e.Button == MouseButtons.Right)
		{
			ShowRightClickMenu(item.Item);
			return;
		}

		if (e.Button != MouseButtons.Left)
		{
			return;
		}

		var rects = GetActionRectangles(item.Bounds, item.Item, true);

		if (item.Item.Package.Mod is Mod mod)
		{
			if (rects.IncludedRect.Contains(e.Location))
			{
				mod.IsIncluded = !mod.IsIncluded;
				return;
			}

			if (rects.EnabledRect.Contains(e.Location))
			{
				mod.IsEnabled = !mod.IsEnabled;
				return;
			}

			if (versionRect.Contains(e.Location	))
			{
				Clipboard.SetText(item.Item.Package.Mod.Version.GetString());
			}
		}
		else
		{
			if (rects.IncludedRect.Contains(e.Location))
			{
				item.Item.IsIncluded = !item.Item.IsIncluded;
				return;
			}
		}

		if (rects.CenterRect.Contains(e.Location))
		{
			(FindForm() as BasePanelForm)?.PushPanel(null, new PC_PackagePage(item.Item.Package));
			ScrollTo(item.Item);
			return;
		}

		if (rects.FolderRect.Contains(e.Location))
		{
			OpenFolder(item.Item);
			return;
		}

		if (item.Item.Workshop)
		{
			if (rects.SteamRect.Contains(e.Location))
			{
				OpenSteamLink(item.Item.SteamPage);
				return;
			}

			if (rects.SteamIdRect.Contains(e.Location))
			{
				Clipboard.SetText(item.Item.SteamId.ToString());
				return;
			}

			if (rects.AuthorRect.Contains(e.Location))
			{
				OpenSteamLink($"{item.Item.Author?.ProfileUrl}myworkshopfiles");
				return;
			}
		}
	}

	private void ShowRightClickMenu(T item)
	{
		SlickToolStrip.Show(Program.MainForm
			, new SlickStripItem(Locale.ReDownloadPackage, () => Redownload(item), Properties.Resources.I_ReDownload_16)
			, new SlickStripItem(Locale.DeletePackage, () => ContentUtil.Delete(item.Folder), Properties.Resources.I_Steam_16, !item.Workshop && !item.BuiltIn)
			, new SlickStripItem(Locale.UnsubscribePackage, async () => await CitiesManager.Subscribe(new[] { item.SteamId }, true), Properties.Resources.I_Steam_16, item.Workshop && !item.BuiltIn)
			, new SlickStripItem(Locale.Copy, () => { }, Properties.Resources.I_Copy_16, item.Workshop, fade: true)
			, new SlickStripItem(Locale.CopyWorkshopLink, () => Clipboard.SetText(item.SteamPage), null, item.Workshop, tab: 1)
			, new SlickStripItem(Locale.CopyWorkshopId, () => Clipboard.SetText(item.SteamId.ToString()), null, item.Workshop, tab: 1)
			, new SlickStripItem(Locale.CopyAuthorLink, () => Clipboard.SetText($"{item.Author?.ProfileUrl}myworkshopfiles"), null, !string.IsNullOrWhiteSpace(item.Author?.ProfileUrl), tab: 1)
			, new SlickStripItem(Locale.CopyAuthorId, () => Clipboard.SetText(item.Author?.ProfileUrl.Split(new[] { '\\', '/' }, StringSplitOptions.RemoveEmptyEntries).Last()), null, !string.IsNullOrWhiteSpace(item.Author?.ProfileUrl), tab: 1)
			);
	}

	private void Redownload(T item)
	{
		SteamUtil.ReDownload(item.SteamId);
	}

	private void OpenSteamLink(string? url)
	{
		try
		{ Process.Start(url); }
		catch { }
	}

	private void OpenFolder(T item)
	{
		try
		{ Process.Start(item.Folder); }
		catch { }
	}

	protected override void OnPaint(PaintEventArgs e)
	{
		if (Loading)
		{
			base.OnPaint(e);
		}
		else if (!Items.Any())
		{
			e.Graphics.DrawString(Locale.NoLocalPackagesFound +"\r\n" + Locale.CheckFolderInOptions, UI.Font(9.75F, FontStyle.Italic), new SolidBrush(ForeColor), ClientRectangle, new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center });
		}
		else if (!SafeGetItems().Any())
		{
			e.Graphics.DrawString(Locale.NoPackagesMatchFilters, UI.Font(9.75F, FontStyle.Italic), new SolidBrush(ForeColor), ClientRectangle, new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center });
		}
		else
		{
			base.OnPaint(e);
		}
	}

	protected override void OnPaintItem(ItemPaintEventArgs<T> e)
	{
		var large = DoubleSizeOnHover && (e.HoverState.HasFlag(HoverState.Hovered) || e.HoverState.HasFlag(HoverState.Pressed));
		var rects = GetActionRectangles(e.ClipRectangle, e.Item, large);
		var inclEnableRect = (rects.EnabledRect == Rectangle.Empty ? rects.IncludedRect : Rectangle.Union(rects.IncludedRect, rects.EnabledRect)).Pad(0, Padding.Vertical, 0, Padding.Vertical);
		var isPressed = e.HoverState.HasFlag(HoverState.Pressed);

		if (isPressed && !rects.CenterRect.Contains(CursorLocation))
		{
			e.HoverState &= ~HoverState.Pressed;
		}

		base.OnPaintItem(e);

		bool isIncluded;

		if (CentralManager.SessionSettings.AdvancedIncludeEnable && e.Item.Package.Mod is Mod mod)
		{
			isIncluded = e.Item.IsIncluded;

			if (isIncluded)
			{
				e.Graphics.FillRoundedRectangle(inclEnableRect.Gradient(Color.FromArgb(inclEnableRect.Contains(CursorLocation) ? 150 : 255, mod.IsEnabled ? FormDesign.Design.GreenColor : FormDesign.Design.RedColor), 1.5F), inclEnableRect, 4);
			}
			else if (mod.IsEnabled)
			{
				e.Graphics.FillRoundedRectangle(inclEnableRect.Gradient(Color.FromArgb(inclEnableRect.Contains(CursorLocation) ? 150 : 255, FormDesign.Design.YellowColor), 1.5F), inclEnableRect, 4);
			}
			else if (inclEnableRect.Contains(CursorLocation))
			{
				e.Graphics.FillRoundedRectangle(inclEnableRect.Gradient(Color.FromArgb(20, ForeColor), 1.5F), inclEnableRect, 4);
			}

			e.Graphics.DrawImage((isIncluded ? Properties.Resources.I_Ok : Properties.Resources.I_Enabled).Color(rects.IncludedRect.Contains(CursorLocation) ? FormDesign.Design.ActiveColor : isIncluded ? FormDesign.Design.ActiveForeColor : ForeColor), rects.IncludedRect.CenterR(24, 24));
			e.Graphics.DrawImage((mod.IsEnabled ? Properties.Resources.I_Checked : Properties.Resources.I_Unchecked).Color(rects.EnabledRect.Contains(CursorLocation) ? FormDesign.Design.ActiveColor : isIncluded ? FormDesign.Design.ActiveForeColor : base.ForeColor), rects.EnabledRect.CenterR(24, 24));
		}
		else
		{
			isIncluded = e.Item.IsIncluded;

			if (isIncluded)
			{
				e.Graphics.FillRoundedRectangle(inclEnableRect.Gradient(Color.FromArgb(inclEnableRect.Contains(CursorLocation) ? 150 : 255, FormDesign.Design.GreenColor), 1.5F), inclEnableRect, 4);
			}
			else if (inclEnableRect.Contains(CursorLocation))
			{
				e.Graphics.FillRoundedRectangle(inclEnableRect.Gradient(Color.FromArgb(20, ForeColor), 1.5F), inclEnableRect, 4);
			}

			e.Graphics.DrawImage((isIncluded ? Properties.Resources.I_Ok : Properties.Resources.I_Enabled).Color(inclEnableRect.Contains(CursorLocation) ? FormDesign.Design.ActiveColor : isIncluded ? FormDesign.Design.ActiveForeColor : ForeColor), inclEnableRect.CenterR(24, 24));
		}

		var iconRectangle = rects.IconRect;
		var textRect = rects.TextRect;

		using var iconImg = e.Item.IconImage ?? (e.Item is Package ? Properties.Resources.I_CollectionIcon : e.Item is Asset ? Properties.Resources.I_AssetIcon : Properties.Resources.I_ModIcon).Color(FormDesign.Design.IconColor);
		e.Graphics.DrawRoundedImage(iconImg, iconRectangle, (int)(4 * UI.FontScale), FormDesign.Design.AccentBackColor);

		e.Graphics.DrawString(e.Item.ToString().RemoveVersionText(), UI.Font(large ? 11.25F : 9F, FontStyle.Bold), new SolidBrush(e.HoverState.HasFlag(HoverState.Pressed) ? FormDesign.Design.ActiveForeColor : rects.CenterRect.Contains(CursorLocation) && e.HoverState.HasFlag(HoverState.Hovered) ? FormDesign.Design.ActiveColor : ForeColor), textRect, new StringFormat { Trimming = StringTrimming.EllipsisCharacter });

		var versionText = e.Item.Package.Mod is Mod mod_ ? (mod_.BuiltIn ? Locale.Vanilla : "v" + mod_.Version.GetString()) : e.Item.FileSize.SizeString();
		versionRect = DrawLabel(e, versionText, null, FormDesign.Design.YellowColor.MergeColor(FormDesign.Design.BackColor, 40), new Rectangle(textRect.X, e.ClipRectangle.Y, (int)(100 * UI.FontScale), e.ClipRectangle.Height), ContentAlignment.BottomLeft);

		var timeRect = DrawLabel(e, CentralManager.SessionSettings.ShowDatesRelatively ? e.Item.ServerTime.If(DateTime.MinValue, e.Item.LocalTime).ToLocalTime().ToRelatedString(true, false) : e.Item.LocalTime.ToLocalTime().ToString("g"), Properties.Resources.I_UpdateTime, FormDesign.Design.AccentColor.MergeColor(FormDesign.Design.BackColor, 50), new Rectangle(versionRect.Right + Padding.Left, e.ClipRectangle.Y, (int)(100 * UI.FontScale), e.ClipRectangle.Height), ContentAlignment.BottomLeft);

		GetStatusDescriptors(e.Item, out var text, out var icon, out var color);
		var statusRect = string.IsNullOrEmpty(text) ? timeRect : DrawLabel(e, text, icon, color.MergeColor(FormDesign.Design.BackColor, 65), new Rectangle(timeRect.Right + Padding.Left, e.ClipRectangle.Y, (int)(100 * UI.FontScale), e.ClipRectangle.Height), ContentAlignment.BottomLeft);

		if (e.Item.Workshop)
		{
			if (large && e.Item.Author is not null)
			{
				var size = e.Graphics.Measure("by " + e.Item.Author.Name, UI.Font(9.75F)).ToSize();
				var authorRect = rects.AuthorRect.Align(new Size(size.Width + Padding.Horizontal + Padding.Right + size.Height, size.Height + Padding.Vertical*2), ContentAlignment.MiddleRight);
				authorRect.X -= Padding.Left;
				authorRect.Y += Padding.Top;
				var avatarRect = authorRect.Pad(Padding).Align(new(size.Height, size.Height), ContentAlignment.MiddleLeft);

				e.Graphics.FillRoundedRectangle(new SolidBrush(FormDesign.Design.BackColor), authorRect, (int)(6 * UI.FontScale));

				e.Graphics.DrawString("by " + e.Item.Author.Name, UI.Font(9.75F), new SolidBrush(FormDesign.Design.ForeColor), authorRect.Pad(avatarRect.Width + Padding.Horizontal, 0, 0, 0), new StringFormat { LineAlignment = StringAlignment.Center });

				using var authorImg = e.Item.AuthorIconImage ?? Properties.Resources.I_AuthorIcon.Color(FormDesign.Design.IconColor);
				e.Graphics.DrawRoundImage(authorImg, avatarRect);
			}
			else
			{
				DrawLabel(e, e.Item.Author?.Name, Properties.Resources.I_Developer_16, rects.AuthorRect.Contains(CursorLocation) ? FormDesign.Design.ActiveColor : FormDesign.Design.AccentColor.MergeColor(FormDesign.Design.ActiveColor, 75).MergeColor(FormDesign.Design.BackColor, 40), rects.AuthorRect, ContentAlignment.TopLeft);
			}

			DrawLabel(e, e.Item.SteamId.ToString(), Properties.Resources.I_Steam_16, rects.SteamIdRect.Contains(CursorLocation) ? FormDesign.Design.ActiveColor : FormDesign.Design.AccentColor.MergeColor(FormDesign.Design.ActiveColor, 75).MergeColor(FormDesign.Design.BackColor, 40), rects.SteamIdRect, ContentAlignment.BottomLeft);
		}

		var report = e.Item.Package.CompatibilityReport;
		if (report is not null)
		{
			DrawLabel(e, LocaleHelper.GetGlobalText($"CR_{report.Severity}"), Properties.Resources.I_CompatibilityReport_16, (report.Severity switch
			{
				ReportSeverity.MinorIssues => FormDesign.Design.YellowColor,
				ReportSeverity.MajorIssues => FormDesign.Design.YellowColor.MergeColor(FormDesign.Design.RedColor),
				ReportSeverity.Unsubscribe => FormDesign.Design.RedColor,
				ReportSeverity.Remarks => FormDesign.Design.ButtonColor,
				_ => FormDesign.Design.GreenColor.MergeColor(FormDesign.Design.AccentColor, 20)
			}).MergeColor(FormDesign.Design.BackColor, 65), new Rectangle(statusRect.Right + Padding.Left, e.ClipRectangle.Y, (int)(100 * UI.FontScale), e.ClipRectangle.Height), ContentAlignment.BottomLeft);
		}

		SlickButton.DrawButton(e, rects.FolderRect, string.Empty, Font, ImageManager.GetIcon(nameof(Properties.Resources.I_Folder)), null, rects.FolderRect.Contains(CursorLocation) ? e.HoverState | (isPressed ? HoverState.Pressed : 0) : HoverState.Normal);

		if (e.Item.Workshop)
		{
			SlickButton.DrawButton(e, rects.SteamRect, string.Empty, Font, ImageManager.GetIcon(nameof(Properties.Resources.I_Steam)), null, rects.SteamRect.Contains(CursorLocation) ? e.HoverState | (isPressed ? HoverState.Pressed : 0) : HoverState.Normal);
		}

		if (!isIncluded)
		{
			var filledRect = e.ClipRectangle.Pad(0, -Padding.Top, 0, -Padding.Bottom);
			e.Graphics.SetClip(filledRect);
			e.Graphics.FillRectangle(new SolidBrush(Color.FromArgb(e.HoverState.HasFlag(HoverState.Hovered) ? 30 : 85, BackColor)), filledRect);
		}
	}

	private Rectangle DrawLabel(ItemPaintEventArgs<T> e, string? text, Bitmap? icon, Color color, Rectangle rectangle, ContentAlignment alignment)
	{
		if (text == null)
		{
			return Rectangle.Empty;
		}

		var large = DoubleSizeOnHover && (e.HoverState.HasFlag(HoverState.Hovered) || e.HoverState.HasFlag(HoverState.Pressed));
		var size = e.Graphics.Measure(text, UI.Font(large ? 9F : 7.5F)).ToSize();

		if (icon is not null)
		{
			size.Width += icon.Width + Padding.Left;
		}

		size.Width += Padding.Left;

		rectangle = rectangle.Pad(Padding).Align(size, alignment);

		using var backBrush = rectangle.Gradient(color);
		using var foreBrush = new SolidBrush(color.GetTextColor());

		e.Graphics.FillRoundedRectangle(backBrush, rectangle, (int)(3 * UI.FontScale));
		e.Graphics.DrawString(text, UI.Font(large ? 9F : 7.5F), foreBrush, icon is null ? rectangle : rectangle.Pad(icon.Width + (Padding.Left * 2) - 2, 0, 0, 0), new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center });

		if (icon is not null)
		{
			e.Graphics.DrawImage(icon.Color(color.GetTextColor()), rectangle.Pad(Padding.Left, 0, 0, 0).Align(icon.Size, ContentAlignment.MiddleLeft));
		}

		return rectangle;
	}

	private void GetStatusDescriptors(T mod, out string text, out Bitmap? icon, out Color color)
	{
		if (!mod.Workshop)
		{
			text = Locale.Local;
			icon = Properties.Resources.I_Local_16;
			color = FormDesign.Design.YellowColor.MergeColor(FormDesign.Design.AccentColor);
			return;
		}

		switch (mod.Status)
		{
			case DownloadStatus.OK:
				text = Locale.UpToDate;
				icon = Properties.Resources.I_Ok_16;
				color = FormDesign.Design.GreenColor.MergeColor(FormDesign.Design.AccentColor, 20);
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

	private Rectangles GetActionRectangles(Rectangle rectangle, T item, bool doubleSize)
	{
		var rects = new Rectangles();

		if (CentralManager.SessionSettings.AdvancedIncludeEnable && item.Package.Mod is not null)
		{
			rects.IncludedRect = rectangle.Pad(1 * Padding.Left, 0, 0, 0).Align(new Size((int)(26 * UI.FontScale), rectangle.Height), ContentAlignment.MiddleLeft);
			rects.EnabledRect = rects.IncludedRect.Pad(rects.IncludedRect.Width, 0, -rects.IncludedRect.Width, 0);
		}
		else if (item is Package)
		{
			rects.IncludedRect = rectangle.Pad(1 * Padding.Left, 0, 0, 0).Align(new Size(CentralManager.SessionSettings.AdvancedIncludeEnable ? (int)(26 * UI.FontScale * 2) : rectangle.Height, rectangle.Height), ContentAlignment.MiddleLeft);
		}
		else
		{
			rects.IncludedRect = rectangle.Pad(1 * Padding.Left, 0, 0, 0).Align(new Size(rectangle.Height, rectangle.Height), ContentAlignment.MiddleLeft);
		}

		var buttonRectangle = rectangle.Pad(0, 0, Padding.Right, 0).Align(new Size(ItemHeight, ItemHeight), ContentAlignment.TopRight);
		var iconSize = rectangle.Height - Padding.Vertical;

		rects.FolderRect = buttonRectangle;
		rects.IconRect = rectangle.Pad(Math.Max(rects.IncludedRect.Right, rects.EnabledRect.Right) + (2 * Padding.Left)).Align(new Size(iconSize, iconSize), ContentAlignment.MiddleLeft);
		rects.TextRect = rectangle.Pad(rects.IconRect.X + rects.IconRect.Width + Padding.Left, 0, (item.Workshop ? (2 * Padding.Left) + (2 * buttonRectangle.Width) + (int)(100 * UI.FontScale) : 0) + rectangle.Width - buttonRectangle.X, rectangle.Height / 2);

		if (item.Workshop)
		{
			buttonRectangle.X -= Padding.Left + buttonRectangle.Width;
			rects.SteamRect = buttonRectangle;

			if (doubleSize && DoubleSizeOnHover)
			{
				rects.SteamIdRect = new Rectangle(buttonRectangle.X - (int)(100 * UI.FontScale), rectangle.Y + Padding.Top, (int)(100 * UI.FontScale), rectangle.Height / 4);
				rects.AuthorRect = new Rectangle(rectangle.X, rectangle.Y + (rectangle.Height / 2), rectangle.Width, rectangle.Height / 2);
			}
			else
			{
				rects.AuthorRect = new Rectangle(buttonRectangle.X - (int)(100 * UI.FontScale), rectangle.Y + (rectangle.Height / 2), (int)(100 * UI.FontScale), rectangle.Height / 2);
				rects.SteamIdRect = new Rectangle(buttonRectangle.X - (int)(100 * UI.FontScale), rectangle.Y, (int)(100 * UI.FontScale), rectangle.Height / 2);
			}

			rects.CenterRect = new Rectangle(rects.IconRect.X, rectangle.Y, rects.SteamIdRect.X - rects.IconRect.X, rectangle.Height);
		}
		else
		{
			rects.CenterRect = new Rectangle(rects.IconRect.X, rectangle.Y, buttonRectangle.X - rects.IconRect.X, rectangle.Height);
		}

		return rects;
	}

	struct Rectangles
	{
		internal Rectangle IncludedRect;
		internal Rectangle EnabledRect;
		internal Rectangle FolderRect;
		internal Rectangle IconRect;
		internal Rectangle TextRect;
		internal Rectangle SteamRect;
		internal Rectangle SteamIdRect;
		internal Rectangle CenterRect;
		internal Rectangle AuthorRect;

		internal bool Contain(Point location)
		{
			return
				IncludedRect.Contains(location) ||
				EnabledRect.Contains(location) ||
				FolderRect.Contains(location) ||
				CenterRect.Contains(location) ||
				SteamRect.Contains(location) ||
				AuthorRect.Contains(location) ||
				SteamIdRect.Contains(location);
		}
	}
}
