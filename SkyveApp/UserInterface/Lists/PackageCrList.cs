﻿using Extensions;

using SkyveApp.Domain.Compatibility;
using SkyveApp.Domain.Compatibility.Api;
using SkyveApp.UserInterface.Panels;
using SkyveApp.Utilities;
using SkyveApp.Utilities.Managers;

using SlickControls;

using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace SkyveApp.UserInterface.Lists;
internal class PackageCrList : SlickStackedListControl<ulong, PackageCrList.Rectangles>
{
	public PackageCrList()
	{
		HighlightOnHover = true;
		SeparateWithLines = true;
		ItemHeight = 32;
	}

	protected override void UIChanged()
	{
		base.UIChanged();

		Padding = UI.Scale(new Padding(3, 2, 3, 2), UI.FontScale);
		Font = UI.Font(7F, FontStyle.Bold);
	}

	protected override IEnumerable<DrawableItem<ulong, PackageCrList.Rectangles>> OrderItems(IEnumerable<DrawableItem<ulong, PackageCrList.Rectangles>> items)
	{
		return items.OrderByDescending(x => SteamUtil.GetItem(x.Item)?.ServerTime);
	}

	protected override bool IsItemActionHovered(DrawableItem<ulong, PackageCrList.Rectangles> item, Point location)
	{
		return true;
	}

	protected override void OnPaintItemList(ItemPaintEventArgs<ulong, PackageCrList.Rectangles> e)
	{
		base.OnPaintItemList(e);

		var Package = SteamUtil.GetItem(e.Item);
		var imageRect = e.ClipRectangle.Pad(Padding);
		imageRect.Width = imageRect.Height;
		var image = Package?.IconImage;
		var panel = PanelContent.GetParentPanel(this);

		if ((panel is PC_CompatibilityManagement cm && cm.CurrentPackage?.SteamId == e.Item) || (panel is PC_ReviewRequests rr && rr.CurrentPackage == e.Item))
		{
			e.Graphics.FillRoundedRectangle(new SolidBrush(FormDesign.Design.ActiveColor), imageRect.Align(new Size(2 * Padding.Left, imageRect.Height), ContentAlignment.MiddleLeft), Padding.Left);

			imageRect.X += 3 * Padding.Left;
		}

		if (image is not null)
		{
			e.Graphics.DrawRoundedImage(image, imageRect, (int)(3 * UI.FontScale), FormDesign.Design.AccentBackColor);
		}
		else
		{
			using var generic = Properties.Resources.I_CollectionIcon.Color(FormDesign.Design.IconColor);

			e.Graphics.DrawRoundedImage(generic, imageRect, (int)(3 * UI.FontScale), FormDesign.Design.AccentBackColor);
		}

		List<(Color Color, string Text)>? tags = null;

		var textRect = e.ClipRectangle.Pad(imageRect.Right + Padding.Left, 0, 0, 0).AlignToFontSize(Font, ContentAlignment.TopLeft);

		e.Graphics.DrawString(Package?.CleanName(out tags) ?? Locale.UnknownPackage, Font, new SolidBrush(e.HoverState.HasFlag(HoverState.Pressed) ? FormDesign.Design.ActiveForeColor : ForeColor), textRect, new StringFormat { Trimming = StringTrimming.EllipsisCharacter });

		var tagRect = new Rectangle(textRect.Right, textRect.Y, 0, textRect.Height);

		if (tags is not null)
		{
			foreach (var item in tags)
			{
				tagRect.X -= Padding.Left + e.DrawLabel(item.Text, null, item.Color, tagRect, ContentAlignment.TopRight, smaller: true).Width;
			}
		}

		if (Package is null)
		{
			return;
		}

		var cr = Package.GetCompatibilityInfo();

		if (cr.Data is null)
		{
			return;
		}

		e.DrawLabel(LocaleCR.Get(cr.Data.Package.Stability.ToString()), null, CRNAttribute.GetNotification(cr.Data.Package.Stability).GetColor().MergeColor(FormDesign.Design.BackColor), e.ClipRectangle.Pad(imageRect.Right + Padding.Left, 0, 0, 0), ContentAlignment.BottomLeft, smaller: true);

		if (cr.Data.Package.ReviewDate > Package.ServerTime)
		{
			e.DrawLabel(Locale.UpToDate, null, FormDesign.Design.GreenColor.MergeColor(FormDesign.Design.BackColor), e.ClipRectangle.Pad(imageRect.Right + Padding.Left, 0, 0, 0), ContentAlignment.BottomRight, smaller: true);
		}
	}

	internal class Rectangles : IDrawableItemRectangles<ulong>
	{
		public ulong Item { get; set; }

		public bool GetToolTip(Control instance, Point location, out string text, out Point point)
		{
			text = string.Empty;
			point = default;
			return false;
		}

		public bool IsHovered(Control instance, Point location)
		{
			return true;
		}
	}
}
