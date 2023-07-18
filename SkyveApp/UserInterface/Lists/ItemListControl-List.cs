using SkyveApp.Systems.CS1.Utilities;

using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Windows.Forms;

namespace SkyveApp.UserInterface.Lists;

partial class ItemListControl<T>
{
	protected override void OnPaintItemList(ItemPaintEventArgs<T, Rectangles> e)
	{
		var localPackage = e.Item.LocalPackage;
		var localParentPackage = localPackage?.LocalParentPackage;
		var workshopInfo = e.Item.GetWorkshopInfo();
		var partialIncluded = false;
		var isPressed = false;
		var isIncluded = (localPackage is not null && _packageUtil.IsIncluded(e.Item.LocalPackage!, out partialIncluded)) || partialIncluded;

		var compatibilityReport = e.Item.GetCompatibilityInfo();
		var notificationType = compatibilityReport?.GetNotification();
		var statusText = (string?)null;
		var statusIcon = (DynamicIcon?)null;
		var statusColor = Color.Empty;

		if (e.IsSelected)
		{
			e.BackColor = FormDesign.Design.GreenColor.MergeColor(FormDesign.Design.BackColor);
		}
		if (notificationType > NotificationType.Info)
		{
			e.BackColor = notificationType.Value.GetColor().MergeColor(FormDesign.Design.BackColor, 25);
		}
		else if (GetStatusDescriptors(e.Item, out statusText, out statusIcon, out statusColor))
		{
			e.BackColor = statusColor.MergeColor(FormDesign.Design.BackColor).MergeColor(FormDesign.Design.BackColor, 25);
		}
		else
		{
			e.BackColor = BackColor;
		}

		if (!IsPackagePage && e.HoverState.HasFlag(HoverState.Hovered) && (e.Rects.CenterRect.Contains(CursorLocation) || e.Rects.IconRect.Contains(CursorLocation)))
		{
			e.BackColor = e.BackColor.MergeColor(FormDesign.Design.ActiveColor, e.HoverState.HasFlag(HoverState.Pressed) ? 0 : 90);

			isPressed = e.HoverState.HasFlag(HoverState.Pressed);
		}

		base.OnPaintItemList(e);

		DrawThumbnail(e);
		DrawTitleAndTagsAndVersion(e, localParentPackage, workshopInfo, isPressed);
		DrawIncludedButton(e, e.Rects, isIncluded, partialIncluded, true, localParentPackage, out var activeColor);

		var scoreX = DrawScore(e, workshopInfo) + Padding.Horizontal;

		if (workshopInfo?.Author is not null)
		{
			DrawAuthor(e, workshopInfo.Author, scoreX);
		}
		else if (e.Item.IsLocal)
		{
			DrawFolderName(e, localParentPackage!, scoreX);
		}

		var maxTagX = DrawButtons(e, isPressed, localParentPackage);

		DrawTags(e, maxTagX);

		e.Graphics.ResetClip();

		DrawCompatibilityAndStatusList(e, notificationType, statusText, statusIcon, statusColor);

		if (!isIncluded && localPackage is not null)
		{
			using var brush = new SolidBrush(Color.FromArgb(85, BackColor));
			e.Graphics.FillRectangle(brush, e.ClipRectangle.InvertPad(GridPadding));
		}
	}

	private void DrawCompatibilityAndStatusList(ItemPaintEventArgs<T, ItemListControl<T>.Rectangles> e, NotificationType? notificationType, string? statusText, DynamicIcon? statusIcon, Color statusColor)
	{
		if (notificationType > NotificationType.Info)
		{
			e.Rects.CompatibilityRect = e.Graphics.DrawLargeLabel(new(e.ClipRectangle.Right - Padding.Horizontal, e.ClipRectangle.Top + Padding.Top), LocaleCR.Get($"{notificationType}"), "I_CompatibilityReport", notificationType.Value.GetColor(), ContentAlignment.TopRight, Padding, Math.Max(e.Rects.SteamRect.Y, e.Rects.FolderRect.Y) - e.ClipRectangle.Top - Padding.Vertical, CursorLocation);
		}

		if (statusText is not null && statusIcon is not null)
		{
			e.Rects.DownloadStatusRect = e.Graphics.DrawLargeLabel(new(notificationType > NotificationType.Info ? (e.Rects.CompatibilityRect.X - GridPadding.Left) : e.ClipRectangle.Right - Padding.Horizontal, e.ClipRectangle.Top + Padding.Top), notificationType > NotificationType.Info ? "" : statusText, statusIcon, statusColor, ContentAlignment.TopRight, Padding, Math.Max(e.Rects.SteamRect.Y, e.Rects.FolderRect.Y) - e.ClipRectangle.Top - Padding.Vertical, CursorLocation);
		}
	}

	//protected override void OnPaintItemList(ItemPaintEventArgs<T, Rectangles> e)
	//{
	//	var localPackage = e.Item.LocalParentPackage;
	//	var rects = e.Rects;
	//	var isPressed = e.HoverState.HasFlag(HoverState.Pressed);
	//	var labelRect = new Rectangle(rects.TextRect.X, rects.CenterRect.Bottom, 0, e.ClipRectangle.Bottom - rects.CenterRect.Bottom);
	//	var compatibilityReport = e.Item.GetCompatibilityInfo();
	//	var compatibilityInfo = _compatibilityManager.GetPackageInfo(e.Item);
	//	var workshopInfo = e.Item.GetWorkshopInfo();

	//	if (isPressed && (IsPackagePage || (!rects.CenterRect.Contains(CursorLocation) && !rects.IconRect.Contains(CursorLocation))))
	//	{
	//		e.HoverState &= ~HoverState.Pressed;
	//	}

	//	if (e.IsSelected)
	//	{
	//		e.BackColor = FormDesign.Design.GreenColor.MergeColor(FormDesign.Design.BackColor);
	//	}

	//	base.OnPaintItemList(e);

	//	if (e.IsSelected)
	//	{
	//		using var gBrush = new Pen(FormDesign.Design.GreenColor, (float)(1.5 * UI.FontScale));
	//		e.Graphics.DrawRectangle(gBrush, e.ClipRectangle.Pad((int)gBrush.Width));
	//	}

	//	if (workshopInfo is not null && (workshopInfo.IsIncompatible || workshopInfo.IsBanned || compatibilityInfo?.Stability is PackageStability.Broken))
	//	{
	//		var stripeWidth = (int)(19 * UI.UIScale);
	//		var step = e.ClipRectangle.Height;
	//		var diagonalLength = (int)Math.Sqrt(2 * Math.Pow(Height, 2));
	//		var colors = new[]
	//		{
	//			FormDesign.Design.AccentColor.MergeColor(e.BackColor),
	//			(workshopInfo.IsIncompatible || workshopInfo.IsBanned ? FormDesign.Design.RedColor : FormDesign.Design.YellowColor).MergeColor(e.BackColor, 35),
	//		};

	//		// Create a pen with a width equal to the stripe width
	//		using var pen = new Pen(colors[0], stripeWidth);

	//		var odd = false;
	//		// Draw the yellow and black diagonal lines
	//		for (var i = e.ClipRectangle.X - diagonalLength; i < e.ClipRectangle.Right; i += stripeWidth)
	//		{
	//			if (odd)
	//			{
	//				pen.Color = colors[0];
	//			}
	//			else
	//			{
	//				pen.Color = colors[1];
	//			}

	//			odd = !odd;

	//			e.Graphics.DrawLine(pen, i - step, e.ClipRectangle.Y + (2 * step), i + (step * 2), e.ClipRectangle.Y - step);
	//		}
	//	}
	//	else if (compatibilityInfo?.Stability is PackageStability.Broken)
	//	{
	//		e.Graphics.FillRectangle(new SolidBrush(Color.FromArgb(50, FormDesign.Design.RedColor)), e.ClipRectangle);
	//	}

	//	var partialIncluded = false;
	//	var isIncluded = (localPackage is not null && _packageUtil.IsIncluded(e.Item.LocalPackage!, out partialIncluded)) || partialIncluded;

	//	if (!IsSelection)
	//	{
	//		DrawIncludedButton(e, rects, isIncluded, partialIncluded, localPackage, out _);
	//	}

	//	DrawThumbnailAndTitle(e, rects, workshopInfo);

	//	if (!large && !IsPackagePage)
	//	{
	//		if (!(workshopInfo?.IsIncompatible ?? false) && compatibilityInfo?.Stability is not PackageStability.Broken)
	//		{
	//			labelRect.X += DrawScore(e, large, rects, labelRect, workshopInfo);
	//		}
	//	}

	//	var isVersion = localPackage?.Mod is not null && !localPackage.IsBuiltIn && !IsPackagePage;
	//	var versionText = isVersion ? "v" + localPackage!.Mod!.Version.GetString() : (localPackage?.IsBuiltIn ?? false) ? Locale.Vanilla : (e.Item is ILocalPackage lp ? lp.LocalSize.SizeString() : workshopInfo?.ServerSize.SizeString());

	//	if (!string.IsNullOrEmpty(versionText))
	//	{
	//		rects.VersionRect = e.Graphics.DrawLabel(versionText, null, isVersion ? FormDesign.Design.YellowColor : FormDesign.Design.YellowColor.MergeColor(FormDesign.Design.BackColor, 40), labelRect, large ? ContentAlignment.TopLeft : ContentAlignment.BottomLeft, mousePosition: isVersion ? CursorLocation : null);
	//		labelRect.X += Padding.Left + rects.VersionRect.Width;
	//	}

	//	var date = workshopInfo?.ServerTime ?? e.Item.LocalParentPackage?.LocalTime;

	//	if (date.HasValue && !IsPackagePage)
	//	{
	//		var dateText = _settings.UserSettings.ShowDatesRelatively ? date.Value.ToRelatedString(true, false) : date.Value.ToString("g");
	//		rects.DateRect = e.Graphics.DrawLabel(dateText, IconManager.GetSmallIcon("I_UpdateTime"), FormDesign.Design.AccentColor.MergeColor(FormDesign.Design.BackColor, 50), labelRect, large ? ContentAlignment.TopLeft : ContentAlignment.BottomLeft, mousePosition: CursorLocation);
	//		labelRect.X += Padding.Left + rects.DateRect.Width;
	//	}

	//	if (large)
	//	{
	//		labelRect.X = rects.TextRect.X;

	//		if (!IsPackagePage && !(workshopInfo?.IsIncompatible ?? false) && compatibilityInfo?.Stability is not PackageStability.Broken)
	//		{
	//			labelRect.X += DrawScore(e, large, rects, labelRect, workshopInfo);
	//		}
	//	}

	//	foreach (var item in e.Item.GetTags(IsPackagePage))
	//	{
	//		using var tagIcon = IconManager.GetSmallIcon(item.Icon);

	//		var tagRect = e.Graphics.DrawLabel(item.Value, tagIcon, FormDesign.Design.ButtonColor, labelRect, ContentAlignment.BottomLeft, mousePosition: CursorLocation);

	//		rects.TagRects[item] = tagRect;

	//		labelRect.X += Padding.Left + tagRect.Width;
	//	}

	//	if (IsPackagePage)
	//	{
	//		rects.SteamIdRect = rects.SteamRect = rects.AuthorRect = rects.FolderRect = rects.IconRect = rects.CenterRect = Rectangle.Empty;
	//		return;
	//	}

	//	var packageStatus = _packageUtil.GetStatus(e.Item, out _);
	//	var hasCrOrStatus = packageStatus > DownloadStatus.OK || (compatibilityReport is not null && compatibilityReport.GetNotification() > NotificationType.Info);
	//	var brushRect = new Rectangle(rects.SteamIdRect.X - (int)((hasCrOrStatus ? 2 : 1) * 120 * UI.FontScale), (int)e.Graphics.ClipBounds.Y, (int)(120 * UI.FontScale), (int)e.Graphics.ClipBounds.Height);
	//	using (var brush = new LinearGradientBrush(brushRect, Color.Empty, e.BackColor, LinearGradientMode.Horizontal))
	//	{
	//		e.Graphics.FillRectangle(brush, brushRect);
	//		e.Graphics.FillRectangle(new SolidBrush(e.BackColor), new Rectangle(rects.SteamIdRect.X - (hasCrOrStatus ? (int)(120 * UI.FontScale) : 0), (int)e.Graphics.ClipBounds.Y, Width, (int)e.Graphics.ClipBounds.Height));
	//	}

	//	var steamIdX = rects.SteamIdRect.X;

	//	DrawAuthorAndSteamId(e, large, rects, localPackage, workshopInfo);

	//	if (large)
	//	{
	//		rects.CompatibilityRect.X += rects.SteamIdRect.X - steamIdX;
	//		rects.DownloadStatusRect.X += rects.SteamIdRect.X - steamIdX;
	//	}

	//	if (compatibilityReport is not null && compatibilityReport.GetNotification() > NotificationType.Info)
	//	{
	//		var labelColor = compatibilityReport.GetNotification().GetColor();

	//		rects.CompatibilityRect = e.Graphics.DrawLabel(LocaleCR.Get($"{compatibilityReport.GetNotification()}"), IconManager.GetSmallIcon("I_CompatibilityReport"), labelColor, rects.CompatibilityRect, large ? ContentAlignment.MiddleRight : ContentAlignment.BottomRight, mousePosition: CursorLocation);

	//		if (large)
	//		{
	//			rects.DownloadStatusRect.X = rects.CompatibilityRect.X - rects.DownloadStatusRect.Width;
	//		}
	//	}
	//	else
	//	{
	//		rects.DownloadStatusRect = rects.CompatibilityRect;
	//		rects.CompatibilityRect = Rectangle.Empty;
	//	}

	//	rects.DownloadStatusRect = DrawStatusDescriptor(e, rects, large ? ContentAlignment.MiddleRight : ContentAlignment.TopRight);

	//	DrawButtons(e, rects, isPressed, localPackage);

	//	if (!isIncluded && !IsGenericPage) // fade excluded item
	//	{
	//		using var fadedBrush = new SolidBrush(Color.FromArgb(e.HoverState.HasFlag(HoverState.Hovered) ? 25 : 75, BackColor));
	//		var filledRect = e.ClipRectangle.Pad(0, -Padding.Top, 0, -Padding.Bottom);

	//		e.Graphics.SetClip(filledRect);
	//		e.Graphics.FillRectangle(fadedBrush, filledRect);
	//	}
	//}

	private int DrawScore(ItemPaintEventArgs<T, Rectangles> e, IWorkshopInfo? workshopInfo)
	{
		var score = workshopInfo?.Score ?? -1;

		if (score != -1)
		{
			var clip = e.Graphics.ClipBounds;
			GetScoreRect(e, out var labelH, out var scoreRect);
			var small = UI.FontScale < 1.25;
			var backColor = score > 90 && workshopInfo!.Subscribers >= 50000 ? FormDesign.Modern.ActiveColor : FormDesign.Design.GreenColor.MergeColor(FormDesign.Design.RedColor, score).MergeColor(FormDesign.Design.BackColor, 75);

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

			if (workshopInfo!.Subscribers < 50000 || score <= 90)
			{
				if (small)
				{
					using var scoreIcon = IconManager.GetSmallIcon("I_Vote");

					e.Graphics.SetClip(scoreRect.CenterR(scoreIcon.Size).Pad(0, scoreIcon.Height - (scoreIcon.Height * workshopInfo!.Subscribers / 15000), 0, 0));
					e.Graphics.DrawImage(scoreIcon.Color(FormDesign.Modern.ActiveColor), scoreRect.CenterR(scoreIcon.Size));
					e.Graphics.SetClip(clip);
				}
				else
				{
					using var pen = new Pen(Color.FromArgb(score >= 75 ? 255 : 200, FormDesign.Modern.ActiveColor), (float)(1.5 * UI.FontScale)) { EndCap = LineCap.Round, StartCap = LineCap.Round };
					e.Graphics.DrawArc(pen, scoreRect.Pad(-1), 90 - (Math.Min(360, 360F * workshopInfo!.Subscribers / 15000) / 2), Math.Min(360, 360F * workshopInfo!.Subscribers / 15000));
				}
			}

			return labelH + Padding.Left;
		}

		return 0;
	}

	private void GetScoreRect(ItemPaintEventArgs<T, ItemListControl<T>.Rectangles> e, out int labelH, out Rectangle scoreRect)
	{
		//if (GridView)
		{
			var padding = GridView ? GridPadding : Padding;
			var height = e.Rects.IconRect.Bottom - Math.Max(e.Rects.TextRect.Bottom, Math.Max(e.Rects.VersionRect.Bottom, e.Rects.DateRect.Bottom)) - padding.Bottom;
			labelH = e.Rects.IconRect.Bottom - Math.Max(e.Rects.TextRect.Bottom, Math.Max(e.Rects.VersionRect.Bottom, e.Rects.DateRect.Bottom)) - padding.Horizontal;
			scoreRect = new Rectangle(e.Rects.TextRect.X + (GridView ? 0 : padding.Left), e.Rects.IconRect.Bottom - height + (height / 2) - (labelH / 2) + 1, labelH, labelH);
			return;
		}

		//labelH = (int)e.Graphics.Measure(" ", UI.Font(large ? 9F : 7.5F)).Height - 1;
		//labelH -= labelH % 2;
		//scoreRect = rects.ScoreRect = labelRect.Pad(Padding).Align(new Size(labelH, labelH), ContentAlignment.BottomLeft);
	}

	private Rectangle DrawStatusDescriptor(ItemPaintEventArgs<T, Rectangles> e, Rectangles rects, ContentAlignment contentAlignment)
	{
		if (GetStatusDescriptors(e.Item, out var text, out var icon, out var color))
		{
			using (icon!.Small)
			{
				return e.Graphics.DrawLabel(text, icon, color.MergeColor(FormDesign.Design.BackColor, 65), rects.DownloadStatusRect, contentAlignment, mousePosition: CursorLocation);
			}
		}
		else
		{
			return Rectangle.Empty;
		}
	}

	private void DrawButtons(ItemPaintEventArgs<T, Rectangles> e, Rectangles rects, bool isPressed, ILocalPackageWithContents? package)
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

		if (e.Item.GetWorkshopInfo()?.Url is not null)
		{
			using var icon = IconManager.GetIcon("I_Steam", rects.SteamRect.Height / 2);
			SlickButton.DrawButton(e, rects.SteamRect, string.Empty, Font, icon, null, rects.SteamRect.Contains(CursorLocation) ? e.HoverState | (isPressed ? HoverState.Pressed : 0) : HoverState.Normal);
		}
	}

	private void DrawAuthorAndSteamId(ItemPaintEventArgs<T, Rectangles> e, bool large, Rectangles rects, ILocalPackageWithContents? package, IWorkshopInfo? workshopInfo)
	{
		if (workshopInfo?.Url is null)
		{
			rects.SteamIdRect = e.Graphics.DrawLabel(Path.GetFileName(package?.Folder), IconManager.GetSmallIcon("I_Folder"), FormDesign.Design.ActiveColor.MergeColor(FormDesign.Design.ButtonColor, 30), rects.SteamIdRect, large ? ContentAlignment.MiddleLeft : ContentAlignment.BottomLeft, mousePosition: CursorLocation);
			rects.AuthorRect = Rectangle.Empty;
			return;
		}

		if (large && workshopInfo.Author is not null)
		{
			using var font = UI.Font(9.75F);
			var size = e.Graphics.Measure(workshopInfo.Author.Name, font).ToSize();
			var authorRect = rects.AuthorRect.Align(new Size(size.Width + Padding.Horizontal + Padding.Right + size.Height, size.Height + (Padding.Vertical * 2)), ContentAlignment.MiddleRight);
			authorRect.X -= Padding.Left;
			authorRect.Y += Padding.Top;
			var avatarRect = authorRect.Pad(Padding).Align(new(size.Height, size.Height), ContentAlignment.MiddleLeft);

			using var brush = new SolidBrush(FormDesign.Design.BackColor.Tint(Lum: FormDesign.Design.Type.If(FormDesignType.Dark, 4, -4)).MergeColor(FormDesign.Design.ActiveColor, authorRect.Contains(CursorLocation) ? 65 : 100));
			e.Graphics.FillRoundedRectangle(brush, authorRect, (int)(4 * UI.FontScale));

			using var brush1 = new SolidBrush(FormDesign.Design.ForeColor);
			e.Graphics.DrawString(workshopInfo.Author.Name, font, brush1, authorRect.Pad(avatarRect.Width + Padding.Horizontal, 0, 0, 0), new StringFormat { LineAlignment = StringAlignment.Center });

			var authorImg = workshopInfo.Author.GetUserAvatar();

			if (authorImg is null)
			{
				using var authorIcon = Properties.Resources.I_AuthorIcon.Color(FormDesign.Design.IconColor);

				e.Graphics.DrawRoundImage(authorIcon, avatarRect);
			}
			else
			{
				e.Graphics.DrawRoundImage(authorImg, avatarRect);
			}

			if (_compatibilityManager.IsUserVerified(workshopInfo.Author))
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
			rects.AuthorRect = e.Graphics.DrawLabel(workshopInfo.Author?.Name, IconManager.GetSmallIcon("I_Developer"), FormDesign.Design.ActiveColor.MergeColor(FormDesign.Design.ButtonColor, 30), rects.AuthorRect, ContentAlignment.TopLeft, mousePosition: CursorLocation);
		}

		rects.SteamIdRect = e.Graphics.DrawLabel(e.Item.Id.ToString(), IconManager.GetSmallIcon("I_Steam"), FormDesign.Design.ActiveColor.MergeColor(FormDesign.Design.ButtonColor, 30), rects.SteamIdRect, large ? ContentAlignment.MiddleRight : ContentAlignment.BottomLeft, mousePosition: CursorLocation);
	}

	private void DrawThumbnailAndTitle(ItemPaintEventArgs<T, Rectangles> e, Rectangles rects, bool large, IWorkshopInfo? workshopInfo)
	{
		var iconRectangle = rects.IconRect;

		var iconImg = workshopInfo.GetThumbnail();

		if (iconImg is null)
		{
			using var generic = (e.Item is ILocalPackageWithContents ? Properties.Resources.I_CollectionIcon : e.Item.IsMod ? Properties.Resources.I_ModIcon : Properties.Resources.I_AssetIcon).Color(FormDesign.Design.IconColor);

			e.Graphics.DrawRoundedImage(generic, iconRectangle, (int)(4 * UI.FontScale), FormDesign.Design.AccentBackColor);
		}
		else
		{
			try
			{
				if (e.Item.IsLocal)
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

		var mod = e.Item is not IAsset;
		var text = mod ? e.Item.CleanName(out tags) : e.Item.ToString();
		using var font = UI.Font(large ? 11.25F : 9F, FontStyle.Bold);
		var textSize = e.Graphics.Measure(text, font);

		using var brush = new SolidBrush(IsPackagePage ? base.ForeColor : e.HoverState.HasFlag(HoverState.Pressed) ? FormDesign.Design.ActiveForeColor : (rects.CenterRect.Contains(CursorLocation) || rects.IconRect.Contains(CursorLocation)) && e.HoverState.HasFlag(HoverState.Hovered) ? FormDesign.Design.ActiveColor : base.ForeColor);
		e.Graphics.DrawString(text, font, brush, rects.TextRect.Pad(0, 0, -9999, 0), new StringFormat { Trimming = StringTrimming.EllipsisCharacter });

		if (tags is null)
		{
			return;
		}

		var tagRect = new Rectangle(rects.TextRect.X + (int)textSize.Width, rects.TextRect.Y, 0, (int)textSize.Height);

		foreach (var item in tags)
		{
			tagRect.X += Padding.Left + e.Graphics.DrawLabel(item.Text, null, item.Color, tagRect, ContentAlignment.MiddleLeft, smaller: true).Width;
		}
	}

	private void DrawIncludedButton(ItemPaintEventArgs<T, Rectangles> e, Rectangles rects, bool isIncluded, bool partialIncluded, bool large, ILocalPackageWithContents? package, out Color activeColor)
	{
		activeColor = FormDesign.Design.ActiveColor;

		if (package is null && e.Item.IsLocal)
		{
			return; // missing local item
		}

		var inclEnableRect = rects.EnabledRect == Rectangle.Empty ? rects.IncludedRect : Rectangle.Union(rects.IncludedRect, rects.EnabledRect);

		//if (!GridView)
		//{
		//	inclEnableRect = inclEnableRect.Pad(0, Padding.Top, 0, Padding.Bottom).Pad(2);
		//}

		var incl = new DynamicIcon(_subscriptionsManager.IsSubscribing(e.Item) ? "I_Wait" : partialIncluded ? "I_Slash" : isIncluded ? "I_Ok" : package is null ? "I_Add" : "I_Enabled");
		var mod = package?.Mod;
		var required = mod is not null && _modLogicManager.IsRequired(mod, _modUtil);

		if (_settings.UserSettings.AdvancedIncludeEnable && mod is not null)
		{
			var enabl = new DynamicIcon(mod.IsEnabled() ? "I_Checked" : "I_Checked_OFF");
			if (isIncluded)
			{
				using var brush = inclEnableRect.Gradient(Color.FromArgb(inclEnableRect.Contains(CursorLocation) && !required && e.HoverState.HasFlag(HoverState.Hovered) ? 150 : 255, activeColor = partialIncluded ? FormDesign.Design.YellowColor : mod.IsEnabled() ? FormDesign.Design.GreenColor : FormDesign.Design.RedColor), 1.5F);
				e.Graphics.FillRoundedRectangle(brush, inclEnableRect, (int)(3 * UI.FontScale));
			}
			else if (mod.IsEnabled())
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

	private ItemListControl<T>.Rectangles GenerateListRectangles(T item, Rectangle rectangle)
	{
		var rects = new Rectangles(item)
		{
			IconRect = rectangle.Align(new Size(rectangle.Height - Padding.Vertical, rectangle.Height - Padding.Vertical), ContentAlignment.MiddleLeft)
		};

		if (_settings.UserSettings.AdvancedIncludeEnable && item.LocalParentPackage?.Mod is not null)
		{
			rects.EnabledRect = rects.IncludedRect = rectangle.Pad(Padding).Align(new Size((int)(28 * UI.FontScale), rects.IconRect.Height / 2), ContentAlignment.MiddleLeft);

			rects.IncludedRect.Y -= rects.IncludedRect.Height / 2;
			rects.EnabledRect.Y += rects.EnabledRect.Height / 2;
		}
		else
		{
			rects.IncludedRect = rectangle.Pad(Padding).Align(UI.Scale(new Size(28, 28), UI.FontScale), ContentAlignment.MiddleLeft);
		}

		rects.IconRect.X += rects.IncludedRect.Right + Padding.Left;

		rects.TextRect = rectangle.Pad(rects.IconRect.Right + Padding.Left, 0, 0, rectangle.Height).AlignToFontSize(UI.Font(CompactList ? 8.25F : 9F, FontStyle.Bold), ContentAlignment.TopLeft);

		//rects.TextRect.Width = rects.IncludedRect.X - rects.TextRect.X;

		rects.CenterRect = rects.TextRect;

		return rects;

		//var rects = new Rectangles(item);


		//var includeItemHeight = doubleSize ? (ItemHeight / 2) : ItemHeight;

		//if (!IsSelection)
		//{
		//	if (_settings.UserSettings.AdvancedIncludeEnable && item.LocalParentPackage?.Mod is not null)
		//	{
		//		rects.IncludedRect = rectangle.Pad(1 * Padding.Left, 0, 0, 0).Align(new Size(includeItemHeight * 9 / 10, rectangle.Height), ContentAlignment.MiddleLeft);
		//		rects.EnabledRect = rects.IncludedRect.Pad(rects.IncludedRect.Width, 0, -rects.IncludedRect.Width, 0);
		//	}
		//	else if (item is not IAsset)
		//	{
		//		rects.IncludedRect = rectangle.Pad(1 * Padding.Left, 0, 0, 0).Align(new Size(_settings.UserSettings.AdvancedIncludeEnable ? (includeItemHeight * 2 * 9 / 10) : includeItemHeight + 1, rectangle.Height), ContentAlignment.MiddleLeft);
		//	}
		//	else
		//	{
		//		rects.IncludedRect = rectangle.Pad(1 * Padding.Left, 0, 0, 0).Align(new Size(includeItemHeight + 1, rectangle.Height), ContentAlignment.MiddleLeft);
		//	}
		//}

		//var buttonRectangle = rectangle.Pad(0, 0, Padding.Right, 0).Align(new Size(ItemHeight / (doubleSize ? 2 : 1), ItemHeight / (doubleSize ? 2 : 1)), ContentAlignment.TopRight);
		//var iconSize = rectangle.Height - Padding.Vertical;

		//rects.FolderRect = buttonRectangle;
		//rects.IconRect = rectangle.Pad(Math.Max(rects.IncludedRect.Right, rects.EnabledRect.Right) + (2 * Padding.Left)).Align(new Size(iconSize, iconSize), ContentAlignment.MiddleLeft);
		//rects.TextRect = rectangle.Pad(rects.IconRect.X + rects.IconRect.Width + Padding.Left, 0, (true ? (2 * Padding.Left) + (2 * buttonRectangle.Width) + (int)(120 * UI.FontScale) : 0) + rectangle.Width - buttonRectangle.X, rectangle.Height / 2);

		//if (item.GetWorkshopInfo()?.Url is not null)
		//{
		//	buttonRectangle.X -= Padding.Left + buttonRectangle.Width;
		//	rects.SteamRect = buttonRectangle;
		//}

		//if (doubleSize)
		//{
		//	rects.SteamIdRect = new Rectangle(buttonRectangle.X - (int)(120 * UI.FontScale), rectangle.Y, (int)(120 * UI.FontScale), rectangle.Height / 2);
		//	rects.AuthorRect = new Rectangle(rectangle.X, rectangle.Y + (rectangle.Height / 2), rectangle.Width, rectangle.Height / 2);
		//	rects.CenterRect = new Rectangle(rects.IconRect.X - 1, rectangle.Y, rects.SteamIdRect.X - rects.IconRect.X, (int)FontMeasuring.Measure(" ", UI.Font(11.25F, FontStyle.Bold)).Height + Padding.Bottom);
		//	rects.CompatibilityRect = rects.SteamIdRect;
		//	rects.CompatibilityRect.X -= rects.SteamIdRect.Width;
		//	rects.DownloadStatusRect = rects.CompatibilityRect;
		//	rects.DownloadStatusRect.X -= rects.SteamIdRect.Width;
		//}
		//else
		//{
		//	rects.AuthorRect = new Rectangle(buttonRectangle.X - (int)(120 * UI.FontScale), rectangle.Y + (rectangle.Height / 2), (int)(120 * UI.FontScale), rectangle.Height / 2);
		//	rects.SteamIdRect = new Rectangle(buttonRectangle.X - (int)(120 * UI.FontScale), rectangle.Y, (int)(120 * UI.FontScale), rectangle.Height / 2);
		//	rects.CompatibilityRect = new Rectangle(rects.SteamIdRect.X - (int)(120 * UI.FontScale), rectangle.Y, (int)(120 * UI.FontScale), rectangle.Height / 2);
		//	rects.DownloadStatusRect = new Rectangle(rects.AuthorRect.X - (int)(120 * UI.FontScale), rectangle.Y + (rectangle.Height / 2), (int)(120 * UI.FontScale), rectangle.Height / 2);
		//	rects.CenterRect = new Rectangle(rects.IconRect.X - 1, rectangle.Y, rects.SteamIdRect.X - rects.IconRect.X, rectangle.Height / 2);
		//}

		////if (item.IsLocal)
		////{
		////	rects.SteamIdRect = rects.SteamIdRect.Pad(-Padding.Left - buttonRectangle.Width, 0, 0, 0);
		////}

		//if (IsSelection)
		//{
		//	rects.CenterRect = new Rectangle(rects.IconRect.X - 1, rectangle.Y, buttonRectangle.X - rects.IconRect.X, rectangle.Height);
		//}

		//return rects;
	}
}
