using Extensions;

using SkyveApp.Domain;
using SkyveApp.Domain.Compatibility;
using SkyveApp.Domain.Compatibility.Enums;
using SkyveApp.Domain.Interfaces;
using SkyveApp.UserInterface.Panels;
using SkyveApp.Utilities;
using SkyveApp.Utilities.IO;
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
internal class CompatibilityReportList : SlickStackedListControl<CompatibilityInfo, CompatibilityReportList.Rectangles>
{
	public CompatibilityReportList()
	{
		HighlightOnHover = true;
		SeparateWithLines = true;
		AllowDrop = true;
	}

	protected override void UIChanged()
	{
		ItemHeight = 100;

		base.UIChanged();

		Padding = UI.Scale(new Padding(3), UI.FontScale);
	}

	protected override IEnumerable<DrawableItem<CompatibilityInfo, Rectangles>> OrderItems(IEnumerable<DrawableItem<CompatibilityInfo, Rectangles>> items)
	{
		return items.OrderByDescending(x => x.Item.Package.CleanName());
	}

	protected override void OnItemMouseClick(DrawableItem<CompatibilityInfo, Rectangles> item, MouseEventArgs e)
	{
		base.OnItemMouseClick(item, e);

		if (e.Button == MouseButtons.Right)
		{
			ShowRightClickMenu(item.Item.Package);
			return;
		}

		if (e.Button != MouseButtons.Left)
		{
			return;
		}

		var rects = item.Rectangles;

		if (rects.FolderRect.Contains(e.Location))
		{
			OpenFolder(item.Item.Package);
			return;
		}

		if (item.Item.Package.Workshop && rects.SteamRect.Contains(e.Location))
		{
			OpenSteamLink($"https://steamcommunity.com/workshop/filedetails?id={item.Item.Package.SteamId}");
			return;
		}

		if (item.Item.Package.Workshop && rects.AuthorRect.Contains(e.Location) && item.Item.Package.Author is not null)
		{
			OpenSteamLink($"{item.Item.Package.Author.ProfileUrl}myworkshopfiles");

			return;
		}

		if (item.Item.Package.Package?.Mod is Mod mod)
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
				Clipboard.SetText(item.Item.Package.Package.Mod.Version.GetString());
			}
		}
		else
		{
			if (rects.IncludedRect.Contains(e.Location))
			{
				if (item.Item.Package.Package is null)
				{
					SubscriptionsManager.Subscribe(new[] { item.Item.Package.SteamId });
					return;
				}

				item.Item.Package.IsIncluded = !item.Item.Package.IsIncluded;

				return;
			}
		}

		if (rects.CenterRect.Contains(e.Location) || rects.IconRect.Contains(e.Location))
		{
			(FindForm() as BasePanelForm)?.PushPanel(null, item.Item.Package.IsCollection ? new PC_ViewCollection(item.Item.Package) : new PC_PackagePage(item.Item.Package));

			if (CentralManager.SessionSettings.UserSettings.ResetScrollOnPackageClick)
			{
				ScrollTo(item.Item);
			}

			return;
		}

		var Message = item.Item.ReportItems.FirstOrDefault(x => x.Status.Notification == item.Item.Notification && !CompatibilityManager.IsSnoozed(x));

		foreach (var rect in rects.buttonRects)
		{
			if (rect.Value.Contains(e.Location))
			{
				if (e.Button == MouseButtons.Left)
				{
					Clicked(item.Item, Message, rect.Key, true);
				}

				return;
			}
		}

		foreach (var rect in rects.modRects)
		{
			if (rect.Value.Contains(e.Location))
			{
				if (e.Button == MouseButtons.Left)
				{
					Clicked(item.Item, Message, rect.Key, false);
				}
				else if (e.Button == MouseButtons.Right && rect.Key.Package is not null)
				{
					var items = PC_PackagePage.GetRightClickMenuItems(rect.Key.Package);

					this.TryBeginInvoke(() => SlickToolStrip.Show(Program.MainForm, items));
				}

				return;
			}
		}

		if (e.Button == MouseButtons.Left && rects.snoozeRect.Contains(e.Location))
		{
			CompatibilityManager.ToggleSnoozed(Message);
			FilterChanged();
		}

		if (e.Button == MouseButtons.Left && rects.allButtonRect.Contains(e.Location))
		{
			switch (Message.Status.Action)
			{
				case StatusAction.SubscribeToPackages:
					SubscriptionsManager.Subscribe(Message.Packages.Where(x => x.Package?.Package is null).Select(x => x.SteamId));
					ContentUtil.SetBulkIncluded(Message.Packages.SelectWhereNotNull(x => x.Package)!, true);
					ContentUtil.SetBulkEnabled(Message.Packages.SelectWhereNotNull(x => x.Package?.Package?.Mod)!, true);
					break;
				case StatusAction.RequiresConfiguration:
					CompatibilityManager.ToggleSnoozed(Message);
					FilterChanged();
					break;
				case StatusAction.UnsubscribeThis:
					SubscriptionsManager.UnSubscribe(new[] { item.Item.Package.SteamId });
					break;
				case StatusAction.UnsubscribeOther:
					SubscriptionsManager.UnSubscribe(Message.Packages.Select(x => x.SteamId));
					break;
				case StatusAction.ExcludeThis:
					item.Item.Package.IsIncluded = false;
					break;
				case StatusAction.ExcludeOther:
					foreach (var p in Message.Packages)
					{
						if (p.Package is not null)
						{
							p.Package.IsIncluded = false;
						}
					}
					break;
				case StatusAction.RequestReview:
					Program.MainForm.PushPanel(null, new PC_RequestReview(item.Item.Package));
					break;
			}
		}
	}

	public void ShowRightClickMenu(IPackage item)
	{
		var items = PC_PackagePage.GetRightClickMenuItems(item);

		this.TryBeginInvoke(() => SlickToolStrip.Show(FindForm() as SlickForm, items));
	}

	private void OpenSteamLink(string? url)
	{
		PlatformUtil.OpenUrl(url);
	}

	private void OpenFolder(IPackage item)
	{
		try
		{
			if (item is Asset asset)
			{
				PlatformUtil.OpenFolder(asset.FileName);
			}
			else
			{
				PlatformUtil.OpenFolder(item.Package?.Folder);
			}
		}
		catch { }
	}

	protected override void OnPaintItemList(ItemPaintEventArgs<CompatibilityInfo, Rectangles> e)
	{
		var Package = e.Item.Package;
		var rects = e.Rects;
		var inclEnableRect = (rects.EnabledRect == Rectangle.Empty ? rects.IncludedRect : Rectangle.Union(rects.IncludedRect, rects.EnabledRect)).Pad(0, Padding.Top, 0, Padding.Bottom).Pad(2);
		var partialIncluded = Package.Package?.IsPartiallyIncluded() ?? false;
		var isIncluded = partialIncluded || Package.IsIncluded;
		var isPressed = e.HoverState.HasFlag(HoverState.Pressed);
		var isHovered = e.HoverState.HasFlag(HoverState.Hovered);

		if (!rects.CenterRect.Contains(CursorLocation) && !rects.IconRect.Contains(CursorLocation))
		{
			e.HoverState &= ~HoverState.Pressed & ~HoverState.Hovered;
		}

		base.OnPaintItemList(e);

		if (isHovered)
		{
			e.HoverState |= HoverState.Hovered;
		}

		PaintIncludedButton(e, rects, inclEnableRect, isIncluded, partialIncluded, false);
		DrawThumbnailAndTitle(e, rects, false);

		var brushRect = new Rectangle((int)(275 * UI.FontScale) - (int)(50 * UI.FontScale), e.ClipRectangle.Y, (int)(50 * UI.FontScale), e.ClipRectangle.Height);
		using (var brush = new LinearGradientBrush(brushRect, Color.Empty, e.BackColor, LinearGradientMode.Horizontal))
		{
			e.Graphics.FillRectangle(brush, brushRect);
			e.Graphics.FillRectangle(new SolidBrush(e.BackColor), new Rectangle((int)(275 * UI.FontScale), e.ClipRectangle.Y, Width, e.ClipRectangle.Height));
		}

		DrawAuthorAndSteamId(e, true, rects);

		DrawButtons(e, rects, false);

		var labelRect = new Rectangle(rects.AuthorRect.X, rects.AuthorRect.Y - Padding.Top, 0, 0);

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

		var item = e.Item.ReportItems.FirstOrDefault(x => x.Status.Notification == e.Item.Notification && !CompatibilityManager.IsSnoozed(x));

		DrawReport(e, item, rects);
	}

	private void DrawReport(ItemPaintEventArgs<CompatibilityInfo, Rectangles> e, ReportItem Message, Rectangles rects)
	{
		if (Message.Status is null)
		{
			return;
		}

		using var icon = Message.Status.Notification.GetIcon(false).Large;
		var actionHovered = false;
		var cursor = PointToClient(Cursor.Position);
		var pad = (int)(4 * UI.FontScale);
		var note = string.IsNullOrWhiteSpace(Message.Status.Note) ? null : LocaleCRNotes.Get(Message.Status.Note!).One;
		var color = Message.Status.Notification.GetColor().MergeColor(BackColor, 60);
		var ClientRectangle = e.ClipRectangle.Pad((int)(275 * UI.FontScale), 0, 0, 0);
		var iconRect = ClientRectangle.Align(icon.Size, ContentAlignment.TopLeft).Pad(0, 0, -pad * 2, -pad * 2);
		var messageSize = e.Graphics.Measure(Message.Message, UI.Font(7.5F), ClientRectangle.Width - iconRect.Width - (pad * 2) - (Message.Packages.Length > 0 ? (int)(200 * UI.FontScale) : 0));
		var noteSize = e.Graphics.Measure(note, UI.Font(6.75F), ClientRectangle.Width - iconRect.Width - (pad * 2) - (Message.Packages.Length > 0 ? (int)(200 * UI.FontScale) : 0));
		var y = (int)(messageSize.Height + noteSize.Height + (noteSize.Height == 0 ? 0 : pad * 2));
		using var brush = new SolidBrush(color);

		if (Message.Status.Notification > NotificationType.Info && Message.PackageId != 0)
		{
			// = new Rectangle(e.ClipRectangle.Location, new Size(ClientRectangle.X, e.ClipRectangle.Height)).Align(iconRect.Size, ContentAlignment.BottomRight);
			actionHovered |= rects.snoozeRect.Contains(cursor);
			var purple = Color.FromArgb(100, 60, 220);
			var isSnoozed = CompatibilityManager.IsSnoozed(Message);

			SlickTip.SetTo(this, !actionHovered ? string.Empty : isSnoozed ? Locale.UnSnooze : Locale.Snooze, false, rects.snoozeRect.Location);

			if (HoverState.HasFlag(HoverState.Hovered) && !HoverState.HasFlag(HoverState.Pressed) && rects.snoozeRect.Contains(cursor))
			{
				e.Graphics.FillRoundedRectangle(new SolidBrush(Color.FromArgb(125, purple)), rects.snoozeRect, pad);
			}
			else if (isSnoozed || (HoverState.HasFlag(HoverState.Pressed) && rects.snoozeRect.Contains(cursor)))
			{
				e.Graphics.FillRoundedRectangle(new SolidBrush(purple), rects.snoozeRect, pad);
			}
			else
			{
				e.Graphics.FillRoundedRectangle(new SolidBrush(Color.FromArgb(20, purple)), rects.snoozeRect, pad);
			}

			using var snoozeIcon = IconManager.GetIcon("I_Snooze", rects.snoozeRect.Height * 3 / 4);
			e.Graphics.DrawImage(snoozeIcon.Color((isSnoozed || (HoverState.HasFlag(HoverState.Pressed) && rects.snoozeRect.Contains(cursor))) ? purple.GetTextColor() : FormDesign.Design.IconColor), rects.snoozeRect.CenterR(snoozeIcon.Size));
		}
		else
		{
			rects.snoozeRect = default;
		}

		GetAllButton(Message, out var allText, out var allIcon, out var colorStyle);

		e.Graphics.DrawString(Message.Message, UI.Font(7.5F), new SolidBrush(ForeColor), ClientRectangle.Pad(iconRect.Width + pad + (int)(5 * UI.FontScale), 0, (Message.Packages.Length > 0 ? (int)(200 * UI.FontScale) : 0), 0));

		if (note is not null)
		{
			e.Graphics.DrawString(note, UI.Font(6.75F), new SolidBrush(Color.FromArgb(200, ForeColor)), ClientRectangle.Pad(iconRect.Width + pad + (int)(5 * UI.FontScale), string.IsNullOrWhiteSpace(Message.Message) ? 0 : ((int)messageSize.Height + pad), (Message.Packages.Length > 0 ? (int)(200 * UI.FontScale) : 0), 0));
		}

		if (allText is not null)
		{
			var buttonIcon = IconManager.GetIcon(allIcon);
			var buttonSize = SlickButton.GetSize(e.Graphics, buttonIcon, allText, UI.Font(8.25F), UI.Scale(new Padding(4), UI.FontScale));

			rects.allButtonRect = ClientRectangle.Pad(Padding.Left, y, 0, 0).Pad(iconRect.Width + pad, pad, 0, 0).Align(buttonSize, Message.Packages.Length > 0 ? ContentAlignment.TopCenter : ContentAlignment.TopLeft);

			SlickButton.DrawButton(e, rects.allButtonRect, allText, UI.Font(8.25F), buttonIcon, UI.Scale(new Padding(4), UI.FontScale), rects.allButtonRect.Contains(cursor) ? HoverState & ~HoverState.Focused : HoverState.Normal, colorStyle);

			actionHovered |= rects.allButtonRect.Contains(cursor);

			y += rects.allButtonRect.Height + (pad * 2);
		}

		e.Graphics.FillRoundedRectangle(brush, iconRect, pad);
		e.Graphics.FillRoundedRectangle(brush, new Rectangle(ClientRectangle.X + iconRect.Width - (2 * pad), ClientRectangle.Y, 2 * pad, Math.Min(y, ClientRectangle.Height - pad)), pad);

		e.Graphics.DrawImage(icon.Color(color.GetTextColor()), iconRect.CenterR(icon.Size));

		if (Message.Packages.Length > 0)
		{
			var isDlc = Message.Type == ReportType.DlcMissing;
			var rect = ClientRectangle.Align(new Size((int)(200 * UI.FontScale), 0), ContentAlignment.TopRight);

			rect.Height = rects.IconRect.Height - pad;

			foreach (var packageID in Message.Packages)
			{
				var fore = ForeColor;
				var dlc = isDlc ? SteamUtil.Dlcs.FirstOrDefault(x => x.Id == packageID) : null;
				var package = packageID.Package;

				actionHovered |= rect.Contains(cursor);

				rects.modRects[packageID] = rect;

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
					rect.Y += rects.modRects[packageID].Height + pad;
					continue;
				}

				var buttonIcon = IconManager.GetSmallIcon(iconName);
				var buttonSize = SlickButton.GetSize(e.Graphics, buttonIcon, buttonText, UI.Font(6.75F), UI.Scale(new Padding(3), UI.FontScale));

				rects.buttonRects[packageID] = rects.modRects[packageID].Align(buttonSize, ContentAlignment.MiddleRight);

				if (rect.Contains(cursor) && (!rects.buttonRects.ContainsKey(packageID) || !rects.buttonRects[packageID].Contains(cursor)))
				{
					if (HoverState.HasFlag(HoverState.Pressed))
					{
						fore = FormDesign.Design.ActiveColor;
					}

					using var gradientbrush = new LinearGradientBrush(ClientRectangle.Pad(rect.Height / 2, 0, 0, 0), Color.FromArgb(50, fore), Color.Empty, LinearGradientMode.Horizontal);

					e.Graphics.FillRectangle(gradientbrush, rect.Pad(rect.Height / 2, 0, 0, 0));
				}

				if (!(package?.Workshop ?? true) && package?.IconImage is not null)
				{
					using var unsatImg = new Bitmap(package.IconImage, new Size(rect.Height, rect.Height)).Tint(Sat: 0);
					e.Graphics.DrawRoundedImage(unsatImg, rect.Align(new Size(rect.Height, rect.Height), ContentAlignment.TopLeft), (int)(4 * UI.FontScale), FormDesign.Design.AccentBackColor);
				}
				else
				{
					e.Graphics.DrawRoundedImage(dlc?.Thumbnail ?? package?.IconImage ?? Properties.Resources.I_ModIcon.Color(fore), rect.Align(new Size(isDlc ? (rect.Height * 460 / 215) : rect.Height, rect.Height), ContentAlignment.TopLeft), pad, FormDesign.Design.AccentBackColor);
				}

				List<(Color Color, string Text)>? tags = null;

				var textRect = rect.Pad((isDlc ? rect.Height * 460 / 215 : rect.Height) + (int)(3 * UI.FontScale), 0, 0, 0).AlignToFontSize(Font, ContentAlignment.MiddleLeft);

				e.Graphics.DrawString(dlc?.Name.Remove("Cities: Skylines - ").Replace("Content Creator Pack", "CCP") ?? package?.CleanName(out tags) ?? Locale.UnknownPackage, UI.Font(7.5F, FontStyle.Bold), new SolidBrush(fore), textRect, new StringFormat { Trimming = StringTrimming.EllipsisCharacter });

				var tagRect = new Rectangle(textRect.Left, textRect.Y, 0, textRect.Height);

				if (tags is not null)
				{
					foreach (var item in tags)
					{
						tagRect.X += Padding.Left + e.DrawLabel(item.Text, null, item.Color, tagRect, ContentAlignment.BottomLeft, smaller: true).Width;
					}
				}

				SlickButton.DrawButton(e, rects.buttonRects[packageID], buttonText, UI.Font(6.75F), buttonIcon, UI.Scale(new Padding(3), UI.FontScale), rects.buttonRects[packageID].Contains(cursor) ? HoverState & ~HoverState.Focused : HoverState.Normal, Message.Status.Action is StatusAction.SelectOne ? ColorStyle.Active : ColorStyle.Green);

				rect.Y += rect.Height + pad;
			}

			y = rect.Y;
		}
	}

	private void PaintIncludedButton(ItemPaintEventArgs<CompatibilityInfo, Rectangles> e, Rectangles rects, Rectangle inclEnableRect, bool isIncluded, bool partialIncluded, bool large)
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

	private void DrawButtons(ItemPaintEventArgs<CompatibilityInfo, Rectangles> e, Rectangles rects, bool isPressed)
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

	private void DrawAuthorAndSteamId(ItemPaintEventArgs<CompatibilityInfo, Rectangles> e, bool large, Rectangles rects)
	{
		if (!e.Item.Package.Workshop)
		{
			e.DrawLabel(Path.GetFileName(e.Item.Package?.Folder), IconManager.GetSmallIcon("I_Folder"), FormDesign.Design.ActiveColor.MergeColor(FormDesign.Design.ButtonColor, 30), rects.AuthorRect, ContentAlignment.MiddleLeft, false, mousePosition: CursorLocation);
			rects.AuthorRect = Rectangle.Empty;
			return;
		}

		if (large && e.Item.Package.Author is not null)
		{
			using var font = UI.Font(8.25F);
			var size = e.Graphics.Measure(e.Item.Package.Author.Name, font).ToSize();
			var authorRect = rects.AuthorRect.Align(new Size(size.Width + Padding.Horizontal + rects.AuthorRect.Height, rects.AuthorRect.Height - 2), ContentAlignment.TopLeft);
			var avatarRect = authorRect.Align(new(authorRect.Height - 2, authorRect.Height - 2), ContentAlignment.MiddleLeft).Pad(Padding);

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
		}
		else
		{
			rects.AuthorRect = e.DrawLabel(e.Item.Package.Author?.Name, IconManager.GetSmallIcon("I_Developer"), FormDesign.Design.ActiveColor.MergeColor(FormDesign.Design.ButtonColor, 30), rects.AuthorRect, ContentAlignment.TopLeft, true, mousePosition: CursorLocation);
		}
	}

	private void DrawThumbnailAndTitle(ItemPaintEventArgs<CompatibilityInfo, Rectangles> e, Rectangles rects, bool large)
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
		using var font = UI.Font(large ? 11.25F : 9.75F, FontStyle.Bold);
		var textSize = e.Graphics.Measure(text, font);

		using var brush = new SolidBrush(e.HoverState.HasFlag(HoverState.Pressed) ? FormDesign.Design.ActiveForeColor : (rects.CenterRect.Contains(CursorLocation) || rects.IconRect.Contains(CursorLocation)) && e.HoverState.HasFlag(HoverState.Hovered) ? FormDesign.Design.ActiveColor : base.ForeColor);
		e.Graphics.DrawString(text, font, brush, rects.TextRect.Pad(0, Padding.Top, -9999, 0), new StringFormat { Trimming = StringTrimming.EllipsisCharacter });

		if (tags is null)
		{
			return;
		}

		var tagRect = new Rectangle(rects.TextRect.X + (int)textSize.Width, rects.TextRect.Y, 0, (int)textSize.Height);

		foreach (var item in tags)
		{
			tagRect.X += Padding.Left + e.DrawLabel(item.Text, null, item.Color, tagRect, ContentAlignment.MiddleLeft, true).Width;
		}
	}

	private void Clicked(CompatibilityInfo info, ReportItem Message, PseudoPackage item, bool button)
	{
		var package = item.Package;

		if (!button)
		{
			if (Message.Type is ReportType.DlcMissing)
			{
				PlatformUtil.OpenUrl($"https://store.steampowered.com/app/{item.SteamId}");
			}
			else if (package is not null)
			{
				Program.MainForm.PushPanel(null, package.IsCollection ? new PC_ViewCollection(package) : new PC_PackagePage(package));
			}
			else
			{
				PlatformUtil.OpenUrl($"https://steamcommunity.com/workshop/filedetails/?id={item.SteamId}");
			}

			return;
		}

		var p = package?.Package;

		if (p is null)
		{
			//_subscribingTo.Add(item);

			Loading = true;
			
			SubscriptionsManager.Subscribe(new[] { item.SteamId });
		}
		else
		{
			p.IsIncluded = true;

			if (p.Mod is not null)
			{
				p.Mod.IsEnabled = true;
			}
		}

		switch (Message.Status.Action)
		{
			case StatusAction.SelectOne:
				foreach (var id in Message.Packages)
				{
					if (id != item)
					{
						var pp = id.Package;

						if (pp is not null)
						{
							pp.IsIncluded = false;
						}
					}
				}
				break;
			case StatusAction.Switch:
				info.Package.IsIncluded = false;
				info.Package.IsIncluded = false;
				break;
		}
	}

	private void GetAllButton(ReportItem Message, out string? allText, out string? allIcon, out ColorStyle colorStyle)
	{
		allText = null;
		allIcon = null;
		colorStyle = ColorStyle.Red;

		switch (Message.Status.Action)
		{
			case StatusAction.SubscribeToPackages:
				if (Message.Packages.Length > 1)
				{
					var max = Message.Packages.Max(x =>
					{
						var p = SteamUtil.GetItem(x)?.Package;

						if (p is null)
						{
							return 3;
						}
						else if (!p.IsIncluded)
						{
							return 2;
						}
						else if (!(p.Mod?.IsEnabled ?? true))
						{
							return 1;
						}

						return 0;
					});

					colorStyle = ColorStyle.Green;
					allText = max switch { 3 => Locale.SubscribeAll, 2 => Locale.IncludeAll, 1 => Locale.EnableAll, _ => null };
					allIcon = max switch { 3 => "I_Add", 2 => "I_Check", 1 => "I_Enabled", _ => null };
				}
				break;
			case StatusAction.RequiresConfiguration:
				allText = CompatibilityManager.IsSnoozed(Message) ? Locale.UnSnooze : Locale.Snooze;
				allIcon = "I_Snooze";
				colorStyle = ColorStyle.Active;
				break;
			case StatusAction.UnsubscribeThis:
				allText = Locale.Unsubscribe;
				allIcon = "I_RemoveSteam";
				break;
			case StatusAction.UnsubscribeOther:
				allText = Message.Packages.Length switch { 0 => null, 1 => Locale.Unsubscribe, _ => Locale.UnsubscribeAll };
				allIcon = "I_RemoveSteam";
				break;
			case StatusAction.ExcludeThis:
				allText = Locale.Exclude;
				allIcon = "I_X";
				break;
			case StatusAction.ExcludeOther:
				allText = Message.Packages.Length switch { 0 => null, 1 => Locale.Exclude, _ => Locale.ExcludeAll };
				allIcon = "I_X";
				break;
			case StatusAction.RequestReview:
				allText = LocaleCR.RequestReview;
				allIcon = "I_RequestReview";
				colorStyle = ColorStyle.Active;
				break;
		}
	}

	protected override Rectangles GenerateRectangles(CompatibilityInfo item, Rectangle rectangle)
	{
		var section = (ItemHeight / 3) - (Padding.Top / 2);
		var rects = new Rectangles(item);

		if (CentralManager.SessionSettings.UserSettings.AdvancedIncludeEnable && item.Package.Package?.Mod is not null)
		{
			rects.IncludedRect = rectangle.Pad(1 * Padding.Left, 0, 0, 0).Align(new Size(section + Padding.Horizontal, rectangle.Height), ContentAlignment.MiddleLeft);
			rects.EnabledRect = rects.IncludedRect.Pad(rects.IncludedRect.Width, 0, -rects.IncludedRect.Width, 0);
		}
		else
		{
			rects.IncludedRect = rectangle.Pad(1 * Padding.Left, 0, 0, 0).Align(new Size(section + Padding.Horizontal, rectangle.Height), ContentAlignment.MiddleLeft);
		}

		var buttonRectangle = rectangle.Pad(0, 0, Padding.Right, 0).Align(new Size(section, section), ContentAlignment.TopRight);

		rects.FolderRect = rectangle.Pad(Math.Max(rects.IncludedRect.Right, rects.EnabledRect.Right) + (2 * Padding.Left), 0, 0, 0).Align(new Size(section, section), ContentAlignment.BottomLeft);
		rects.SteamRect = rectangle.Pad(Math.Max(rects.IncludedRect.Right, rects.EnabledRect.Right) + (2 * Padding.Left), 0, 0, 0).Align(new Size(section, section), ContentAlignment.MiddleLeft);
		rects.IconRect = rectangle.Pad(Math.Max(rects.IncludedRect.Right, rects.EnabledRect.Right) + (2 * Padding.Left), 0, 0, 0).Align(new Size(section, section), ContentAlignment.TopLeft);
		rects.TextRect = rectangle.Pad(rects.IconRect.X + rects.IconRect.Width + Padding.Left, 0, (item.Package.Workshop ? (2 * Padding.Left) + (2 * buttonRectangle.Width) + (int)(100 * UI.FontScale) : 0) + rectangle.Width - buttonRectangle.X, rectangle.Height / 2);
		rects.AuthorRect = rects.FolderRect;
		rects.AuthorRect.X += rects.AuthorRect.Width + Padding.Left;
		rects.AuthorRect.Width = 0;
		rects.CenterRect = new Rectangle(rects.IconRect.Location, new Size(rectangle.Width - rects.IconRect.X, rects.IconRect.Height));
		rects.snoozeRect = rectangle.Align(rects.FolderRect.Size, ContentAlignment.BottomRight);

		return rects;
	}

	protected override void OnDragEnter(DragEventArgs drgevent)
	{
		base.OnDragEnter(drgevent);


		if (drgevent.Data.GetDataPresent(DataFormats.FileDrop))
		{
			var file = ((string[])drgevent.Data.GetData(DataFormats.FileDrop)).FirstOrDefault();

			if (Path.GetExtension(file).ToLower() is ".zip" or ".json")
			{
				drgevent.Effect = DragDropEffects.Copy;
				Invalidate();
			}
			return;
		}

		drgevent.Effect = DragDropEffects.None;
		Invalidate();
	}

	protected override void OnDragLeave(EventArgs e)
	{
		base.OnDragLeave(e);

		Invalidate();
	}

	protected override void OnDragDrop(DragEventArgs drgevent)
	{
		base.OnDragDrop(drgevent);

		var file = ((string[])drgevent.Data.GetData(DataFormats.FileDrop)).FirstOrDefault();

		if (file != null)
		{
			if (LocationManager.Platform is not Platform.Windows)
			{
				var realPath = IOUtil.ToRealPath(file);

				if (ExtensionClass.FileExists(realPath))
				{
					file = realPath!;
				}
			}

			(PanelContent.GetParentPanel(this) as PC_CompatibilityReport)!.Import(file);
		}

		Invalidate();
	}

	public class Rectangles : IDrawableItemRectangles<CompatibilityInfo>
	{
		internal Rectangle IncludedRect;
		internal Rectangle EnabledRect;
		internal Rectangle FolderRect;
		internal Rectangle IconRect;
		internal Rectangle TextRect;
		internal Rectangle SteamRect;
		internal Rectangle CenterRect;
		internal Rectangle AuthorRect;
		internal Rectangle VersionRect;
		internal Rectangle DateRect;
		internal Dictionary<PseudoPackage, Rectangle> buttonRects = new();
		internal Dictionary<PseudoPackage, Rectangle> modRects = new();
		internal Rectangle allButtonRect;
		internal Rectangle snoozeRect;

		public CompatibilityInfo Item { get; set; }

		public Rectangles(CompatibilityInfo item)
		{
			Item = item;
		}

		public bool GetToolTip(Control instance, Point location, out string text, out Point point)
		{
			if (Item.Package.Workshop)
			{
				if (SteamRect.Contains(location))
				{
					text = Locale.ViewOnSteam;
					point = SteamRect.Location;
					return true;
				}

				if (AuthorRect.Contains(location))
				{
					text = Locale.OpenAuthorPage;
					point = AuthorRect.Location;
					return true;
				}
			}

			if (FolderRect.Contains(location))
			{
				text = Locale.OpenLocalFolder;
				point = FolderRect.Location;
				return true;
			}

			if (Item.Package.Package?.Mod is not null)
			{
				if (IncludedRect.Contains(location))
				{
					text = Locale.ExcludeInclude;
					point = IncludedRect.Location;
					return true;
				}

				if (EnabledRect.Contains(location))
				{
					text = Locale.EnableDisable;
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
					text = Locale.ExcludeInclude;
					point = IncludedRect.Location;
					return true;
				}
			}

			if (CenterRect.Contains(location) || IconRect.Contains(location))
			{
				text = Locale.OpenPackagePage;
				point = CenterRect.Location;
				return true;
			}

			text = string.Empty;
			point = default;
			return false;
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
				DateRect.Contains(location) ||
				allButtonRect.Contains(location) ||
				snoozeRect.Contains(location) ||
				buttonRects.Values.Any(x => x.Contains(location)) ||
				modRects.Values.Any(x => x.Contains(location)) ||
				(VersionRect.Contains(location) && Item?.Package.Package?.Mod is not null);
		}
	}
}
