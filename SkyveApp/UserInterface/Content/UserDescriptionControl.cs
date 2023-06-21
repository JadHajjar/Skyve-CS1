using Extensions;

using SkyveApp.Domain;
using SkyveApp.Domain.Compatibility.Enums;
using SkyveApp.Domain.Enums;
using SkyveApp.Domain.Interfaces;
using SkyveApp.Domain.Steam;
using SkyveApp.Services;
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
internal class UserDescriptionControl : SlickImageControl
{
	private Rectangles? rects;
	public SteamUser? User { get; private set; }
	public PC_UserPage? UserPage { get; private set; }

	private readonly IContentManager _contentManager;
	private readonly ICompatibilityManager _compatibilityManager;

    public UserDescriptionControl()
    {
		_contentManager = Program.Services.GetService<IContentManager>();
		_compatibilityManager = Program.Services.GetService<ICompatibilityManager>();
	}

    public void SetUser(SteamUser user, PC_UserPage? page)
	{
		UserPage = page;
		User = user;

		Invalidate();
	}

	protected override void UIChanged()
	{
		Padding = UI.Scale(new Padding(4), UI.FontScale);
	}

	//protected override void OnMouseClick(MouseEventArgs e)
	//{
	//	base.OnMouseClick(e);

	//	if (e.Button != MouseButtons.Left || rects is null || User is null)
	//	{
	//		return;
	//	}

	//	if (rects.FolderRect.Contains(e.Location))
	//	{
	//		PlatformUtil.OpenFolder(Package.Folder);
	//		return;
	//	}

	//	if (Package.Workshop && rects.SteamRect.Contains(e.Location))
	//	{
	//		PlatformUtil.OpenUrl($"https://steamcommunity.com/workshop/filedetails?id={Package.SteamId}");
	//		return;
	//	}

	//	if (rects.SteamIdRect.Contains(e.Location))
	//	{
	//		if (Package.Workshop)
	//		{
	//			Clipboard.SetText(Package.SteamId.ToString());
	//		}
	//		else
	//		{
	//			Clipboard.SetText(Path.GetFileName(Package.Folder));
	//		}

	//		return;
	//	}

	//	if (Package.Workshop && rects.AuthorRect.Contains(e.Location) && Package.Author is not null)
	//	{
	//		PlatformUtil.OpenUrl($"{Package.Author.ProfileUrl}myworkshopfiles");

	//		return;
	//	}

	//	if (rects.MoreRect.Contains(e.Location))
	//	{
	//		var items = PC_PackagePage.GetRightClickMenuItems(Package);

	//		this.TryBeginInvoke(() => SlickToolStrip.Show(Program.MainForm, items));
	//	}

	//	if (e.Location.X > rects.SteamIdRect.X)
	//	{
	//		return;
	//	}

	//	if (Package.Package?.Mod is Mod mod)
	//	{
	//		if (rects.IncludedRect.Contains(e.Location))
	//		{
	//			mod.IsIncluded = !mod.IsIncluded;

	//			return;
	//		}

	//		if (rects.EnabledRect.Contains(e.Location))
	//		{
	//			mod.IsEnabled = !mod.IsEnabled;

	//			return;
	//		}

	//		if (rects.VersionRect.Contains(e.Location))
	//		{
	//			Clipboard.SetText(Package.Package.Mod.Version.GetString());
	//		}
	//	}
	//	else
	//	{
	//		if (rects.IncludedRect.Contains(e.Location))
	//		{
	//			if (Package.Package is null)
	//			{
	//				SubscriptionsManager.Subscribe(new[] { Package.SteamId });
	//				return;
	//			}

	//			Package.IsIncluded = !Package.IsIncluded;

	//			return;
	//		}
	//	}

	//	if (rects.ScoreRect.Contains(e.Location))
	//	{
	//		new RatingInfoForm { Icon = Program.MainForm?.Icon }.ShowDialog(Program.MainForm);
	//		return;
	//	}


	//	if (rects.CompatibilityRect.Contains(e.Location))
	//	{
	//		if (UserPage is not null)
	//		{
	//			UserPage.T_CR.Selected = true;
	//		}

	//		return;
	//	}

	//	if (rects.DateRect.Contains(e.Location))
	//	{
	//		var date = (Package.ServerTime == DateTime.MinValue && Package is Asset asset ? asset.LocalTime : Package.ServerTime).ToLocalTime();
	//		Clipboard.SetText(date.ToString("g"));
	//		return;
	//	}

	//	foreach (var tag in rects.TagRects)
	//	{
	//		if (tag.Value.Contains(e.Location))
	//		{
	//			Clipboard.SetText(tag.Key.Value);

	//			return;
	//		}
	//	}
	//}

	//protected override void OnMouseMove(MouseEventArgs e)
	//{
	//	base.OnMouseMove(e);

	//	if (rects is null)
	//	{
	//		Cursor = Cursors.Default;
	//		return;
	//	}

	//	var location = e.Location;

	//	Cursor = rects.Contain(location) ? Cursors.Hand : Cursors.Default;

	//	if (rects.FolderRect.Contains(location))
	//	{
	//		setTip(Locale.OpenLocalFolder, rects.FolderRect);
	//		return;
	//	}

	//	if (Package!.Workshop)
	//	{
	//		if (rects.SteamRect.Contains(location))
	//		{
	//			setTip(Locale.ViewOnSteam, rects.SteamRect);
	//			return;
	//		}

	//		if (rects.SteamIdRect.Contains(location))
	//		{
	//			setTip(string.Format(Locale.CopyToClipboard, Package.SteamId), rects.SteamIdRect);
	//			return;
	//		}

	//		if (rects.AuthorRect.Contains(location))
	//		{
	//			setTip(Locale.OpenAuthorPage, rects.AuthorRect);
	//			return;
	//		}
	//	}

	//	else if (rects.SteamIdRect.Contains(location))
	//	{
	//		var folder = Path.GetFileName(Package.Folder);
	//		setTip(string.Format(Locale.CopyToClipboard, folder), rects.SteamIdRect);
	//		return;
	//	}

	//	if (e.Location.X > rects.SteamIdRect.X)
	//	{
	//		SlickTip.SetTo(this, string.Empty);
	//		return;
	//	}

	//	if (Package.Package?.Mod is not null)
	//	{
	//		if (rects.IncludedRect.Contains(location))
	//		{
	//			setTip($"{Locale.ExcludeInclude}\r\n\r\n{string.Format(Locale.ControlClickTo, Locale.FilterByThisIncludedStatus.ToString().ToLower())}", rects.IncludedRect);
	//		}

	//		if (rects.EnabledRect.Contains(location))
	//		{
	//			setTip($"{Locale.EnableDisable}\r\n\r\n{string.Format(Locale.ControlClickTo, Locale.FilterByThisEnabledStatus.ToString().ToLower())}", rects.EnabledRect);
	//		}

	//		if (rects.VersionRect.Contains(location))
	//		{
	//			setTip(Locale.CopyVersionNumber, rects.VersionRect);
	//		}
	//	}
	//	else
	//	{
	//		if (rects.IncludedRect.Contains(location))
	//		{
	//			if (Package.Package != null)
	//			{
	//				setTip($"{Locale.ExcludeInclude}\r\n\r\n{string.Format(Locale.ControlClickTo, Locale.FilterByThisIncludedStatus.ToString().ToLower())}", rects.IncludedRect);
	//			}
	//			else
	//			{
	//				setTip(Locale.SubscribeToItem, rects.IncludedRect);
	//			}
	//		}
	//	}

	//	if (rects.ScoreRect.Contains(location))
	//	{
	//		setTip(string.Format(Locale.RatingCount, (Package!.PositiveVotes > Package.NegativeVotes ? '+' : '-') + Math.Abs(Package.PositiveVotes - (Package.NegativeVotes / 10) - Package.Reports).ToString("N0"), $"({SteamUtil.GetScore(Package)}%)") + "\r\n" + string.Format(Locale.SubscribersCount, Package.Subscriptions.ToString("N0")), rects.ScoreRect);
	//		return;
	//	}

	//	if (rects.CenterRect.Contains(location) || rects.IconRect.Contains(location))
	//	{
	//		setTip(Locale.OpenPackagePage, rects.CenterRect);
	//		return;
	//	}

	//	if (rects.CompatibilityRect.Contains(location))
	//	{
	//		setTip(Locale.ViewPackageCR, rects.CompatibilityRect);
	//		return;
	//	}

	//	if (rects.DateRect.Contains(location))
	//	{
	//		var date = (Package.ServerTime == DateTime.MinValue && Package is Asset asset ? asset.LocalTime : Package.ServerTime).ToLocalTime();
	//		setTip(string.Format(Locale.CopyToClipboard, date.ToString("g")), rects.DateRect);
	//		return;
	//	}

	//	foreach (var tag in rects.TagRects)
	//	{
	//		if (tag.Value.Contains(location))
	//		{
	//			setTip(string.Format(Locale.CopyToClipboard, tag.Key), tag.Value);
	//			return;
	//		}
	//	}

	//	SlickTip.SetTo(this, string.Empty);

	//	void setTip(string text, Rectangle rectangle) => SlickTip.SetTo(this, text, offset: rectangle.Location);
	//}

	protected override void OnPaint(PaintEventArgs e)
	{
		if (UserPage is not null)
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

		if (User == null)
		{
			return;
		}

		rects = new Rectangles() { Item = User };

		DrawTitle(e);
		DrawButtons(e);

		var count = _contentManager.Packages.Count(x => x.Author?.SteamId == User.SteamId);

		if (count == 0)
			return;

		var text = Locale.YouHavePackagesUser.FormatPlural(count, User.Name);
		using var font = UI.Font(9F);
		var textSize = e.Graphics.Measure(text, font);

		rects!.TextRect = ClientRectangle.Pad(0, Height / 2, 0, 0).Pad(Padding).Align(Size.Ceiling(textSize), ContentAlignment.TopLeft);

		using var brush = new SolidBrush(FormDesign.Design.InfoColor);
		e.Graphics.DrawString(text, font, brush, rects.TextRect, new StringFormat { Trimming = StringTrimming.EllipsisCharacter });
	}

	private void DrawButtons(PaintEventArgs e)
	{
		rects!.SteamRect = ClientRectangle.Pad(0, 0, 0, Height / 2).Pad(Padding).Align(UI.Scale(new Size(24, 24), UI.FontScale), ContentAlignment.BottomRight);

		using var icon = IconManager.GetIcon("I_Steam", rects.SteamRect.Height / 2);
		SlickButton.DrawButton(e, rects.SteamRect, string.Empty, Font, icon, null, rects.SteamRect.Contains(CursorLocation) ? HoverState & ~HoverState.Focused : HoverState.Normal);
	}

	private void DrawTitle(PaintEventArgs e)
	{
		var text = User!.Name;
		using var font = UI.Font(11.25F, FontStyle.Bold);
		var textSize = e.Graphics.Measure(text, font);

		rects!.TextRect = ClientRectangle.Pad(0, 0, 0, Height / 2).Pad(Padding).Align(Size.Ceiling(textSize), ContentAlignment.BottomLeft);

		using var brush = new SolidBrush(FormDesign.Design.ForeColor);
		e.Graphics.DrawString(text, font, brush, rects.TextRect, new StringFormat { Trimming = StringTrimming.EllipsisCharacter });

		if (_compatibilityManager.CompatibilityData.Authors.TryGet(User.SteamId)?.Verified ?? false)
		{
			var checkRect = rects.TextRect.Align(UI.Scale(new Size(16, 16), UI.FontScale), ContentAlignment.MiddleLeft);
			checkRect.X += rects.TextRect.Width;

			e.Graphics.FillEllipse(new SolidBrush(FormDesign.Design.GreenColor), checkRect);

			checkRect = checkRect.Pad((int)(3 * UI.FontScale));

			using var img = IconManager.GetIcon("I_Check", checkRect.Height);
			e.Graphics.DrawImage(img.Color(Color.White), checkRect.Pad(0, 0, -1, -1));
		}
	}

	private class Rectangles
	{
		internal SteamUser Item;

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
				TagRects.Any(x => x.Value.Contains(location)) ||
				SteamIdRect.Contains(location);
		}
	}
}
