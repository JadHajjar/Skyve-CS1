using Extensions;

using SkyveApp.Domain;
using SkyveApp.Domain.Enums;
using SkyveApp.Domain.Interfaces;
using SkyveApp.Domain.Steam;
using SkyveApp.Services;
using SkyveApp.Services.Interfaces;
using SkyveApp.Systems.Compatibility;
using SkyveApp.Systems.Compatibility.Domain.Enums;
using SkyveApp.UserInterface.Forms;
using SkyveApp.UserInterface.Panels;
using SkyveApp.Utilities;
using SkyveApp.Utilities.IO;

using SlickControls;

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Windows.Forms;

using NotificationType = SkyveApp.Domain.Compatibility.Enums.NotificationType;

namespace SkyveApp.UserInterface.Lists;
internal class ItemListControl<T> : SlickStackedListControl<T, ItemListControl<T>.Rectangles> where T : IPackage
{
	private PackageSorting sorting;
	private Rectangle PopupSearchRect1;
	private Rectangle PopupSearchRect2;

	public event Action<NotificationType>? CompatibilityReportSelected;
	public event Action<DownloadStatus>? DownloadStatusSelected;
	public event Action<DateTime>? DateSelected;
	public event Action<TagItem>? TagSelected;
	public event Action<SteamUser>? AuthorSelected;
	public event Action<bool>? FilterByIncluded;
	public event Action<bool>? FilterByEnabled;
	public event Action<string>? AddToSearch;
	public event Action<T>? PackageSelected;
	public event Action? OpenWorkshopSearch;
	public event Action? OpenWorkshopSearchInBrowser;
	public event EventHandler? FilterRequested;

	private readonly ISettings _settings;
	private readonly INotifier _notifier;
	private readonly ICompatibilityManager _compatibilityManager;
	private readonly IModLogicManager _modLogicManager;
	private readonly ISubscriptionsManager _subscriptionsManager;

	public ItemListControl()
	{
		HighlightOnHover = true;
		SeparateWithLines = true;

		_settings = ServiceCenter.Get<ISettings>();
		_notifier = ServiceCenter.Get<INotifier>();
		_compatibilityManager = ServiceCenter.Get<ICompatibilityManager>();
		_modLogicManager = ServiceCenter.Get<IModLogicManager>();
		_subscriptionsManager = ServiceCenter.Get<ISubscriptionsManager>();

		if (!_notifier.IsContentLoaded)
		{
			Loading = true;

			_notifier.ContentLoaded += () => Loading = false;
		}

		sorting = _settings.SessionSettings.UserSettings.PackageSorting;
		SortDesc = _settings.SessionSettings.UserSettings.PackageSortingDesc;
	}

	public IEnumerable<T> FilteredItems => SafeGetItems().Select(x => x.Item);

	public int FilteredCount => SafeGetItems().Count;

	public bool SortDesc { get; private set; }
	public bool PackagePage { get; set; }
	public bool TextSearchNotEmpty { get; set; }
	public bool IsGenericPage { get; set; }
	public bool IsSelection { get; set; }

	protected override void UIChanged()
	{
		ItemHeight = _settings.SessionSettings.UserSettings.LargeItemOnHover ? 64 : 36;

		base.UIChanged();

		Padding = UI.Scale(new Padding(3, 2, 3, 2), UI.FontScale);
	}

	public override void FilterChanged()
	{
		if (!IsHandleCreated)
		{
			base.FilterChanged();
		}
		else
		{
			FilterRequested?.Invoke(this, EventArgs.Empty);
		}
	}

	internal void DoFilterChanged()
	{
		base.FilterChanged();

		AutoInvalidate = !Loading && Items.Any() && !SafeGetItems().Any();
	}

	public void SetSorting(PackageSorting packageSorting, bool desc)
	{
		if (sorting == packageSorting && SortDesc == desc)
		{
			return;
		}

		SortDesc = desc;
		sorting = packageSorting;

		if (!IsHandleCreated)
		{
			SortingChanged();
		}
		else
		{
			new BackgroundAction(() => SortingChanged()).Run();
		}
	}

	protected override IEnumerable<DrawableItem<T, Rectangles>> OrderItems(IEnumerable<DrawableItem<T, Rectangles>> items)
	{
		items = sorting switch
		{
			PackageSorting.FileSize => items
				.OrderBy(x => x.Item.FileSize),

			PackageSorting.Name => items
				.OrderBy(x => x.Item.ToString()),

			PackageSorting.Author => items
				.OrderBy(x => x.Item.Author?.Name ?? string.Empty),

			PackageSorting.Status => items
				.OrderBy(x => x.Item.Package?.Status),

			PackageSorting.UpdateTime => items
				.OrderBy(x => x.Item.ServerTime == DateTime.MinValue && x.Item is Asset asset ? asset.LocalTime : x.Item.ServerTime),

			PackageSorting.SubscribeTime => items
				.OrderBy(x => x.Item.Package?.LocalTime),

			PackageSorting.Mod => items
				.OrderBy(x => Path.GetFileName(x.Item.Package?.Mod?.FileName ?? string.Empty)),

			PackageSorting.None => items,

			PackageSorting.CompatibilityReport => items
				.OrderBy(x => x.Item.GetCompatibilityInfo().Notification),

			PackageSorting.Subscribers => items
				.OrderBy(x => x.Item.Subscriptions),

			PackageSorting.Votes => items
				.OrderBy(x => SteamUtil.GetScore(x.Item)).ThenBy(x => x.Item.PositiveVotes - (x.Item.NegativeVotes / 10) - x.Item.Reports),

			_ => items
				.OrderBy(x => !x.Item.IsIncluded)
				.ThenBy(x => !x.Item.Workshop)
				.ThenBy(x => !x.Item.IsMod)
				.ThenBy(x => x.Item.ToString())
		};

		if (SortDesc)
		{
			return items.Reverse();
		}

		return items;
	}

	protected override void OnItemMouseClick(DrawableItem<T, Rectangles> item, MouseEventArgs e)
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

		var rects = item.Rectangles;
		var filter = ModifierKeys.HasFlag(Keys.Control) != _settings.SessionSettings.UserSettings.FlipItemCopyFilterAction;

		if (rects.FolderRect.Contains(e.Location))
		{
			OpenFolder(item.Item);
			return;
		}

		if (item.Item.Workshop && rects.SteamRect.Contains(e.Location))
		{
			OpenSteamLink($"https://steamcommunity.com/workshop/filedetails?id={item.Item.SteamId}");
			return;
		}

		if (rects.SteamIdRect.Contains(e.Location))
		{
			if (filter)
			{
				if (item.Item.Workshop)
				{
					AddToSearch?.Invoke(item.Item.SteamId.ToString());
				}
				else
				{
					AddToSearch?.Invoke(Path.GetFileName(item.Item.Package?.Folder));
				}
			}
			else
			{
				if (item.Item.Workshop)
				{
					Clipboard.SetText(item.Item.SteamId.ToString());
				}
				else
				{
					Clipboard.SetText(Path.GetFileName(item.Item.Package?.Folder));
				}
			}

			return;
		}

		if (item.Item.Workshop && rects.AuthorRect.Contains(e.Location) && item.Item.Author is not null)
		{
			if (filter)
			{
				AuthorSelected?.Invoke(item.Item.Author);
			}
			else
			{
				var pc = new PC_UserPage(item.Item.Author?.SteamId ?? 0);

				(FindForm() as BasePanelForm)?.PushPanel(null, pc);

				//OpenSteamLink($"{item.Item.Author.ProfileUrl}myworkshopfiles");
			}

			return;
		}

		if (rects.CompatibilityRect.Contains(e.Location))
		{
			if (filter)
			{
				CompatibilityReportSelected?.Invoke(item.Item.GetCompatibilityInfo().Notification);
			}
			else
			{
				var pc = new PC_PackagePage((IPackage?)item.Item.Package ?? item.Item);

				(FindForm() as BasePanelForm)?.PushPanel(null, pc);

				pc.T_CR.Selected = true;

				if (_settings.SessionSettings.UserSettings.ResetScrollOnPackageClick)
				{
					ScrollTo(item.Item);
				}
			}
			return;
		}

		if (rects.DownloadStatusRect.Contains(e.Location))
		{
			if (filter && item.Item.Package is not null)
			{
				DownloadStatusSelected?.Invoke(item.Item.Package.Status);
			}

			return;
		}

		var minX = -Math.Min(-rects.SteamIdRect.X, Math.Min(-rects.DownloadStatusRect.X, -rects.CompatibilityRect.X));
		if (e.Location.X > minX)
		{
			return;
		}

		if (item.Item.Package?.Mod is Mod mod)
		{
			if (rects.IncludedRect.Contains(e.Location))
			{
				if (ModifierKeys.HasFlag(Keys.Control))
				{
					FilterByIncluded?.Invoke(mod.IsIncluded);
				}
				else
				{
					mod.IsIncluded = !mod.IsIncluded;
				}

				return;
			}

			if (rects.EnabledRect.Contains(e.Location))
			{
				if (ModifierKeys.HasFlag(Keys.Control))
				{
					FilterByEnabled?.Invoke(mod.IsEnabled);
				}
				else
				{
					mod.IsEnabled = !mod.IsEnabled;
				}

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
				if (item.Item.Package is null)
				{
					if (item.Item.Workshop)
					{
						_subscriptionsManager.Subscribe(new[] { item.Item.SteamId });
					}

					return;
				}

				if (ModifierKeys.HasFlag(Keys.Control))
				{
					FilterByIncluded?.Invoke(item.Item.IsIncluded);
				}
				else
				{
					item.Item.IsIncluded = !item.Item.IsIncluded;
				}

				return;
			}
		}

		if (rects.ScoreRect.Contains(e.Location))
		{
			new RatingInfoForm { Icon = Program.MainForm?.Icon }.ShowDialog(Program.MainForm);
			return;
		}

		if (rects.CenterRect.Contains(e.Location) || rects.IconRect.Contains(e.Location))
		{
			if (IsSelection)
			{
				PackageSelected?.Invoke(item.Item);
				return;
			}

			(FindForm() as BasePanelForm)?.PushPanel(null, item.Item.IsCollection ? new PC_ViewCollection(item.Item) : new PC_PackagePage((IPackage?)item.Item.Package ?? item.Item));

			if (_settings.SessionSettings.UserSettings.ResetScrollOnPackageClick)
			{
				ScrollTo(item.Item);
			}

			return;
		}

		if (rects.DateRect.Contains(e.Location))
		{
			var date = (item.Item.ServerTime == DateTime.MinValue && item.Item is Asset asset ? asset.LocalTime : item.Item.ServerTime).ToLocalTime();
			if (filter)
			{
				DateSelected?.Invoke(date);
			}
			else
			{
				Clipboard.SetText(date.ToString("g"));
			}
			return;
		}

		foreach (var tag in rects.TagRects)
		{
			if (tag.Value.Contains(e.Location))
			{
				if (filter)
				{
					TagSelected?.Invoke(tag.Key);
				}
				else
				{
					Clipboard.SetText(tag.Key.Value);
				}

				return;
			}
		}
	}

	public void ShowRightClickMenu(T item)
	{
		var items = PC_PackagePage.GetRightClickMenuItems(item);

		this.TryBeginInvoke(() => SlickToolStrip.Show(FindForm() as SlickForm, items));
	}

	private void OpenSteamLink(string? url)
	{
		PlatformUtil.OpenUrl(url);
	}

	private void OpenFolder(T item)
	{
		try
		{
			if (item is Asset asset)
			{
				PlatformUtil.OpenFolder(asset.FilePath);
			}
			else
			{
				PlatformUtil.OpenFolder(item.Package?.Folder);
			}
		}
		catch { }
	}

	protected override void OnMouseClick(MouseEventArgs e)
	{
		base.OnMouseClick(e);

		if (e.Button == MouseButtons.Left && PopupSearchRect1.Contains(e.Location))
		{
			OpenWorkshopSearch?.Invoke();
		}

		if (e.Button == MouseButtons.Left && PopupSearchRect2.Contains(e.Location))
		{
			OpenWorkshopSearchInBrowser?.Invoke();
		}
	}

	protected override void OnPaint(PaintEventArgs e)
	{
		try
		{
			PopupSearchRect1 = PopupSearchRect2 = Rectangle.Empty;

			if (Loading)
			{
				base.OnPaint(e);
			}
			else if (!Items.Any())
			{
				e.Graphics.DrawString(Locale.NoLocalPackagesFound + "\r\n" + Locale.CheckFolderInOptions, UI.Font(9.75F, FontStyle.Italic), new SolidBrush(FormDesign.Design.LabelColor), ClientRectangle, new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center });
			}
			else if (!SafeGetItems().Any())
			{
				if (!TextSearchNotEmpty)
				{
					e.Graphics.DrawString(Locale.NoPackagesMatchFilters, UI.Font(9.75F, FontStyle.Italic), new SolidBrush(FormDesign.Design.LabelColor), ClientRectangle.Pad(0, 0, 0, Height / 3), new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center });
					return;
				}

				CursorLocation = PointToClient(Cursor.Position);

				e.Graphics.DrawString(Locale.NoPackagesMatchFilters, UI.Font(9.75F, FontStyle.Italic), new SolidBrush(FormDesign.Design.LabelColor), ClientRectangle.Pad(0, 0, 0, Height / 3), new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center });

				using var icon1 = IconManager.GetIcon("I_Steam");
				using var icon2 = IconManager.GetIcon("I_Link");

				var buttonSize1 = SlickButton.GetSize(e.Graphics, icon1, Locale.SearchWorkshop, UI.Font(9.75F));
				var buttonSize2 = SlickButton.GetSize(e.Graphics, icon2, Locale.SearchWorkshopBrowser, UI.Font(9.75F));
				PopupSearchRect1 = ClientRectangle.Pad(0, Height / 3, 0, 0).CenterR(buttonSize1);

				SlickButton.GetColors(out var fore, out var back, PopupSearchRect1.Contains(CursorLocation) ? HoverState : HoverState.Normal);

				if (!PopupSearchRect1.Contains(CursorLocation))
				{
					back = Color.Empty;
				}

				SlickButton.DrawButton(e, PopupSearchRect1.Location, PopupSearchRect1.Size, Locale.SearchWorkshop, UI.Font(9.75F), back, fore, icon1, UI.Scale(new Padding(7), UI.UIScale), true, PopupSearchRect1.Contains(CursorLocation) ? HoverState : HoverState.Normal, ColorStyle.Active);

				PopupSearchRect2 = new Rectangle(PopupSearchRect1.X, PopupSearchRect1.Bottom + (Padding.Vertical * 2), buttonSize2.Width, buttonSize2.Height);

				SlickButton.GetColors(out fore, out back, PopupSearchRect2.Contains(CursorLocation) ? HoverState : HoverState.Normal);

				if (!PopupSearchRect2.Contains(CursorLocation))
				{
					back = Color.Empty;
				}

				SlickButton.DrawButton(e, PopupSearchRect2.Location, PopupSearchRect2.Size, Locale.SearchWorkshopBrowser, UI.Font(9.75F), back, fore, icon2, UI.Scale(new Padding(7), UI.UIScale), true, PopupSearchRect2.Contains(CursorLocation) ? HoverState : HoverState.Normal, ColorStyle.Active);

				Cursor = PopupSearchRect1.Contains(CursorLocation) || PopupSearchRect2.Contains(CursorLocation) ? Cursors.Hand : Cursors.Default;
			}
			else
			{
				base.OnPaint(e);
			}
		}
		catch { }
	}

	protected override void OnPaintItemList(ItemPaintEventArgs<T, Rectangles> e)
	{
		var package = e.Item.Package;
		var large = _settings.SessionSettings.UserSettings.LargeItemOnHover;
		var rects = e.Rects;
		var inclEnableRect = (rects.EnabledRect == Rectangle.Empty ? rects.IncludedRect : Rectangle.Union(rects.IncludedRect, rects.EnabledRect)).Pad(0, Padding.Top, 0, Padding.Bottom).Pad(2);
		var isPressed = e.HoverState.HasFlag(HoverState.Pressed);
		var labelRect = new Rectangle(rects.TextRect.X, rects.CenterRect.Bottom, 0, e.ClipRectangle.Bottom - rects.CenterRect.Bottom);
		var report = e.Item.GetCompatibilityInfo();

		if (isPressed && (PackagePage || (!rects.CenterRect.Contains(CursorLocation) && !rects.IconRect.Contains(CursorLocation))))
		{
			e.HoverState &= ~HoverState.Pressed;
		}

		base.OnPaintItemList(e);

		if (e.Item.Incompatible || e.Item.Banned || report.Data?.Package.Stability is PackageStability.Broken)
		{
			var stripeWidth = (int)(19 * UI.UIScale);
			var step = e.ClipRectangle.Height;
			var diagonalLength = (int)Math.Sqrt(2 * Math.Pow(Height, 2));
			var colors = new[]
			{
				FormDesign.Design.AccentColor.MergeColor(e.BackColor),
				(e.Item.Incompatible || e.Item.Banned?FormDesign.Design.RedColor: FormDesign.Design.YellowColor).MergeColor(e.BackColor, 35),
			};

			// Create a pen with a width equal to the stripe width
			using var pen = new Pen(colors[0], stripeWidth);

			var odd = false;
			// Draw the yellow and black diagonal lines
			for (var i = e.ClipRectangle.X - diagonalLength; i < e.ClipRectangle.Right; i += stripeWidth)
			{
				if (odd)
				{
					pen.Color = colors[0];
				}
				else
				{
					pen.Color = colors[1];
				}

				odd = !odd;

				e.Graphics.DrawLine(pen, i - step, e.ClipRectangle.Y + (2 * step), i + (step * 2), e.ClipRectangle.Y - step);
			}
		}
		else if (report.Data?.Package.Stability is PackageStability.Broken)
		{
			e.Graphics.FillRectangle(new SolidBrush(Color.FromArgb(50, FormDesign.Design.RedColor)), e.ClipRectangle);
		}

		var partialIncluded = e.Item is Package p && p.IsPartiallyIncluded();
		var isIncluded = partialIncluded || e.Item.IsIncluded;

		if (!IsSelection)
		{
			DrawIncludedButton(e, rects, inclEnableRect, isIncluded, partialIncluded, large, package);
		}

		DrawThumbnailAndTitle(e, rects, large);

		if (!large && !PackagePage)
		{
			if (!e.Item.Incompatible && report.Data?.Package.Stability is not PackageStability.Broken)
			{
				labelRect.X += DrawScore(e, large, rects, labelRect);
			}
		}

		var isVersion = package?.Mod is not null && !package.BuiltIn && !PackagePage;
		var versionText = isVersion ? "v" + package!.Mod!.Version.GetString() : package?.BuiltIn ?? false ? Locale.Vanilla : (e.Item.FileSize == 0 ? string.Empty : e.Item.FileSize.SizeString());

		if (!string.IsNullOrEmpty(versionText))
		{
			rects.VersionRect = DrawLabel(e, versionText, null, isVersion ? FormDesign.Design.YellowColor : FormDesign.Design.YellowColor.MergeColor(FormDesign.Design.BackColor, 40), labelRect, large ? ContentAlignment.TopLeft : ContentAlignment.BottomLeft, isVersion);
			labelRect.X += Padding.Left + rects.VersionRect.Width;
		}

		var date = (e.Item.ServerTime == DateTime.MinValue && e.Item is Asset asset ? asset.LocalTime : e.Item.ServerTime).ToLocalTime();

		if (date.Year > 2000 && !PackagePage)
		{
			var dateText = _settings.SessionSettings.UserSettings.ShowDatesRelatively ? date.ToRelatedString(true, false) : date.ToString("g");
			rects.DateRect = DrawLabel(e, dateText, IconManager.GetSmallIcon("I_UpdateTime"), FormDesign.Design.AccentColor.MergeColor(FormDesign.Design.BackColor, 50), labelRect, large ? ContentAlignment.TopLeft : ContentAlignment.BottomLeft, true);
			labelRect.X += Padding.Left + rects.DateRect.Width;
		}

		if (large)
		{
			labelRect.X = rects.TextRect.X;

			if (!PackagePage && !e.Item.Incompatible && report?.Data?.Package.Stability is not PackageStability.Broken)
			{
				labelRect.X += DrawScore(e, large, rects, labelRect);
			}
		}

		foreach (var item in (PackagePage && e.Item is Asset asset1 ? asset1.GetAssetTags() : e.Item.Tags).Distinct(x => x.Value))
		{
			using var tagIcon = item.Icon.Small;

			var tagRect = DrawLabel(e, item.Value, tagIcon, FormDesign.Design.ButtonColor, labelRect, ContentAlignment.BottomLeft, true);

			rects.TagRects[item] = tagRect;

			labelRect.X += Padding.Left + tagRect.Width;
		}

		if (PackagePage)
		{
			rects.SteamIdRect = rects.SteamRect = rects.AuthorRect = rects.FolderRect = rects.IconRect = rects.CenterRect = Rectangle.Empty;
			return;
		}

		var hasCrOrStatus = e.Item.Package?.Status > DownloadStatus.OK || (report is not null && report.Notification > NotificationType.Info);
		var brushRect = new Rectangle(rects.SteamIdRect.X - (int)((hasCrOrStatus ? 2 : 1) * 120 * UI.FontScale), (int)e.Graphics.ClipBounds.Y, (int)(120 * UI.FontScale), (int)e.Graphics.ClipBounds.Height);
		using (var brush = new LinearGradientBrush(brushRect, Color.Empty, e.BackColor, LinearGradientMode.Horizontal))
		{
			e.Graphics.FillRectangle(brush, brushRect);
			e.Graphics.FillRectangle(new SolidBrush(e.BackColor), new Rectangle(rects.SteamIdRect.X - (hasCrOrStatus ? (int)(120 * UI.FontScale) : 0), (int)e.Graphics.ClipBounds.Y, Width, (int)e.Graphics.ClipBounds.Height));
		}

		var steamIdX = rects.SteamIdRect.X;

		DrawAuthorAndSteamId(e, large, rects, package);

		if (large)
		{
			rects.CompatibilityRect.X += rects.SteamIdRect.X - steamIdX;
			rects.DownloadStatusRect.X += rects.SteamIdRect.X - steamIdX;
		}

		if (report is not null && report.Notification > NotificationType.Info)
		{
			var labelColor = report.Notification.GetColor();

			rects.CompatibilityRect = DrawLabel(e, LocaleCR.Get($"{report.Notification}"), IconManager.GetSmallIcon("I_CompatibilityReport"), labelColor,e.Item.Workshop ? rects.CompatibilityRect: rects.AuthorRect, large ? ContentAlignment.MiddleRight : ContentAlignment.BottomRight, true);

			if (large)
			{
				rects.DownloadStatusRect.X = rects.CompatibilityRect.X - rects.DownloadStatusRect.Width;
			}
		}
		else
		{
			rects.DownloadStatusRect = rects.CompatibilityRect;
			rects.CompatibilityRect = Rectangle.Empty;
		}

		rects.DownloadStatusRect = DrawStatusDescriptor(e, rects, large ? ContentAlignment.MiddleRight : ContentAlignment.TopRight);

		DrawButtons(e, rects, isPressed, package);

		if (!isIncluded && !IsGenericPage) // fade excluded item
		{
			using var fadedBrush = new SolidBrush(Color.FromArgb(e.HoverState.HasFlag(HoverState.Hovered) ? 25 : 75, BackColor));
			var filledRect = e.ClipRectangle.Pad(0, -Padding.Top, 0, -Padding.Bottom);

			e.Graphics.SetClip(filledRect);
			e.Graphics.FillRectangle(fadedBrush, filledRect);
		}
	}

	private int DrawScore(ItemPaintEventArgs<T, Rectangles> e, bool large, Rectangles rects, Rectangle labelRect)
	{
		var score = SteamUtil.GetScore(e.Item);

		if (e.Item.Workshop && score != -1)
		{
			var clip = e.Graphics.ClipBounds;
			var labelH = (int)e.Graphics.Measure(" ", UI.Font(large ? 9F : 7.5F)).Height - 1;
			labelH -= labelH % 2;
			var small = UI.FontScale < 1.25;
			var scoreRect = rects.ScoreRect = labelRect.Pad(Padding).Align(new Size(labelH, labelH), ContentAlignment.BottomLeft);
			var backColor = score > 90 && e.Item.Subscriptions >= 50000 ? FormDesign.Modern.ActiveColor : FormDesign.Design.GreenColor.MergeColor(FormDesign.Design.RedColor, score).MergeColor(FormDesign.Design.BackColor, 75);

			if (!small)
			{
				e.Graphics.FillEllipse(new SolidBrush(backColor), scoreRect);
			}
			else
			{
				scoreRect.Y--;
			}

			using var scoreFilled = IconManager.GetSmallIcon("I_VoteFilled");

			if (score < 75)
			{
				using var scoreIcon = IconManager.GetSmallIcon("I_Vote");

				e.Graphics.DrawImage(scoreIcon.Color(small ? backColor : backColor.GetTextColor()), scoreRect.CenterR(scoreIcon.Size));

				e.Graphics.SetClip(scoreRect.CenterR(scoreFilled.Size).Pad(0, scoreFilled.Height - (scoreFilled.Height * score / 105), 0, 0));
				e.Graphics.DrawImage(scoreFilled.Color(small ? backColor : backColor.GetTextColor()), scoreRect.CenterR(scoreFilled.Size));
				e.Graphics.SetClip(clip);
			}
			else
			{
				e.Graphics.DrawImage(scoreFilled.Color(small ? backColor : backColor.GetTextColor()), scoreRect.CenterR(scoreFilled.Size));
			}

			if (e.Item.Subscriptions < 50000 || score <= 90)
			{
				if (small)
				{
					using var scoreIcon = IconManager.GetSmallIcon("I_Vote");

					e.Graphics.SetClip(scoreRect.CenterR(scoreIcon.Size).Pad(0, scoreIcon.Height - (scoreIcon.Height * e.Item.Subscriptions / 15000), 0, 0));
					e.Graphics.DrawImage(scoreIcon.Color(FormDesign.Modern.ActiveColor), scoreRect.CenterR(scoreIcon.Size));
					e.Graphics.SetClip(clip);
				}
				else
				{
					using var pen = new Pen(Color.FromArgb(score >= 75 ? 255 : 200, FormDesign.Modern.ActiveColor), (float)(1.5 * UI.FontScale)) { EndCap = LineCap.Round, StartCap = LineCap.Round };
					e.Graphics.DrawArc(pen, scoreRect.Pad(-1), 90 - (Math.Min(360, 360F * e.Item.Subscriptions / 15000) / 2), Math.Min(360, 360F * e.Item.Subscriptions / 15000));
				}
			}

			return labelH + Padding.Left;
		}

		return 0;
	}

	private Rectangle DrawStatusDescriptor(ItemPaintEventArgs<T, Rectangles> e, Rectangles rects, ContentAlignment contentAlignment)
	{
		GetStatusDescriptors(e.Item, out var text, out var icon, out var color);

		if (!string.IsNullOrEmpty(text))
		{
			using (icon)
			{
				return DrawLabel(e, text, icon, color.MergeColor(FormDesign.Design.BackColor, 65), rects.DownloadStatusRect, contentAlignment, true);
			}
		}
		else
		{
			return Rectangle.Empty;
		}
	}

	private void DrawButtons(ItemPaintEventArgs<T, Rectangles> e, Rectangles rects, bool isPressed, Package? package)
	{
		if (package is null)
		{
			rects.SteamRect = Rectangle.Union(rects.SteamRect, rects.FolderRect);
			rects.FolderRect = Rectangle.Empty;
		}
		else
		{
			using var icon = IconManager.GetIcon("I_Folder", rects.FolderRect.Height / 2);
			SlickButton.DrawButton(e, rects.FolderRect, string.Empty, Font, icon, null, rects.FolderRect.Contains(CursorLocation) ? e.HoverState | (isPressed ? HoverState.Pressed : 0) : HoverState.Normal);
		}

		if (e.Item.Workshop)
		{
			using var icon = IconManager.GetIcon("I_Steam", rects.SteamRect.Height / 2);
			SlickButton.DrawButton(e, rects.SteamRect, string.Empty, Font, icon, null, rects.SteamRect.Contains(CursorLocation) ? e.HoverState | (isPressed ? HoverState.Pressed : 0) : HoverState.Normal);
		}
	}

	private void DrawAuthorAndSteamId(ItemPaintEventArgs<T, Rectangles> e, bool large, Rectangles rects, Package? package)
	{
		if (!e.Item.Workshop)
		{
			rects.SteamIdRect = DrawLabel(e, Path.GetFileName(package?.Folder), IconManager.GetSmallIcon("I_Folder"), FormDesign.Design.ActiveColor.MergeColor(FormDesign.Design.ButtonColor, 30), rects.SteamIdRect, large ? ContentAlignment.MiddleLeft : ContentAlignment.BottomLeft, true);
			rects.AuthorRect = Rectangle.Empty;
			return;
		}

		if (large && e.Item.Author is not null)
		{
			using var font = UI.Font(9.75F);
			var size = e.Graphics.Measure(e.Item.Author.Name, font).ToSize();
			var authorRect = rects.AuthorRect.Align(new Size(size.Width + Padding.Horizontal + Padding.Right + size.Height, size.Height + (Padding.Vertical * 2)), ContentAlignment.MiddleRight);
			authorRect.X -= Padding.Left;
			authorRect.Y += Padding.Top;
			var avatarRect = authorRect.Pad(Padding).Align(new(size.Height, size.Height), ContentAlignment.MiddleLeft);

			using var brush = new SolidBrush(FormDesign.Design.BackColor.Tint(Lum: FormDesign.Design.Type.If(FormDesignType.Dark, 4, -4)).MergeColor(FormDesign.Design.ActiveColor, authorRect.Contains(CursorLocation) ? 65 : 100));
			e.Graphics.FillRoundedRectangle(brush, authorRect, (int)(4 * UI.FontScale));

			using var brush1 = new SolidBrush(FormDesign.Design.ForeColor);
			e.Graphics.DrawString(e.Item.Author.Name, font, brush1, authorRect.Pad(avatarRect.Width + Padding.Horizontal, 0, 0, 0), new StringFormat { LineAlignment = StringAlignment.Center });

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

			if (_compatibilityManager.CompatibilityData.Authors.TryGet(e.Item.Author.SteamId)?.Verified ?? false)
			{
				var checkRect = avatarRect.Align(new Size(avatarRect.Height / 3, avatarRect.Height / 3), ContentAlignment.BottomRight);

				e.Graphics.FillEllipse(new SolidBrush(FormDesign.Design.GreenColor), checkRect.Pad(-(int)(2 * UI.FontScale)));

				using var img = IconManager.GetIcon("I_Check", checkRect.Height);
				e.Graphics.DrawImage(img.Color(Color.White), checkRect.Pad(0, 0, -1, -1));
			}

			rects.AuthorRect = authorRect;
		}
		else
		{
			rects.AuthorRect = DrawLabel(e, e.Item.Author?.Name, IconManager.GetSmallIcon("I_Developer"), FormDesign.Design.ActiveColor.MergeColor(FormDesign.Design.ButtonColor, 30), rects.AuthorRect, ContentAlignment.TopLeft, true);
		}

		rects.SteamIdRect = DrawLabel(e, e.Item.SteamId.ToString(), IconManager.GetSmallIcon("I_Steam"), FormDesign.Design.ActiveColor.MergeColor(FormDesign.Design.ButtonColor, 30), rects.SteamIdRect, large ? ContentAlignment.MiddleRight : ContentAlignment.BottomLeft, true);
	}

	private void DrawThumbnailAndTitle(ItemPaintEventArgs<T, Rectangles> e, Rectangles rects, bool large)
	{
		var iconRectangle = rects.IconRect;

		var iconImg = e.Item.IconImage;

		if (iconImg is null)
		{
			using var generic = (e.Item is Package ? Properties.Resources.I_CollectionIcon : e.Item.IsMod ? Properties.Resources.I_ModIcon : Properties.Resources.I_AssetIcon).Color(FormDesign.Design.IconColor);

			e.Graphics.DrawRoundedImage(generic, iconRectangle, (int)(4 * UI.FontScale), FormDesign.Design.AccentBackColor);
		}
		else
		{
			try
			{
				if (!e.Item.Workshop)
				{
					using var unsatImg = new Bitmap(iconImg, iconRectangle.Size).Tint(Sat: 0);
					e.Graphics.DrawRoundedImage(unsatImg, iconRectangle, (int)(4 * UI.FontScale), FormDesign.Design.AccentBackColor);
				}
				else
				{
					e.Graphics.DrawRoundedImage(iconImg, iconRectangle, (int)(4 * UI.FontScale), FormDesign.Design.AccentBackColor);
				}
			}
			catch { }
		}

		List<(Color Color, string Text)>? tags = null;

		var mod = e.Item is not Asset;
		var text = mod ? e.Item.CleanName(out tags) : e.Item.ToString();
		using var font = UI.Font(large ? 11.25F : 9F, FontStyle.Bold);
		var textSize = e.Graphics.Measure(text, font);

		using var brush = new SolidBrush(PackagePage ? base.ForeColor : e.HoverState.HasFlag(HoverState.Pressed) ? FormDesign.Design.ActiveForeColor : (rects.CenterRect.Contains(CursorLocation) || rects.IconRect.Contains(CursorLocation)) && e.HoverState.HasFlag(HoverState.Hovered) ? FormDesign.Design.ActiveColor : base.ForeColor);
		e.Graphics.DrawString(text, font, brush, rects.TextRect.Pad(0, 0, -9999, 0), new StringFormat { Trimming = StringTrimming.EllipsisCharacter });

		if (tags is null)
		{
			return;
		}

		var tagRect = new Rectangle(rects.TextRect.X + (int)textSize.Width, rects.TextRect.Y, 0, (int)textSize.Height);

		foreach (var item in tags)
		{
			tagRect.X += Padding.Left + DrawLabel(e, item.Text, null, item.Color, tagRect, ContentAlignment.MiddleLeft, smaller: true).Width;
		}
	}

	private void DrawIncludedButton(ItemPaintEventArgs<T, Rectangles> e, Rectangles rects, Rectangle inclEnableRect, bool isIncluded, bool partialIncluded, bool large, Package? package)
	{
		if (package is null && !e.Item.Workshop)
		{
			return; // missing local item
		}

		var incl = new DynamicIcon(_subscriptionsManager.IsSubscribing(e.Item) ? "I_Wait" : partialIncluded ? "I_Slash" : isIncluded ? "I_Ok" : package is null ? "I_Add" : "I_Enabled");
		var mod = package?.Mod;
		var required = mod is not null && _modLogicManager.IsRequired(mod);

		if (_settings.SessionSettings.UserSettings.AdvancedIncludeEnable && mod is not null)
		{
			var activeColor = FormDesign.Design.ActiveColor;
			var enabl = new DynamicIcon(mod.IsEnabled ? "I_Checked" : "I_Checked_OFF");
			if (isIncluded)
			{
				using var brush = inclEnableRect.Gradient(Color.FromArgb(inclEnableRect.Contains(CursorLocation) && !required && e.HoverState.HasFlag(HoverState.Hovered) ? 150 : 255, activeColor = partialIncluded ? FormDesign.Design.YellowColor : mod.IsEnabled ? FormDesign.Design.GreenColor : FormDesign.Design.RedColor), 1.5F);
				e.Graphics.FillRoundedRectangle(brush, inclEnableRect, (int)(3 * UI.FontScale));
			}
			else if (mod.IsEnabled)
			{
				using var brush = inclEnableRect.Gradient(Color.FromArgb(inclEnableRect.Contains(CursorLocation) && !required && e.HoverState.HasFlag(HoverState.Hovered) ? 150 : 255, activeColor = FormDesign.Design.YellowColor), 1.5F);
				e.Graphics.FillRoundedRectangle(brush, inclEnableRect, (int)(3 * UI.FontScale));
			}
			else if (inclEnableRect.Contains(CursorLocation) && !required && e.HoverState.HasFlag(HoverState.Hovered))
			{
				using var brush = inclEnableRect.Gradient(Color.FromArgb(20, ForeColor), 1.5F);
				e.Graphics.FillRoundedRectangle(brush, inclEnableRect, (int)(3 * UI.FontScale));
			}

			if (required)
			{
				e.Graphics.FillRoundedRectangle(new SolidBrush(Color.FromArgb(200, BackColor)), inclEnableRect, (int)(3 * UI.FontScale));
			}

			using var includedIcon = (large ? incl.Large : incl.Get(rects.IncludedRect.Height / 2)).Color((required || (rects.IncludedRect.Contains(CursorLocation) && e.HoverState.HasFlag(HoverState.Hovered))) ? activeColor : isIncluded ? FormDesign.Design.ActiveForeColor : ForeColor);
			using var enabledIcon = (large ? enabl.Large : enabl.Get(rects.IncludedRect.Height / 2)).Color((required || (rects.EnabledRect.Contains(CursorLocation) && e.HoverState.HasFlag(HoverState.Hovered))) ? activeColor : isIncluded ? FormDesign.Design.ActiveForeColor : base.ForeColor);
			e.Graphics.DrawImage(includedIcon, rects.IncludedRect.CenterR(includedIcon.Size));
			e.Graphics.DrawImage(enabledIcon, rects.EnabledRect.CenterR(enabledIcon.Size));
		}
		else
		{
			var activeColor = FormDesign.Design.ActiveColor;
			if (isIncluded)
			{
				using var brush = inclEnableRect.Gradient(Color.FromArgb(inclEnableRect.Contains(CursorLocation) && !required && e.HoverState.HasFlag(HoverState.Hovered) ? 150 : 255, activeColor = partialIncluded ? FormDesign.Design.YellowColor : FormDesign.Design.GreenColor), 1.5F);
				e.Graphics.FillRoundedRectangle(brush, inclEnableRect, (int)(3 * UI.FontScale));
			}
			else if (inclEnableRect.Contains(CursorLocation) && !required && e.HoverState.HasFlag(HoverState.Hovered))
			{
				using var brush = inclEnableRect.Gradient(Color.FromArgb(20, ForeColor), 1.5F);
				e.Graphics.FillRoundedRectangle(brush, inclEnableRect, (int)(3 * UI.FontScale));
			}

			if (required)
			{
				e.Graphics.FillRoundedRectangle(new SolidBrush(Color.FromArgb(200, BackColor)), inclEnableRect, (int)(3 * UI.FontScale));
			}

			using var icon = (large ? incl.Large : incl.Get(rects.IncludedRect.Height / 2)).Color(required || (rects.IncludedRect.Contains(CursorLocation) && e.HoverState.HasFlag(HoverState.Hovered)) ? activeColor : isIncluded ? FormDesign.Design.ActiveForeColor : ForeColor);
			e.Graphics.DrawImage(icon, inclEnableRect.CenterR(icon.Size));
		}
	}

	private Rectangle DrawLabel(ItemPaintEventArgs<T, Rectangles> e, string? text, Bitmap? icon, Color color, Rectangle rectangle, ContentAlignment alignment, bool action = false, bool smaller = false)
	{
		if (text == null)
		{
			return Rectangle.Empty;
		}

		var large = _settings.SessionSettings.UserSettings.LargeItemOnHover;
		using var font = UI.Font((large ? 9F : 7.5F) - (smaller ? 0.5F : 0F));
		var size = e.Graphics.Measure(text, font).ToSize();

		if (icon is not null)
		{
			size.Width += icon.Width + Padding.Left;
		}

		size.Width += Padding.Left;

		if (rectangle.Width > 0 && size.Width > rectangle.Width)
		{
			if (alignment == ContentAlignment.TopLeft)
			{
				alignment = ContentAlignment.TopRight;
			}
			else if (alignment == ContentAlignment.MiddleLeft)
			{
				alignment = ContentAlignment.MiddleRight;
			}
			else if (alignment == ContentAlignment.BottomLeft)
			{
				alignment = ContentAlignment.BottomRight;
			}
		}

		rectangle = rectangle.Pad(smaller ? Padding.Left / 2 : Padding.Left).Align(size, alignment);

		if (action && (!rectangle.Contains(CursorLocation) || !e.HoverState.HasFlag(HoverState.Hovered)))
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
		switch (mod.Package?.Status)
		{
			case DownloadStatus.Unknown:
				text = Locale.StatusUnknown;
				icon = IconManager.GetSmallIcon("I_Question");
				color = FormDesign.Design.YellowColor;
				return;
			case DownloadStatus.OutOfDate:
				text = Locale.OutOfDate;
				icon = IconManager.GetSmallIcon("I_OutOfDate");
				color = FormDesign.Design.YellowColor;
				return;
			case DownloadStatus.NotDownloaded:
				text = Locale.Missing;
				icon = IconManager.GetSmallIcon("I_Question");
				color = FormDesign.Design.RedColor;
				return;
			case DownloadStatus.PartiallyDownloaded:
				text = Locale.PartiallyDownloaded;
				icon = IconManager.GetSmallIcon("I_Broken");
				color = FormDesign.Design.RedColor;
				return;
			case DownloadStatus.Removed:
				text = Locale.RemovedFromSteam;
				icon = IconManager.GetSmallIcon("I_ContentRemoved");
				color = FormDesign.Design.RedColor;
				return;
		}

		text = string.Empty;
		icon = null;
		color = Color.White;
	}

	protected override ItemListControl<T>.Rectangles GenerateRectangles(T item, Rectangle rectangle)
	{
		var rects = new Rectangles(item);
		var doubleSize = _settings.SessionSettings.UserSettings.LargeItemOnHover;
		var includeItemHeight = doubleSize ? (ItemHeight / 2) : ItemHeight;

		if (!IsSelection)
		{
			if (_settings.SessionSettings.UserSettings.AdvancedIncludeEnable && item.Package?.Mod is not null)
			{
				rects.IncludedRect = rectangle.Pad(1 * Padding.Left, 0, 0, 0).Align(new Size(includeItemHeight * 9 / 10, rectangle.Height), ContentAlignment.MiddleLeft);
				rects.EnabledRect = rects.IncludedRect.Pad(rects.IncludedRect.Width, 0, -rects.IncludedRect.Width, 0);
			}
			else if (item is not Asset)
			{
				rects.IncludedRect = rectangle.Pad(1 * Padding.Left, 0, 0, 0).Align(new Size(_settings.SessionSettings.UserSettings.AdvancedIncludeEnable ? (includeItemHeight * 2 * 9 / 10) : includeItemHeight + 1, rectangle.Height), ContentAlignment.MiddleLeft);
			}
			else
			{
				rects.IncludedRect = rectangle.Pad(1 * Padding.Left, 0, 0, 0).Align(new Size(includeItemHeight + 1, rectangle.Height), ContentAlignment.MiddleLeft);
			}
		}

		var buttonRectangle = rectangle.Pad(0, 0, Padding.Right, 0).Align(new Size(ItemHeight / (doubleSize ? 2 : 1), ItemHeight / (doubleSize ? 2 : 1)), ContentAlignment.TopRight);
		var iconSize = rectangle.Height - Padding.Vertical;

		rects.FolderRect = buttonRectangle;
		rects.IconRect = rectangle.Pad(Math.Max(rects.IncludedRect.Right, rects.EnabledRect.Right) + (2 * Padding.Left)).Align(new Size(iconSize, iconSize), ContentAlignment.MiddleLeft);
		rects.TextRect = rectangle.Pad(rects.IconRect.X + rects.IconRect.Width + Padding.Left, 0, (item.Workshop ? (2 * Padding.Left) + (2 * buttonRectangle.Width) + (int)(120 * UI.FontScale) : 0) + rectangle.Width - buttonRectangle.X, rectangle.Height / 2);

		if (item.Workshop)
		{
			buttonRectangle.X -= Padding.Left + buttonRectangle.Width;
			rects.SteamRect = buttonRectangle;
		}

		if (doubleSize)
		{
			rects.SteamIdRect = new Rectangle(buttonRectangle.X - (int)(120 * UI.FontScale), rectangle.Y, (int)(120 * UI.FontScale), rectangle.Height / 2);
			rects.AuthorRect = new Rectangle(rectangle.X, rectangle.Y + (rectangle.Height / 2), rectangle.Width, rectangle.Height / 2);
			rects.CenterRect = new Rectangle(rects.IconRect.X - 1, rectangle.Y, rects.SteamIdRect.X - rects.IconRect.X, (int)FontMeasuring.Measure(" ", UI.Font(11.25F, FontStyle.Bold)).Height + Padding.Bottom);
			rects.CompatibilityRect = rects.SteamIdRect;
			rects.CompatibilityRect.X -= rects.SteamIdRect.Width;
			rects.DownloadStatusRect = rects.CompatibilityRect;
			rects.DownloadStatusRect.X -= rects.SteamIdRect.Width;
		}
		else
		{
			rects.AuthorRect = new Rectangle(buttonRectangle.X - (int)(120 * UI.FontScale), rectangle.Y + (rectangle.Height / 2), (int)(120 * UI.FontScale), rectangle.Height / 2);
			rects.SteamIdRect = new Rectangle(buttonRectangle.X - (int)(120 * UI.FontScale), rectangle.Y, (int)(120 * UI.FontScale), rectangle.Height / 2);
			rects.CompatibilityRect = new Rectangle(rects.SteamIdRect.X - (int)(120 * UI.FontScale), rectangle.Y, (int)(120 * UI.FontScale), rectangle.Height / 2);
			rects.DownloadStatusRect = new Rectangle(rects.AuthorRect.X - (int)(120 * UI.FontScale), rectangle.Y + (rectangle.Height / 2), (int)(120 * UI.FontScale), rectangle.Height / 2);
			rects.CenterRect = new Rectangle(rects.IconRect.X - 1, rectangle.Y, rects.SteamIdRect.X - rects.IconRect.X, rectangle.Height / 2);
		}

		if (!item.Workshop)
		{
			rects.SteamIdRect = rects.SteamIdRect.Pad(-Padding.Left - buttonRectangle.Width, 0, 0, 0);
		}

		if (IsSelection)
		{
			rects.CenterRect = new Rectangle(rects.IconRect.X - 1, rectangle.Y, buttonRectangle.X - rects.IconRect.X, rectangle.Height);
		}

		return rects;
	}

	public class Rectangles : IDrawableItemRectangles<T>
	{
		internal Dictionary<TagItem, Rectangle> TagRects = new();
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
		internal Rectangle ScoreRect;

		public T Item { get; set; }

		public Rectangles(T item)
		{
			Item = item;
		}

		public bool IsHovered(Control instance, Point location)
		{
			return
				IncludedRect.Contains(location) ||
				EnabledRect.Contains(location) ||
				FolderRect.Contains(location) ||
				SteamRect.Contains(location) ||
				AuthorRect.Contains(location) ||
				IconRect.Contains(location) ||
				CenterRect.Contains(location) ||
				DownloadStatusRect.Contains(location) ||
				CompatibilityRect.Contains(location) ||
				DateRect.Contains(location) ||
				(VersionRect.Contains(location) && Item?.Package?.Mod is not null) ||
				TagRects.Any(x => x.Value.Contains(location)) ||
				SteamIdRect.Contains(location);
		}
		public bool GetToolTip(Control instance, Point location, out string text, out Point point)
		{
			if (Item.Workshop)
			{
				if (SteamRect.Contains(location))
				{
					text = Locale.ViewOnSteam;
					point = SteamRect.Location;
					return true;
				}

				if (SteamIdRect.Contains(location))
				{
					text = getFilterTip(string.Format(Locale.CopyToClipboard, Item.SteamId), string.Format(Locale.AddToSearch, Item.SteamId));
					point = SteamIdRect.Location;
					return true;
				}

				if (AuthorRect.Contains(location))
				{
					text = getFilterTip(Locale.OpenAuthorPage, Locale.FilterByThisAuthor);
					point = AuthorRect.Location;
					return true;
				}
			}

			else if (SteamIdRect.Contains(location))
			{
				var folder = Path.GetFileName(Item.Package?.Folder);
				text = getFilterTip(string.Format(Locale.CopyToClipboard, folder), string.Format(Locale.AddToSearch, folder));
				point = AuthorRect.Location;
				return true;
			}

			if (FolderRect.Contains(location))
			{
				text = Locale.OpenLocalFolder;
				point = FolderRect.Location;
				return true;
			}

			if (CompatibilityRect.Contains(location))
			{
				text = getFilterTip(Locale.ViewPackageCR, Locale.FilterByThisReportStatus);
				point = CompatibilityRect.Location;
				return true;
			}

			if (DownloadStatusRect.Contains(location))
			{
				if (ServiceCenter.Get<ISettings>().SessionSettings.UserSettings.FlipItemCopyFilterAction)
				{
					text = Locale.FilterByThisPackageStatus + "\r\n\r\n" + Item.Package?.StatusReason;
					point = DownloadStatusRect.Location;
					return true;
				}
				else
				{
					text = Item.Package?.StatusReason + "\r\n\r\n" + string.Format(Locale.ControlClickTo, Locale.FilterByThisPackageStatus.ToString().ToLower());
					point = DownloadStatusRect.Location;
					return true;
				}
			}

			var minX = -Math.Min(-SteamIdRect.X, Math.Min(-DownloadStatusRect.X, -CompatibilityRect.X));
			if (SteamIdRect != Rectangle.Empty && location.X > minX)
			{
				text = string.Empty;
				point = default;
				return false;
			}

			if (ScoreRect.Contains(location))
			{
				text = string.Format(Locale.RatingCount, (Item.PositiveVotes > Item.NegativeVotes ? '+' : '-') + Math.Abs(Item.PositiveVotes - (Item.NegativeVotes / 10) - Item.Reports).ToString("N0"), $"({SteamUtil.GetScore(Item)}%)") + "\r\n" + string.Format(Locale.SubscribersCount, Item.Subscriptions.ToString("N0"));
				point = ScoreRect.Location;
				return true;
			}

			if (Item.Package?.Mod is not null)
			{
				if (IncludedRect.Contains(location))
				{
					text = $"{Locale.ExcludeInclude}\r\n\r\n{string.Format(Locale.ControlClickTo, Locale.FilterByThisIncludedStatus.ToString().ToLower())}";
					point = IncludedRect.Location;
					return true;
				}

				if (EnabledRect.Contains(location))
				{
					text = $"{Locale.EnableDisable}\r\n\r\n{string.Format(Locale.ControlClickTo, Locale.FilterByThisEnabledStatus.ToString().ToLower())}";
					point = EnabledRect.Location;
					return true;
				}

				if (VersionRect.Contains(location))
				{
					text = Locale.CopyVersionNumber;
					point = VersionRect.Location;
					return true;
				}
			}
			else
			{
				if (IncludedRect.Contains(location))
				{
					if (Item.Package != null)
					{
						text = $"{Locale.ExcludeInclude}\r\n\r\n{string.Format(Locale.ControlClickTo, Locale.FilterByThisIncludedStatus.ToString().ToLower())}";
						point = IncludedRect.Location;
						return true;
					}
					else if (Item.Workshop)
					{
						text = Locale.SubscribeToItem;
						point = IncludedRect.Location;
						return true;
					}
				}
			}

			if (CenterRect.Contains(location) || IconRect.Contains(location))
			{
				if ((instance as ItemListControl<T>)!.IsSelection)
				{
					text = Locale.SelectThisPackage;
				}
				else
				{
					text = Locale.OpenPackagePage;
				}

				point = CenterRect.Location;
				return true;
			}

			if (DateRect.Contains(location))
			{
				var date = (Item.ServerTime == DateTime.MinValue && Item is Asset asset ? asset.LocalTime : Item.ServerTime).ToLocalTime();
				text = getFilterTip(string.Format(Locale.CopyToClipboard, date.ToString("g")), Locale.FilterSinceThisDate);
				point = DateRect.Location;
				return true;
			}

			foreach (var tag in TagRects)
			{
				if (tag.Value.Contains(location))
				{
					text = getFilterTip(string.Format(Locale.CopyToClipboard, tag.Key), string.Format(Locale.FilterByThisTag, tag.Key));
					point = tag.Value.Location;
					return true;
				}
			}

			text = string.Empty;
			point = default;
			return false;

			static string getFilterTip(string? text, string? alt)
			{
				var tip = string.Empty;

				if (ServiceCenter.Get<ISettings>().SessionSettings.UserSettings.FlipItemCopyFilterAction)
				{
					ExtensionClass.Swap(ref text, ref alt);
				}

				if (text is not null)
				{
					tip += text + "\r\n\r\n";
				}

				if (alt is not null)
				{
					tip += string.Format(Locale.ControlClickTo, alt.ToLower());
				}

				return tip.Trim();
			}
		}
	}
}
