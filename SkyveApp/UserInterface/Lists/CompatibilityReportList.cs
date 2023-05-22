using Extensions;

using SkyveApp.Domain;
using SkyveApp.Domain.Compatibility;
using SkyveApp.Utilities;
using SkyveApp.Utilities.Managers;

using SlickControls;

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace SkyveApp.UserInterface.Lists;
internal class CompatibilityReportList : SlickStackedListControl<CompatibilityInfo>
{
	private readonly Dictionary<DrawableItem<CompatibilityInfo>, Rectangles> _itemRects = new();
	public CompatibilityReportList()
	{
		HighlightOnHover = true;
		SeparateWithLines = true;
	}

	protected override void UIChanged()
	{
		ItemHeight = 80;

		base.UIChanged();

		Padding = UI.Scale(new Padding(3), UI.FontScale);
	}

	//protected override IEnumerable<DrawableItem<CompatibilityInfo>> OrderItems(IEnumerable<DrawableItem<CompatibilityInfo>> items)
	//{
	//	return items.OrderByDescending(x => SteamUtil.GetItem(x.Item)?.ServerTime);
	//}

	protected override bool IsItemActionHovered(DrawableItem<CompatibilityInfo> item, Point location)
	{
		return true;
	}

	protected override void OnPaintItem(ItemPaintEventArgs<CompatibilityInfo> e)
	{
		base.OnPaintItem(e);

		var Package = e.Item.Package;
		var rects = _itemRects[e.DrawableItem] = GetActionRectangles(e.Graphics, e.ClipRectangle.Pad(0, 0, e.ClipRectangle.Width- (int)(300 * UI.FontScale), 0), e.Item, true);
		var inclEnableRect = (rects.EnabledRect == Rectangle.Empty ? rects.IncludedRect : Rectangle.Union(rects.IncludedRect, rects.EnabledRect)).Pad(0, Padding.Top, 0, Padding.Bottom).Pad(2);
		var partialIncluded = Package.Package?.IsPartiallyIncluded() ?? false;
		var isIncluded = partialIncluded || Package.IsIncluded;

		PaintIncludedButton(e, rects, inclEnableRect, isIncluded, partialIncluded, false);
		DrawThumbnailAndTitle(e, rects, false);

		var brushRect = new Rectangle((int)(300 * UI.FontScale) - (int)(50 * UI.FontScale), e.ClipRectangle.Y, (int)(50 * UI.FontScale), e.ClipRectangle.Height);
		using (var brush = new LinearGradientBrush(brushRect, Color.Empty, e.BackColor, LinearGradientMode.Horizontal))
		{
			e.Graphics.FillRectangle(brush, brushRect);
			e.Graphics.FillRectangle(new SolidBrush(e.BackColor), new Rectangle((int)(300 * UI.FontScale), e.ClipRectangle.Y, Width, e.ClipRectangle.Height));
		}

		DrawAuthorAndSteamId(e, true, rects);

		DrawButtons(e, rects, false);

		var labelRect = new Rectangle(rects.AuthorRect.X, rects.AuthorRect.Y-Padding.Top, 0, 0);

		var date = e.Item.Package.ServerTime.ToLocalTime();

		if (date.Year > 2000)
		{
			var dateText = CentralManager.SessionSettings.UserSettings.ShowDatesRelatively ? date.ToRelatedString(true, false) : date.ToString("g");
			rects.DateRect = e.DrawLabel(dateText, IconManager.GetSmallIcon("I_UpdateTime"), FormDesign.Design.AccentColor.MergeColor(FormDesign.Design.BackColor, 50), labelRect, ContentAlignment.BottomLeft, true, mousePosition: CursorLocation);
			labelRect.Y -= Padding.Top + rects.DateRect.Height;
		}

		var isVersion = e.Item.Package.Package?.Mod is not null && !e.Item.Package.Package.BuiltIn;
		var versionText = isVersion ? "v" + e.Item.Package.Package!.Mod!.Version.GetString() : e.Item.Package.Package?.BuiltIn ?? false ? Locale.Vanilla : (e.Item.Package.FileSize == 0 ? string.Empty : e.Item.Package.FileSize.SizeString());

		if (!string.IsNullOrEmpty(versionText))
		{
			rects.VersionRect = e.DrawLabel(versionText, null, isVersion ? FormDesign.Design.YellowColor : FormDesign.Design.YellowColor.MergeColor(FormDesign.Design.BackColor, 40), labelRect, ContentAlignment.BottomLeft, true, mousePosition: CursorLocation);
			labelRect.Y += Padding.Top + rects.VersionRect.Height;
		}

		var item = e.Item.ReportItems.FirstOrDefault(x => x.Status.Notification == e.Item.Notification);

		DrawReport(e, item, rects);
	}

	private void DrawReport(ItemPaintEventArgs<CompatibilityInfo> e, ReportItem Message, Rectangles rects)
	{
		using var icon = Message.Status.Notification.GetIcon(false).Large;
		var actionHovered = false;
		var cursor = PointToClient(Cursor.Position);
		var pad = (int)(4 * UI.FontScale);
		var note = string.IsNullOrWhiteSpace(Message.Status.Note) ? null : LocaleCRNotes.Get(Message.Status.Note!).One.Replace("\r\n\r\n", "\r\n");
		var color = Message.Status.Notification.GetColor().MergeColor(BackColor, 60);
		var ClientRectangle = e.ClipRectangle.Pad((int)(355*UI.FontScale), 0, 0, 0);
		var iconRect = ClientRectangle.Align(icon.Size, ContentAlignment.MiddleLeft).Pad(0, 0, -pad * 2, -pad * 2);
		var messageSize = e.Graphics.Measure(Message.Message.Replace("\r\n\r\n", "\r\n"), UI.Font(8.25F), Width - iconRect.Width - pad);
		var noteSize = e.Graphics.Measure(note, UI.Font(7.5F), Width - iconRect.Width - pad);
		var y = (int)(messageSize.Height + noteSize.Height + (noteSize.Height == 0 ? 0 : pad * 2));
		using var brush = new SolidBrush(color);

		var allText = "";
		//GetAllButton(out var allText, out var allIcon, out var colorStyle);

		e.Graphics.FillRoundedRectangle(brush, iconRect, pad);

		e.Graphics.DrawImage(icon.Color(color.GetTextColor()), iconRect.CenterR(icon.Size));

		e.Graphics.DrawString(Message.Message.Replace("\r\n\r\n", "\r\n"), UI.Font(8.25F), new SolidBrush(ForeColor), ClientRectangle.Pad(iconRect.Width + pad+ (int)(5 * UI.FontScale), 0, (int)(175 * UI.FontScale), 0), new StringFormat { LineAlignment = y < Height && allText is null && !Message.Packages.Any() ? StringAlignment.Center : StringAlignment.Near });

		if (note is not null)
		{
			e.Graphics.DrawString(note, UI.Font(7.5F), new SolidBrush(Color.FromArgb(200, ForeColor)), ClientRectangle.Pad(iconRect.Width + pad + (int)(5 * UI.FontScale), string.IsNullOrWhiteSpace(Message.Message) ? 0 : ((int)messageSize.Height + pad), (int)(175 * UI.FontScale), 0));
		}

		//if (allText is not null)
		//{
		//	var buttonIcon = IconManager.GetIcon(allIcon);
		//	var buttonSize = SlickButton.GetSize(e.Graphics, buttonIcon, allText, UI.Font(8.25F), UI.Scale(new Padding(4), UI.FontScale));

		//	allButtonRect = new Rectangle(0, y, Width, 0).Pad(iconRect.Width + pad, pad, 0, 0).Align(buttonSize, Message.Packages.Length > 0 ? ContentAlignment.TopCenter : ContentAlignment.TopLeft);

		//	SlickButton.DrawButton(e, allButtonRect, allText, UI.Font(8.25F), buttonIcon, UI.Scale(new Padding(4), UI.FontScale), allButtonRect.Contains(cursor) ? HoverState & ~HoverState.Focused : HoverState.Normal, colorStyle);

		//	actionHovered |= allButtonRect.Contains(cursor);

		//	y += allButtonRect.Height + (pad * 2);
		//}

		if (Message.Packages.Length > 0)
		{
			var isDlc = Message.Type == ReportType.DlcMissing;
			var rect = ClientRectangle.Align(new Size((int)(175 * UI.FontScale), 0), ContentAlignment.TopRight);

			rect.Height = (int)(40 * UI.FontScale);

			foreach (var packageID in Message.Packages)
			{
				var fore = ForeColor;

				actionHovered |= rect.Contains(cursor);

				//_modRects[packageID] = rect;

				if (rect.Contains(cursor))// && (!_buttonRects.ContainsKey(packageID) || !_buttonRects[packageID].Contains(cursor)))
				{
					if (HoverState.HasFlag(HoverState.Pressed))
					{
						fore = FormDesign.Design.ActiveColor;
					}

					using var gradientbrush = new LinearGradientBrush(ClientRectangle.Pad(rect.Height / 2, 0, 0, 0), Color.FromArgb(50, fore), Color.Empty, LinearGradientMode.Horizontal);

					e.Graphics.FillRectangle(gradientbrush, rect.Pad(rect.Height / 2, 0, 0, 0));
				}

				var dlc = isDlc ? SteamUtil.Dlcs.FirstOrDefault(x => x.Id == packageID) : null;
				var package = packageID.Package;

				if (!(package?.Workshop ?? true) && package?.IconImage is not null)
				{
					using var unsatImg = new Bitmap(package.IconImage, UI.Scale(new Size(40, 40), UI.FontScale)).Tint(Sat: 0);
					e.Graphics.DrawRoundedImage(unsatImg, rect.Align(UI.Scale(new Size(40, 40), UI.FontScale), ContentAlignment.TopLeft), (int)(4 * UI.FontScale), FormDesign.Design.AccentBackColor);
				}
				else
				{
					e.Graphics.DrawRoundedImage(dlc?.Thumbnail ?? package?.IconImage ?? Properties.Resources.I_ModIcon.Color(fore), rect.Align(UI.Scale(new Size(isDlc ? (40 * 460 / 215) : 40, 40), UI.FontScale), ContentAlignment.TopLeft), pad, FormDesign.Design.AccentBackColor);
				}

				List<(Color Color, string Text)>? tags = null;

				var textRect = rect.Pad((int)(((isDlc ? 40 * 460 / 215 : 40) + 3) * UI.FontScale), 0, 0, 0).AlignToFontSize(Font, ContentAlignment.MiddleLeft);

				e.Graphics.DrawString(dlc?.Name.Remove("Cities: Skylines - ").Replace("Content Creator Pack", "CCP") ?? package?.CleanName(out tags) ?? Locale.UnknownPackage, UI.Font(7.5F, FontStyle.Bold), new SolidBrush(fore), textRect, new StringFormat { Trimming = StringTrimming.EllipsisCharacter });

				var tagRect = new Rectangle(textRect.Left, textRect.Y, 0, textRect.Height);

				if (tags is not null)
				{
					foreach (var item in tags)
					{
						tagRect.X += Padding.Left + e.DrawLabel(item.Text, null, item.Color, tagRect, ContentAlignment.BottomLeft, smaller: true).Width;
					}
				}

				string? buttonText = null;
				string? iconName = null;

				switch (Message.Status.Action)
				{
					case StatusAction.SubscribeToPackages:
						var p = package?.Package;

						if (p is null)
						{
							buttonText = Locale.Subscribe;
							iconName = "I_Add";
						}
						else if (!p.IsIncluded)
						{
							buttonText = Locale.Include;
							iconName = "I_Check";
						}
						else if (!(p.Mod?.IsEnabled ?? true))
						{
							buttonText = Locale.Enable;
							iconName = "I_Enabled";
						}
						break;
					case StatusAction.SelectOne:
						buttonText = Locale.SelectThisPackage;
						iconName = "I_Ok";
						break;
					case StatusAction.Switch:
						buttonText = Locale.Switch;
						iconName = "I_Switch";
						break;
				}

				if (buttonText is null || package?.IsCollection == true)
				{
					//rect.Y += _modRects[packageID].Height + pad;
					continue;
				}

				var buttonIcon = IconManager.GetIcon(iconName);
				var buttonSize = SlickButton.GetSize(e.Graphics, buttonIcon, buttonText, UI.Font(7.5F), UI.Scale(new Padding(3), UI.FontScale));

				//if (_subscribingTo.Contains(packageID))
				//{
				//	_buttonRects[packageID] = Rectangle.Empty;
				//	DrawLoader(e.Graphics, rect.Align(new Size(24, 24), ContentAlignment.BottomRight));
				//}
				//else
				//{
				//	_buttonRects[packageID] = _modRects[packageID].Align(buttonSize, ContentAlignment.BottomRight);

				//	SlickButton.DrawButton(e, _buttonRects[packageID], buttonText, UI.Font(7.5F), buttonIcon, UI.Scale(new Padding(3), UI.FontScale), _buttonRects[packageID].Contains(cursor) ? HoverState & ~HoverState.Focused : HoverState.Normal, Message.Status.Action is StatusAction.SelectOne ? ColorStyle.Active : ColorStyle.Green);
				//}

				//rect.Y += _modRects[packageID].Height + pad;
			}

			y = rect.Y;
		}
	}

	private void PaintIncludedButton(ItemPaintEventArgs<CompatibilityInfo> e, Rectangles rects, Rectangle inclEnableRect, bool isIncluded, bool partialIncluded, bool large)
	{
		var incl = new DynamicIcon(partialIncluded ? "I_Slash" : isIncluded ? "I_Ok" : "I_Enabled");
		if (CentralManager.SessionSettings.UserSettings.AdvancedIncludeEnable && e.Item.Package.Package?.Mod is Mod mod)
		{
			var activeColor = FormDesign.Design.ActiveColor;
			var enabl = new DynamicIcon(mod.IsEnabled ? "I_Checked" : "I_Checked_OFF");
			if (isIncluded)
			{
				using var brush = inclEnableRect.Gradient(Color.FromArgb(inclEnableRect.Contains(CursorLocation) ? 150 : 255, activeColor = partialIncluded ? FormDesign.Design.YellowColor : mod.IsEnabled ? FormDesign.Design.GreenColor : FormDesign.Design.RedColor), 1.5F);
				e.Graphics.FillRoundedRectangle(brush, inclEnableRect, (int)(3 * UI.FontScale));
			}
			else if (mod.IsEnabled)
			{
				using var brush = inclEnableRect.Gradient(Color.FromArgb(inclEnableRect.Contains(CursorLocation) ? 150 : 255, activeColor = FormDesign.Design.YellowColor), 1.5F);
				e.Graphics.FillRoundedRectangle(brush, inclEnableRect, (int)(3 * UI.FontScale));
			}
			else if (inclEnableRect.Contains(CursorLocation))
			{
				using var brush = inclEnableRect.Gradient(Color.FromArgb(20, ForeColor), 1.5F);
				e.Graphics.FillRoundedRectangle(brush, inclEnableRect, (int)(3 * UI.FontScale));
			}

			using var includedIcon = (large ? incl.Large : incl.Get(rects.IncludedRect.Width / 2)).Color(rects.IncludedRect.Contains(CursorLocation) ? activeColor : isIncluded ? FormDesign.Design.ActiveForeColor : ForeColor);
			using var enabledIcon = (large ? enabl.Large : enabl.Get(rects.IncludedRect.Width / 2)).Color(rects.EnabledRect.Contains(CursorLocation) ? activeColor : isIncluded ? FormDesign.Design.ActiveForeColor : base.ForeColor);
			e.Graphics.DrawImage(includedIcon, rects.IncludedRect.CenterR(includedIcon.Size));
			e.Graphics.DrawImage(enabledIcon, rects.EnabledRect.CenterR(enabledIcon.Size));
		}
		else
		{
			var activeColor = FormDesign.Design.ActiveColor;
			if (isIncluded)
			{
				using var brush = inclEnableRect.Gradient(Color.FromArgb(inclEnableRect.Contains(CursorLocation) ? 150 : 255, activeColor = partialIncluded ? FormDesign.Design.YellowColor : FormDesign.Design.GreenColor), 1.5F);
				e.Graphics.FillRoundedRectangle(brush, inclEnableRect, (int)(3 * UI.FontScale));
			}
			else if (inclEnableRect.Contains(CursorLocation))
			{
				using var brush = inclEnableRect.Gradient(Color.FromArgb(20, ForeColor), 1.5F);
				e.Graphics.FillRoundedRectangle(brush, inclEnableRect, (int)(3 * UI.FontScale));
			}

			using var icon = (large ? incl.Large : incl.Get(rects.IncludedRect.Width / 2)).Color(rects.IncludedRect.Contains(CursorLocation) ? activeColor : isIncluded ? FormDesign.Design.ActiveForeColor : ForeColor);
			e.Graphics.DrawImage(icon, inclEnableRect.CenterR(icon.Size));
		}
	}

	private void DrawButtons(ItemPaintEventArgs<CompatibilityInfo> e, Rectangles rects, bool isPressed)
	{
		if (e.Item.Package.Package is null)
		{
			rects.SteamRect = Rectangle.Union(rects.SteamRect, rects.FolderRect);
			rects.FolderRect = Rectangle.Empty;
		}
		else
		{
			using var icon = IconManager.GetIcon("I_Folder", rects.FolderRect.Height / 2);
			SlickButton.DrawButton(e, rects.FolderRect, string.Empty, Font, icon, null, rects.FolderRect.Contains(CursorLocation) ? e.HoverState | (isPressed ? HoverState.Pressed : 0) : HoverState.Normal);
		}

		if (e.Item.Package.Workshop)
		{
			using var icon = IconManager.GetIcon("I_Steam", rects.SteamRect.Height / 2);
			SlickButton.DrawButton(e, rects.SteamRect, string.Empty, Font, icon, null, rects.SteamRect.Contains(CursorLocation) ? e.HoverState | (isPressed ? HoverState.Pressed : 0) : HoverState.Normal);
		}
	}

	private void DrawAuthorAndSteamId(ItemPaintEventArgs<CompatibilityInfo> e, bool large, Rectangles rects)
	{
		if (!e.Item.Package.Workshop)
		{
			rects.SteamIdRect = e.DrawLabel(Path.GetFileName(e.Item.Package?.Folder), IconManager.GetSmallIcon("I_Folder"), FormDesign.Design.ActiveColor.MergeColor(FormDesign.Design.ButtonColor, 30), rects.AuthorRect, ContentAlignment.MiddleLeft, false, mousePosition: CursorLocation);
			rects.AuthorRect = Rectangle.Empty;
			return;
		}

		if (large && e.Item.Package.Author is not null)
		{
			using var font = UI.Font(7.5F);
			var size = e.Graphics.Measure(e.Item.Package.Author.Name, font).ToSize();
			var authorRect = rects.AuthorRect.Align(new Size(size.Width + Padding.Horizontal  + rects.AuthorRect.Height, rects.AuthorRect.Height - 2), ContentAlignment.TopLeft);
			var avatarRect = authorRect.Align(new(authorRect.Height, authorRect.Height), ContentAlignment.MiddleLeft).Pad(Padding);

			using var brush = new SolidBrush(FormDesign.Design.BackColor.Tint(Lum: FormDesign.Design.Type.If(FormDesignType.Dark, 4, -4)).MergeColor(FormDesign.Design.ActiveColor, authorRect.Contains(CursorLocation) ? 65 : 100));
			e.Graphics.FillRoundedRectangle(brush, authorRect, (int)(4 * UI.FontScale));

			using var brush1 = new SolidBrush(FormDesign.Design.ForeColor);
			e.Graphics.DrawString(e.Item.Package.Author.Name, font, brush1, authorRect.Pad(avatarRect.Width + Padding.Horizontal, 0, 0, 0), new StringFormat { LineAlignment = StringAlignment.Center });

			var authorImg = e.Item.Package.AuthorIconImage;

			if (authorImg is null)
			{
				using var authorIcon = Properties.Resources.I_AuthorIcon.Color(FormDesign.Design.IconColor);

				e.Graphics.DrawRoundImage(authorIcon, avatarRect);
			}
			else
			{
				e.Graphics.DrawRoundImage(authorImg, avatarRect);
			}

			if (CompatibilityManager.CompatibilityData.Authors.TryGet(e.Item.Package.Author.SteamId)?.Verified ?? false)
			{
				var checkRect = avatarRect.Align(new Size(avatarRect.Height / 3, avatarRect.Height / 3), ContentAlignment.BottomRight);

				e.Graphics.FillEllipse(new SolidBrush(FormDesign.Design.GreenColor), checkRect.Pad(-(int)(2 * UI.FontScale)));

				using var img = IconManager.GetIcon("I_Check", checkRect.Height);
				e.Graphics.DrawImage(img.Color(Color.White), checkRect.Pad(0, 0, -1, -1));
			}

			rects.SteamIdRect = rects.AuthorRect = authorRect;
			rects.SteamIdRect.Y -= authorRect.Height + Padding.Bottom;
		}
		else
		{
			rects.AuthorRect = e.DrawLabel(e.Item.Package.Author?.Name, IconManager.GetSmallIcon("I_Developer"), FormDesign.Design.ActiveColor.MergeColor(FormDesign.Design.ButtonColor, 30), rects.AuthorRect, ContentAlignment.TopLeft, true, mousePosition: CursorLocation);
		}

		//rects.SteamIdRect = e.DrawLabel(e.Item.Package.SteamId.ToString(), IconManager.GetSmallIcon("I_Steam"), FormDesign.Design.ActiveColor.MergeColor(FormDesign.Design.ButtonColor, 30), rects.SteamIdRect, ContentAlignment.BottomRight, true, mousePosition: CursorLocation);
	}

	private void DrawThumbnailAndTitle(ItemPaintEventArgs<CompatibilityInfo> e, Rectangles rects, bool large)
	{
		var iconRectangle = rects.IconRect;

		var iconImg = e.Item.Package.IconImage;

		if (iconImg is null)
		{
			using var generic = Properties.Resources.I_CollectionIcon.Color(FormDesign.Design.IconColor);

			e.Graphics.DrawRoundedImage(generic, iconRectangle, (int)(4 * UI.FontScale), FormDesign.Design.AccentBackColor);
		}
		else
		{
			try
			{
				if (!e.Item.Package.Workshop)
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


		var text = e.Item.Package.CleanName(out var tags);
		using var font = UI.Font(large ? 11.25F : 9F, FontStyle.Bold);
		var textSize = e.Graphics.Measure(text, font);

		using var brush = new SolidBrush(e.HoverState.HasFlag(HoverState.Pressed) ? FormDesign.Design.ActiveForeColor : (rects.CenterRect.Contains(CursorLocation) || rects.IconRect.Contains(CursorLocation)) && e.HoverState.HasFlag(HoverState.Hovered) ? FormDesign.Design.ActiveColor : base.ForeColor);
		e.Graphics.DrawString(text, font, brush, rects.TextRect.Pad(0, 0, -9999, 0), new StringFormat { Trimming = StringTrimming.EllipsisCharacter });

		var tagRect = new Rectangle(rects.TextRect.X + (int)textSize.Width, rects.TextRect.Y, 0, (int)textSize.Height);

		if (e.Item.Data?.Package.Stability is PackageStability.Broken)
		{
			tagRect.X += Padding.Left + e.DrawLabel(LocaleCR.Broken.One.ToUpper(), null, Color.FromArgb(225, FormDesign.Design.RedColor), tagRect, ContentAlignment.MiddleLeft, true).Width;
		}

		if (e.Item.Package.Incompatible)
		{
			tagRect.X += Padding.Left + e.DrawLabel(LocaleCR.Incompatible.One.ToUpper(), null, Color.FromArgb(225, FormDesign.Design.RedColor), tagRect, ContentAlignment.MiddleLeft, true).Width;
		}

		if (tags is null)
		{
			return;
		}

		foreach (var item in tags)
		{
			tagRect.X += Padding.Left + e.DrawLabel(item.Text, null, item.Color, tagRect, ContentAlignment.MiddleLeft, true).Width;
		}
	}

	private Rectangles GetActionRectangles(Graphics g, Rectangle rectangle, CompatibilityInfo item, bool doubleSize)
	{
		var section = ItemHeight / 3 - Padding.Top / 2;
		var rects = new Rectangles() { Item = item };
		var includeItemHeight = doubleSize ? (ItemHeight / 2) : ItemHeight;

		if (CentralManager.SessionSettings.UserSettings.AdvancedIncludeEnable && item.Package.Package?.Mod is not null)
		{
			rects.IncludedRect = rectangle.Pad(1 * Padding.Left, 0, 0, 0).Align(new Size(section+Padding.Horizontal, rectangle.Height), ContentAlignment.MiddleLeft);
			rects.EnabledRect = rects.IncludedRect.Pad(rects.IncludedRect.Width, 0, -rects.IncludedRect.Width, 0);
		}
		else
		{
			rects.IncludedRect = rectangle.Pad(1 * Padding.Left, 0, 0, 0).Align(new Size(section + Padding.Horizontal, rectangle.Height), ContentAlignment.MiddleLeft);
		}

		var buttonRectangle = rectangle.Pad(0, 0, Padding.Right, 0).Align(new Size(section, section), ContentAlignment.TopRight);
		var iconSize = rectangle.Height - Padding.Vertical;

		rects.FolderRect = rectangle.Pad(Math.Max(rects.IncludedRect.Right, rects.EnabledRect.Right) + (2 * Padding.Left),0,0,0).Align(new Size(section, section), ContentAlignment.BottomLeft);
		rects.SteamRect = rectangle.Pad(Math.Max(rects.IncludedRect.Right, rects.EnabledRect.Right) + (2 * Padding.Left), 0, 0, 0).Align(new Size(section, section), ContentAlignment.MiddleLeft);
		rects.IconRect = rectangle.Pad(Math.Max(rects.IncludedRect.Right, rects.EnabledRect.Right) + (2 * Padding.Left), 0, 0, 0).Align(new Size(section, section), ContentAlignment.TopLeft);
		rects.TextRect = rectangle.Pad(rects.IconRect.X + rects.IconRect.Width + Padding.Left, 0, (item.Package.Workshop ? (2 * Padding.Left) + (2 * buttonRectangle.Width) + (int)(100 * UI.FontScale) : 0) + rectangle.Width - buttonRectangle.X, rectangle.Height / 2);

		if (item.Package.Workshop)
		{
			buttonRectangle.X -= Padding.Left + buttonRectangle.Width;
			//rects.SteamRect = buttonRectangle;
		}

		rects.SteamIdRect = new Rectangle(buttonRectangle.X - (int)(100 * UI.FontScale), rectangle.Y, (int)(100 * UI.FontScale), rectangle.Height / 2);
		rects.AuthorRect = rects.FolderRect;
		rects.AuthorRect.X += rects.AuthorRect.Width + Padding.Left;
		rects.AuthorRect.Width = 0;
		rects.CenterRect = new Rectangle(rects.IconRect.X - 1, rectangle.Y, rects.SteamIdRect.X - rects.IconRect.X, rectangle.Height / 3);

		if (!item.Package.Workshop)
		{
			rects.SteamIdRect = rects.SteamIdRect.Pad(-Padding.Left - buttonRectangle.Width, 0, 0, 0);
		}

		return rects;
	}

	private class Rectangles
	{
		internal CompatibilityInfo? Item;

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

		internal bool Contain(Point location)
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
				(VersionRect.Contains(location) && Item?.Package.Package?.Mod is not null) ||
				TagRects.Any(x => x.Value.Contains(location)) ||
				SteamIdRect.Contains(location);
		}
	}
}
