using Microsoft.Extensions.DependencyInjection;

using SkyveApp.Systems.CS1;
using SkyveApp.Systems.CS1.Utilities;

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
		IsRunning = true;
		CurrentDirectory = Application.StartupPath;
		ExecutablePath = Application.ExecutablePath;

		ISave.AppName = "Skyve-CS1";
		ISave.CustomSaveDirectory = CurrentDirectory;

		ServiceCenter.Provider = BuildServices();
	}

	private static IServiceProvider BuildServices()
	{
		var services = new ServiceCollection();

		services.AddSkyveSystems();

		services.AddCs1SkyveSystems();

		return services.BuildServiceProvider();
	}

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

			if (CommandUtil.NoWindow)
			{
				ServiceCenter.Get<ILogger>().Info("[Console] Running without UI window");
				ServiceCenter.Get<ICentralManager>().Start();
				return;
			}

			SlickCursors.Initialize();
			Locale.Load();
			LocaleCR.Load();
			LocaleSlickUI.Load();

			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);

			if (OSVersion.Version.Major == 6)
			{
				SetProcessDPIAware();
			}

			if (!ServiceCenter.Get<ISettings>().SessionSettings.FirstTimeSetupCompleted && string.IsNullOrEmpty(ConfigurationManager.AppSettings[nameof(ILocationManager.GamePath)]))
			{
				if (MessagePrompt.Show(Locale.FirstSetupInfo, Locale.SetupIncomplete, PromptButtons.OKIgnore, PromptIcons.Hand) == DialogResult.OK)
				{
					return;
				}
			}

			Application.Run(SystemsProgram.MainForm = MainForm = new MainForm());
		}
		catch (Exception ex)
		{
			MessagePrompt.GetError(ex, "App failed to start", out var message, out var details);
			MessageBox.Show(details, message);
		}
	}

	private static void BackgroundAction_BackgroundTaskError(BackgroundAction b, Exception e)
	{
		ServiceCenter.Get<ILogger>().Exception(e, $"The background action ({b}) failed");
	}

	[System.Runtime.InteropServices.DllImport("user32.dll")]
	private static extern bool SetProcessDPIAware();
}
#nullable enable
