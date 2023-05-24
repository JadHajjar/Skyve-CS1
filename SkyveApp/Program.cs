using Extensions;

using SkyveApp.Utilities;
using SkyveApp.Utilities.Managers;

using SlickControls;

using System;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;

using static System.Environment;

namespace SkyveApp;
#nullable disable
internal static class Program
{
	internal static bool IsRunning { get; }
	internal static string CurrentDirectory { get; }
	internal static string ExecutablePath { get; }
	internal static MainForm MainForm { get; private set; }

	static Program()
	{
		ISave.AppName = "Skyve-CS1";
		CurrentDirectory = Application.StartupPath;
		ExecutablePath = Application.ExecutablePath;
		IsRunning = true;
	}

	/// <summary>
	///  The main entry point for the application.
	/// </summary>
	[STAThread]
	private static void Main(string[] args)
	{
		try
		{
			if (CommandUtil.Parse(args))
			{
				return;
			}

			try
			{
				var toolPath = ExecutablePath;
				var openTools = !CommandUtil.NoWindow && !Debugger.IsAttached && Process.GetProcessesByName(Path.GetFileNameWithoutExtension(toolPath)).Length > 1;

				if (openTools)
				{
					File.WriteAllText(Path.Combine(Directory.GetParent(toolPath).FullName, "Wake"), "It's time to wake up");

					return;
				}

				ExtensionClass.DeleteFile(Path.Combine(Directory.GetParent(toolPath).FullName, "Wake"));
			}
			catch { }

			var localAppData = GetFolderPath(SpecialFolder.LocalApplicationData);
			if (Directory.Exists(Path.Combine(localAppData, "Load Order Tool")))
			{
				ExtensionClass.MoveFolder(Path.Combine(localAppData, "Load Order Tool"), Path.Combine(localAppData, ISave.AppName), false);

				try
				{
					ExtensionClass.DeleteFolder(Path.Combine(localAppData, "Load Order Tool"));

					File.Delete(Path.Combine(localAppData, ISave.AppName, "Logs", "LoadOrderToolTwo.log"));
				}
				catch { }
			}

			BackgroundAction.BackgroundTaskError += BackgroundAction_BackgroundTaskError;

			if (!CentralManager.SessionSettings.FirstTimeSetupCompleted && string.IsNullOrEmpty(ConfigurationManager.AppSettings[nameof(LocationManager.GamePath)]))
			{
				MessagePrompt.Show(Locale.FirstSetupInfo, Locale.SetupIncomplete, PromptButtons.OK, PromptIcons.Hand);
				return;
			}

			if (CommandUtil.NoWindow)
			{
				Log.Info("[Console] Running without UI window");
				CentralManager.Start();
				return;
			}

			SlickCursors.Initialize();
			Locale.Load();
			LocaleCR.Load();
			LocaleSlickUI.Load();

			if (OSVersion.Version.Major == 6)
			{
				SetProcessDPIAware();
			}

			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			Application.Run(MainForm = new MainForm());
		}
		catch (Exception ex)
		{
			MessagePrompt.GetError(ex, "App failed to start", out var message, out var details);
			MessageBox.Show(details, message);
		}
	}

	private static void BackgroundAction_BackgroundTaskError(BackgroundAction b, Exception e)
	{
		Log.Exception(e, $"The background action ({b}) failed", false);
	}

	[System.Runtime.InteropServices.DllImport("user32.dll")]
	private static extern bool SetProcessDPIAware();
}
#nullable enable
