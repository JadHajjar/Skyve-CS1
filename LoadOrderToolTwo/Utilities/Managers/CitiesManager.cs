using Extensions;

using LoadOrderShared;

using LoadOrderToolTwo.Utilities.IO;

using SlickControls;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Threading.Tasks;
using System.Timers;

namespace LoadOrderToolTwo.Utilities.Managers;

public static class CitiesManager
{
	public static event MonitorTickDelegate? MonitorTick;

	public delegate void MonitorTickDelegate(bool isAvailable, bool isRunning);

	static CitiesManager()
	{
		var citiesMonitorTimer = new Timer(1000);

		citiesMonitorTimer.Elapsed += CitiesMonitorTimer_Elapsed;
		citiesMonitorTimer.Start();
	}

	private static void CitiesMonitorTimer_Elapsed(object sender, ElapsedEventArgs e)
	{
		MonitorTick?.Invoke(CitiesAvailable(), IsRunning());
	}

	public static bool CitiesAvailable()
	{
		var file = CentralManager.CurrentProfile.LaunchSettings.UseCitiesExe
			? LocationManager.Combine(LocationManager.GamePath, LocationManager.CitiesExe)
			: LocationManager.SteamPathWithExe;

		return File.Exists(file);
	}

	public static void Launch()
	{
		UpdateFiles();

		var args = GetCommandArgs();
		var file = CentralManager.CurrentProfile.LaunchSettings.UseCitiesExe
			? LocationManager.Combine(LocationManager.GamePath, LocationManager.CitiesExe)
			: LocationManager.SteamPathWithExe;
		
		IOUtil.Execute(Path.GetDirectoryName(file), Path.GetFileName(file), string.Join(" ", args));
	}

	private static void UpdateFiles()
	{
		try
		{
			bool success;
			if (CentralManager.CurrentProfile.LaunchSettings.DebugMono)
			{
				if (!CommandUtil.NoWindow && !CentralManager.SessionSettings.FpsBoosterLogWarning)
				{
					var fpsBooster = CentralManager.Mods.FirstOrDefault(mod => Path.GetFileNameWithoutExtension(mod.FileName) == "FPS_Booster");

					if (fpsBooster != null && fpsBooster.IsEnabled && fpsBooster.IsIncluded)
					{
						var result = MessagePrompt.Show(Locale.DisableFpsBoosterDebug, PromptButtons.YesNo);

						if (result == System.Windows.Forms.DialogResult.Yes)
						{
							fpsBooster.IsIncluded = false;
						}
						else
						{
							CentralManager.SessionSettings.FpsBoosterLogWarning = true;
							CentralManager.SessionSettings.Save();
						}
					}
				}

				success = MonoFile.Instance.UseDebug();
			}
			else
			{
				success = MonoFile.Instance.UseRelease();
			}

			if (!success)
			{
				if (MonoFile.Instance.ReleaseIsUsed() is bool bReleaseMono)
				{
					Log.Warning("reverting CentralManager.CurrentProfile.LaunchSettings.ReleaseMono to " + bReleaseMono);
					CentralManager.CurrentProfile.LaunchSettings.DebugMono = !bReleaseMono;
				}
			}

			if (CentralManager.CurrentProfile.LaunchSettings.UnityProfiler)
			{
				success = CitiesFile.Instance.UseDebug();
			}
			else
			{
				success = CitiesFile.Instance.UseRelease();
			}

			if (!success)
			{
				if (CitiesFile.Instance.ReleaseIsUsed() is bool bReleaseCities)
				{
					Log.Warning("reverting CentralManager.CurrentProfile.LaunchSettings.ReleaseMono to " + bReleaseCities);
					CentralManager.CurrentProfile.LaunchSettings.UnityProfiler = !bReleaseCities;
				}
			}
		}
		catch (Exception ex)
		{
			ex.Log();
		}
	}

	private static string quote(string path)
	{
		return '"' + path + '"';
	}

	private static string[] GetCommandArgs()
	{
		var args = new List<string>();
		if (!CentralManager.CurrentProfile.LaunchSettings.UseCitiesExe)
		{
			args.Add("-applaunch 255710");
		}

		if (CentralManager.CurrentProfile.LaunchSettings.NoWorkshop)
		{
			args.Add("-noWorkshop");
		}

		if (CentralManager.CurrentProfile.LaunchSettings.ResetAssets)
		{
			args.Add("-reset-assets");
		}

		if (CentralManager.CurrentProfile.LaunchSettings.NoAssets)
		{
			args.Add("-noAssets");
		}

		if (CentralManager.CurrentProfile.LaunchSettings.NoMods)
		{
			args.Add("-disableMods");
		}

		if (CentralManager.CurrentProfile.LaunchSettings.LHT)
		{
			args.Add("-LHT");
		}

		if (CentralManager.CurrentProfile.LaunchSettings.DevUi)
		{
			args.Add("-enable-dev-ui");
		}

		if (CentralManager.CurrentProfile.LaunchSettings.RefreshWorkshop)
		{
			args.Add("-refreshWorkshop");
		}

		if (CentralManager.CurrentProfile.LaunchSettings.LoadSaveGame)
		{
			if (File.Exists(IOUtil.ToRealPath(CentralManager.CurrentProfile.LaunchSettings.SaveToLoad)))
			{
				args.Add("--loadSave=" + quote(CentralManager.CurrentProfile.LaunchSettings.SaveToLoad!));
			}
			else
			{
				args.Add("-continuelastsave");
			}
		}
		else if (CentralManager.CurrentProfile.LaunchSettings.StartNewGame)
		{
			if (File.Exists(IOUtil.ToRealPath(CentralManager.CurrentProfile.LaunchSettings.SaveToLoad)))
			{
				args.Add("--newGame=" + quote(CentralManager.CurrentProfile.LaunchSettings.SaveToLoad!));
			}
			else
			{
				args.Add("-newGame");
			}
		}

		//var extraArgs = textBoxExtraArgs.Text.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
		//args.AddRange(extraArgs);

		return args.ToArray();
	}

	public static async Task<bool> Subscribe(IEnumerable<string> ids, bool unsub = false)
	{
		return await Subscribe(UGCListTransfer.ToNumber(ids), unsub);
	}

	public static async Task<bool> Subscribe(IEnumerable<ulong> ids, bool unsub = false)
	{
		if (!ids.Any())
		{
			return false;
		}

		if (IsRunning())
		{
			MessagePrompt.Show(Locale.CloseCitiesToSub, PromptButtons.OK, PromptIcons.Hand, Program.MainForm);
			return false;
		}

		if (!CentralManager.SessionSettings.SubscribeInfoShown)
		{
			MessagePrompt.Show(Locale.SubscribingRequiresGameToOpen, PromptButtons.OK, PromptIcons.Info, Program.MainForm);
			
			CentralManager.SessionSettings.SubscribeInfoShown = true;
			CentralManager.SessionSettings.Save();
		}

		if (unsub)
		{
			ContentUtil.DeleteAll(ids);
		}

		UGCListTransfer.SendList(ids, false);

		var command = unsub ?
			$"-applaunch 255710 -unsubscribe" :
			$"-applaunch 255710 -subscribe";

		var file = LocationManager.SteamPathWithExe;

		IOUtil.Execute(Path.GetDirectoryName(file), Path.GetFileName(file), command);

		var stopwatch = Stopwatch.StartNew();

		while (!IsRunning() && stopwatch.ElapsedMilliseconds < 60000)
		{
			await Task.Delay(100);
		}

		while (IsRunning() && stopwatch.ElapsedMilliseconds < 60000)
		{
			await Task.Delay(100);
		}

		await Task.Delay(1000);

		if (!unsub)
		{
			SteamUtil.ReDownload(ids.ToArray());
		}

		return true;
	}

	public static bool IsRunning()
	{
		try
		{
			return LocationManager.Platform is Platform.Windows && Process.GetProcessesByName("Cities").Length > 0;
		}
		catch { return false; }
	}

	public static void Kill()
	{
		try
		{
			foreach (var proc in Process.GetProcessesByName("Cities"))
			{
				KillProcessAndChildren(proc);
			}
		}
		catch (Exception ex) { ex.Log(); }
	}

	private static void KillProcessAndChildren(Process proc)
	{
		foreach (var childProc in GetChildProcesses(proc))
		{
			KillProcessAndChildren(childProc);
		}

		proc.Kill();
	}

	private static List<Process> GetChildProcesses(Process proc)
	{
		var childProcs = new List<Process>();

		var mos = new ManagementObjectSearcher(
			$"Select * From Win32_Process Where ParentProcessID={proc.Id}");

		foreach (var mo in mos.Get().Cast<ManagementObject>())
		{
			var childPid = Convert.ToInt32(mo["ProcessID"]);
			var childProc = Process.GetProcessById(childPid);
			childProcs.Add(childProc);
		}

		return childProcs;
	}
}