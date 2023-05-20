using Extensions;

using SkyveApp.UserInterface.Panels;
using SkyveApp.Utilities;
using SkyveApp.Utilities.Managers;

using SlickControls;

using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace SkyveApp;

public partial class MainForm : BasePanelForm
{
	private readonly System.Timers.Timer _startTimeoutTimer = new(15000) { AutoReset = false };
	private bool isGameRunning;
	private bool? buttonStateRunning;

	public MainForm()
	{
		InitializeComponent();

		base_PB_Icon.UserDraw = true;
		base_PB_Icon.Paint += Base_PB_Icon_Paint;

		SlickTip.SetTo(base_PB_Icon, string.Format(Locale.LaunchTooltip, "[F5]"));

		var currentVersion = Assembly.GetExecutingAssembly().GetName().Version;

		L_Version.Text = "v" + currentVersion.ToString(3) + " Beta";

		try
		{ FormDesign.Initialize(this, DesignChanged); }
		catch { }

		try
		{
			SetPanel<PC_MainPage>(PI_Dashboard);
		}
		catch (Exception ex)
		{ OnNextIdle(() => MessagePrompt.Show(ex, "Failed to load the dashboard", form: this)); }

		new BackgroundAction("Loading content", CentralManager.Start).Run();

		var timer = new System.Timers.Timer(1000);

		timer.Elapsed += Timer_Elapsed;

		timer.Start();

		CitiesManager.MonitorTick += CitiesManager_MonitorTick;

		isGameRunning = CitiesManager.IsRunning();

		_startTimeoutTimer.Elapsed += StartTimeoutTimer_Elapsed;

		ConnectionHandler.ConnectionChanged += ConnectionHandler_ConnectionChanged;

		if (File.Exists(LocationManager.Combine(Program.CurrentDirectory, "batch.bat")))
		{
			try
			{ ExtensionClass.DeleteFile(LocationManager.Combine(Program.CurrentDirectory, "batch.bat")); }
			catch { }
		}
	}

	private void ConnectionHandler_ConnectionChanged(ConnectionState newState)
	{
		base_PB_Icon.Invalidate();
	}

	private void StartTimeoutTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
	{
		buttonStateRunning = null;
		base_PB_Icon.Loading = false;

		if (CurrentPanel is PC_MainPage mainPage)
		{
			mainPage.B_StartStop.Loading = false;
		}
	}

	private void CitiesManager_MonitorTick(bool isAvailable, bool isRunning)
	{
		isGameRunning = isRunning;

		if (buttonStateRunning is null || buttonStateRunning == isRunning)
		{
			if (_startTimeoutTimer.Enabled)
			{
				_startTimeoutTimer.Stop();
			}

			if (base_PB_Icon.Loading != isRunning)
			{
				base_PB_Icon.Loading = isRunning;
			}

			base_PB_Icon.LoaderSpeed = 0.15;

			buttonStateRunning = null;
		}
	}

	private void Base_PB_Icon_Paint(object sender, PaintEventArgs e)
	{
		e.Graphics.SetUp(base_PB_Icon.BackColor);

		using var icon = new Bitmap(IconManager.GetIcons("I_AppIcon").FirstOrDefault(x => x.Key > base_PB_Icon.Width).Value).Color(base_PB_Icon.HoverState.HasFlag(HoverState.Hovered)? FormDesign.Design.MenuForeColor.MergeColor(FormDesign.Design.ActiveColor, 85) : FormDesign.Design.MenuForeColor);

		var useGlow = !ConnectionHandler.IsConnected
			|| (buttonStateRunning is not null && buttonStateRunning != isGameRunning)
			|| isGameRunning
			// || unsaved profile changes changes
			|| base_PB_Icon.HoverState.HasFlag(HoverState.Pressed);

		e.Graphics.DrawImage(icon, base_PB_Icon.ClientRectangle);

		if (useGlow)
		{
			using var glowIcon = new Bitmap(IconManager.GetIcons("I_GlowAppIcon").FirstOrDefault(x => x.Key > base_PB_Icon.Width).Value);

			var color = FormDesign.Design.ActiveColor;
			var minimum = 0;

			if (buttonStateRunning is null && isGameRunning)
			{
				minimum = 120;
				color = Color.FromArgb(15, 153, 212);
			}

			//if (false) // unsaved profile changes changes
			//{
			//	minimum = 100;
			//	color = Color.FromArgb(122, 81, 207);
			//}

			if (!ConnectionHandler.IsConnected)
			{
				minimum = 80;
				color = Color.FromArgb(194, 38, 33);
			}

			glowIcon.Tint(Sat: color.GetSaturation(), Hue: color.GetHue());

			if (base_PB_Icon.Loading && !base_PB_Icon.HoverState.HasFlag(HoverState.Pressed))
			{
				var loops = 10;
				var target = 256;
				var perc = -Math.Cos(base_PB_Icon.LoaderPercentage * loops * Math.PI / 200) * (target - minimum) / 2 + (target + minimum) / 2;
				var alpha = (byte)perc;

				if (alpha == 0)
					return;

				glowIcon.Alpha(alpha);
			}

			e.Graphics.DrawImage(glowIcon, base_PB_Icon.ClientRectangle);
		}
	}

	private void Timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
	{
		if (!LocationManager.FileExists(LocationManager.Combine(Program.CurrentDirectory, "Wake")))
		{
			return;
		}

		ExtensionClass.DeleteFile(LocationManager.Combine(Program.CurrentDirectory, "Wake"));

		if (isGameRunning)
		{
			SendKeys.SendWait("%{TAB}");
		}

		this.TryInvoke(this.ShowUp);
	}

	protected override void UIChanged()
	{
		base.UIChanged();

		MinimumSize = UI.Scale(new Size(600, 350), UI.FontScale);
	}

	protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
	{
		if (keyData == Keys.F5)
		{
			if (CitiesManager.CitiesAvailable())
			{
				LaunchStopCities();

				return true;
			}
		}

		return base.ProcessCmdKey(ref msg, keyData);
	}

	protected override void OnAppIconClicked()
	{
		LaunchStopCities();
	}

	public void LaunchStopCities()
	{
		if (CitiesManager.CitiesAvailable())
		{
			if (LocationManager.Platform is Platform.Windows)
			{
				if (CurrentPanel is PC_MainPage mainPage)
				{
					mainPage.B_StartStop.Loading = true;
				}

				base_PB_Icon.Loading = true;
				base_PB_Icon.LoaderSpeed = 1;
			}

			if (CitiesManager.IsRunning())
			{
				buttonStateRunning = false;
				new BackgroundAction("Stopping Cities: Skylines", CitiesManager.Kill).Run();
			}
			else
			{
				buttonStateRunning = true;
				new BackgroundAction("Starting Cities: Skylines", CitiesManager.Launch).Run();
			}

			_startTimeoutTimer.Stop();
			_startTimeoutTimer.Start();
		}
	}

	protected override void OnCreateControl()
	{
		base.OnCreateControl();

		if (CentralManager.SessionSettings.LastWindowsBounds != null)
		{
			if (!SystemInformation.VirtualScreen.Contains(CentralManager.SessionSettings.LastWindowsBounds.Value.Location))
			{
				return;
			}

			Bounds = CentralManager.SessionSettings.LastWindowsBounds.Value;

			LastUiScale = UI.UIScale;
		}

		if (CentralManager.SessionSettings.WindowWasMaximized)
		{
			WindowState = FormWindowState.Minimized;
			WindowState = FormWindowState.Maximized;
		}

		var currentVersion = Assembly.GetExecutingAssembly().GetName().Version;

		if (currentVersion.ToString() != CentralManager.SessionSettings.LastVersionNotification)
		{
			if (CentralManager.SessionSettings.FirstTimeSetupCompleted)
			{
				PushPanel<PC_LotChangeLog>(null);
			}

			CentralManager.SessionSettings.LastVersionNotification = currentVersion.ToString();
			CentralManager.SessionSettings.Save();
		}
	}

	protected override void OnFormClosing(FormClosingEventArgs e)
	{
		base.OnFormClosing(e);

		if (!TopMost)
		{
			if (CentralManager.SessionSettings.WindowWasMaximized = WindowState == FormWindowState.Maximized)
			{
				if (SystemInformation.VirtualScreen.IntersectsWith(RestoreBounds))
				{
					CentralManager.SessionSettings.LastWindowsBounds = RestoreBounds;
				}
			}
			else
			{
				if (SystemInformation.VirtualScreen.IntersectsWith(Bounds))
				{
					CentralManager.SessionSettings.LastWindowsBounds = Bounds;
				}
			}

			CentralManager.SessionSettings.Save();
		}
	}

	private void PI_Dashboard_OnClick(object sender, MouseEventArgs e)
	{
		SetPanel<PC_MainPage>(PI_Dashboard);
	}

	private void PI_Mods_OnClick(object sender, MouseEventArgs e)
	{
		SetPanel<PC_Mods>(PI_Mods);
	}

	private void PI_Assets_OnClick(object sender, MouseEventArgs e)
	{
		SetPanel<PC_Assets>(PI_Assets);
	}

	private void PI_Profiles_OnClick(object sender, MouseEventArgs e)
	{
		SetPanel<PC_Profiles>(PI_Profiles);
	}

	private void PI_ModReview_OnClick(object sender, MouseEventArgs e)
	{
		SetPanel<PC_ModUtilities>(PI_ModUtilities);
	}

	private void PI_Packages_OnClick(object sender, MouseEventArgs e)
	{
		SetPanel<PC_Packages>(PI_Packages);
	}

	private void PI_Options_OnClick(object sender, MouseEventArgs e)
	{
		SetPanel<PC_Options>(PI_Options);
	}

	private void PI_Troubleshoot_OnClick(object sender, MouseEventArgs e)
	{
		SetPanel<PC_HelpAndLogs>(PI_Troubleshoot);
	}

	private void PI_DLCs_OnClick(object sender, MouseEventArgs e)
	{
		SetPanel<PC_DLCs>(PI_DLCs);
	}

	private void PI_Compatibility_OnClick(object sender, MouseEventArgs e)
	{
		SetPanel<PC_CompatibilityReport>(PI_Compatibility);
	}
}
