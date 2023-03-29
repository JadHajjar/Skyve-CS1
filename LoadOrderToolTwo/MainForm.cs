﻿using Extensions;

using LoadOrderToolTwo.UserInterface.Panels;
using LoadOrderToolTwo.Utilities.Managers;

using SlickControls;

using System;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;

namespace LoadOrderToolTwo;

public partial class MainForm : BasePanelForm
{
	private bool startBoundsSet;
	private bool isAppRunning;
	private bool? buttonStateRunning;

	public MainForm()
	{
		InitializeComponent();

		base_PB_Icon.UserDraw = true;
		base_PB_Icon.Paint += Base_PB_Icon_Paint;

#if DEBUG
		L_Version.Text = "v" + Assembly.GetExecutingAssembly().GetName().Version.ToString(4);
#else
		L_Version.Text = "v" + Assembly.GetExecutingAssembly().GetName().Version.ToString(3) + " Alpha";
#endif
		try
		{ FormDesign.Initialize(this, DesignChanged); }
		catch { }

		try
		{ SetPanel<PC_MainPage>(PI_Dashboard); }
		catch (Exception ex)
		{ MessagePrompt.Show(ex, "Failed to load the dashboard"); }

		new BackgroundAction("Loading content", CentralManager.Start).Run();

		if (LocationManager.Platform is Platform.Windows)
		{
			var timer = new System.Timers.Timer(1000);

			timer.Elapsed += Timer_Elapsed;

			timer.Start();
		}

		CitiesManager.MonitorTick += CitiesManager_MonitorTick;

		isAppRunning = CitiesManager.IsRunning();
	}

	private void CitiesManager_MonitorTick(bool isAvailable, bool isRunning)
	{
		isAppRunning = isRunning;

		if (buttonStateRunning is null || buttonStateRunning == isRunning)
		{
			base_PB_Icon.Loading = isRunning;
			buttonStateRunning = null;
		}
	}

	private void Base_PB_Icon_Paint(object sender, PaintEventArgs e)
	{
		e.Graphics.Clear(base_PB_Icon.BackColor);

		using var icon = base_PB_Icon.Width > 48 ? Properties.Resources.AppIcon_96 : Properties.Resources.AppIcon_48;

		if (buttonStateRunning is not null && buttonStateRunning != isAppRunning)
			icon.Color(FormDesign.Design.ActiveColor);
		else if (base_PB_Icon.HoverState.HasFlag(HoverState.Pressed))
			icon.Color(FormDesign.Design.ActiveColor);
		else if (base_PB_Icon.HoverState.HasFlag(HoverState.Hovered))
			icon.Color(FormDesign.Design.ActiveColor.MergeColor(FormDesign.Design.MenuForeColor));
		else
			icon.Color(FormDesign.Design.MenuForeColor);

		if (base_PB_Icon.Loading)
		{
			e.Graphics.TranslateTransform(base_PB_Icon.Width / 2f, base_PB_Icon.Height / 2f);
			e.Graphics.RotateTransform((float)(base_PB_Icon.LoaderPercentage * -3.6));
			e.Graphics.TranslateTransform(-base_PB_Icon.Width / 2f, -base_PB_Icon.Height / 2f);
		}

		e.Graphics.DrawImage(icon, new Rectangle(Point.Empty, base_PB_Icon.Size));
	}

	private void Timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
	{
		if (File.Exists(Path.Combine(LocationManager.CurrentDirectory, "Wake")))
		{
			File.Delete(Path.Combine(LocationManager.CurrentDirectory, "Wake"));

			SendKeys.SendWait("%{TAB}");

			this.TryInvoke(this.ShowUp);
		}
	}

	protected override void UIChanged()
	{
		base.UIChanged();

		PI_Dashboard.Icon = ImageManager.GetIcon(nameof(Properties.Resources.I_Dashboard));
		PI_Mods.Icon = ImageManager.GetIcon(nameof(Properties.Resources.I_Mods));
		PI_Assets.Icon = ImageManager.GetIcon(nameof(Properties.Resources.I_Assets));
		PI_Profiles.Icon = ImageManager.GetIcon(nameof(Properties.Resources.I_ProfileSettings));
		PI_Options.Icon = ImageManager.GetIcon(nameof(Properties.Resources.I_UserOptions));
		PI_Compatibility.Icon = ImageManager.GetIcon(nameof(Properties.Resources.I_CompatibilityReport));
		PI_ModUtilities.Icon = ImageManager.GetIcon(nameof(Properties.Resources.I_Wrench));
		PI_Troubleshoot.Icon = ImageManager.GetIcon(nameof(Properties.Resources.I_AskHelp));
		PI_Packages.Icon = ImageManager.GetIcon(nameof(Properties.Resources.I_Package));
		PI_DLCs.Icon = ImageManager.GetIcon(nameof(Properties.Resources.I_Dlc));
	}

	protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
	{
		if (keyData == (Keys.Control | Keys.S) && CitiesManager.CitiesAvailable())
		{
			LaunchStopCities();

			return true;
		}

		return base.ProcessCmdKey(ref msg, keyData);
	}

	protected override void OnAppIconClicked()
	{
		LaunchStopCities();
	}

	private void LaunchStopCities()
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
		}
	}

	protected override void OnDeactivate(EventArgs e)
	{
		if (TopMost)
		{
			Thread.Sleep(200);
			TopMost = false;
			this.ShowUp();
			return;
		}

		base.OnDeactivate(e);
	}

	protected override void OnCreateControl()
	{
		base.OnCreateControl();

		if (!startBoundsSet)
		{
			if (CentralManager.SessionSettings.WindowBounds != null)
			{
				Bounds = CentralManager.SessionSettings.WindowBounds.Value;

				LastUiScale = UI.UIScale;
			}

			if (CentralManager.SessionSettings.WindowIsMaximized)
			{
				WindowState = FormWindowState.Minimized;
				WindowState = FormWindowState.Maximized;
			}

			startBoundsSet = true;
		}
	}

	protected override void OnFormClosing(FormClosingEventArgs e)
	{
		base.OnFormClosing(e);

		if (!TopMost && startBoundsSet)
		{
			if ((CentralManager.SessionSettings.WindowIsMaximized = WindowState == FormWindowState.Maximized))
			{
				CentralManager.SessionSettings.WindowBounds = RestoreBounds;
			}
			else
				CentralManager.SessionSettings.WindowBounds = Bounds;

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
		SetPanel<PC_CompatibilityReport>(PI_Troubleshoot);
		//SetPanel<PC_HelpAndLogs>(PI_Troubleshoot);
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
