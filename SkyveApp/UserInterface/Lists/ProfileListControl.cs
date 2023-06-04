using Extensions;

using SkyveApp.Domain;
using SkyveApp.Domain.Enums;
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
using System.Linq;
using System.Windows.Forms;

namespace SkyveApp.UserInterface.Lists;
internal class ProfileListControl : SlickStackedListControl<IProfile, ProfileListControl.Rectangles>
{
	private ProfileSorting sorting;
	private readonly IOSelectionDialog imagePrompt;

	public IEnumerable<IProfile> FilteredItems => SafeGetItems().Select(x => x.Item);

	public bool ReadOnly { get; set; }

	public event Action<Profile>? LoadProfile;
	public event Action<Profile>? MergeProfile;
	public event Action<Profile>? ExcludeProfile;
	public event Action<Profile>? DisposeProfile;

	public ProfileListControl()
	{
		HighlightOnHover = true;
		SeparateWithLines = true;
		ItemHeight = CentralManager.SessionSettings.UserSettings.LargeItemOnHover ? 64 : 36;
		GridItemSize = new Size(305, 160);

		sorting = CentralManager.SessionSettings.UserSettings.ProfileSorting;

		Loading = !ProfileManager.ProfilesLoaded;

		if (!Loading)
		{
			SetItems(ProfileManager.Profiles.Skip(1));
		}

		ProfileManager.ProfileUpdated += ProfileManager_ProfileUpdated;
		ProfileManager.ProfileChanged += ProfileManager_ProfileChanged;

		imagePrompt = new IOSelectionDialog()
		{
			ValidExtensions = IO.ImageExtensions
		};
	}

	private void ProfileManager_ProfileChanged(IProfile obj)
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
		}

		SetItems(ProfileManager.Profiles.Skip(1));
	}

	protected override void OnViewChanged()
	{
		if (GridView)
		{
			Padding = UI.Scale(new Padding(5), UI.FontScale);
		}
		else
		{
			Padding = UI.Scale(new Padding(3, 2, 3, 2), UI.FontScale);
		}
	}

	protected override void UIChanged()
	{
		base.UIChanged();

		OnViewChanged();
	}

	protected override IEnumerable<DrawableItem<IProfile, Rectangles>> OrderItems(IEnumerable<DrawableItem<IProfile, Rectangles>> items)
	{
		return sorting switch
		{
			ProfileSorting.Color => items.OrderByDescending(x => x.Item.IsFavorite).ThenBy(x => x.Item.Color?.GetHue() ?? float.MaxValue).ThenBy(x => x.Item.Color?.GetBrightness() ?? float.MaxValue).ThenBy(x => x.Item.Color?.GetSaturation() ?? float.MaxValue),
			ProfileSorting.Name => items.OrderByDescending(x => x.Item.IsFavorite).ThenBy(x => x.Item.Name),
			ProfileSorting.DateCreated => items.OrderByDescending(x => x.Item.IsFavorite).ThenByDescending(x => x.Item.DateCreated),
			ProfileSorting.Usage => items.OrderByDescending(x => x.Item.IsFavorite).ThenByDescending(x => x.Item.Usage).ThenBy(x => x.Item.LastEditDate),
			ProfileSorting.LastEdit => items.OrderByDescending(x => x.Item.IsFavorite).ThenByDescending(x => x.Item.LastEditDate),
			ProfileSorting.LastUsed or _ => items.OrderByDescending(x => x.Item.IsFavorite).ThenByDescending(x => x.Item.LastUsed),
		};
	}

	protected override void OnItemMouseClick(DrawableItem<IProfile, Rectangles> item, MouseEventArgs e)
	{
		base.OnItemMouseClick(item, e);

		if (e.Button == MouseButtons.Right)
		{
			ShowRightClickMenu(item.Item);
			return;
		}

		if (e.Button == MouseButtons.Middle)
		{
			if (item.Rectangles.Icon.Contains(e.Location) && !ReadOnly)
			{
				item.Item.Color = null;
				ProfileManager.Save((item.Item as Profile)!);
			}
			else if (item.Rectangles.EditThumbnail.Contains(e.Location) && item.Item is Profile profile)
			{
				profile.BannerBytes = null;
				ProfileManager.Save(profile);
			}
		}

		if (e.Button != MouseButtons.Left)
		{
			return;
		}

		if (item.Rectangles.Favorite.Contains(e.Location) && !ReadOnly)
		{
			item.Item.IsFavorite = !item.Item.IsFavorite;
			ProfileManager.Save((item.Item as Profile)!);
		}
		else if (item.Rectangles.Load.Contains(e.Location) && !ReadOnly)
		{
			LoadProfile?.Invoke((item.Item as Profile)!);
		}
		else if (item.Rectangles.Icon.Contains(e.Location) && !ReadOnly)
		{
			ChangeColor(item.Item);
		}
		else if (item.Rectangles.Folder.Contains(e.Location) && !ReadOnly)
		{
			PlatformUtil.OpenFolder(ProfileManager.GetFileName((item.Item as Profile)!));
		}
		else if (item.Rectangles.Author.Contains(e.Location))
		{
			Program.MainForm.PushPanel(new PC_UserPage(item.Item.Author));
		}
		else if (item.Rectangles.EditThumbnail.Contains(e.Location) && item.Item is Profile profile)
		{
			if (imagePrompt.PromptFile(Program.MainForm) == DialogResult.OK)
			{
				try
				{
					var converter = new ImageConverter();
					using var img = Image.FromFile(imagePrompt.SelectedPath);
					profile.BannerBytes = (byte[])converter.ConvertTo(img, typeof(byte[]));
					ProfileManager.Save(profile);

					Invalidate(item.Item);
				}
				catch { }
			}
		}
	}

	private void ChangeColor(IProfile item)
	{
		var colorDialog = new SlickColorPicker(item.Color ?? Color.Red);

		if (colorDialog.ShowDialog() != DialogResult.OK)
		{
			return;
		}

		item.Color = colorDialog.Color;
		ProfileManager.Save((item as Profile)!);
	}

	private void ShowRightClickMenu(IProfile item)
	{
		//var items = new SlickStripItem[]
		//{
		//	  new (Locale.ViewThisProfilesPackages, "I_ViewFile", action: () => { Program.MainForm.PushPanel(new PC_GenericPackageList(item.Assets.Concat(item.Mods)) { Text = item.Name }); }),
		//	  new (item.IsFavorite ? Locale.UnFavoriteThisProfile : Locale.FavoriteThisProfile, "I_Star", action: () => { item.IsFavorite = !item.IsFavorite; ProfileManager.Save(item); })
		//	, new (Locale.ChangeProfileColor, "I_Paint", action: () => this.TryBeginInvoke(() => ChangeColor(item)))
		//	, new (Locale.CreateShortcutProfile, "I_Link", LocationManager.Platform is Platform.Windows, action: () => ProfileManager.CreateShortcut(item))
		//	, new (Locale.OpenProfileFolder, "I_Folder", action: () => PlatformUtil.OpenFolder(ProfileManager.GetFileName(item)))
		//	, new (string.Empty)
		//	, new (Locale.ShareProfile, "I_Share", action: async () => await ProfileManager.Share(item))
		//	, new (string.Empty)
		//	, new (Locale.ProfileReplace, "I_Import", action: () => LoadProfile?.Invoke(item))
		//	, new (Locale.ProfileMerge, "I_Merge", action: () => MergeProfile?.Invoke(item))
		//	, new (Locale.ProfileExclude, "I_Exclude", action: () => ExcludeProfile?.Invoke(item))
		//	, new (string.Empty)
		//	, new (Locale.ProfileDelete, "I_Disposable", action: () => DisposeProfile?.Invoke(item))
		//};

		//this.TryBeginInvoke(() => SlickToolStrip.Show(Program.MainForm, items));
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

	protected override void OnPaintItemGrid(ItemPaintEventArgs<IProfile, Rectangles> e)
	{
		var large = CentralManager.SessionSettings.UserSettings.LargeItemOnHover;
		var isPressed = e.HoverState.HasFlag(HoverState.Pressed);
		var textColor = FormDesign.Design.ForeColor;

		e.HoverState &= ~HoverState.Pressed;

		base.OnPaintItemGrid(e);

		if (e.Item.Banner is null)
		{
			using var brush = new SolidBrush(e.Item.Color ?? FormDesign.Design.AccentColor);

			e.Graphics.FillRoundedRectangle(brush, e.Rects.Thumbnail.Pad(-Padding.Left, -Padding.Top, -Padding.Right, 0), (int)(5 * UI.FontScale), botLeft: false, botRight: false);

			textColor = brush.Color.GetTextColor();
		}
		else
		{
			e.Graphics.DrawRoundedImage(e.Item.Banner, e.Rects.Thumbnail.Pad(-Padding.Left, -Padding.Top, -Padding.Right, 0), (int)(5 * UI.FontScale), botLeft: false, botRight: false);

			using var brush = new SolidBrush(Color.FromArgb(50, e.Item.Color ?? Color.Black));
			using var gradientBrush = new LinearGradientBrush(e.Rects.Thumbnail.Pad(-Padding.Left, e.Rects.Thumbnail.Height / 3, -Padding.Right, 0), Color.FromArgb(200, e.Item.Color ?? Color.Black), Color.FromArgb(0, e.Item.Color ?? Color.Black), -90);

			e.Graphics.FillRoundedRectangle(brush, e.Rects.Thumbnail.Pad(-Padding.Left, -Padding.Top, -Padding.Right, 0), (int)(5 * UI.FontScale), botLeft: false, botRight: false);
			e.Graphics.FillRectangle(gradientBrush, e.Rects.Thumbnail.Pad(-Padding.Left, e.Rects.Thumbnail.Height / 3, -Padding.Right, 0));
		
			textColor = brush.Color.GetTextColor();
		}

		if (e.Item.IsFavorite)
		{
			e.Graphics.FillRoundedRectangle(e.Rects.Favorite.Gradient(Color.FromArgb(e.Rects.Favorite.Contains(CursorLocation) ? 150 : 255, FormDesign.Design.YellowColor), 1.5F), e.Rects.Favorite, 4);
		}
		else if (e.Rects.Favorite.Contains(CursorLocation))
		{
			e.Graphics.FillRoundedRectangle(e.Rects.Favorite.Gradient(Color.FromArgb(40, textColor), 1.5F), e.Rects.Favorite, 4);
		}

		var fav = new DynamicIcon(e.Item.IsFavorite ? "I_StarFilled" : "I_Star");
		using var favIcon = fav.Get(e.Rects.Favorite.Height * 3 / 4);

		e.Graphics.DrawImage(favIcon.Color(e.Rects.Favorite.Contains(CursorLocation) ? FormDesign.Design.ActiveColor : e.Item.IsFavorite ? FormDesign.Design.ActiveForeColor : textColor), e.Rects.Favorite.CenterR(favIcon.Size));

		if (e.Rects.Icon.Contains(CursorLocation))
		{
			e.Graphics.FillRoundedRectangle(e.Rects.Icon.Gradient(Color.FromArgb(40, textColor), 1.5F), e.Rects.Icon, 4);
		}

		using var profileIcon = e.Item.GetIcon().Get(e.Rects.Favorite.Height * 3 / 4);

		e.Graphics.DrawImage(profileIcon.Color(e.Rects.Icon.Contains(CursorLocation) ? FormDesign.Design.ActiveColor : textColor), e.Rects.Icon.CenterR(profileIcon.Size));

		e.Graphics.DrawString(e.Item.Name, UI.Font(9.75F, FontStyle.Bold), new SolidBrush(textColor), e.Rects.Text.AlignToFontSize(UI.Font(9F, FontStyle.Bold), ContentAlignment.MiddleLeft, e.Graphics), new StringFormat { Trimming = StringTrimming.EllipsisCharacter });

		var labelRects = e.Rects.Content;

		labelRects.Y += e.DrawLabel(Locale.IncludedCount.FormatPlural(e.Item.ModCount, Locale.Mod.FormatPlural(e.Item.ModCount).ToLower()), IconManager.GetSmallIcon("I_Mods"), FormDesign.Design.AccentColor.MergeColor(FormDesign.Design.BackColor, 75), labelRects, ContentAlignment.TopLeft).Height + Padding.Top;
		labelRects.Y += e.DrawLabel(Locale.IncludedCount.FormatPlural(e.Item.AssetCount, Locale.Asset.FormatPlural(e.Item.AssetCount).ToLower()), IconManager.GetSmallIcon("I_Assets"), FormDesign.Design.AccentColor.MergeColor(FormDesign.Design.BackColor, 75), labelRects, ContentAlignment.TopLeft).Height + Padding.Top;

		if (e.Item.Author != 0)
		{
			var name = SteamUtil.GetUser(e.Item.Author)?.Name;

			e.Rects.Author = e.DrawLabel(name, IconManager.GetSmallIcon("I_User"), FormDesign.Design.ActiveColor.MergeColor(FormDesign.Design.BackColor, 25), labelRects, ContentAlignment.TopLeft);
		}

		if (e.Item == ProfileManager.CurrentProfile)
		{
			e.DrawLabel(Locale.CurrentProfile, IconManager.GetSmallIcon("I_Ok"), FormDesign.Design.ActiveColor, e.Rects.Content, ContentAlignment.TopRight);
		}
		else if (e.Item.IsMissingItems)
		{
			e.DrawLabel(Locale.IncludesItemsYouDoNotHave, IconManager.GetSmallIcon("I_MinorIssues"), FormDesign.Design.RedColor.MergeColor(FormDesign.Design.BackColor, 50), e.Rects.Content, ContentAlignment.TopRight);
		}

		var loadSize = SlickButton.GetSize(e.Graphics, IconManager.GetIcon("I_Folder"), Locale.LoadProfile, Font, null);
		loadSize.Height = e.Rects.Folder.Height;
		e.Rects.Load = e.Rects.Content.Align(loadSize, ContentAlignment.BottomRight);

		if (e.Item.Banner is not null)
		{
			e.Rects.Folder.X = e.Rects.Load.X - Padding.Left - e.Rects.Folder.Width;

			if (e.Rects.EditThumbnail.Contains(CursorLocation))
			{
				e.Graphics.FillRoundedRectangle(e.Rects.EditThumbnail.Gradient(Color.FromArgb(40, textColor), 1.5F), e.Rects.EditThumbnail, 4);
			}

			using var editIcon = IconManager.GetIcon("I_EditImage", e.Rects.Favorite.Height * 3 / 4);

			e.Graphics.DrawImage(editIcon.Color(e.Rects.EditThumbnail.Contains(CursorLocation) ? FormDesign.Design.ActiveColor : textColor), e.Rects.EditThumbnail.CenterR(editIcon.Size));
		}
		else
		SlickButton.DrawButton(e, e.Rects.EditThumbnail, string.Empty, Font, IconManager.GetIcon("I_EditImage"), null, e.Rects.EditThumbnail.Contains(CursorLocation) ? e.HoverState | (isPressed ? HoverState.Pressed : 0) : HoverState.Normal);

		SlickButton.DrawButton(e, e.Rects.Folder, string.Empty, Font, IconManager.GetIcon("I_Folder"), null, e.Rects.Folder.Contains(CursorLocation) ? e.HoverState | (isPressed ? HoverState.Pressed : 0) : HoverState.Normal);

		SlickButton.DrawButton(e, e.Rects.Load, Locale.LoadProfile, Font, IconManager.GetIcon("I_Import"), null, e.Rects.Load.Contains(CursorLocation) ? e.HoverState | (isPressed ? HoverState.Pressed : 0) : HoverState.Normal);

		if (large)
		{
			SlickButton.DrawButton(e, e.Rects.Merge, string.Empty, Font, IconManager.GetIcon("I_Merge"), null, e.Rects.Merge.Contains(CursorLocation) ? e.HoverState | (isPressed ? HoverState.Pressed : 0) : HoverState.Normal);
			SlickButton.DrawButton(e, e.Rects.Exclude, string.Empty, Font, IconManager.GetIcon("I_Exclude"), null, e.Rects.Exclude.Contains(CursorLocation) ? e.HoverState | (isPressed ? HoverState.Pressed : 0) : HoverState.Normal);
		}
	}

	protected override void OnPaintItemList(ItemPaintEventArgs<IProfile, Rectangles> e)
	{
		var large = CentralManager.SessionSettings.UserSettings.LargeItemOnHover;
		var isPressed = e.HoverState.HasFlag(HoverState.Pressed);

		e.HoverState &= ~HoverState.Pressed;

		base.OnPaintItemList(e);

		var isIncluded = e.Item.IsFavorite;

		if (isIncluded)
		{
			e.Graphics.FillRoundedRectangle(e.Rects.Favorite.Gradient(Color.FromArgb(e.Rects.Favorite.Contains(CursorLocation) ? 150 : 255, FormDesign.Design.YellowColor), 1.5F), e.Rects.Favorite.Pad(0, Padding.Vertical, 0, Padding.Vertical), 4);
		}
		else if (e.Rects.Favorite.Contains(CursorLocation))
		{
			e.Graphics.FillRoundedRectangle(e.Rects.Favorite.Gradient(Color.FromArgb(40, ForeColor), 1.5F), e.Rects.Favorite.Pad(0, Padding.Vertical, 0, Padding.Vertical), 4);
		}

		var incl = new DynamicIcon(isIncluded ? "I_StarFilled" : "I_Star");
		using var icon = large ? incl.Large : incl.Get(e.Rects.Favorite.Height / 2);

		e.Graphics.DrawImage(icon.Color(e.Rects.Favorite.Contains(CursorLocation) ? FormDesign.Design.ActiveColor : isIncluded ? FormDesign.Design.ActiveForeColor : ForeColor), e.Rects.Favorite.CenterR(icon.Size));

		var iconColor = FormDesign.Design.IconColor;

		if (e.Item.Color != null)
		{
			iconColor = e.Item.Color.Value.GetTextColor();

			e.Graphics.FillRoundedRectangle(e.Rects.Icon.Gradient(e.Item.Color.Value, 1.5F), e.Rects.Icon.Pad(0, Padding.Vertical, 0, Padding.Vertical), 4);
		}

		if (e.Rects.Icon.Contains(CursorLocation))
		{
			e.Graphics.FillRoundedRectangle(e.Rects.Icon.Gradient(Color.FromArgb(40, ForeColor), 1.5F), e.Rects.Icon.Pad(0, Padding.Vertical, 0, Padding.Vertical), 4);
		}

		using var profileIcon = large ? e.Item.GetIcon().Get(e.Rects.Favorite.Height * 3 / 4) : e.Item.GetIcon().Default;

		e.Graphics.DrawImage(profileIcon.Color(e.Rects.Icon.Contains(CursorLocation) ? FormDesign.Design.ActiveColor : iconColor), e.Rects.Icon.CenterR(profileIcon.Size));

		e.Graphics.DrawString(e.Item.Name, UI.Font(large ? 11.25F : 9F, FontStyle.Bold), new SolidBrush(e.HoverState.HasFlag(HoverState.Pressed) ? FormDesign.Design.ActiveForeColor : ForeColor), large ? e.Rects.Text.Pad(Padding) : e.Rects.Text, new StringFormat { Trimming = StringTrimming.EllipsisCharacter });

		if (large)
		{
			e.Rects.Text = e.Rects.Text.Pad(0, (int)e.Graphics.Measure(" ", UI.Font(large ? 11.25F : 9F, FontStyle.Bold)).Height + Padding.Bottom, 0, 0);
		}

		var x = e.Rects.Text.X;

		if (e.Item == ProfileManager.CurrentProfile)
		{
			e.Rects.Text.X += e.DrawLabel(Locale.CurrentProfile, IconManager.GetSmallIcon("I_Ok"), FormDesign.Design.ActiveColor, e.Rects.Text, large ? ContentAlignment.TopLeft : ContentAlignment.BottomLeft).Width + Padding.Left;
		}

		if (e.Item.IsMissingItems)
		{
			e.Rects.Text.X += e.DrawLabel(Locale.IncludesItemsYouDoNotHave, IconManager.GetSmallIcon("I_MinorIssues"), FormDesign.Design.RedColor.MergeColor(FormDesign.Design.BackColor, 50), e.Rects.Text, large ? ContentAlignment.TopLeft : ContentAlignment.BottomLeft).Width + Padding.Left;
		}

		if (large)
		{
			e.Rects.Text.X = x;
		}

		e.Rects.Text.X += e.DrawLabel(Locale.IncludedCount.FormatPlural(e.Item.ModCount, Locale.Mod.FormatPlural(e.Item.ModCount).ToLower()), IconManager.GetSmallIcon("I_Mods"), FormDesign.Design.AccentColor.MergeColor(FormDesign.Design.BackColor, 75), e.Rects.Text, ContentAlignment.BottomLeft).Width + Padding.Left;
		e.Rects.Text.X += e.DrawLabel(Locale.IncludedCount.FormatPlural(e.Item.AssetCount, Locale.Asset.FormatPlural(e.Item.AssetCount).ToLower()), IconManager.GetSmallIcon("I_Assets"), FormDesign.Design.AccentColor.MergeColor(FormDesign.Design.BackColor, 75), e.Rects.Text, ContentAlignment.BottomLeft).Width + Padding.Left;

		SlickButton.DrawButton(e, e.Rects.Folder, string.Empty, Font, IconManager.GetIcon("I_Folder"), null, e.Rects.Folder.Contains(CursorLocation) ? e.HoverState | (isPressed ? HoverState.Pressed : 0) : HoverState.Normal);

		var loadSize = SlickButton.GetSize(e.Graphics, IconManager.GetIcon("I_Folder"), Locale.LoadProfile, Font, null);
		e.Rects.Load = new Rectangle(e.Rects.Folder.X - Padding.Left - loadSize.Width, e.Rects.Folder.Y, loadSize.Width, e.Rects.Folder.Height);

		SlickButton.DrawButton(e, e.Rects.Load, Locale.LoadProfile, Font, IconManager.GetIcon("I_Import"), null, e.Rects.Load.Contains(CursorLocation) ? e.HoverState | (isPressed ? HoverState.Pressed : 0) : HoverState.Normal);

		if (large)
		{
			SlickButton.DrawButton(e, e.Rects.Merge, string.Empty, Font, IconManager.GetIcon("I_Merge"), null, e.Rects.Merge.Contains(CursorLocation) ? e.HoverState | (isPressed ? HoverState.Pressed : 0) : HoverState.Normal);
			SlickButton.DrawButton(e, e.Rects.Exclude, string.Empty, Font, IconManager.GetIcon("I_Exclude"), null, e.Rects.Exclude.Contains(CursorLocation) ? e.HoverState | (isPressed ? HoverState.Pressed : 0) : HoverState.Normal);
		}
	}

	protected override Rectangles GenerateRectangles(IProfile item, Rectangle rectangle)
	{
		var rects = new Rectangles(item);

		if (GridView)
		{
			var thumbAvailable = item.Banner is not null;
			var size = UI.Scale(new Size(32, 32), UI.FontScale);

			rects.Thumbnail = new Rectangle(rectangle.X, rectangle.Y, rectangle.Width, thumbAvailable ? ((rectangle.Width + Padding.Horizontal) * 2 / 7 - Padding.Top) : (rectangle.Height * 1 / 3));
			rects.Content = new Rectangle(rectangle.X, rectangle.Y + rects.Thumbnail.Height + Padding.Top, rectangle.Width, rectangle.Height - rects.Thumbnail.Height - Padding.Top);

			rects.Icon = rects.Thumbnail.Pad(Padding).Pad(0, 0, 0, thumbAvailable ? 0 : Padding.Bottom).Align(size, thumbAvailable ? ContentAlignment.BottomLeft : ContentAlignment.MiddleLeft);

			rects.Favorite = rects.Thumbnail.Pad(Padding).Pad(0, 0, 0, thumbAvailable ? 0 : Padding.Bottom).Align(size, thumbAvailable ? ContentAlignment.BottomRight : ContentAlignment.MiddleRight);

			rects.Text = new Rectangle(rects.Icon.Right + Padding.Left, rects.Icon.Y, rectangle.Width - ((Padding.Horizontal - rects.Icon.Right) * 2), rects.Icon.Height);

			if (thumbAvailable)
			{
				rects.Folder = rectangle.Align(UI.Scale(new Size(24, 24), UI.FontScale), ContentAlignment.BottomLeft);

				rects.EditThumbnail = rects.Thumbnail.Pad(Padding).Align(size, ContentAlignment.TopRight);
			}
			else
			{
				rects.EditThumbnail = rectangle.Align(UI.Scale(new Size(24, 24), UI.FontScale), ContentAlignment.BottomLeft);

				rects.Folder = rects.EditThumbnail;
				rects.Folder.X += rects.Folder.Width + Padding.Left;
			}
		}
		else
		{
			rects.Favorite = rectangle.Pad(1 * Padding.Left, 0, 0, 0).Align(new Size(rectangle.Height - 2, rectangle.Height - 2), ContentAlignment.MiddleLeft);
			rects.Folder = rectangle.Pad(0, 0, Padding.Right, 0).Align(CentralManager.SessionSettings.UserSettings.LargeItemOnHover ? new Size(ItemHeight / 2, ItemHeight / 2) : new Size(ItemHeight, ItemHeight), ContentAlignment.TopRight);

			if (CentralManager.SessionSettings.UserSettings.LargeItemOnHover)
			{
				rects.Exclude = rects.Merge = rects.Folder.Pad(0, rects.Folder.Height, 0, -rects.Folder.Height);
				rects.Merge.X -= rects.Merge.Width + Padding.Right;
			}

			rects.Icon = rectangle.Pad(rects.Favorite.Right + (2 * Padding.Left)).Align(rects.Favorite.Size, ContentAlignment.MiddleLeft);

			rects.Text = new Rectangle(rects.Icon.Right + Padding.Left, rectangle.Y, rects.Folder.X - rects.Icon.Right - (2 * Padding.Left), rectangle.Height);
		}

		return rects;
	}

	public class Rectangles : IDrawableItemRectangles<IProfile>
	{
		public IProfile Item { get; set; }

		public Rectangles(IProfile item)
		{
			Item = item;
		}

		internal Rectangle Thumbnail;
		internal Rectangle Favorite;
		internal Rectangle Icon;
		internal Rectangle Folder;
		internal Rectangle Text;
		internal Rectangle Content;
		internal Rectangle Load;
		internal Rectangle Exclude;
		internal Rectangle Merge;
		internal Rectangle EditThumbnail;
		internal Rectangle Author;

		public bool IsHovered(Control instance, Point location)
		{
			return
				Favorite.Contains(location) ||
				Icon.Contains(location) ||
				Load.Contains(location) ||
				Merge.Contains(location) ||
				Author.Contains(location) ||
				EditThumbnail.Contains(location) ||
				Exclude.Contains(location) ||
				Folder.Contains(location);
		}

		public bool GetToolTip(Control instance, Point location, out string text, out Point point)
		{
			if (Favorite.Contains(location))
			{
				text = Item.IsFavorite ? Locale.UnFavoriteThisProfile : Locale.FavoriteThisProfile;
				point = Favorite.Location;
				return true;
			}

			if (Icon.Contains(location))
			{
				text = Locale.ChangeProfileColor;
				point = Icon.Location;
				return true;
			}

			if (Load.Contains(location))
			{
				text = Locale.ProfileReplace;
				point = Load.Location;
				return true;
			}

			if (Folder.Contains(location))
			{
				text = Locale.OpenProfileFolder;
				point = Folder.Location;
				return true;
			}

			text = string.Empty;
			point = default;

			return false;
		}
	}
}
