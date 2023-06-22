using Extensions;
using SkyveApp.Services;
using SkyveApp.Services.Interfaces;
using SkyveApp.Utilities;
using SkyveApp.Utilities.IO;

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
	internal static ServiceCollection Services { get; }

	static Program()
	{
		IsRunning = true;
		CurrentDirectory = Application.StartupPath;
		ExecutablePath = Application.ExecutablePath;
		Services = new();

		ISave.AppName = "Skyve-CS1";
		ISave.CustomSaveDirectory = CurrentDirectory;

		Services.AddSingleton<ICitiesManager, CitiesManager>();
		Services.AddSingleton<ICompatibilityManager, CompatibilityManager>();
		Services.AddSingleton<IContentManager, ContentManager>();
		Services.AddSingleton<IImageManager, ImageManager>();
		Services.AddSingleton<ILocationManager, LocationManager>();
		Services.AddSingleton<ILogger, Logger>();
		Services.AddSingleton<IModLogicManager, ModLogicManager>();
		Services.AddSingleton<IProfileManager, ProfileManager>();
		Services.AddSingleton<ISettings, SettingsService>();
		Services.AddSingleton<ISubscriptionsManager, SubscriptionsManager>();
		Services.AddSingleton<IUpdateManager, UpdateManager>();
		Services.AddSingleton<IAssetUtil, AssetsUtil>();
		Services.AddSingleton<IColossalOrderUtil, ColossalOrderUtil>();
		Services.AddSingleton<IModUtil, ModsUtil>();
		Services.AddSingleton<INotifier, NotifierService>();

		Services.AddTransient<ICompatibilityUtil, CompatibilityUtil>();
		Services.AddTransient<IContentUtil, ContentUtil>();
		Services.AddTransient<ILogUtil, LogUtil>();
		Services.AddTransient<IOUtil>();
		Services.AddTransient<AssemblyUtil>();

		Services.AddSingleton<CentralManager>();
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
				var folder = GetFolderPath(SpecialFolder.LocalApplicationData);

				Directory.CreateDirectory(Path.Combine(folder, ISave.AppName));

				if (Directory.Exists(Path.Combine(folder, ISave.AppName)))
				{
					ISave.CustomSaveDirectory = folder;
				}
			}
			catch { }

			try
			{
				var openTools = !CommandUtil.NoWindow && !Debugger.IsAttached && Process.GetProcessesByName(Path.GetFileNameWithoutExtension(ExecutablePath)).Length > 1;

				if (openTools && !CrossIO.FileExists(CrossIO.Combine(CurrentDirectory, "Wake")))
				{
					File.WriteAllText(Path.Combine(CurrentDirectory, "Wake"), "It's time to wake up");

					return;
				}

				CrossIO.DeleteFile(CrossIO.Combine(CurrentDirectory, "Wake"));
			}
			catch { }

			var localAppData = GetFolderPath(SpecialFolder.LocalApplicationData);
			if (Directory.Exists(CrossIO.Combine(localAppData, "Load Order Tool")))
			{
				CrossIO.MoveFolder(CrossIO.Combine(localAppData, "Load Order Tool"), CrossIO.Combine(localAppData, ISave.AppName), false);

				try
				{
					CrossIO.DeleteFolder(CrossIO.Combine(localAppData, "Load Order Tool"));

					CrossIO.DeleteFile(CrossIO.Combine(localAppData, ISave.AppName, "Logs", "LoadOrderToolTwo.log"));
				}
				catch { }
			}

			BackgroundAction.BackgroundTaskError += BackgroundAction_BackgroundTaskError;

			if (!Services.GetService<ISettings>().SessionSettings.FirstTimeSetupCompleted && string.IsNullOrEmpty(ConfigurationManager.AppSettings[nameof(LocationManager.GamePath)]))
			{
				MessagePrompt.Show(Locale.FirstSetupInfo, Locale.SetupIncomplete, PromptButtons.OK, PromptIcons.Hand);
				return;
			}

			if (CommandUtil.NoWindow)
			{
				Services.GetService<ILogger>().Info("[Console] Running without UI window");
				Services.GetService<CentralManager>().Start();
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

#if DEBUG
			Services.CheckForLoops();
#endif

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
		Services.GetService<ILogger>().Exception(e, $"The background action ({b}) failed");
	}

	[System.Runtime.InteropServices.DllImport("user32.dll")]
	private static extern bool SetProcessDPIAware();
}
#nullable enable
