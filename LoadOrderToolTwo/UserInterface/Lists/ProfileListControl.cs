using Extensions;

using LoadOrderToolTwo.Domain;
using LoadOrderToolTwo.Domain.Enums;
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
	private readonly Dictionary<DrawableItem<Profile>, Rectangles> _itemRects = new();
	private ProfileSorting sorting;

	public IEnumerable<Profile> FilteredItems => SafeGetItems().Select(x => x.Item);

	public event Action<Profile>? LoadProfile;
	public event Action<Profile>? MergeProfile;
	public event Action<Profile>? ExcludeProfile;
	public event Action<Profile>? DisposeProfile;

	public ProfileListControl()
	{
		HighlightOnHover = true;
		SeparateWithLines = true;

		sorting = CentralManager.SessionSettings.UserSettings.ProfileSorting;

		Loading = !ProfileManager.ProfilesLoaded;

		if (!Loading)
		{
			SetItems(ProfileManager.Profiles.Skip(1));
		}

		ProfileManager.ProfileUpdated += ProfileManager_ProfileUpdated;
		ProfileManager.ProfileChanged += ProfileManager_ProfileChanged;
	}

	private void ProfileManager_ProfileChanged(Profile obj)
	{
		Invalidate();
	}

	internal void SetSorting(ProfileSorting selectedItem)
	{
		if (sorting == selectedItem)
		{
			return;
		}

		sorting = selectedItem;
		CentralManager.SessionSettings.UserSettings.ProfileSorting = selectedItem;
		CentralManager.SessionSettings.Save();
		ResetScroll();

		SortingChanged();
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing)
		{
			ProfileManager.ProfileChanged -= ProfileManager_ProfileChanged;
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
		ItemHeight = CentralManager.SessionSettings.UserSettings.LargeItemOnHover?64: 36;

		base.UIChanged();

		Padding = UI.Scale(new Padding(3, 2, 3, 2), UI.FontScale);
	}

	protected override IEnumerable<DrawableItem<Profile>> OrderItems(IEnumerable<DrawableItem<Profile>> items)
	{
		return sorting switch
		{
			ProfileSorting.Color => items.OrderByDescending(x => x.Item.IsFavorite).ThenBy(x => x.Item.Color?.GetHue() ?? float.MaxValue).ThenBy(x => x.Item.Color?.GetBrightness() ?? float.MaxValue).ThenBy(x => x.Item.Color?.GetSaturation() ?? float.MaxValue),
			ProfileSorting.Name => items.OrderByDescending(x => x.Item.IsFavorite).ThenBy(x => x.Item.Name),
			ProfileSorting.DateCreated => items.OrderByDescending(x => x.Item.IsFavorite).ThenByDescending(x => x.Item.DateCreated),
			ProfileSorting.Usage => items.OrderByDescending(x => x.Item.IsFavorite).ThenByDescending(x => x.Item.ForGameplay).ThenByDescending(x => x.Item.ForAssetEditor).ThenBy(x => x.Item.LastEditDate),
			ProfileSorting.LastEdit => items.OrderByDescending(x => x.Item.IsFavorite).ThenByDescending(x => x.Item.LastEditDate),
			ProfileSorting.LastUsed or _ => items.OrderByDescending(x => x.Item.IsFavorite).ThenByDescending(x => x.Item.LastUsed),
		};
	}

	protected override bool IsItemActionHovered(DrawableItem<Profile> item, Point location)
	{
		var rects = _itemRects.TryGet(item);

		if (rects.IncludedRect.Contains(location))
		{
			setTip(item.Item.IsFavorite ? Locale.UnFavoriteThisProfile : Locale.FavoriteThisProfile, rects.IncludedRect);
		}
		else if (rects.IconRect.Contains(location))
		{
			setTip(Locale.ChangeProfileColor, rects.IconRect);
		}
		else if (rects.LoadRect.Contains(location))
		{
			setTip(Locale.ProfileReplace, rects.LoadRect);
		}
		else if (rects.FolderRect.Contains(location))
		{
			setTip(Locale.OpenProfileFolder, rects.FolderRect);
		}

		void setTip(string text, Rectangle rectangle) => SlickTip.SetTo(this, text, offset: new Point(rectangle.X, item.Bounds.Y));

		return rects.Contain(location);
	}

	protected override void OnItemMouseClick(DrawableItem<Profile> item, MouseEventArgs e)
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

		if (rects.IncludedRect.Contains(e.Location))
		{
			item.Item.IsFavorite = !item.Item.IsFavorite;
			ProfileManager.Save(item.Item);
		}
		else if (rects.LoadRect.Contains(e.Location))
		{
			LoadProfile?.Invoke(item.Item);
		}
		else if (rects.IconRect.Contains(e.Location))
		{
			ChangeColor(item.Item);
		}
		else if (rects.FolderRect.Contains(e.Location))
		{
			PlatformUtil.OpenFolder(ProfileManager.GetFileName(item.Item));
		}
	}

	private void ChangeColor(Profile item)
	{
		var colorDialog = new SlickColorPicker(item.Color ?? Color.Red);

		if (colorDialog.ShowDialog() != DialogResult.OK)
		{
			return;
		}

		item.Color = colorDialog.Color;
		ProfileManager.Save(item);
	}

	private void ShowRightClickMenu(Profile item)
	{
		var items = new SlickStripItem[]
		{
			  new (item.IsFavorite ? Locale.UnFavoriteThisProfile : Locale.FavoriteThisProfile, "I_Star", action: () => { item.IsFavorite = !item.IsFavorite; ProfileManager.Save(item); })
			, new (Locale.ChangeProfileColor, "I_Theme", action: () => this.TryBeginInvoke(() => ChangeColor(item)))
			, new (Locale.CreateShortcutProfile, "I_Link", LocationManager.Platform is Platform.Windows, action: () => ProfileManager.CreateShortcut(item))
			, new (Locale.OpenProfileFolder, "I_Folder", action: () => PlatformUtil.OpenFolder(ProfileManager.GetFileName(item)))
			, new (string.Empty)
			, new (Locale.ProfileReplace, "I_Import", action: () => LoadProfile?.Invoke(item))
			, new (Locale.ProfileMerge, "I_Merge", action: () => MergeProfile?.Invoke(item))
			, new (Locale.ProfileExclude, "I_Exclude", action: () => ExcludeProfile?.Invoke(item))
			, new (string.Empty)
			, new (Locale.ProfileDelete, "I_Disposable", action: () => DisposeProfile?.Invoke(item))
		};

		this.TryBeginInvoke(() => SlickToolStrip.Show(Program.MainForm, items));
	}

	protected override void OnPaint(PaintEventArgs e)
	{
		if (Loading)
		{
			base.OnPaint(e);
		}
		else if (!Items.Any())
		{
			e.Graphics.DrawString(Locale.NoProfilesFound, UI.Font(9.75F, FontStyle.Italic), new SolidBrush(ForeColor), ClientRectangle, new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center });
		}
		else if (!SafeGetItems().Any())
		{
			e.Graphics.DrawString(Locale.NoProfilesMatchFilters, UI.Font(9.75F, FontStyle.Italic), new SolidBrush(ForeColor), ClientRectangle, new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center });
		}
		else
		{
			base.OnPaint(e);
		}
	}

	protected override void OnPaintItem(ItemPaintEventArgs<Profile> e)
	{
		var large = CentralManager.SessionSettings.UserSettings.LargeItemOnHover;
		var rects = _itemRects[e.DrawableItem] = GetActionRectangles(e.ClipRectangle);
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

		using var icon = IconManager.GetLargeIcon(isIncluded ? "I_StarFilled" : "I_Star");

		e.Graphics.DrawImage(icon.Color(rects.IncludedRect.Contains(CursorLocation) ? FormDesign.Design.ActiveColor : isIncluded ? FormDesign.Design.ActiveForeColor : ForeColor), rects.IncludedRect.CenterR(icon.Size));

		var iconColor = FormDesign.Design.IconColor;

		if (e.Item.Color != null)
		{
			iconColor = e.Item.Color.Value.GetTextColor();

			e.Graphics.FillRoundedRectangle(rects.IconRect.Gradient(e.Item.Color.Value, 1.5F), rects.IconRect.Pad(0, Padding.Vertical, 0, Padding.Vertical), 4);
		}

		if (rects.IconRect.Contains(CursorLocation))
		{
			e.Graphics.FillRoundedRectangle(rects.IconRect.Gradient(Color.FromArgb(20, ForeColor), 1.5F), rects.IconRect.Pad(0, Padding.Vertical, 0, Padding.Vertical), 4);
		}

		using var profileIcon = e.Item.GetIcon().Default;

		e.Graphics.DrawImage(profileIcon.Color(rects.IconRect.Contains(CursorLocation) ? FormDesign.Design.ActiveColor : iconColor), rects.IconRect.CenterR(profileIcon.Size));

		e.Graphics.DrawString(e.Item.Name, UI.Font(large ? 11.25F : 9F, FontStyle.Bold), new SolidBrush(e.HoverState.HasFlag(HoverState.Pressed) ? FormDesign.Design.ActiveForeColor : ForeColor), rects.TextRect, new StringFormat { Trimming = StringTrimming.EllipsisCharacter });

		if (e.Item == ProfileManager.CurrentProfile)
		{
			rects.TextRect.X += DrawLabel(e, Locale.CurrentProfile, IconManager.GetSmallIcon("I_Ok"), FormDesign.Design.ActiveColor, rects.TextRect, ContentAlignment.BottomLeft).Width + Padding.Left;
		}

		if (e.Item.IsMissingItems)
		{
			rects.TextRect.X += DrawLabel(e, Locale.IncludesItemsYouDoNotHave, IconManager.GetSmallIcon("I_MinorIssues"), FormDesign.Design.RedColor.MergeColor(FormDesign.Design.BackColor, 50), rects.TextRect, ContentAlignment.BottomLeft).Width + Padding.Left;
		}

		rects.TextRect.X += DrawLabel(e, $"{e.Item.Mods.Count} {(e.Item.Mods.Count == 1 ? Locale.ModIncluded : Locale.ModIncludedPlural)}", IconManager.GetSmallIcon("I_Mods"), FormDesign.Design.AccentColor.MergeColor(FormDesign.Design.BackColor, 50), rects.TextRect, ContentAlignment.BottomLeft).Width + Padding.Left;
		rects.TextRect.X += DrawLabel(e, $"{e.Item.Assets.Count} {(e.Item.Assets.Count == 1 ? Locale.AssetIncluded : Locale.AssetIncludedPlural)}", IconManager.GetSmallIcon("I_Assets"), FormDesign.Design.AccentColor.MergeColor(FormDesign.Design.BackColor, 50), rects.TextRect, ContentAlignment.BottomLeft).Width + Padding.Left;

		SlickButton.DrawButton(e, rects.FolderRect, string.Empty, Font, IconManager.GetIcon("I_Folder"), null, rects.FolderRect.Contains(CursorLocation) ? e.HoverState | (isPressed ? HoverState.Pressed : 0) : HoverState.Normal);

		var loadSize = SlickButton.GetSize(e.Graphics, IconManager.GetIcon("I_Folder"), Locale.LoadProfile, Font, null);
		rects.LoadRect = new Rectangle(rects.FolderRect.X - Padding.Left - loadSize.Width, rects.FolderRect.Y, loadSize.Width, rects.FolderRect.Height);

		SlickButton.DrawButton(e, rects.LoadRect, Locale.LoadProfile, Font, IconManager.GetIcon("I_Import"), null, rects.LoadRect.Contains(CursorLocation) ? e.HoverState | (isPressed ? HoverState.Pressed : 0) : HoverState.Normal);
	}

	private Rectangle DrawLabel(ItemPaintEventArgs<Profile> e, string? text, Bitmap? icon, Color color, Rectangle rectangle, ContentAlignment alignment)
	{
		if (text == null)
		{
			return Rectangle.Empty;
		}

		var large = CentralManager.SessionSettings.UserSettings.LargeItemOnHover;
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
		e.Graphics.DrawString(text, UI.Font(large ? 9F : 7.5F), foreBrush, icon is null ? rectangle : rectangle.Pad(icon.Width + (Padding.Left * 2) - 2, 0, 0, 0), new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center });

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

		rects.IconRect = rectangle.Pad(rects.IncludedRect.Right + (2 * Padding.Left)).Align(rects.IncludedRect.Size, ContentAlignment.MiddleLeft);

		rects.TextRect = new Rectangle(rects.IconRect.Right + Padding.Left, rectangle.Y, rects.FolderRect.X - rects.IconRect.Right - (2 * Padding.Left), rectangle.Height);

		return rects;
	}

	private class Rectangles
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
