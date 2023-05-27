using Extensions;

using SkyveApp.Domain.Compatibility;
using SkyveApp.UserInterface.Panels;
using SkyveApp.Utilities;
using SkyveApp.Utilities.Managers;

using SlickControls;

using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace SkyveApp.UserInterface.Lists;
internal class ReviewRequestList : SlickStackedListControl<ReviewRequest>
{
	public ReviewRequestList()
	{
		HighlightOnHover = true;
		SeparateWithLines = true;
		ItemHeight = 64;
	}

	protected override void UIChanged()
	{
		base.UIChanged();

		Padding = UI.Scale(new Padding(3, 2, 3, 2), UI.FontScale);
		Font = UI.Font(8.25F, FontStyle.Bold);
	}

	protected override IEnumerable<DrawableItem<ReviewRequest>> OrderItems(IEnumerable<DrawableItem<ReviewRequest>> items)
	{
		return items.OrderBy(x => x.Item.Timestamp);
	}

	protected override bool IsItemActionHovered(DrawableItem<ReviewRequest> item, Point location)
	{
		return true;
	}

	protected override void OnPaint(PaintEventArgs e)
	{
		if (!Items.Any())
		{
			e.Graphics.DrawString(Locale.SelectPackage, UI.Font(9F, FontStyle.Italic), new SolidBrush(ForeColor), ClientRectangle, new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center });
		}

		base.OnPaint(e);
	}

	protected override void OnPaintItem(ItemPaintEventArgs<ReviewRequest> e)
	{
		base.OnPaintItem(e);

		var User = SteamUtil.GetUser(e.Item.UserId);
		var imageRect = e.ClipRectangle.Pad(Padding);
		imageRect.Width = imageRect.Height/=2;
		var image = User?.AvatarImage;

		if (image is not null)
		{
			e.Graphics.DrawRoundedImage(image, imageRect, (int)(3 * UI.FontScale), FormDesign.Design.AccentBackColor);
		}
		else
		{
			using var generic = Properties.Resources.I_CollectionIcon.Color(FormDesign.Design.IconColor);

			e.Graphics.DrawRoundedImage(generic, imageRect, (int)(3 * UI.FontScale), FormDesign.Design.AccentBackColor);
		}

		var textRect = e.ClipRectangle.Pad(imageRect.Right + Padding.Left, 0, 0, 0);
		using var brush = new SolidBrush(e.HoverState.HasFlag(HoverState.Pressed) ? FormDesign.Design.ActiveForeColor : ForeColor);
		e.Graphics.DrawString(User?.Name ?? Locale.UnknownUser, Font, brush, textRect, new StringFormat { Trimming = StringTrimming.EllipsisCharacter });

		using var typeIcon = IconManager.GetSmallIcon(e.Item.IsInteraction ? "I_Switch" : e.Item.IsStatus ? "I_Statuses" : "I_Content");
		using var dateIcon = IconManager.GetSmallIcon("I_UpdateTime");
		var r = e.DrawLabel(e.Item.Timestamp.ToLocalTime().ToString("g"), dateIcon, FormDesign.Design.AccentColor, e.ClipRectangle, ContentAlignment.BottomLeft, true);
		e.DrawLabel(e.Item.IsInteraction ? "Interaction" : e.Item.IsStatus ? "Status" : "Other", typeIcon, FormDesign.Design.AccentColor, e.ClipRectangle.Pad(0, 0, 0, r.Height + Padding.Top), ContentAlignment.BottomLeft, true);
	

		e.Graphics.DrawString(e.Item.PackageNote, UI.Font(7.5F), brush, textRect.Pad((int)(125*UI.FontScale),0,0,0), new StringFormat { LineAlignment = StringAlignment.Center, Alignment =	 StringAlignment.Far });
	}
}
