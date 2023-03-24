using Extensions;

using LoadOrderToolTwo.Utilities;
using LoadOrderToolTwo.Utilities.Managers;
using SlickControls;

using System;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Forms;

using static System.Environment;

namespace LoadOrderToolTwo;
#nullable disable
internal static class Program
{
	internal static bool IsRunning { get; private set; }
	internal static MainForm MainForm { get; private set; }

	/// <summary>
	///  The main entry point for the application.
	/// </summary>
	[STAThread]
	private static void Main(string[] args)
	{
		try
		{
			if (args.Contains("-stub"))
			{
				Process.Start(Application.ExecutablePath);

				return;
			}

			try
			{
				var toolPath = Application.ExecutablePath;
				var openTools = Process.GetProcessesByName(Path.GetFileNameWithoutExtension(toolPath)).Length > 1;

				if (openTools)
				{
					File.WriteAllText(Path.Combine(Directory.GetParent(toolPath).FullName, "Wake"), "It's time to wake up");

					return;
				}
			}
			catch { }

			IsRunning = true;

			SlickCursors.Initialize();
			ConnectionHandler.Start();
			ISave.CustomSaveDirectory = GetFolderPath(SpecialFolder.LocalApplicationData);
			BackgroundAction.BackgroundTaskError += (b, e) => Log.Exception(e, $"The background action ({b}) failed", false);

			if (OSVersion.Version.Major == 6)
			{
				SetProcessDPIAware();
			}

			//AppDomain.CurrentDomain.TypeResolve += AssemblyUtil.CurrentDomain_AssemblyResolve;
			//AppDomain.CurrentDomain.AssemblyResolve += AssemblyUtil.CurrentDomain_AssemblyResolve;
			//AppDomain.CurrentDomain.ReflectionOnlyAssemblyResolve += AssemblyUtil.ReflectionResolveInterface;
			//AppDomain.CurrentDomain.AssemblyResolve += AssemblyUtil.ResolveInterface;

			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);

			if (!CentralManager.SessionSettings.FirstTimeSetupCompleted && string.IsNullOrEmpty(ConfigurationManager.AppSettings[nameof(LocationManager.GamePath)]))
			{
				MessagePrompt.Show("Please enable the mod inside Cities: Skylines first before using the tool", "Set-up Incomplete", PromptButtons.OK, PromptIcons.Hand);
				return;
			}

			Application.Run(MainForm = new MainForm());
		}
		catch (Exception ex)
		{
			MessagePrompt.GetError(ex, "App failed to start", out var message, out var details);
			MessageBox.Show(details, message);
		}
	}

	[System.Runtime.InteropServices.DllImport("user32.dll")]
	private static extern bool SetProcessDPIAware();
}
#nullable enable
