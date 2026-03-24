using Microsoft.Extensions.DependencyInjection;

using Skyve.App.CS1.Services;
using Skyve.App.Interfaces;
using Skyve.Domain;
using Skyve.Domain.Systems;
using Skyve.Systems;
using Skyve.Systems.CS1;
using Skyve.Systems.CS1.Utilities;

using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;

using static System.Environment;

namespace Skyve.App.CS1;
#nullable disable
internal static class Program
{
	static Program()
	{
		App.Program.IsRunning = true;
		App.Program.CurrentDirectory = Application.StartupPath;
		App.Program.ExecutablePath = Application.ExecutablePath;

		SaveHandler.AppName = "Skyve-CS1";
		ServiceCenter.Provider = BuildServices();
	}

	private static IServiceProvider BuildServices()
	{
		var services = new ServiceCollection();

		services.AddSkyveSystems();

		services.AddCs1SkyveSystems();

		services.AddSingleton(new SaveHandler());
		services.AddSingleton<IInterfaceService, InterfaceService>();
		services.AddSingleton<IAppInterfaceService, InterfaceService>();
		services.AddSingleton<ICustomPackageService, CustomPackageService>();

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

			if (!CommandUtil.NoWindow && !Debugger.IsAttached && IsAlreadyRunning())
			{
				if (ServiceCenter.Provider.GetService<NamedPipelineUtil>().SendToRunningInstance(args))
				{
					return;
				}
			}

			BackgroundAction.BackgroundTaskError += BackgroundAction_BackgroundTaskError;

			SlickCursors.Initialize();
			Locale.Load();
			LocaleCR.Load();
			LocaleSlickUI.Load();

			if (CommandUtil.NoWindow)
			{
				ServiceCenter.Get<ILogger>().Info("[Console] Running without UI window");
				ServiceCenter.Get<ICentralManager>().Start();
				return;
			}

			ServiceCenter.Provider.GetService<NamedPipelineUtil>().StartNamedPipeServer();

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

			Application.Run(SystemsProgram.MainForm = App.Program.MainForm = new MainForm()
			{
				Icon = Properties.Resources.Skyve
			});
		}
		catch (Exception ex)
		{
			MessagePrompt.GetError(ex, "App failed to start", out var message, out var details);
			MessageBox.Show(details, message);
		}
	}

	private static bool IsAlreadyRunning()
	{
		var process = Process.GetCurrentProcess();

		return Process.GetProcessesByName(process.ProcessName).Any(x =>
			x.Id != process.Id &&
			x.MainModule?.FileName == process.MainModule.FileName
		);
	}

	private static void BackgroundAction_BackgroundTaskError(BackgroundAction b, Exception e)
	{
		ServiceCenter.Get<ILogger>().Exception(e, $"The background action ({b}) failed");
	}

	[System.Runtime.InteropServices.DllImport("user32.dll")]
	private static extern bool SetProcessDPIAware();
}
#nullable enable
