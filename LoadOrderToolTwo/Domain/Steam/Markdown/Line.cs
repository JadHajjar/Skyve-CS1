using Extensions;

using SlickControls;

using System.Drawing;

namespace LoadOrderToolTwo.Domain.Steam.Markdown;

internal class Line : Component
{
	public override void Draw(Graphics g, bool draw, HoverState hoverState, Point mouse, int width, ref int height)
	{
		g.DrawLine(new Pen(FormDesign.Design.AccentColor, 2.5F), 10, height + 5, width - 10, height + 5);

		height += 10;

		base.Draw(g, draw, hoverState, mouse, width, ref height);
	}
}
