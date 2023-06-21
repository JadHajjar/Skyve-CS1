using Extensions;

using SkyveApp.Services.Interfaces;
using SkyveApp.Utilities;
using SkyveApp.Utilities.IO;

using SkyveShared;

using SlickControls;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Threading.Tasks;
using System.Timers;

namespace SkyveApp.Services;

public class CitiesManager : ICitiesManager
{
    private readonly ILogger _logger;
    private readonly ILocationManager _locationManager;
    private readonly IProfileManager _profileManager;
    private readonly IContentManager _contentManager;
	private readonly ISettings _settings;

    public event MonitorTickDelegate? MonitorTick;

    public delegate void MonitorTickDelegate(bool isAvailable, bool isRunning);

	CitiesManager(ILogger logger, ILocationManager locationManager, IProfileManager profileManager, ISettings settings, IContentManager contentManager)
	{
		_logger = logger;
		_locationManager = locationManager;
		_profileManager = profileManager;

		var citiesMonitorTimer = new Timer(1000);

		citiesMonitorTimer.Elapsed += CitiesMonitorTimer_Elapsed;
		citiesMonitorTimer.Start();
		_settings = settings;
		_contentManager = contentManager;
	}

	private void CitiesMonitorTimer_Elapsed(object sender, ElapsedEventArgs e)
    {
        MonitorTick?.Invoke(CitiesAvailable(), IsRunning());
    }

    public bool CitiesAvailable()
    {
        var file = _profileManager.CurrentProfile.LaunchSettings.UseCitiesExe
            ? _locationManager.CitiesPathWithExe
            : _locationManager.SteamPathWithExe;

        return CrossIO.FileExists(file);
    }

    public void Launch()
    {
        UpdateFiles();

        var args = GetCommandArgs();
        var file = _profileManager.CurrentProfile.LaunchSettings.UseCitiesExe
            ? _locationManager.CitiesPathWithExe
            : _locationManager.SteamPathWithExe;

        IOUtil.Execute(file, string.Join(" ", args));
    }

    private void UpdateFiles()
    {
        try
        {
            bool success;
            if (_profileManager.CurrentProfile.LaunchSettings.DebugMono)
            {
                if (!CommandUtil.NoWindow && !_settings.SessionSettings.FpsBoosterLogWarning)
                {
                    var fpsBooster = _contentManager.Mods.FirstOrDefault(mod => Path.GetFileNameWithoutExtension(mod.FileName) == "FPS_Booster");

                    if (fpsBooster != null && fpsBooster.IsEnabled && fpsBooster.IsIncluded)
                    {
                        var result = MessagePrompt.Show(Locale.DisableFpsBoosterDebug, PromptButtons.YesNo);

                        if (result == System.Windows.Forms.DialogResult.Yes)
                        {
                            fpsBooster.IsIncluded = false;
                        }
                        else
                        {
							_settings.SessionSettings.FpsBoosterLogWarning = true;
							_settings.SessionSettings.Save();
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
                    _logger.Warning("reverting _profileManager.CurrentProfile.LaunchSettings.ReleaseMono to " + bReleaseMono);
					_profileManager.CurrentProfile.LaunchSettings.DebugMono = !bReleaseMono;
                }
            }

            if (_profileManager.CurrentProfile.LaunchSettings.UnityProfiler)
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
                    _logger.Warning("reverting _profileManager.CurrentProfile.LaunchSettings.ReleaseMono to " + bReleaseCities);
					_profileManager.CurrentProfile.LaunchSettings.UnityProfiler = !bReleaseCities;
                }
            }
        }
        catch (Exception ex)
        {
            ex.Log();
        }
    }

    private string quote(string path)
    {
        return '"' + path + '"';
    }

    private string[] GetCommandArgs()
    {
        var args = new List<string>();

        if (!_profileManager.CurrentProfile.LaunchSettings.UseCitiesExe)
        {
            args.Add("-applaunch 255710");
        }

        if (_profileManager.CurrentProfile.LaunchSettings.NoWorkshop)
        {
            args.Add("-noWorkshop");
        }

        if (_profileManager.CurrentProfile.LaunchSettings.ResetAssets)
        {
            args.Add("-reset-assets");
        }

        if (_profileManager.CurrentProfile.LaunchSettings.NoAssets)
        {
            args.Add("-noAssets");
        }

        if (_profileManager.CurrentProfile.LaunchSettings.NoMods)
        {
            args.Add("-disableMods");
        }

        if (_profileManager.CurrentProfile.LaunchSettings.LHT)
        {
            args.Add("-LHT");
        }

        if (_profileManager.CurrentProfile.LaunchSettings.DevUi)
        {
            args.Add("-enable-dev-ui");
        }

        if (_profileManager.CurrentProfile.LaunchSettings.RefreshWorkshop)
        {
            args.Add("-refreshWorkshop");
        }

        if (_profileManager.CurrentProfile.LaunchSettings.NewAsset)
        {
            args.Add("-newAsset");
        }

        if (_profileManager.CurrentProfile.LaunchSettings.LoadAsset)
        {
            args.Add("-loadAsset");
        }

        if (_profileManager.CurrentProfile.LaunchSettings.LoadSaveGame)
        {
            if (CrossIO.FileExists(_profileManager.CurrentProfile.LaunchSettings.SaveToLoad))
            {
                args.Add("--loadSave=" + quote(_profileManager.CurrentProfile.LaunchSettings.SaveToLoad!));
            }
            else
            {
                args.Add("-continuelastsave");
            }
        }
        else if (_profileManager.CurrentProfile.LaunchSettings.StartNewGame)
        {
            if (CrossIO.FileExists(_profileManager.CurrentProfile.LaunchSettings.MapToLoad))
            {
                args.Add("--newGame=" + quote(_profileManager.CurrentProfile.LaunchSettings.MapToLoad!));
            }
            else
            {
                args.Add("-newGame");
            }
        }

        if (!string.IsNullOrWhiteSpace(_profileManager.CurrentProfile.LaunchSettings.CustomArgs))
        {
            args.Add(_profileManager.CurrentProfile.LaunchSettings.CustomArgs!);
        }

        return args.ToArray();
    }

    public void RunStub()
    {
        var command = $"-applaunch 255710 -stub";

        var file = _locationManager.SteamPathWithExe;

        IOUtil.Execute(file, command);
    }

    public bool IsRunning()
    {
        try
        {
            return CrossIO.CurrentPlatform is Platform.Windows && Process.GetProcessesByName("Cities").Length > 0;
        }
        catch { return false; }
    }

    public void Kill()
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

    private void KillProcessAndChildren(Process proc)
    {
        //foreach (var childProc in GetChildProcesses(proc))
        //{
        //	KillProcessAndChildren(childProc);
        //}

        proc.Kill();
    }

    private List<Process> GetChildProcesses(Process proc)
    {
        var childProcs = new List<Process>();

        try
        {
            var mos = new ManagementObjectSearcher(
            $"Select * From Win32_Process Where ParentProcessID={proc.Id}");

            foreach (var mo in mos.Get().Cast<ManagementObject>())
            {
                var childPid = Convert.ToInt32(mo["ProcessID"]);
                var childProc = Process.GetProcessById(childPid);
                childProcs.Add(childProc);
            }
        }
        catch { }

        return childProcs;
    }
}