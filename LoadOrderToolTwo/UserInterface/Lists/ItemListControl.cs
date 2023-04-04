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
	private readonly Dictionary<DrawableItem<T>, Rectangles> _itemRects=new ();

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

		SlickToolStrip.Show(Program.MainForm
			, new SlickStripItem(Locale.IncludeAllItemsInThisPackage, () => { item.Package.IsIncluded = true; Invalidate(); }, Properties.Resources.I_Ok_16, !isPackageIncluded)
			, new SlickStripItem(Locale.ExcludeAllItemsInThisPackage, () => { item.Package.IsIncluded = false; Invalidate(); }, Properties.Resources.I_Cancel_16, isPackageIncluded)
			, new SlickStripItem(Locale.ReDownloadPackage, () => Redownload(item), Properties.Resources.I_ReDownload_16)
			, new SlickStripItem(Locale.MovePackageToLocalFolder, () => ContentUtil.MoveToLocalFolder(item), Properties.Resources.I_Local_16, item.Workshop)
			, new SlickStripItem(Locale.DeletePackage, () => ContentUtil.DeleteAll(item.Folder), Properties.Resources.I_Steam_16, !item.Workshop && !item.BuiltIn)
			, new SlickStripItem(Locale.UnsubscribePackage, async () => await CitiesManager.Subscribe(new[] { item.SteamId }, true), Properties.Resources.I_Steam_16, item.Workshop && !item.BuiltIn)
			, new SlickStripItem(Locale.Copy, () => { }, Properties.Resources.I_Copy_16, item.Workshop, fade: true)
			, new SlickStripItem(Locale.CopyWorkshopLink, () => Clipboard.SetText(item.SteamPage), null, item.Workshop, tab: 1)
			, new SlickStripItem(Locale.CopyWorkshopId, () => Clipboard.SetText(item.SteamId.ToString()), null, item.Workshop, tab: 1)
			, new SlickStripItem(Locale.CopyAuthorLink, () => Clipboard.SetText($"{item.Author?.ProfileUrl}myworkshopfiles"), null, !string.IsNullOrWhiteSpace(item.Author?.ProfileUrl), tab: 1)
			, new SlickStripItem(Locale.CopyAuthorId, () => Clipboard.SetText(item.Author?.ProfileUrl.Split(new[] { '\\', '/' }, StringSplitOptions.RemoveEmptyEntries).Last()), null, !string.IsNullOrWhiteSpace(item.Author?.ProfileUrl), tab: 1)
			, new SlickStripItem(Locale.CopyAuthorSteamId, () => Clipboard.SetText(item.Author?.SteamId), null, !string.IsNullOrWhiteSpace(item.Author?.SteamId), tab: 1)
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

		if (isPressed && !rects.CenterRect.Contains(CursorLocation) && !rects.IconRect.Contains(CursorLocation))
		{
			e.HoverState &= ~HoverState.Pressed;
		}

		base.OnPaintItem(e);

		bool isIncluded;

		if (CentralManager.SessionSettings.UserSettings.AdvancedIncludeEnable && e.Item.Package.Mod is Mod mod)
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

		var iconImg = e.Item.IconImage;

		if (iconImg is null)
		{
			using var generic = (e.Item is Package ? Properties.Resources.I_CollectionIcon : e.Item is Asset ? Properties.Resources.I_AssetIcon : Properties.Resources.I_ModIcon).Color(FormDesign.Design.IconColor);

			e.Graphics.DrawRoundedImage(generic, iconRectangle, (int)(4 * UI.FontScale), FormDesign.Design.AccentBackColor);
		}
		else
		{
			try { e.Graphics.DrawRoundedImage(iconImg, iconRectangle, (int)(4 * UI.FontScale), FormDesign.Design.AccentBackColor); }
			catch { }
		}

		e.Graphics.DrawString(e.Item.ToString().RemoveVersionText(), UI.Font(large ? 11.25F : 9F, FontStyle.Bold), new SolidBrush(e.HoverState.HasFlag(HoverState.Pressed) ? FormDesign.Design.ActiveForeColor : (rects.CenterRect.Contains(CursorLocation)||rects.IconRect.Contains(CursorLocation)) && e.HoverState.HasFlag(HoverState.Hovered) ? FormDesign.Design.ActiveColor : ForeColor), textRect, new StringFormat { Trimming = StringTrimming.EllipsisCharacter });

		var labelX = textRect.X;
		var versionText = e.Item.Package.Mod is Mod mod_ ? mod_.BuiltIn ? Locale.Vanilla : "v" + mod_.Version.GetString() : e.Item.FileSize.SizeString();
		labelX = (rects.VersionRect = DrawLabel(e, versionText, null, FormDesign.Design.YellowColor, new Rectangle(labelX, e.ClipRectangle.Y, (int)(100 * UI.FontScale), e.ClipRectangle.Height), ContentAlignment.BottomLeft, e.Item.Package.Mod is not null)).Right;

		labelX = DrawLabel(e, CentralManager.SessionSettings.UserSettings.ShowDatesRelatively ? e.Item.ServerTime.If(DateTime.MinValue, e.Item.LocalTime).ToLocalTime().ToRelatedString(true, false) : e.Item.LocalTime.ToLocalTime().ToString("g"), Properties.Resources.I_UpdateTime, FormDesign.Design.AccentColor.MergeColor(FormDesign.Design.BackColor, 50), new Rectangle(labelX + Padding.Left, e.ClipRectangle.Y, (int)(100 * UI.FontScale), e.ClipRectangle.Height), ContentAlignment.BottomLeft).Right;

		GetStatusDescriptors(e.Item, out var text, out var icon, out var color);
		if (!string.IsNullOrEmpty(text))
		{
			labelX = DrawLabel(e, text, icon, color.MergeColor(FormDesign.Design.BackColor, 65), new Rectangle(labelX + Padding.Left, e.ClipRectangle.Y, (int)(100 * UI.FontScale), e.ClipRectangle.Height), ContentAlignment.BottomLeft).Right;
		}

		if (e.Item.Workshop)
		{
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
				rects.AuthorRect = Rectangle.Empty;
				DrawLabel(e, e.Item.Author?.Name, Properties.Resources.I_Developer_16, rects.AuthorRect.Contains(CursorLocation) ? FormDesign.Design.ActiveColor : FormDesign.Design.AccentColor.MergeColor(FormDesign.Design.ActiveColor, 75).MergeColor(FormDesign.Design.BackColor, 40), rects.AuthorRect, ContentAlignment.TopLeft);
			}

			DrawLabel(e, e.Item.SteamId.ToString(), Properties.Resources.I_Steam_16, rects.SteamIdRect.Contains(CursorLocation) ? FormDesign.Design.ActiveColor : FormDesign.Design.AccentColor.MergeColor(FormDesign.Design.ActiveColor, 75).MergeColor(FormDesign.Design.BackColor, 40), rects.SteamIdRect, ContentAlignment.BottomLeft);
		}
		else
			rects.AuthorRect = Rectangle.Empty;

		var report = e.Item.Package.CompatibilityReport;
		if (report is not null && report.Severity != ReportSeverity.NothingToReport)
		{
			labelX = DrawLabel(e, LocaleHelper.GetGlobalText($"CR_{report.Severity}"), Properties.Resources.I_CompatibilityReport_16, (report.Severity switch
			{
				ReportSeverity.MinorIssues => FormDesign.Design.YellowColor,
				ReportSeverity.MajorIssues => FormDesign.Design.YellowColor.MergeColor(FormDesign.Design.RedColor),
				ReportSeverity.Unsubscribe => FormDesign.Design.RedColor,
				ReportSeverity.Remarks => FormDesign.Design.ButtonColor,
				_ => FormDesign.Design.GreenColor.MergeColor(FormDesign.Design.AccentColor, 20)
			}).MergeColor(FormDesign.Design.BackColor, 65), new Rectangle(labelX + Padding.Left, e.ClipRectangle.Y, (int)(100 * UI.FontScale), e.ClipRectangle.Height), ContentAlignment.BottomLeft).Right;
		}

		var tags = new List<string>();

		if (e.Item.Tags is not null)
		{
			tags.AddRange(e.Item.Tags);
		}

		if (e.Item is Asset asset)
		{
			tags.AddRange(asset.AssetTags);
		}

		foreach (var item in tags.Distinct().OrderBy(x => x))
		{
			labelX = DrawLabel(e, item, null, FormDesign.Design.ButtonColor, new Rectangle(labelX + Padding.Left, e.ClipRectangle.Y, (int)(100 * UI.FontScale), e.ClipRectangle.Height), ContentAlignment.BottomLeft).Right;
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

	private Rectangle DrawLabel(ItemPaintEventArgs<T> e, string? text, Bitmap? icon, Color color, Rectangle rectangle, ContentAlignment alignment, bool action = false)
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

		if (action && !rectangle.Contains(CursorLocation))
		{
			color = color.MergeColor(FormDesign.Design.BackColor, 40);
		}

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

	class Rectangles
	{
		internal T? Item;

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
				(VersionRect.Contains(location) && Item?.Package.Mod is not null) ||
				SteamIdRect.Contains(location);
		}
	}
}
