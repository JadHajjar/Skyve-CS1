using Skyve.App.UserInterface.Dashboard;

using System.Windows.Forms;

namespace Skyve.App.CS1.UserInterface.Dashboard;
public class D_LaunchGame : IDashboardItem
{
	protected override DrawingDelegate GetDrawingMethod(int width)
	{
		if (width / UI.FontScale < 100)
		{
			return DrawSmall;
		}

		return Draw;
	}

	private void DrawSmall(PaintEventArgs e, bool applyDrawing, ref int preferredHeight)
	{
		DrawSquareButton(e, applyDrawing, ref preferredHeight, new ButtonDrawArgs
		{
			Rectangle = e.ClipRectangle,
			Icon = "I_CS"
		});

		preferredHeight = e.ClipRectangle.Width;
	}

	private void Draw(PaintEventArgs e, bool applyDrawing, ref int preferredHeight)
	{
		DrawButton(e, applyDrawing, ref preferredHeight, new ButtonDrawArgs
		{
			Text = LocaleHelper.GetGlobalText("StartCities"),
			Rectangle = e.ClipRectangle,
			Icon = "I_CS"
		});

		preferredHeight -= Margin.Bottom;
	}
}
