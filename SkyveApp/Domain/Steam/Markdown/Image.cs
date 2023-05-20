using Extensions;

using SkyveApp.Utilities.Managers;

using SlickControls;

using System;
using System.Drawing;

namespace SkyveApp.Domain.Steam.Markdown;

internal class Image : Component
{
	public override void Draw(Graphics g, bool draw, HoverState hoverState, Point mouse, int width, ref int height)
	{
		if (!string.IsNullOrEmpty(Text))
		{
			var image = ImageManager.GetImage(Text, true, BBCode.EscapeImageUrl(Text!)).Result;

			if (image != null)
			{
				var size = image.Size.GetProportionalDownscaledSize(Math.Min(width, 630), true);

				if (draw)
				{
					g.DrawImage(image, new Rectangle(new Point(0, height), size));
				}

				height += size.Height;
			}
		}

		base.Draw(g, draw, hoverState, mouse, width, ref height);
	}
}
