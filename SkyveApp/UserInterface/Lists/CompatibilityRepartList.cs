using Extensions;

using SkyveApp.Domain.Compatibility;
using SkyveApp.Domain.Interfaces;
using SkyveApp.Utilities;

using SlickControls;

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SkyveApp.UserInterface.Lists;
internal class CompatibilityRepartList : SlickStackedListControl<CompatibilityInfo>
{
    public CompatibilityRepartList()
    {
		Dock = DockStyle.Top;
        HorizontalScrolling = true;
        GridView = true;
        GridItemSize = new(300, 100);
    }

	protected override void UIChanged()
	{
		base.UIChanged();

        Padding = UI.Scale(new Padding(5), UI.FontScale);

        Height = GridItemSize.Height + Padding.Vertical;
	}

	protected override void OnPaintItem(ItemPaintEventArgs<CompatibilityInfo> e)
	{
		using var backBrush = new SolidBrush(FormDesign.Design.AccentBackColor);
		e.Graphics.FillRoundedRectangle(backBrush, e.ClipRectangle, Padding.Left);

		var package = e.Item.Package;

		PaintThumbnailAndTitle(e, package);
	}

	private void PaintThumbnailAndTitle(ItemPaintEventArgs<CompatibilityInfo> e, IPackage? package)
	{
		var rect = e.ClipRectangle.Pad(Padding);
		var imageRect = rect.Align(UI.Scale(new Size(32,32),UI.FontScale), ContentAlignment.TopLeft);
		var image = package?.IconImage;

		if (image is not null)
		{
			e.Graphics.DrawRoundedImage(image, imageRect, (int)(4 * UI.FontScale), FormDesign.Design.AccentBackColor);
		}
		else
		{
			using var generic = Properties.Resources.I_CollectionIcon.Color(FormDesign.Design.IconColor);

			e.Graphics.DrawRoundedImage(generic, imageRect, (int)(4 * UI.FontScale), FormDesign.Design.AccentBackColor);
		}

		List<(Color Color, string Text)>? tags = null;

		var textRect = rect.Pad(imageRect.Width + Padding.Left, 0,  0, 0).AlignToFontSize(Font, ContentAlignment.MiddleLeft);

		e.Graphics.DrawString(package?.CleanName(out tags) ?? Locale.UnknownPackage, Font, new SolidBrush(ForeColor), textRect, new StringFormat { Trimming = StringTrimming.EllipsisCharacter });

		var tagRect = new Rectangle(textRect.Right, textRect.Y, 0, textRect.Height);

		if (tags is not null)
		{
			foreach (var item in tags)
			{
				tagRect.X -= Padding.Left + e.DrawLabel(item.Text, null, item.Color, tagRect, ContentAlignment.MiddleRight, smaller: true).Width;
			}
		}
	}
}
