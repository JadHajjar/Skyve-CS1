using Skyve.App.UserInterface.Dashboard;

using System.Windows.Forms;

namespace Skyve.App.CS1.UserInterface.Dashboard;
public class D_LaunchGame : IDashboardItem
{
	private readonly ICitiesManager _citiesManager;
	private bool isRunning;

	public D_LaunchGame()
	{
		MinimumWidth = 50;

		ServiceCenter.Get(out _citiesManager);

		_citiesManager.MonitorTick += _citiesManager_MonitorTick;
		_citiesManager.LaunchingStatusChanged += _citiesManager_LaunchingStatusChanged;
	}

	protected override void Dispose(bool disposing)
	{
		_citiesManager.MonitorTick -= _citiesManager_MonitorTick;
		_citiesManager.LaunchingStatusChanged -= _citiesManager_LaunchingStatusChanged;

		base.Dispose(disposing);
	}

	private void _citiesManager_LaunchingStatusChanged(bool obj)
	{
		this.TryInvoke(() =>
		{
			Loading = obj;
			Enabled = !obj;
		});
	}

	private void _citiesManager_MonitorTick(bool isAvailable, bool isRunning)
	{
		if (this.isRunning != isRunning)
		{
			this.isRunning = isRunning;

			OnResizeRequested();
		}
	}

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
		DrawSquareButton(e, applyDrawing, ref preferredHeight, App.Program.MainForm.LaunchStopCities, new ButtonDrawArgs
		{
			Rectangle = e.ClipRectangle,
			Icon = isRunning ? "I_Stop" : "I_CS",
			Enabled = Enabled,
			Control = this
		});

		preferredHeight = e.ClipRectangle.Width;
	}

	private void Draw(PaintEventArgs e, bool applyDrawing, ref int preferredHeight)
	{
		DrawButton(e, applyDrawing, ref preferredHeight, App.Program.MainForm.LaunchStopCities, new ButtonDrawArgs
		{
			Text = LocaleHelper.GetGlobalText(isRunning ? "StopCities" : "StartCities"),
			Rectangle = e.ClipRectangle,
			Icon = isRunning ? "I_Stop" : "I_CS",
			Enabled = Enabled,
			Control = this
		});

		preferredHeight -= Margin.Bottom;
	}
}
