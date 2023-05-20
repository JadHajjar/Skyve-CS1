using Extensions;

using SlickControls;

using System.Drawing;

namespace SkyveApp.Domain.Steam.Markdown;

internal class PlainText : Component
{
	protected virtual FontStyle FontStyle => FontStyle.Regular;

	public override void Draw(Graphics g, bool draw, HoverState hoverState, Point mouse, int width, ref int height)
	{
		if (string.IsNullOrWhiteSpace(Text))
		{
			return;
		}

		using var font = UI.Font(8.25F, FontStyle);

		g.DrawStringItem(Text, font, FormDesign.Design.ForeColor, width, 0, ref height, draw);

		base.Draw(g, draw, hoverState, mouse, width, ref height);
	}
}
