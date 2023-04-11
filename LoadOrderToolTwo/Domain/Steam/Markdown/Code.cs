using Extensions;

using SlickControls;

using System.Drawing;

namespace LoadOrderToolTwo.Domain.Steam.Markdown;

internal class Code : Component
{
	public override void Draw(Graphics g, bool draw, HoverState hoverState, Point mouse, int width, ref int height)
	{
		if (string.IsNullOrWhiteSpace(Text))
		{
			return;
		}

		using var font = UI.Font("Consolas", 8.25F);

		var rect = new Rectangle((int)(((1 * 12) + 6) * UI.FontScale), height, width - (int)(((2 * 12) + 6) * UI.FontScale), 0);

		g.DrawStringItem(Text, font, Color.Empty, width, 2, ref height, false);

		if (draw)
		{
			rect.Height = height - rect.Y;

			using var backBrush = new SolidBrush(FormDesign.Design.BackColor.Tint(Lum: FormDesign.Design.Type.If(FormDesignType.Dark, 10, -10)));
			using var brush = new SolidBrush(FormDesign.Design.ForeColor);

			g.FillRoundedRectangle(backBrush, rect, rect.Height / 6);

			g.DrawString(Text, font, brush, rect.Pad((int)(1 * 12 * UI.FontScale), 0, (int)(1 * 12 * UI.FontScale), 0), new StringFormat { LineAlignment = StringAlignment.Center });
		}

		base.Draw(g, draw, hoverState, mouse, width, ref height);
	}
}
