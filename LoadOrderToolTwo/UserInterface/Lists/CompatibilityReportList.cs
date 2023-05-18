using Extensions;

using LoadOrderToolTwo.Domain.Compatibility;
using LoadOrderToolTwo.Domain.Interfaces;
using LoadOrderToolTwo.Utilities;

using SlickControls;

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LoadOrderToolTwo.UserInterface.Lists;
internal class CompatibilityReportList : SlickStackedListControl<CompatibilityInfo>
{
    public CompatibilityReportList()
    {
		Dock = DockStyle.Top;
        VerticalScrolling = true;
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

		List<string>? tags = null;

		var textRect = rect.Pad(imageRect.Width + Padding.Left, 0,  0, 0).AlignToFontSize(Font, ContentAlignment.MiddleLeft);

		e.Graphics.DrawString(package?.Name?.RemoveVersionText(out tags) ?? Locale.UnknownPackage, Font, new SolidBrush(ForeColor), textRect, new StringFormat { Trimming = StringTrimming.EllipsisCharacter });

		var tagRect = new Rectangle(textRect.Right, textRect.Y, 0, textRect.Height);

		if (tags is not null)
		{
			foreach (var item in tags)
			{
				if (item.ToLower() is "stable" or "deprecated" or "obsolete" or "abandoned" or "broken")
				{ continue; }

				var color = item.ToLower() switch
				{
					"alpha" or "experimental" => Color.FromArgb(200, FormDesign.Design.YellowColor.MergeColor(FormDesign.Design.RedColor)),
					"beta" or "test" or "testing" => Color.FromArgb(180, FormDesign.Design.YellowColor),
					_ => (Color?)null
				};

				tagRect.X -= Padding.Left + e.DrawLabel(color is null ? item : LocaleHelper.GetGlobalText(item.ToUpper()), null, color ?? FormDesign.Design.ButtonColor, tagRect, ContentAlignment.MiddleRight, smaller: true).Width;
			}
		}
	}
}
