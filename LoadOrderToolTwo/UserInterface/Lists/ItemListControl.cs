using Extensions;

using LoadOrderToolTwo.Domain;
using LoadOrderToolTwo.Domain.Enums;
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

namespace LoadOrderToolTwo.UserInterface.Lists;
internal class ItemListControl<T> : SlickStackedListControl<T> where T : IPackage
{
	private PackageSorting sorting;
	private readonly Dictionary<DrawableItem<T>, Rectangles> _itemRects = new();

	public event Action<ReportSeverity>? CompatibilityReportSelected;
	public event Action<DownloadStatus>? DownloadStatusSelected;
	public event Action<DateTime>? DateSelected;
	public event Action<string>? TagSelected;

	public ItemListControl()
	{
		DoubleSizeOnHover = CentralManager.SessionSettings.UserSettings.LargeItemOnHover;
		HighlightOnHover = true;
		SeparateWithLines = true;

		if (!CentralManager.IsContentLoaded)
		{
			Loading = true;

			CentralManager.ContentLoaded += () => Loading = false;
		}

		sorting = CentralManager.SessionSettings.UserSettings.PackageSorting;
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
		if (sorting == packageSorting)
		{
			return;
		}

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

			PackageSorting.SubscribeTime => items
				.OrderByDescending(x => x.Item.SubscribeTime),

			PackageSorting.Mod => items
				.OrderByDescending(x => System.IO.Path.GetFileName(x.Item.Package.Mod?.FileName ?? string.Empty)),

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
		var rects = _itemRects.TryGet(item);

		if (item.Item.Package.Mod is not null)
		{
			if (rects.IncludedRect.Contains(location))
			{
				setTip(Locale.ExcludeInclude, rects.IncludedRect);
			}

			if (rects.EnabledRect.Contains(location))
			{
				setTip(Locale.EnableDisable, rects.EnabledRect);
			}

			if (rects.VersionRect.Contains(location))
			{
				setTip(Locale.CopyVersionNumber, rects.VersionRect);
			}
		}
		else
		{
			if (rects.IncludedRect.Contains(location))
			{
				setTip(Locale.ExcludeInclude, rects.IncludedRect);
			}
		}

		if (rects.CenterRect.Contains(location) || rects.IconRect.Contains(location))
		{
			setTip(Locale.OpenPackagePage, rects.CenterRect);
		}

		if (rects.FolderRect.Contains(location))
		{
			setTip(Locale.OpenLocalFolder, rects.FolderRect);
		}

		if (rects.CompatibilityRect.Contains(location))
		{
			setTip(Locale.FilterByThisReportStatus, rects.CompatibilityRect);
		}

		if (rects.DownloadStatusRect.Contains(location))
		{
			setTip(Locale.FilterByThisPackageStatus, rects.DownloadStatusRect);
		}

		if (rects.DateRect.Contains(location))
		{
			var date = item.Item.ServerTime.If(DateTime.MinValue, item.Item.LocalTime).ToLocalTime();
			setTip(Locale.FilterSinceThisDate, rects.DateRect);
		}

		foreach (var tag in rects.TagRects)
		{
			if (tag.Value.Contains(location))
			{
				setTip(string.Format(Locale.FilterByThisTag, tag.Key), tag.Value);
			}
		}

		if (item.Item.Workshop)
		{
			if (rects.SteamRect.Contains(location))
			{
				setTip(Locale.ViewOnSteam, rects.SteamRect);
			}

			if (rects.SteamIdRect.Contains(location))
			{
				setTip(Locale.CopySteamId, rects.SteamIdRect);
			}

			if (rects.AuthorRect.Contains(location))
			{
				setTip(Locale.OpenAuthorPage, rects.AuthorRect);
			}
		}

		void setTip(string text, Rectangle rectangle) => SlickTip.SetTo(this, text, timeout: 20000, offset: new Point(rectangle.X, item.Bounds.Y));

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

		var rects = _itemRects.TryGet(item);

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

			if (rects.VersionRect.Contains(e.Location))
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

		if (rects.CenterRect.Contains(e.Location) || rects.IconRect.Contains(e.Location))
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

		if (rects.CompatibilityRect.Contains(e.Location))
		{
			CompatibilityReportSelected?.Invoke(item.Item.Package.CompatibilityReport?.Severity ?? ReportSeverity.NothingToReport);
			return;
		}

		if (rects.DownloadStatusRect.Contains(e.Location))
		{
			DownloadStatusSelected?.Invoke(item.Item.Status);
			return;
		}

		if (rects.DateRect.Contains(e.Location))
		{
			var date = item.Item.ServerTime.If(DateTime.MinValue, item.Item.LocalTime).ToLocalTime();
			DateSelected?.Invoke(date);
			return;
		}

		foreach (var tag in rects.TagRects)
		{
			if (tag.Value.Contains(e.Location))
			{
				TagSelected?.Invoke(tag.Key);
				return;
			}
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
		var isPackageIncluded = item.Package.IsIncluded;
		var items = new[]
		{
			  new SlickStripItem(Locale.IncludeAllItemsInThisPackage, () => { item.Package.IsIncluded = true; Invalidate(); }, Properties.Resources.I_Ok_16, !isPackageIncluded)
			, new SlickStripItem(Locale.ExcludeAllItemsInThisPackage, () => { item.Package.IsIncluded = false; Invalidate(); }, Properties.Resources.I_Cancel_16, isPackageIncluded)
			, new SlickStripItem(Locale.ReDownloadPackage, () => Redownload(item), Properties.Resources.I_ReDownload_16, SteamUtil.IsSteamAvailable())
			, new SlickStripItem(Locale.MovePackageToLocalFolder, () => ContentUtil.MoveToLocalFolder(item), Properties.Resources.I_Local_16, item.Workshop)
			, new SlickStripItem(Locale.DeletePackage, () => ContentUtil.DeleteAll(item.Folder), Properties.Resources.I_Steam_16, !item.Workshop && !item.BuiltIn)
			, new SlickStripItem(Locale.UnsubscribePackage, async () => await CitiesManager.Subscribe(new[] { item.SteamId }, true), Properties.Resources.I_Steam_16, item.Workshop && !item.BuiltIn)
			, new SlickStripItem(Locale.Copy, () => { }, Properties.Resources.I_Copy_16, item.Workshop, fade: true)
			, new SlickStripItem(Locale.CopyWorkshopLink, () => Clipboard.SetText(item.SteamPage), null, item.Workshop, tab: 1)
			, new SlickStripItem(Locale.CopyWorkshopId, () => Clipboard.SetText(item.SteamId.ToString()), null, item.Workshop, tab: 1)
			, new SlickStripItem(Locale.CopyAuthorLink, () => Clipboard.SetText($"{item.Author?.ProfileUrl}myworkshopfiles"), null, !string.IsNullOrWhiteSpace(item.Author?.ProfileUrl), tab: 1)
			, new SlickStripItem(Locale.CopyAuthorId, () => Clipboard.SetText(item.Author?.ProfileUrl.Split(new[] { '\\', '/' }, StringSplitOptions.RemoveEmptyEntries).Last()), null, !string.IsNullOrWhiteSpace(item.Author?.ProfileUrl), tab: 1)
			, new SlickStripItem(Locale.CopyAuthorSteamId, () => Clipboard.SetText(item.Author?.SteamId), null, !string.IsNullOrWhiteSpace(item.Author?.SteamId), tab: 1)
		};

		this.TryBeginInvoke(() => SlickToolStrip.Show(Program.MainForm, items));
	}

	private void Redownload(T item)
	{
		SteamUtil.ReDownload(item);
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
		{
			if (item is Asset asset)
			{
				Process.Start("explorer.exe", $"/select, \"{asset.FileName}\"");
			}
			else
			{
				Process.Start(item.Folder);
			}
		}
		catch { }
	}

	protected override void OnPaint(PaintEventArgs e)
	{
		try
		{
			if (Loading)
			{
				base.OnPaint(e);
			}
			else if (!Items.Any())
			{
				e.Graphics.DrawString(Locale.NoLocalPackagesFound + "\r\n" + Locale.CheckFolderInOptions, UI.Font(9.75F, FontStyle.Italic), new SolidBrush(ForeColor), ClientRectangle, new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center });
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
		catch { }
	}

	protected override void OnPaintItem(ItemPaintEventArgs<T> e)
	{
		var large = DoubleSizeOnHover && (e.HoverState.HasFlag(HoverState.Hovered) || e.HoverState.HasFlag(HoverState.Pressed));
		var rects = _itemRects[e.DrawableItem] = GetActionRectangles(e.ClipRectangle, e.Item, large);
		var inclEnableRect = (rects.EnabledRect == Rectangle.Empty ? rects.IncludedRect : Rectangle.Union(rects.IncludedRect, rects.EnabledRect)).Pad(0, Padding.Vertical, 0, Padding.Vertical);
		var isPressed = e.HoverState.HasFlag(HoverState.Pressed);
		var labelRect = new Rectangle(rects.TextRect.X, e.ClipRectangle.Y, 0, e.ClipRectangle.Height);

		if (isPressed && !rects.CenterRect.Contains(CursorLocation) && !rects.IconRect.Contains(CursorLocation))
		{
			e.HoverState &= ~HoverState.Pressed;
		}

		base.OnPaintItem(e);

		var isIncluded = e.Item.IsIncluded;

		PaintIncludedButton(e, rects, inclEnableRect, isIncluded);

		DrawThumbnailAndTitle(e, rects, large);

		var isVersion = e.Item.Package.Mod is not null && !e.Item.Package.Mod.BuiltIn;
		var versionText = isVersion ? "v" + e.Item.Package.Mod!.Version.GetString() : e.Item.Package.Mod?.BuiltIn ?? false ? Locale.Vanilla : e.Item.FileSize.SizeString();
		rects.VersionRect = DrawLabel(e, versionText, null, isVersion ? FormDesign.Design.YellowColor : FormDesign.Design.YellowColor.MergeColor(FormDesign.Design.BackColor, 40), labelRect, ContentAlignment.BottomLeft, isVersion);
		labelRect.X += Padding.Left + rects.VersionRect.Width;

		var date = e.Item.ServerTime.If(DateTime.MinValue, e.Item.LocalTime).ToLocalTime();
		var dateText = CentralManager.SessionSettings.UserSettings.ShowDatesRelatively ? date.ToRelatedString(true, false) : date.ToString("g");
		rects.DateRect = DrawLabel(e, dateText, Properties.Resources.I_UpdateTime, FormDesign.Design.AccentColor.MergeColor(FormDesign.Design.BackColor, 50), labelRect, ContentAlignment.BottomLeft, true);
		labelRect.X += Padding.Left + rects.DateRect.Width;

		GetStatusDescriptors(e.Item, out var text, out var icon, out var color);
		if (!string.IsNullOrEmpty(text))
		{
			rects.DownloadStatusRect = DrawLabel(e, text, icon, color.MergeColor(FormDesign.Design.BackColor, 65), labelRect, ContentAlignment.BottomLeft, true);

			labelRect.X += Padding.Left + rects.DownloadStatusRect.Width;
		}
		else
		{
			rects.DownloadStatusRect = Rectangle.Empty;
		}

		DrawAuthorAndSteamId(e, large, rects);

		var report = e.Item.Package.CompatibilityReport;
		if (report is not null && report.Severity != ReportSeverity.NothingToReport)
		{
			var labelColor = report.Severity switch
			{
				ReportSeverity.MinorIssues => FormDesign.Design.YellowColor,
				ReportSeverity.MajorIssues => FormDesign.Design.YellowColor.MergeColor(FormDesign.Design.RedColor),
				ReportSeverity.Unsubscribe => FormDesign.Design.RedColor,
				ReportSeverity.Remarks => FormDesign.Design.ButtonColor,
				_ => FormDesign.Design.GreenColor.MergeColor(FormDesign.Design.AccentColor, 20)
			};

			rects.CompatibilityRect = DrawLabel(e, LocaleHelper.GetGlobalText($"CR_{report.Severity}"), Properties.Resources.I_CompatibilityReport_16, labelColor, labelRect, ContentAlignment.BottomLeft, true);

			labelRect.X += Padding.Left + rects.CompatibilityRect.Width;
		}
		else
		{
			rects.CompatibilityRect = Rectangle.Empty;
		}

		foreach (var item in e.Item.Tags.Distinct(x => x.Value))
		{
			using var tagIcon = item.Source switch { TagSource.Workshop => Properties.Resources.I_Steam_16, TagSource.FindIt => Properties.Resources.I_Search_16, _ => Properties.Resources.I_Tag_16 };

			var tagRect = DrawLabel(e, item.Value, tagIcon, FormDesign.Design.ButtonColor, labelRect, ContentAlignment.BottomLeft, true);

			rects.TagRects[item.Value] = tagRect;

			labelRect.X += Padding.Left + tagRect.Width;
		}

		DrawButtons(e, rects, isPressed);

		if (!isIncluded) // fade excluded item
		{
			var filledRect = e.ClipRectangle.Pad(0, -Padding.Top, 0, -Padding.Bottom);
			e.Graphics.SetClip(filledRect);
			e.Graphics.FillRectangle(new SolidBrush(Color.FromArgb(e.HoverState.HasFlag(HoverState.Hovered) ? 25 : 75, BackColor)), filledRect);
		}
	}

	private void DrawButtons(ItemPaintEventArgs<T> e, ItemListControl<T>.Rectangles rects, bool isPressed)
	{
		SlickButton.DrawButton(e, rects.FolderRect, string.Empty, Font, ImageManager.GetIcon(nameof(Properties.Resources.I_Folder)), null, rects.FolderRect.Contains(CursorLocation) ? e.HoverState | (isPressed ? HoverState.Pressed : 0) : HoverState.Normal);

		if (e.Item.Workshop)
		{
			SlickButton.DrawButton(e, rects.SteamRect, string.Empty, Font, ImageManager.GetIcon(nameof(Properties.Resources.I_Steam)), null, rects.SteamRect.Contains(CursorLocation) ? e.HoverState | (isPressed ? HoverState.Pressed : 0) : HoverState.Normal);
		}
	}

	private void DrawAuthorAndSteamId(ItemPaintEventArgs<T> e, bool large, ItemListControl<T>.Rectangles rects)
	{
		if (!e.Item.Workshop)
		{
			rects.AuthorRect = Rectangle.Empty;
			return;
		}

		if (large && e.Item.Author is not null)
		{
			var size = e.Graphics.Measure("by " + e.Item.Author.Name, UI.Font(9.75F)).ToSize();
			var authorRect = rects.AuthorRect.Align(new Size(size.Width + Padding.Horizontal + Padding.Right + size.Height, size.Height + (Padding.Vertical * 2)), ContentAlignment.MiddleRight);
			authorRect.X -= Padding.Left;
			authorRect.Y += Padding.Top;
			var avatarRect = authorRect.Pad(Padding).Align(new(size.Height, size.Height), ContentAlignment.MiddleLeft);

			e.Graphics.FillRoundedRectangle(new SolidBrush(authorRect.Contains(CursorLocation) ? FormDesign.Design.ButtonColor : FormDesign.Design.BackColor), authorRect, (int)(6 * UI.FontScale));

			e.Graphics.DrawString("by " + e.Item.Author.Name, UI.Font(9.75F), new SolidBrush(FormDesign.Design.ForeColor), authorRect.Pad(avatarRect.Width + Padding.Horizontal, 0, 0, 0), new StringFormat { LineAlignment = StringAlignment.Center });

			var authorImg = e.Item.AuthorIconImage;

			if (authorImg is null)
			{
				using var authorIcon = Properties.Resources.I_AuthorIcon.Color(FormDesign.Design.IconColor);

				e.Graphics.DrawRoundImage(authorIcon, avatarRect);
			}
			else
			{
				e.Graphics.DrawRoundImage(authorImg, avatarRect);
			}

			rects.AuthorRect = authorRect;
		}
		else
		{
			rects.AuthorRect = DrawLabel(e, e.Item.Author?.Name, Properties.Resources.I_Developer_16, FormDesign.Design.ActiveColor.MergeColor(FormDesign.Design.ButtonColor, 30), rects.AuthorRect, ContentAlignment.TopLeft, true);
		}

		rects.SteamIdRect = DrawLabel(e, e.Item.SteamId.ToString(), Properties.Resources.I_Steam_16, FormDesign.Design.ActiveColor.MergeColor(FormDesign.Design.ButtonColor, 30), rects.SteamIdRect, ContentAlignment.BottomLeft, true);
	}

	private void DrawThumbnailAndTitle(ItemPaintEventArgs<T> e, ItemListControl<T>.Rectangles rects, bool large)
	{
		var iconRectangle = rects.IconRect;

		var iconImg = e.Item.IconImage;

		if (iconImg is null)
		{
			using var generic = (e.Item is Package ? Properties.Resources.I_CollectionIcon : e.Item is Asset ? Properties.Resources.I_AssetIcon : Properties.Resources.I_ModIcon).Color(FormDesign.Design.IconColor);

			e.Graphics.DrawRoundedImage(generic, iconRectangle, (int)(4 * UI.FontScale), FormDesign.Design.AccentBackColor);
		}
		else
		{
			try
			{ e.Graphics.DrawRoundedImage(iconImg, iconRectangle, (int)(4 * UI.FontScale), FormDesign.Design.AccentBackColor); }
			catch { }
		}

		List<string>? tags = null;

		var mod = e.Item.Package.Mod is not null;
		var text = mod ? e.Item.ToString().RemoveVersionText(out tags) : e.Item.ToString();
		var textSize = e.Graphics.Measure(text, UI.Font(large ? 11.25F : 9F, FontStyle.Bold));

		e.Graphics.DrawString(text, UI.Font(large ? 11.25F : 9F, FontStyle.Bold), new SolidBrush(e.HoverState.HasFlag(HoverState.Pressed) ? FormDesign.Design.ActiveForeColor : (rects.CenterRect.Contains(CursorLocation) || rects.IconRect.Contains(CursorLocation)) && e.HoverState.HasFlag(HoverState.Hovered) ? FormDesign.Design.ActiveColor : ForeColor), rects.TextRect, new StringFormat { Trimming = StringTrimming.EllipsisCharacter });

		if (tags is null)
		{
			return;
		}

		var tagRect = new Rectangle(rects.TextRect.X + (int)textSize.Width, rects.TextRect.Y, 1, (int)textSize.Height);

		foreach (var item in tags)
		{
			var color = item.ToLower() switch
			{
				"stable" => Color.FromArgb(180, FormDesign.Design.GreenColor),
				"alpha" or "experimental" => Color.FromArgb(200, FormDesign.Design.YellowColor.MergeColor(FormDesign.Design.RedColor)),
				"beta" => Color.FromArgb(180, FormDesign.Design.YellowColor),
				"deprecated" => Color.FromArgb(225, FormDesign.Design.RedColor),
				_ => (Color?)null
			};

			tagRect.X += Padding.Left + DrawLabel(e, color is null ? item : item.ToUpper(), null, color ?? FormDesign.Design.ButtonColor, tagRect, ContentAlignment.MiddleLeft, smaller: true).Width;
		}
	}

	private void PaintIncludedButton(ItemPaintEventArgs<T> e, ItemListControl<T>.Rectangles rects, Rectangle inclEnableRect, bool isIncluded)
	{
		if (CentralManager.SessionSettings.UserSettings.AdvancedIncludeEnable && e.Item.Package.Mod is Mod mod)
		{
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
	}

	private Rectangle DrawLabel(ItemPaintEventArgs<T> e, string? text, Bitmap? icon, Color color, Rectangle rectangle, ContentAlignment alignment, bool action = false, bool smaller = false)
	{
		if (text == null)
		{
			return Rectangle.Empty;
		}

		var large = DoubleSizeOnHover && (e.HoverState.HasFlag(HoverState.Hovered) || e.HoverState.HasFlag(HoverState.Pressed));
		using var font = UI.Font((large ? 9F : 7.5F) - (smaller ? 0.5F : 0F));
		var size = e.Graphics.Measure(text, font).ToSize();

		if (icon is not null)
		{
			size.Width += icon.Width + Padding.Left;
		}

		size.Width += Padding.Left;

		rectangle = rectangle.Pad(smaller ? Padding.Left/2:Padding.Left).Align(size, alignment);

		if (action && !rectangle.Contains(CursorLocation))
		{
			color = color.MergeColor(FormDesign.Design.BackColor, 50);
		}

		using var backBrush = rectangle.Gradient(color);
		using var foreBrush = new SolidBrush(color.GetTextColor());

		e.Graphics.FillRoundedRectangle(backBrush, rectangle, (int)(3 * UI.FontScale));
		e.Graphics.DrawString(text, font, foreBrush, icon is null ? rectangle : rectangle.Pad(icon.Width + (Padding.Left * 2) - 2, 0, 0, 0), new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center });

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
				break;
			//text = Locale.UpToDate;
			//icon = Properties.Resources.I_Ok_16;
			//color = FormDesign.Design.GreenColor.MergeColor(FormDesign.Design.AccentColor, 20);
			//return;
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
		var rects = new Rectangles() { Item = item };

		if (CentralManager.SessionSettings.UserSettings.AdvancedIncludeEnable && item.Package.Mod is not null)
		{
			rects.IncludedRect = rectangle.Pad(1 * Padding.Left, 0, 0, 0).Align(new Size(ItemHeight * 9 / 10, rectangle.Height), ContentAlignment.MiddleLeft);
			rects.EnabledRect = rects.IncludedRect.Pad(rects.IncludedRect.Width, 0, -rects.IncludedRect.Width, 0);
		}
		else if (item is Package)
		{
			rects.IncludedRect = rectangle.Pad(1 * Padding.Left, 0, 0, 0).Align(new Size(CentralManager.SessionSettings.UserSettings.AdvancedIncludeEnable ? (int)(26 * UI.FontScale * 2) : ItemHeight + 1, rectangle.Height), ContentAlignment.MiddleLeft);
		}
		else
		{
			rects.IncludedRect = rectangle.Pad(1 * Padding.Left, 0, 0, 0).Align(new Size(ItemHeight + 1, rectangle.Height), ContentAlignment.MiddleLeft);
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

			rects.CenterRect = new Rectangle(rects.IconRect.X - 1, rectangle.Y, rects.SteamIdRect.X - rects.IconRect.X, rectangle.Height / 2);
		}
		else
		{
			rects.CenterRect = new Rectangle(rects.IconRect.X - 1, rectangle.Y, buttonRectangle.X - rects.IconRect.X, rectangle.Height / 2);
		}

		return rects;
	}

	private class Rectangles
	{
		internal T? Item;

		internal Dictionary<string, Rectangle> TagRects = new();
		internal Rectangle IncludedRect;
		internal Rectangle EnabledRect;
		internal Rectangle FolderRect;
		internal Rectangle IconRect;
		internal Rectangle TextRect;
		internal Rectangle SteamRect;
		internal Rectangle SteamIdRect;
		internal Rectangle CenterRect;
		internal Rectangle AuthorRect;
		internal Rectangle VersionRect;
		internal Rectangle CompatibilityRect;
		internal Rectangle DownloadStatusRect;
		internal Rectangle DateRect;

		internal bool Contain(Point location)
		{
			return
				IncludedRect.Contains(location) ||
				EnabledRect.Contains(location) ||
				FolderRect.Contains(location) ||
				CenterRect.Contains(location) ||
				SteamRect.Contains(location) ||
				AuthorRect.Contains(location) ||
				IconRect.Contains(location) ||
				DownloadStatusRect.Contains(location) ||
				CompatibilityRect.Contains(location) ||
				DateRect.Contains(location) ||
				(VersionRect.Contains(location) && Item?.Package.Mod is not null) ||
				TagRects.Any(x => x.Value.Contains(location)) ||
				SteamIdRect.Contains(location);
		}
	}
}
