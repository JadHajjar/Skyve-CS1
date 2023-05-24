using SlickControls;

using System.Drawing;

namespace SkyveApp.Domain.Steam.Markdown;

internal class URL : Underline
{
	public override void Draw(Graphics g, bool draw, HoverState hoverState, Point mouse, int width, ref int height)
	{
		base.Draw(g, draw, hoverState, mouse, width, ref height);
	}
}
