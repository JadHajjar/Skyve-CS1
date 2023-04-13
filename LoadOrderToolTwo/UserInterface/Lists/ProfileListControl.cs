using Extensions;

using LoadOrderToolTwo.Domain;
using LoadOrderToolTwo.Utilities;
using LoadOrderToolTwo.Utilities.IO;
using LoadOrderToolTwo.Utilities.Managers;

using SlickControls;

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace LoadOrderToolTwo.UserInterface.Lists;
internal class ProfileListControl : SlickStackedListControl<Profile>
{
	public IEnumerable<Profile> FilteredItems => SafeGetItems().Select(x => x.Item);

	public ProfileListControl()
	{
		HighlightOnHover = true;
		SeparateWithLines = true;

		Loading = !ProfileManager.ProfilesLoaded;
		if(!Loading)
			SetItems(ProfileManager.Profiles.Where(x => !x.Temporary));
		ProfileManager.ProfileUpdated += ProfileManager_ProfileUpdated;
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing)
		{
			ProfileManager.ProfileUpdated -= ProfileManager_ProfileUpdated;
		}

		base.Dispose(disposing);
	}

	private void ProfileManager_ProfileUpdated()
	{
		if (Loading)
		{
			Loading = false;

			SetItems(ProfileManager.Profiles.Where(x => !x.Temporary));
		}
		else
		{
			Invalidate();
		}
	}

	protected override void UIChanged()
	{
		ItemHeight = 36;

		base.UIChanged();

		Padding = UI.Scale(new Padding(3, 2, 3, 2), UI.FontScale);
	}

	protected override IEnumerable<DrawableItem<Profile>> OrderItems(IEnumerable<DrawableItem<Profile>> items)
	{
		return items;
	}

	protected override bool IsItemActionHovered(DrawableItem<Profile> item, Point location)
	{
		var rects = GetActionRectangles(item.Bounds);

		if (rects.IncludedRect.Contains(location))
		{
			setTip(Locale.ExcludeInclude, rects.IncludedRect);
			return true;
		}
		else if (rects.FolderRect.Contains(location))
		{
			setTip(Locale.ViewOnSteam, rects.FolderRect);
			return true;
		}

		void setTip(string text, Rectangle rectangle) => SlickTip.SetTo(this, text, timeout: 20000, offset: new Point(rectangle.X, item.Bounds.Y));

		return false;
	}

	protected override void OnItemMouseClick(DrawableItem<Profile> item, MouseEventArgs e)
	{
		base.OnItemMouseClick(item, e);

		var rects = GetActionRectangles(item.Bounds);

		if (rects.IncludedRect.Contains(e.Location))
		{
			item.Item.IsFavorite = !item.Item.IsFavorite;
		}

		if (rects.FolderRect.Contains(e.Location))
		{
			PlatformUtil.OpenFolder(ProfileManager.GetFileName(item.Item));
		}
	}

	protected override void OnPaint(PaintEventArgs e)
	{
		if (Loading)
		{
			base.OnPaint(e);
		}
		else if (!Items.Any())
		{
			e.Graphics.DrawString(Locale.NoDlcsNoInternet, UI.Font(9.75F, FontStyle.Italic), new SolidBrush(ForeColor), ClientRectangle, new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center });
		}
		else if (!SafeGetItems().Any())
		{
			e.Graphics.DrawString(Locale.NoDlcsOpenGame, UI.Font(9.75F, FontStyle.Italic), new SolidBrush(ForeColor), ClientRectangle, new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center });
		}
		else
		{
			base.OnPaint(e);
		}
	}

	protected override void OnPaintItem(ItemPaintEventArgs<Profile> e)
	{
		var large = DoubleSizeOnHover && (e.HoverState.HasFlag(HoverState.Hovered) || e.HoverState.HasFlag(HoverState.Pressed));
		var rects = GetActionRectangles(e.ClipRectangle);
		var isPressed = e.HoverState.HasFlag(HoverState.Pressed);

		e.HoverState &= ~HoverState.Pressed;

		base.OnPaintItem(e);

		var isIncluded = e.Item.IsFavorite;

		if (isIncluded)
		{
			e.Graphics.FillRoundedRectangle(rects.IncludedRect.Gradient(Color.FromArgb(rects.IncludedRect.Contains(CursorLocation) ? 150 : 255, FormDesign.Design.YellowColor), 1.5F), rects.IncludedRect.Pad(0, Padding.Vertical, 0, Padding.Vertical), 4);
		}
		else if (rects.IncludedRect.Contains(CursorLocation))
		{
			e.Graphics.FillRoundedRectangle(rects.IncludedRect.Gradient(Color.FromArgb(20, ForeColor), 1.5F), rects.IncludedRect.Pad(0, Padding.Vertical, 0, Padding.Vertical), 4);
		}

		using var icon =isIncluded ?Properties.Resources.I_StarFilled :Properties.Resources.I_Star;

		e.Graphics.DrawImage(icon.Color(rects.IncludedRect.Contains(CursorLocation) ? FormDesign.Design.ActiveColor : isIncluded ? FormDesign.Design.ActiveForeColor : ForeColor), rects.IncludedRect.CenterR(icon.Size));

		var iconColor = FormDesign.Design.IconColor;

		if (e.Item.Color != null)
		{
			iconColor = e.Item.Color.Value.GetTextColor();

			e.Graphics.FillRoundedRectangle(rects.IconRect.Gradient(e.Item.Color.Value, 1.5F), rects.IconRect.Pad(0, Padding.Vertical, 0, Padding.Vertical), 4);
		}

		using var profileIcon = e.Item.GetIcon();

		e.Graphics.DrawImage(profileIcon.Color(iconColor), rects.IconRect.CenterR(profileIcon.Size));

		e.Graphics.DrawString(e.Item.Name, UI.Font(large ? 11.25F : 9F, FontStyle.Bold), new SolidBrush(e.HoverState.HasFlag(HoverState.Pressed) ? FormDesign.Design.ActiveForeColor : ForeColor), rects.TextRect, new StringFormat { Trimming = StringTrimming.EllipsisCharacter });

		if(e.Item.IsMissingItems)
		rects.TextRect.X+= DrawLabel(e, Locale.IncludesItemsYouDoNotHave, Properties.Resources.I_MinorIssues_16, FormDesign.Design.RedColor.MergeColor(FormDesign.Design.BackColor, 50), rects.TextRect, ContentAlignment.BottomLeft).Width+Padding.Left;
		rects.TextRect.X+= DrawLabel(e, $"{e.Item.Mods.Count} {(e.Item.Mods.Count == 1 ? Locale.ModIncluded : Locale.ModIncludedPlural)}", Properties.Resources.I_Mods_16, FormDesign.Design.AccentColor.MergeColor(FormDesign.Design.BackColor, 50), rects.TextRect, ContentAlignment.BottomLeft).Width+Padding.Left;
		rects.TextRect.X += DrawLabel(e, $"{e.Item.Assets.Count} {(e.Item.Assets.Count == 1 ? Locale.AssetIncluded : Locale.AssetIncludedPlural)}", Properties.Resources.I_Assets_16, FormDesign.Design.AccentColor.MergeColor(FormDesign.Design.BackColor, 50), rects.TextRect, ContentAlignment.BottomLeft).Width + Padding.Left;

		SlickButton.DrawButton(e, rects.FolderRect, string.Empty, Font, ImageManager.GetIcon(nameof(Properties.Resources.I_Folder)), null, rects.FolderRect.Contains(CursorLocation) ? e.HoverState | (isPressed ? HoverState.Pressed : 0) : HoverState.Normal);

		var loadSize = SlickButton.GetSize(e.Graphics, ImageManager.GetIcon(nameof(Properties.Resources.I_Folder)), Locale.LoadProfile, Font, null);
		rects.LoadRect = new Rectangle(rects.FolderRect.X - Padding.Left - loadSize.Width, rects.FolderRect.Y, loadSize.Width, rects.FolderRect.Height);

		SlickButton.DrawButton(e, rects.LoadRect, Locale.LoadProfile, Font, ImageManager.GetIcon(nameof(Properties.Resources.I_Import)), null, rects.LoadRect.Contains(CursorLocation) ? e.HoverState | (isPressed ? HoverState.Pressed : 0) : HoverState.Normal);
	}
	private Rectangle DrawLabel(ItemPaintEventArgs<Profile> e, string? text, Bitmap? icon, Color color, Rectangle rectangle, ContentAlignment alignment)
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
		e.Graphics.DrawString(text, UI.Font(large ? 9F : 7.5F), foreBrush, icon is null ? rectangle : rectangle.Pad(icon.Width + Padding.Left * 2 - 2, 0, 0, 0), new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center });

		if (icon is not null)
		{
			e.Graphics.DrawImage(icon.Color(color.GetTextColor()), rectangle.Pad(Padding.Left, 0, 0, 0).Align(icon.Size, ContentAlignment.MiddleLeft));
		}

		return rectangle;
	}

	private Rectangles GetActionRectangles(Rectangle rectangle)
	{
		var rects = new Rectangles
		{
			IncludedRect = rectangle.Pad(1 * Padding.Left, 0, 0, 0).Align(new Size(rectangle.Height - 2, rectangle.Height - 2), ContentAlignment.MiddleLeft),
			FolderRect = rectangle.Pad(0, 0, Padding.Right, 0).Align(new Size(ItemHeight, ItemHeight), ContentAlignment.TopRight)
		};

		rects.IconRect = rectangle.Pad(rects.IncludedRect.Right + 2 * Padding.Left).Align(rects.IncludedRect.Size, ContentAlignment.MiddleLeft);

		rects.TextRect = new Rectangle(rects.IconRect.Right + Padding.Left, rectangle.Y, rects.FolderRect.X - rects.IconRect.Right - 2 * Padding.Left, rectangle.Height);

		return rects;
	}

	struct Rectangles
	{
		internal Rectangle IncludedRect;
		internal Rectangle IconRect;
		internal Rectangle FolderRect;
		internal Rectangle TextRect;
		internal Rectangle LoadRect;

		internal bool Contain(Point location)
		{
			return
				IncludedRect.Contains(location) ||
				IconRect.Contains(location) ||
				LoadRect.Contains(location) ||
				FolderRect.Contains(location);
		}
	}
}
