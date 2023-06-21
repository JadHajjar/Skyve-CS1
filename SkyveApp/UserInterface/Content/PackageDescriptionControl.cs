using Extensions;

using SkyveApp.Domain;
using SkyveApp.Domain.Compatibility.Enums;
using SkyveApp.Domain.Enums;
using SkyveApp.Domain.Interfaces;
using SkyveApp.Services.Interfaces;
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

namespace SkyveApp.UserInterface.Content;
internal class PackageDescriptionControl : SlickImageControl
{
	private Rectangles? rects;
	public IPackage? Package { get; private set; }
	public PC_PackagePage? PackagePage { get; private set; }

	private readonly ISubscriptionsManager _subscriptionsManager;
	private readonly ICompatibilityManager _compatibilityManager;
	private readonly ISettings _settings;

	public PackageDescriptionControl()
	{
		_subscriptionsManager = Program.Services.GetService<ISubscriptionsManager>();
		_settings = Program.Services.GetService<ISettings>();
		_compatibilityManager = Program.Services.GetService<ICompatibilityManager>();
	}

	public void SetPackage(IPackage package, PC_PackagePage? page)
	{
		PackagePage = page;
		Package = package;

		if (!string.IsNullOrWhiteSpace(Package.Author?.AvatarUrl))
		{
			Image = null;
			LoadImage(Package.Author?.AvatarUrl, Program.Services.GetService<IImageManager>().GetImage);
		}

		Invalidate();
	}

	protected override void UIChanged()
	{
		Padding = UI.Scale(new Padding(4), UI.FontScale);
	}

	protected override void OnMouseClick(MouseEventArgs e)
	{
		base.OnMouseClick(e);

		if (e.Button != MouseButtons.Left || rects is null || Package is null)
		{
			return;
		}

		if (rects.FolderRect.Contains(e.Location))
		{
			PlatformUtil.OpenFolder(Package.Folder);
			return;
		}

		if (Package.Workshop && rects.SteamRect.Contains(e.Location))
		{
			PlatformUtil.OpenUrl($"https://steamcommunity.com/workshop/filedetails?id={Package.SteamId}");
			return;
		}

		if (rects.SteamIdRect.Contains(e.Location))
		{
			if (Package.Workshop)
			{
				Clipboard.SetText(Package.SteamId.ToString());
			}
			else
			{
				Clipboard.SetText(Path.GetFileName(Package.Folder));
			}

			return;
		}

		if (Package.Workshop && rects.AuthorRect.Contains(e.Location) && Package.Author is not null)
		{
			var pc = new PC_UserPage(Package.Author?.SteamId ?? 0);

			(FindForm() as BasePanelForm)?.PushPanel(null, pc);

			return;
		}

		if (rects.MoreRect.Contains(e.Location))
		{
			var items = PC_PackagePage.GetRightClickMenuItems(Package);

			this.TryBeginInvoke(() => SlickToolStrip.Show(Program.MainForm, items));
		}

		if (e.Location.X > rects.SteamIdRect.X)
		{
			return;
		}

		if (Package.Package?.Mod is Mod mod)
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
				Clipboard.SetText(Package.Package.Mod.Version.GetString());
			}
		}
		else
		{
			if (rects.IncludedRect.Contains(e.Location))
			{
				if (Package.Package is null)
				{
					_subscriptionsManager.Subscribe(new[] { Package.SteamId });
					return;
				}

				Package.IsIncluded = !Package.IsIncluded;

				return;
			}
		}

		if (rects.ScoreRect.Contains(e.Location))
		{
			new RatingInfoForm { Icon = Program.MainForm?.Icon }.ShowDialog(Program.MainForm);
			return;
		}


		if (rects.CompatibilityRect.Contains(e.Location))
		{
			if (PackagePage is not null)
			{
				PackagePage.T_CR.Selected = true;
			}

			return;
		}

		if (rects.DateRect.Contains(e.Location))
		{
			var date = (Package.ServerTime == DateTime.MinValue && Package is Asset asset ? asset.LocalTime : Package.ServerTime).ToLocalTime();
			Clipboard.SetText(date.ToString("g"));
			return;
		}

		foreach (var tag in rects.TagRects)
		{
			if (tag.Value.Contains(e.Location))
			{
				Clipboard.SetText(tag.Key.Value);

				return;
			}
		}
	}

	protected override void OnMouseMove(MouseEventArgs e)
	{
		base.OnMouseMove(e);

		if (rects is null)
		{
			Cursor = Cursors.Default;
			return;
		}

		var location = e.Location;

		Cursor = rects.Contain(location) ? Cursors.Hand : Cursors.Default;

		if (rects.FolderRect.Contains(location))
		{
			setTip(Locale.OpenLocalFolder, rects.FolderRect);
			return;
		}

		if (Package!.Workshop)
		{
			if (rects.SteamRect.Contains(location))
			{
				setTip(Locale.ViewOnSteam, rects.SteamRect);
				return;
			}

			if (rects.SteamIdRect.Contains(location))
			{
				setTip(string.Format(Locale.CopyToClipboard, Package.SteamId), rects.SteamIdRect);
				return;
			}

			if (rects.AuthorRect.Contains(location))
			{
				setTip(Locale.OpenAuthorPage, rects.AuthorRect);
				return;
			}
		}

		else if (rects.SteamIdRect.Contains(location))
		{
			var folder = Path.GetFileName(Package.Folder);
			setTip(string.Format(Locale.CopyToClipboard, folder), rects.SteamIdRect);
			return;
		}

		if (e.Location.X > rects.SteamIdRect.X)
		{
			SlickTip.SetTo(this, string.Empty);
			return;
		}

		if (Package.Package?.Mod is not null)
		{
			if (rects.IncludedRect.Contains(location))
			{
				setTip($"{Locale.ExcludeInclude}\r\n\r\n{string.Format(Locale.ControlClickTo, Locale.FilterByThisIncludedStatus.ToString().ToLower())}", rects.IncludedRect);
			}

			if (rects.EnabledRect.Contains(location))
			{
				setTip($"{Locale.EnableDisable}\r\n\r\n{string.Format(Locale.ControlClickTo, Locale.FilterByThisEnabledStatus.ToString().ToLower())}", rects.EnabledRect);
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
				if (Package.Package != null)
				{
					setTip($"{Locale.ExcludeInclude}\r\n\r\n{string.Format(Locale.ControlClickTo, Locale.FilterByThisIncludedStatus.ToString().ToLower())}", rects.IncludedRect);
				}
				else
				{
					setTip(Locale.SubscribeToItem, rects.IncludedRect);
				}
			}
		}

		if (rects.ScoreRect.Contains(location))
		{
			setTip(string.Format(Locale.RatingCount, (Package!.PositiveVotes > Package.NegativeVotes ? '+' : '-') + Math.Abs(Package.PositiveVotes - (Package.NegativeVotes / 10) - Package.Reports).ToString("N0"), $"({SteamUtil.GetScore(Package)}%)") + "\r\n" + string.Format(Locale.SubscribersCount, Package.Subscriptions.ToString("N0")), rects.ScoreRect);
			return;
		}

		if (rects.CenterRect.Contains(location) || rects.IconRect.Contains(location))
		{
			setTip(Locale.OpenPackagePage, rects.CenterRect);
			return;
		}

		if (rects.CompatibilityRect.Contains(location))
		{
			setTip(Locale.ViewPackageCR, rects.CompatibilityRect);
			return;
		}

		if (rects.DateRect.Contains(location))
		{
			var date = (Package.ServerTime == DateTime.MinValue && Package is Asset asset ? asset.LocalTime : Package.ServerTime).ToLocalTime();
			setTip(string.Format(Locale.CopyToClipboard, date.ToString("g")), rects.DateRect);
			return;
		}

		foreach (var tag in rects.TagRects)
		{
			if (tag.Value.Contains(location))
			{
				setTip(string.Format(Locale.CopyToClipboard, tag.Key), tag.Value);
				return;
			}
		}

		SlickTip.SetTo(this, string.Empty);

		void setTip(string text, Rectangle rectangle) => SlickTip.SetTo(this, text, offset: rectangle.Location);
	}

	protected override void OnPaint(PaintEventArgs e)
	{
		if (PackagePage is not null)
		{
			e.Graphics.Clear(FormDesign.Design.BackColor);
			e.Graphics.FillRectangle(new SolidBrush(FormDesign.Design.AccentBackColor), ClientRectangle.Pad(0, 0, 0, Height / 2));
			e.Graphics.SetUp();
		}
		else
		{
			e.Graphics.SetUp(FormDesign.Design.AccentBackColor);
			e.Graphics.FillRoundedRectangle(new SolidBrush(FormDesign.Design.BackColor), ClientRectangle.Pad(1, (Height / 2) + 1, 1, 1), (int)(5 * UI.FontScale));
		}

		if (Package == null)
		{
			return;
		}

		var package = Package.Package;
		rects = new Rectangles() { Item = Package };

		DrawTitle(e, package);
		DrawButtons(e);

		var labelRect = ClientRectangle.Pad(0, Height / 2, 0, 0).Pad(Padding);

		var isVersion = package?.Mod is not null && !package.BuiltIn;
		var versionText = isVersion ? "v" + package!.Mod!.Version.GetString() : package?.BuiltIn ?? false ? Locale.Vanilla : (Package.FileSize == 0 ? string.Empty : Package.FileSize.SizeString());
		rects.VersionRect = DrawLabel(e, versionText, null, isVersion ? FormDesign.Design.YellowColor : FormDesign.Design.YellowColor.MergeColor(FormDesign.Design.BackColor, 40), labelRect, ContentAlignment.TopLeft, isVersion);
		labelRect.X += Padding.Left + rects.VersionRect.Width;

		var date = (Package.ServerTime == DateTime.MinValue && Package is Asset asset ? asset.LocalTime : Package.ServerTime).ToLocalTime();
		var dateText = _settings.SessionSettings.UserSettings.ShowDatesRelatively ? date.ToRelatedString(true, false) : date.ToString("g");
		rects.DateRect = DrawLabel(e, dateText, IconManager.GetSmallIcon("I_UpdateTime"), FormDesign.Design.AccentColor.MergeColor(FormDesign.Design.BackColor, 50), labelRect, ContentAlignment.TopLeft, true);
		labelRect.X += Padding.Left + rects.DateRect.Width;

		labelRect = DrawStatusDescriptor(e, rects, labelRect, ContentAlignment.TopLeft);

		var report = Package.GetCompatibilityInfo();
		if (report is not null && report.Notification > NotificationType.Info)
		{
			var labelColor = report.Notification.GetColor();

			rects.CompatibilityRect = DrawLabel(e, LocaleCR.Get($"{report.Notification}"), IconManager.GetSmallIcon("I_CompatibilityReport"), labelColor, labelRect, ContentAlignment.TopLeft, true);

			labelRect.X += Padding.Left + rects.CompatibilityRect.Width;
		}
		else
		{
			rects.CompatibilityRect = Rectangle.Empty;
		}

		labelRect = ClientRectangle.Pad(0, Height / 2, 0, 0).Pad(Padding);
		labelRect.Y += rects.VersionRect.Height + Padding.Vertical;

		labelRect.X += DrawScore(e, true, rects, labelRect);

		foreach (var item in Package.Tags.Distinct(x => x.Value))
		{
			using var tagIcon = IconManager.GetSmallIcon(item.Source switch { TagSource.Workshop => "I_Steam", TagSource.FindIt => "I_Search", _ => "I_Tag" });

			var tagRect = DrawLabel(e, item.Value, tagIcon, FormDesign.Design.ButtonColor, labelRect, ContentAlignment.TopLeft, true);

			rects.TagRects[item] = tagRect;

			labelRect.X += Padding.Left + tagRect.Width;
		}

		DrawAuthorAndSteamId(e, rects);
	}

	private void DrawButtons(PaintEventArgs e)
	{
		rects!.MoreRect = ClientRectangle.Pad(0, 0, 0, Height / 2).Pad(Padding).Align(UI.Scale(new Size(32, 32), UI.FontScale), ContentAlignment.BottomRight);
		rects.FolderRect = rects.MoreRect;
		rects.FolderRect.X -= rects.MoreRect.Width + Padding.Left;

		if (Package!.Workshop)
		{
			rects.SteamRect = rects.FolderRect;

			if (!string.IsNullOrEmpty(Package.Folder))
			{
				rects.SteamRect.X -= rects.FolderRect.Width + Padding.Left;
			}
			else
			{
				rects.FolderRect = Rectangle.Empty;
			}
		}

		var brushRect = new Rectangle(rects.SteamRect.X.If(0, rects.FolderRect.X) - (int)(150 * UI.FontScale), 0, (int)(150 * UI.FontScale), Height / 2);
		using (var brush = new LinearGradientBrush(brushRect, Color.Empty, FormDesign.Design.AccentBackColor, LinearGradientMode.Horizontal))
		{
			e.Graphics.FillRectangle(brush, brushRect);
			e.Graphics.FillRectangle(new SolidBrush(FormDesign.Design.AccentBackColor), new Rectangle(rects.SteamRect.X.If(0, rects.FolderRect.X), 0, Width, Height / 2));
		}

		using (var icon = IconManager.GetIcon("I_More", rects.MoreRect.Height / 2))
		{
			SlickButton.DrawButton(e, rects.MoreRect, string.Empty, Font, icon, null, rects.MoreRect.Contains(CursorLocation) ? HoverState & ~HoverState.Focused : HoverState.Normal);
		}

		if (rects.FolderRect != Rectangle.Empty)
		{
			using var icon = IconManager.GetIcon("I_Folder", rects.FolderRect.Height / 2);
			SlickButton.DrawButton(e, rects.FolderRect, string.Empty, Font, icon, null, rects.FolderRect.Contains(CursorLocation) ? HoverState & ~HoverState.Focused : HoverState.Normal);
		}

		if (Package!.Workshop)
		{
			using var icon = IconManager.GetIcon("I_Steam", rects.SteamRect.Height / 2);
			SlickButton.DrawButton(e, rects.SteamRect, string.Empty, Font, icon, null, rects.SteamRect.Contains(CursorLocation) ? HoverState & ~HoverState.Focused : HoverState.Normal);
		}
	}

	private void DrawTitle(PaintEventArgs e, Package? package)
	{
		List<(Color Color, string Text)>? tags = null;

		var mod = true;
		var text = mod ? Package!.CleanName(out tags) : Package!.ToString();
		using var font = UI.Font(15F, FontStyle.Bold);
		var textSize = e.Graphics.Measure(text, font);

		rects!.TextRect = ClientRectangle.Pad(0, 0, 0, Height / 2).Pad(Padding).Align(Size.Ceiling(textSize), ContentAlignment.BottomLeft);

		var partialIncluded = Package!.Package?.IsPartiallyIncluded() ?? false;
		var isIncluded = partialIncluded || Package.IsIncluded;

		if (mod && _settings.SessionSettings.UserSettings.AdvancedIncludeEnable)
		{
			rects!.IncludedRect = rects!.EnabledRect = ClientRectangle.Pad(Padding.Left, 0, 0, Height / 2).Pad(Padding).Align(UI.Scale(new Size(32, 32), UI.FontScale), ContentAlignment.BottomLeft).Pad(1, 1, 2, 2);
			rects!.EnabledRect.X += rects!.EnabledRect.Width;
			rects!.TextRect.X = rects!.EnabledRect.Right + Padding.Left;

			PaintIncludedButton(e, Rectangle.Union(rects.IncludedRect, rects.EnabledRect), isIncluded, partialIncluded, true, package);
		}
		else
		{
			rects!.IncludedRect = ClientRectangle.Pad(Padding.Left, 0, 0, Height / 2).Pad(Padding).Align(UI.Scale(new Size(32, 32), UI.FontScale), ContentAlignment.BottomLeft).Pad(1, 1, 2, 2);
			rects!.TextRect.X = rects!.IncludedRect.Right + Padding.Left;

			PaintIncludedButton(e, rects.IncludedRect, isIncluded, partialIncluded, true, package);
		}

		rects.TextRect.Y = rects!.IncludedRect.Y + ((rects.IncludedRect.Height - rects!.TextRect.Height) / 2);

		using var brush = new SolidBrush(FormDesign.Design.ForeColor);
		e.Graphics.DrawString(text, font, brush, rects.TextRect, new StringFormat { Trimming = StringTrimming.EllipsisCharacter });

		if (tags is null)
		{
			return;
		}

		var tagRect = new Rectangle(rects.TextRect.X + (int)textSize.Width, rects.TextRect.Y, 1, (int)textSize.Height);

		foreach (var item in tags)
		{
			tagRect.X += Padding.Left + DrawLabel(e, item.Text, null, item.Color, tagRect, ContentAlignment.MiddleLeft, title: true).Width;
		}
	}

	private void PaintIncludedButton(PaintEventArgs e, Rectangle inclEnableRect, bool isIncluded, bool partialIncluded, bool large, Package? package)
	{
		var incl = new DynamicIcon(partialIncluded ? "I_Slash" : isIncluded ? "I_Ok" : package is null ? "I_Add" : "I_Enabled");
		if (_settings.SessionSettings.UserSettings.AdvancedIncludeEnable && package?.Mod is Mod mod)
		{
			var enabl = new DynamicIcon(mod.IsEnabled ? "I_Checked" : "I_Checked_OFF");
			if (isIncluded)
			{
				using var brush = inclEnableRect.Gradient(Color.FromArgb(inclEnableRect.Contains(CursorLocation) ? 150 : 255, partialIncluded ? FormDesign.Design.YellowColor : mod.IsEnabled ? FormDesign.Design.GreenColor : FormDesign.Design.RedColor), 1.5F);
				e.Graphics.FillRoundedRectangle(brush, inclEnableRect, 4);
			}
			else if (mod.IsEnabled)
			{
				using var brush = inclEnableRect.Gradient(Color.FromArgb(inclEnableRect.Contains(CursorLocation) ? 150 : 255, FormDesign.Design.YellowColor), 1.5F);
				e.Graphics.FillRoundedRectangle(brush, inclEnableRect, 4);
			}
			else if (inclEnableRect.Contains(CursorLocation))
			{
				using var brush = inclEnableRect.Gradient(Color.FromArgb(20, ForeColor), 1.5F);
				e.Graphics.FillRoundedRectangle(brush, inclEnableRect, 4);
			}

			using var includedIcon = (large ? incl.Large : incl.Get(rects!.IncludedRect.Height / 2)).Color(rects!.IncludedRect.Contains(CursorLocation) ? FormDesign.Design.ActiveColor : isIncluded ? FormDesign.Design.ActiveForeColor : ForeColor);
			using var enabledIcon = (large ? enabl.Large : enabl.Get(rects.IncludedRect.Height / 2)).Color(rects.EnabledRect.Contains(CursorLocation) ? FormDesign.Design.ActiveColor : isIncluded ? FormDesign.Design.ActiveForeColor : base.ForeColor);
			e.Graphics.DrawImage(includedIcon, rects.IncludedRect.CenterR(includedIcon.Size));
			e.Graphics.DrawImage(enabledIcon, rects.EnabledRect.CenterR(enabledIcon.Size));
		}
		else
		{
			if (isIncluded)
			{
				using var brush = inclEnableRect.Gradient(Color.FromArgb(inclEnableRect.Contains(CursorLocation) ? 150 : 255, partialIncluded ? FormDesign.Design.YellowColor : FormDesign.Design.GreenColor), 1.5F);
				e.Graphics.FillRoundedRectangle(brush, inclEnableRect, 4);
			}
			else if (inclEnableRect.Contains(CursorLocation))
			{
				using var brush = inclEnableRect.Gradient(Color.FromArgb(20, ForeColor), 1.5F);
				e.Graphics.FillRoundedRectangle(brush, inclEnableRect, 4);
			}

			using var icon = (large ? incl.Large : incl.Get(rects!.IncludedRect.Height / 2)).Color(rects!.IncludedRect.Contains(CursorLocation) ? FormDesign.Design.ActiveColor : isIncluded ? FormDesign.Design.ActiveForeColor : ForeColor);
			e.Graphics.DrawImage(icon, inclEnableRect.CenterR(icon.Size));
		}
	}

	private void DrawAuthorAndSteamId(PaintEventArgs e, Rectangles rects)
	{
		if (!Package!.Workshop)
		{
			DrawBack(Width - (int)(100 * UI.FontScale));
			rects.SteamIdRect = DrawLabel(e, Path.GetFileName(Package.Folder), IconManager.GetSmallIcon("I_Folder"), FormDesign.Design.ActiveColor.MergeColor(FormDesign.Design.ButtonColor, 30), ClientRectangle.Pad(0, Height / 2, 0, 0).Pad(Padding), ContentAlignment.TopRight, true);
			rects.AuthorRect = Rectangle.Empty;
			return;
		}

		if (Package.Author is not null)
		{
			using var font = UI.Font(9.75F);
			var size = e.Graphics.Measure(Package.Author.Name, font).ToSize();
			var authorRect = ClientRectangle.Pad(0, Height / 2, 0, 0).Pad(Padding).Align(new Size(size.Width + Padding.Horizontal + Padding.Right + size.Height, size.Height + Padding.Vertical), ContentAlignment.TopRight);
			var avatarRect = authorRect.Pad(Padding).Align(new(size.Height, size.Height), ContentAlignment.MiddleLeft);

			rects.SteamIdRect = rects.AuthorRect = authorRect;
			rects.SteamIdRect.Y += authorRect.Height + Padding.Top;
			rects.SteamIdRect.Width += Padding.Right;

			DrawBack(authorRect.X);

			using var brush = new SolidBrush(FormDesign.Design.BackColor.Tint(Lum: FormDesign.Design.Type.If(FormDesignType.Dark, 4, -4)).MergeColor(FormDesign.Design.ActiveColor, authorRect.Contains(CursorLocation) ? 65 : 100));
			e.Graphics.FillRoundedRectangle(brush, authorRect, (int)(4 * UI.FontScale));

			using var brush1 = new SolidBrush(FormDesign.Design.ForeColor);
			e.Graphics.DrawString(Package.Author.Name, font, brush1, authorRect.Pad(avatarRect.Width + Padding.Horizontal, 0, 0, 0), new StringFormat { LineAlignment = StringAlignment.Center });

			var authorImg = Package.AuthorIconImage;

			if (authorImg is null)
			{
				using var authorIcon = Properties.Resources.I_AuthorIcon.Color(FormDesign.Design.IconColor);

				e.Graphics.DrawRoundImage(authorIcon, avatarRect);
			}
			else
			{
				e.Graphics.DrawRoundImage(authorImg, avatarRect);
			}

			if (_compatibilityManager.CompatibilityData.Authors.TryGet(Package.Author.SteamId)?.Verified ?? false)
			{
				var checkRect = avatarRect.Align(new Size(avatarRect.Height / 3, avatarRect.Height / 3), ContentAlignment.BottomRight);

				e.Graphics.FillEllipse(new SolidBrush(FormDesign.Design.GreenColor), checkRect.Pad(-(int)(2 * UI.FontScale)));

				using var img = IconManager.GetIcon("I_Check", checkRect.Height);
				e.Graphics.DrawImage(img.Color(Color.White), checkRect.Pad(0, 0, -1, -1));
			}
		}
		else
		{
			DrawBack(Width - (int)(100 * UI.FontScale));
			rects.SteamIdRect = ClientRectangle.Pad(0, Height / 2, 0, 0).Pad(Padding);
			rects.AuthorRect = Rectangle.Empty;
		}

		rects.SteamIdRect = DrawLabel(e, Package.SteamId.ToString(), IconManager.GetSmallIcon("I_Steam"), FormDesign.Design.ActiveColor.MergeColor(FormDesign.Design.ButtonColor, 30), rects.SteamIdRect, ContentAlignment.TopRight, true);

		void DrawBack(int x)
		{
			var brushRect = new Rectangle(x - (int)(100 * UI.FontScale), Height / 2, (int)(100 * UI.FontScale), Height / 2);
			using var brush = new LinearGradientBrush(brushRect, Color.Empty, FormDesign.Design.BackColor, LinearGradientMode.Horizontal);

			e.Graphics.FillRectangle(brush, brushRect);
			e.Graphics.FillRoundedRectangle(new SolidBrush(FormDesign.Design.BackColor), new Rectangle(x, Height / 2, Width - x, Height / 2), (int)(5 * UI.FontScale), false, botLeft: false);
		}
	}

	private Rectangle DrawStatusDescriptor(PaintEventArgs e, Rectangles rects, Rectangle labelRect, ContentAlignment contentAlignment)
	{
		if (!Package!.Workshop)
		{
			labelRect.X += Padding.Left + DrawLabel(e, Locale.Local, IconManager.GetSmallIcon("I_PC"), FormDesign.Design.YellowColor.MergeColor(FormDesign.Design.AccentColor).MergeColor(FormDesign.Design.BackColor, 65), labelRect, contentAlignment, true).Width;
		}

		GetStatusDescriptors(Package!, out var text, out var icon, out var color);

		if (!string.IsNullOrEmpty(text))
		{
			using (icon)
			{
				rects.DownloadStatusRect = DrawLabel(e, text, icon, color.MergeColor(FormDesign.Design.BackColor, 65), labelRect, contentAlignment, false);
			}

			labelRect.X += Padding.Left + rects.DownloadStatusRect.Width;
		}
		else
		{
			rects.DownloadStatusRect = Rectangle.Empty;
		}

		return labelRect;
	}

	private void GetStatusDescriptors(IPackage mod, out string text, out Bitmap? icon, out Color color)
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

	private Rectangle DrawLabel(PaintEventArgs e, string? text, Bitmap? icon, Color color, Rectangle rectangle, ContentAlignment alignment, bool action = false, bool title = false)
	{
		if (text == null)
		{
			return Rectangle.Empty;
		}

		using var font = UI.Font(9F, title ? FontStyle.Bold : FontStyle.Regular);
		var size = e.Graphics.Measure(text, font).ToSize();

		if (icon is not null)
		{
			size.Width += icon.Width + Padding.Left;
		}

		size.Width += Padding.Left;

		rectangle = rectangle.Pad(title ? Padding.Left / 2 : Padding.Left).Align(size, alignment);

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

	private int DrawScore(PaintEventArgs e, bool large, Rectangles rects, Rectangle labelRect)
	{
		var score = SteamUtil.GetScore(Package!);

		if (Package!.Workshop && score != -1)
		{
			var clip = ClientRectangle;
			var labelH = (int)e.Graphics.Measure(" ", UI.Font(large ? 9F : 7.5F)).Height - 1;
			labelH -= labelH % 2;
			var small = UI.FontScale < 1.25;
			var scoreRect = rects.ScoreRect = labelRect.Pad(Padding).Align(new Size(labelH, labelH), ContentAlignment.TopLeft);
			var backColor = score > 90 && Package.Subscriptions >= 50000 ? FormDesign.Modern.ActiveColor : FormDesign.Design.GreenColor.MergeColor(FormDesign.Design.RedColor, score).MergeColor(FormDesign.Design.BackColor, 75);

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

			if (Package.Subscriptions < 50000 || score <= 90)
			{
				if (small)
				{
					using var scoreIcon = IconManager.GetSmallIcon("I_Vote");

					e.Graphics.SetClip(scoreRect.CenterR(scoreIcon.Size).Pad(0, scoreIcon.Height - (scoreIcon.Height * Package.Subscriptions / 15000), 0, 0));
					e.Graphics.DrawImage(scoreIcon.Color(FormDesign.Modern.ActiveColor), scoreRect.CenterR(scoreIcon.Size));
					e.Graphics.SetClip(clip);
				}
				else
				{
					using var pen = new Pen(Color.FromArgb(score >= 75 ? 255 : 200, FormDesign.Modern.ActiveColor), (float)(1.5 * UI.FontScale)) { EndCap = LineCap.Round, StartCap = LineCap.Round };
					e.Graphics.DrawArc(pen, scoreRect.Pad(-1), 90 - (Math.Min(360, 360F * Package.Subscriptions / 15000) / 2), Math.Min(360, 360F * Package.Subscriptions / 15000));
				}
			}

			return labelH + Padding.Left;
		}

		return 0;
	}

	private class Rectangles
	{
		internal IPackage? Item;

		internal Dictionary<TagItem, Rectangle> TagRects = new();
		internal Rectangle IncludedRect;
		internal Rectangle EnabledRect;
		internal Rectangle FolderRect;
		internal Rectangle MoreRect;
		internal Rectangle IconRect;
		internal Rectangle TextRect;
		internal Rectangle SteamRect;
		internal Rectangle SteamIdRect;
		internal Rectangle CenterRect;
		internal Rectangle AuthorRect;
		internal Rectangle ScoreRect;
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
				MoreRect.Contains(location) ||
				SteamRect.Contains(location) ||
				AuthorRect.Contains(location) ||
				IconRect.Contains(location) ||
				CompatibilityRect.Contains(location) ||
				DateRect.Contains(location) ||
				(VersionRect.Contains(location) && Item?.Package?.Mod is not null) ||
				TagRects.Any(x => x.Value.Contains(location)) ||
				SteamIdRect.Contains(location);
		}
	}
}
