using Extensions;

using SlickControls;

using System.Drawing;

namespace SkyveApp.Domain.Steam.Markdown;

class Title : Component
{
	protected virtual float FontSize => 10.5F;

	public override void Draw(Graphics g, bool draw, HoverState hoverState, Point mouse, int width, ref int height)
	{
		if (string.IsNullOrWhiteSpace(Text))
		{
			return;
		}

		using var font = UI.Font(FontSize, FontStyle.Bold);

		g.DrawStringItem(Text, font, FormDesign.Design.ActiveColor, width, 0, ref height, draw);

		base.Draw(g, draw, hoverState, mouse, width, ref height);
	}
}
class SubTitle : Title
{
	protected override float FontSize => 9.75F;
}
class SubSubTitle : Title
{
	protected override float FontSize => 9F;
}
