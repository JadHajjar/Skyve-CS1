using Extensions;

using LoadOrderToolTwo.Utilities;
using LoadOrderToolTwo.Utilities.Managers;

using SlickControls;

using System;
using System.Drawing;
using System.Windows.Forms;

namespace LoadOrderToolTwo.UserInterface.Panels;
public partial class PC_MainPage : PanelContent
{
	private bool buttonStateRunning;
	public PC_MainPage()
	{
		InitializeComponent();

		B_StartStop.Enabled = CentralManager.IsContentLoaded && CitiesManager.CitiesAvailable();

		if (!CentralManager.IsContentLoaded)
		{
			CentralManager.ContentLoaded += SetButtonEnabledOnLoad;
		}

		CitiesManager.MonitorTick += CitiesManager_MonitorTick;

		RefreshButtonState(CitiesManager.IsRunning(), true);
	}

	private void SetButtonEnabledOnLoad()
	{
		this.TryInvoke(() => B_StartStop.Enabled = CitiesManager.CitiesAvailable());
	}

	protected override void LocaleChanged()
	{
		Text = Locale.Dashboard;

		SlickTip.SetTo(B_StartStop, "Use [Ctrl + S] anywhere in the app to launch/stop");
	}

	private void CitiesManager_MonitorTick(bool isAvailable, bool isRunning)
	{
		this.TryInvoke(() => B_StartStop.Enabled = isAvailable);

		RefreshButtonState(isRunning);
	}

	protected override void UIChanged()
	{
		base.UIChanged();

		B_StartStop.Font = UI.Font(9.75F, FontStyle.Bold);
	}

	private void ProfileBubble_MouseClick(object sender, MouseEventArgs e)
	{
		if (e.Button == MouseButtons.Left)
		{
			Form.PushPanel<PC_Profiles>((Form as MainForm)?.PI_Profiles);
		}
	}

	private void ModsBubble_MouseClick(object sender, MouseEventArgs e)
	{
		if (e.Button == MouseButtons.Left)
		{
			Form.PushPanel<PC_Mods>((Form as MainForm)?.PI_Mods);
		}
	}

	private void AssetsBubble_MouseClick(object sender, MouseEventArgs e)
	{
		if (e.Button == MouseButtons.Left)
		{
			Form.PushPanel<PC_Assets>((Form as MainForm)?.PI_Assets);
		}
	}

	private void B_StartStop_Click(object sender, System.EventArgs e)
	{
		if (CitiesManager.IsRunning())
		{
			B_StartStop.Loading = true;
			new BackgroundAction("Stopping Cities: Skylines", CitiesManager.Kill).Run();
		}
		else
		{
			B_StartStop.Loading = true;
			new BackgroundAction("Starting Cities: Skylines", CitiesManager.Launch).Run();
		}
	}

	private void RefreshButtonState(bool running, bool firstTime = false)
	{
		if (!running)
		{
			if (buttonStateRunning || firstTime)
			{
				this.TryInvoke(() =>
				{
					B_StartStop.Text = Locale.StartCities;
					B_StartStop.Image = Properties.Resources.I_Launch;
					buttonStateRunning = false;
				});
			}

			return;
		}
		
		if (!buttonStateRunning || firstTime)
		{
			this.TryInvoke(() =>
			{
				B_StartStop.Text = Locale.StopCities;
				B_StartStop.Image = Properties.Resources.I_Stop;
				buttonStateRunning = true;
			});
		}
	}
}
