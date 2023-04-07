using Extensions;

using LoadOrderToolTwo.Domain.Utilities;

using SlickControls;

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LoadOrderToolTwo.UserInterface.Generic;
internal class LogTraceControl : SlickControl
{
	public LogTrace Log { get; }

	public LogTraceControl(LogTrace log)
	{
		Dock = DockStyle.Top;
		Log = log;
	}

	protected override void UIChanged()
	{
		Padding = UI.Scale(new Padding(6), UI.FontScale);
		Margin = UI.Scale(new Padding(3), UI.FontScale);
	}

	protected override void OnPaint(PaintEventArgs e)
	{
		e.Graphics.SetUp(BackColor);

		e.Graphics.FillRoundedRectangle(new SolidBrush(FormDesign.Design.AccentBackColor), ClientRectangle.Pad(3), (int)(4 * UI.FontScale));

		var y = Padding.Top;

		e.Graphics.DrawString(Log.Title, UI.Font(9F, FontStyle.Bold), new SolidBrush(ForeColor), new Rectangle(Padding.Left, y, Width - Padding.Horizontal, Height));

		y += Padding.Top + (int)e.Graphics.Measure(Log.Title, UI.Font(9F, FontStyle.Bold), Width - Padding.Horizontal).Height;

		foreach (var item in Log.Trace)
		{
			var trace = item.StartsWith("at");
			e.Graphics.DrawString(item, UI.Font(8.25F), new SolidBrush(ForeColor), new Rectangle(Padding.Left + (trace?Padding.Horizontal:0), y, Width - (trace?2:1) * Padding.Horizontal, Height));

			y += (int)e.Graphics.Measure(item, UI.Font(8.25F), Width - (trace ? 2 : 1)*Padding.Horizontal).Height;
		}

		y += Padding.Bottom;

		if (y != Height)
			Height = y;
	}
}
