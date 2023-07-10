using SkyveApp.Systems.Compatibility.Domain.Api;
using SkyveApp.Systems.CS1.Utilities;

using System.Drawing;
using System.IO;

namespace SkyveApp.UserInterface.Lists;

partial class ItemListControl<T>
{
	protected override void OnPaintItemGrid(ItemPaintEventArgs<T, ItemListControl<T>.Rectangles> e)
	{
		var localPackage = e.Item.LocalPackage;
		var localParentPackage = localPackage?.LocalParentPackage;
		var workshopInfo = e.Item.GetWorkshopInfo();
		var isPressed = e.HoverState.HasFlag(HoverState.Pressed);
		var textColor = FormDesign.Design.ForeColor;

		e.HoverState &= ~HoverState.Pressed;

		base.OnPaintItemGrid(e);

		DrawThumbnail(e);
		DrawTitleAndTagsAndVersion(e, localParentPackage, workshopInfo);

		if (workshopInfo?.Author is not null)
		{
			DrawAuthor(e, workshopInfo.Author);
		}
		else if (e.Item.IsLocal)
		{
			DrawFolderName(e, localParentPackage!);
		}
	}

	private void DrawFolderName(ItemPaintEventArgs<T, ItemListControl<T>.Rectangles> e, ILocalPackageWithContents package)
	{
		using var font = UI.Font(8.25F, FontStyle.Bold);
		var height = e.Rects.IconRect.Bottom - Math.Max(e.Rects.TextRect.Bottom, Math.Max(e.Rects.VersionRect.Bottom, e.Rects.DateRect.Bottom)) - GridPadding.Bottom;
		var size = e.Graphics.Measure(Path.GetFileName(package.Folder), font).ToSize();
		size = new Size(size.Width + GridPadding.Left + height, height);
		var folderRect = new Rectangle(e.Rects.TextRect.X, e.Rects.IconRect.Bottom - size.Height, size.Width, size.Height);
		var iconRect = folderRect.Pad(GridPadding).Align(new(height * 3 / 4, height * 3 / 4), ContentAlignment.MiddleLeft);
		using var brush = new SolidBrush(Color.FromArgb(100, folderRect.Contains(CursorLocation) ? FormDesign.Design.ActiveColor : FormDesign.Design.AccentColor));
		e.Graphics.FillRoundedRectangle(brush, folderRect, (int)(5 * UI.FontScale));
		using var textBrush = new SolidBrush(FormDesign.Design.ForeColor);
		e.Graphics.DrawString(Path.GetFileName(package.Folder), font, textBrush, folderRect.Pad(iconRect.Width + GridPadding.Horizontal, 0, 0, 0), new StringFormat { LineAlignment = StringAlignment.Center });

		using var folderIcon = IconManager.GetIcon("I_Folder", iconRect.Height).Color(FormDesign.Design.IconColor);

		e.Graphics.DrawImage(folderIcon, iconRect.CenterR(folderIcon.Size));

		e.Rects.FolderRect = folderRect;
	}

	private void DrawAuthor(ItemPaintEventArgs<T, ItemListControl<T>.Rectangles> e, IUser author)
	{
		using var font = UI.Font(8.25F, FontStyle.Bold);
		var height = e.Rects.IconRect.Bottom - Math.Max(e.Rects.TextRect.Bottom, Math.Max(e.Rects.VersionRect.Bottom, e.Rects.DateRect.Bottom)) - GridPadding.Bottom;
		var size = e.Graphics.Measure(author!.Name, font).ToSize();
		size = new Size(size.Width + GridPadding.Left + height, height);
		var authorRect = new Rectangle(e.Rects.TextRect.X, e.Rects.IconRect.Bottom - size.Height, size.Width, size.Height);
		var avatarRect = authorRect.Pad(GridPadding).Align(new(height * 3 / 4, height * 3 / 4), ContentAlignment.MiddleLeft);
		using var brush = new SolidBrush(Color.FromArgb(100, authorRect.Contains(CursorLocation) ? FormDesign.Design.ActiveColor : FormDesign.Design.AccentColor));
		e.Graphics.FillRoundedRectangle(brush, authorRect, (int)(5 * UI.FontScale));
		using var textBrush = new SolidBrush(FormDesign.Design.ForeColor);
		e.Graphics.DrawString(author.Name, font, textBrush, authorRect.Pad(avatarRect.Width + GridPadding.Horizontal, 0, 0, 0), new StringFormat { LineAlignment = StringAlignment.Center });

		var authorImg = author.GetUserAvatar();

		if (authorImg is null)
		{
			using var authorIcon = Properties.Resources.I_AuthorIcon.Color(FormDesign.Design.IconColor);

			e.Graphics.DrawRoundImage(authorIcon, avatarRect);
		}
		else
		{
			e.Graphics.DrawRoundImage(authorImg, avatarRect);
		}

		if (_compatibilityManager.IsUserVerified(author))
		{
			var checkRect = avatarRect.Align(new Size(avatarRect.Height / 3, avatarRect.Height / 3), ContentAlignment.BottomRight);

			e.Graphics.FillEllipse(new SolidBrush(FormDesign.Design.GreenColor), checkRect.Pad(-(int)(2 * UI.FontScale)));

			using var img = IconManager.GetIcon("I_Check", checkRect.Height);

			e.Graphics.DrawImage(img.Color(Color.White), checkRect.Pad(0, 0, -1, -1));
		}

		e.Rects.AuthorRect = authorRect;
	}

	private void DrawTitleAndTagsAndVersion(ItemPaintEventArgs<T, ItemListControl<T>.Rectangles> e, ILocalPackageWithContents? localParentPackage, IWorkshopInfo? workshopInfo)
	{
		using var font = UI.Font(10.5F, FontStyle.Bold);
		var tags = new List<(Color Color, string Text)>();
		var mod = e.Item is not IAsset;
		var text = mod ? e.Item.CleanName(out tags) : e.Item.ToString();
		using var brush = new SolidBrush(IsPackagePage ? base.ForeColor : e.HoverState.HasFlag(HoverState.Pressed) ? FormDesign.Design.ActiveForeColor : (e.Rects.CenterRect.Contains(CursorLocation) || e.Rects.IconRect.Contains(CursorLocation)) && e.HoverState.HasFlag(HoverState.Hovered) ? FormDesign.Design.ActiveColor : base.ForeColor);
		e.Graphics.DrawString(text, font, brush, e.Rects.TextRect, new StringFormat { Trimming = StringTrimming.EllipsisCharacter });

		var isVersion = localParentPackage?.Mod is not null && !localParentPackage.IsBuiltIn && !IsPackagePage;
		var versionText = isVersion ? "v" + localParentPackage!.Mod!.Version.GetString() : (localParentPackage?.IsBuiltIn ?? false) ? Locale.Vanilla : (e.Item is ILocalPackage lp ? lp.LocalSize.SizeString() : workshopInfo?.ServerSize.SizeString());
		var date = workshopInfo?.ServerTime ?? e.Item.LocalParentPackage?.LocalTime;

		if (!string.IsNullOrEmpty(versionText))
		{
			tags.Insert(0, (isVersion ? FormDesign.Design.YellowColor : FormDesign.Design.YellowColor.MergeColor(FormDesign.Design.BackColor, 40), versionText!));
		}

		var tagRect = new Rectangle(e.Rects.TextRect.X, e.Rects.TextRect.Bottom + GridPadding.Bottom, 0, 0);

		for (var i = 0; i < tags.Count; i++)
		{
			var rect = e.DrawLabel(tags[i].Text, null, tags[i].Color, tagRect, ContentAlignment.TopLeft, smaller: true, mousePosition: i == 0 && !string.IsNullOrEmpty(versionText) ? null : CursorLocation);

			if (i == 0 && !string.IsNullOrEmpty(versionText))
			{
				e.Rects.VersionRect = rect;
			}

			tagRect.X += GridPadding.Left + rect.Width;
		}

		if (date.HasValue && !IsPackagePage)
		{
			var dateText = _settings.UserSettings.ShowDatesRelatively ? date.Value.ToRelatedString(true, false) : date.Value.ToString("g");

			e.Rects.DateRect = e.DrawLabel(dateText, IconManager.GetSmallIcon("I_UpdateTime"), FormDesign.Design.AccentColor.MergeColor(FormDesign.Design.BackColor, 50), tagRect, ContentAlignment.TopLeft, smaller: true, mousePosition: CursorLocation);
		}
	}

	private void DrawThumbnail(ItemPaintEventArgs<T, ItemListControl<T>.Rectangles> e)
	{
		var thumbnail = e.Item.GetThumbnail();

		if (thumbnail is null)
		{
			using var generic = (e.Item is ILocalPackageWithContents ? Properties.Resources.I_CollectionIcon : e.Item.IsMod ? Properties.Resources.I_ModIcon : Properties.Resources.I_AssetIcon).Color(FormDesign.Design.IconColor);

			drawThumbnail(generic);
		}
		else if (e.Item.IsLocal)
		{
			using var unsatImg = new Bitmap(thumbnail, e.Rects.IconRect.Size).Tint(Sat: 0);

			drawThumbnail(unsatImg);
		}
		else
		{
			drawThumbnail(thumbnail);
		}

		void drawThumbnail(Bitmap generic) => e.Graphics.DrawRoundedImage(generic, e.Rects.IconRect, (int)(5 * UI.FontScale), FormDesign.Design.AccentBackColor);
	}

	private ItemListControl<T>.Rectangles GenerateGridRectangles(T item, Rectangle rectangle)
	{
		var rects = new Rectangles(item)
		{
			IconRect = rectangle.Align(UI.Scale(new Size(64, 64), UI.UIScale), ContentAlignment.TopLeft)
		};

		rects.CenterRect = rects.TextRect = rectangle.Pad(rects.IconRect.Width + GridPadding.Left, 0, 0, rectangle.Height).AlignToFontSize(UI.Font(10.5F, FontStyle.Bold), ContentAlignment.TopLeft);

		return rects;
	}
}
