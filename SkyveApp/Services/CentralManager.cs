using Extensions;

using SkyveApp.Domain;
using SkyveApp.Domain.Utilities;
using SkyveApp.Services.Interfaces;
using SkyveApp.Utilities;
using SkyveShared;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using static System.Environment;

namespace SkyveApp.Services;
internal class CentralManager
{
    private Dictionary<ulong, Package>? indexedPackages;
    private List<Package>? packages;

    public event Action? ContentLoaded;
    public event Action? WorkshopInfoUpdated;
    public event Action? PackageInformationUpdated;
    public event Action? PackageInclusionUpdated;

    private readonly DelayedAction _delayedWorkshopInfoUpdated;

    private readonly IModLogicManager _modLogicManager;
    private readonly ICompatibilityManager _compatibilityManager;
    private readonly IProfileManager _profileManager;
    private readonly ICitiesManager _citiesManager;
    private readonly ILocationManager _locationManager;
    private readonly IUpdateManager _updateManager;
    private readonly ISubscriptionsManager _subscriptionManager;
    private readonly IContentManager _contentManager;
	private readonly ISettings _settings;
    private readonly ILogger _logger;

	public bool IsContentLoaded { get; private set; }

	internal CentralManager(IModLogicManager modLogicManager, ICompatibilityManager compatibilityManager, IProfileManager profileManager, ICitiesManager citiesManager, ILocationManager locationManager, IUpdateManager updateManager, ISubscriptionsManager subscriptionManager, ILogger logger, ISettings settings, IContentManager contentManager)
	{
		_logger = logger;
		_subscriptionManager = subscriptionManager;
		_modLogicManager = modLogicManager;
		_compatibilityManager = compatibilityManager;
		_profileManager = profileManager;
		_citiesManager = citiesManager;
		_locationManager = locationManager;
		_updateManager = updateManager;
		_contentManager = contentManager;
		_settings = settings;

		_delayedWorkshopInfoUpdated = new(300, () => WorkshopInfoUpdated?.Invoke());
	}

	public void Start()
    {
        if (!_settings.SessionSettings.FirstTimeSetupCompleted)
        {
            try
            { RunFirstTimeSetup(); }
            catch (Exception ex) { _logger.Exception(ex, "Failed to complete the First Time Setup", true); }
        }

        ConnectionHandler.AssumeInternetConnectivity = _settings.SessionSettings.UserSettings.AssumeInternetConnectivity;

        ConnectionHandler.Start();

        _logger.Info("Loading packages..");

        var content = ContentUtil.LoadContents();

        _logger.Info($"Loaded {content.Count} packages");

        indexedPackages = content.Where(x => x.SteamId != 0).ToDictionary(x => x.SteamId);
        packages = content;

        _logger.Info($"Loading and applying CR Data..");

        RequestDataUtil.Start(packages);

        _compatibilityManager.LoadCachedData();

		_compatibilityManager.DoFirstCache(packages);

        _logger.Info($"Analyzing packages..");

        try
        { _contentManager. AnalyzePackages(content); }
        catch (Exception ex) { _logger.Exception(ex, "Failed to analyze packages"); }

        _logger.Info($"Finished analyzing packages..");

		_compatibilityManager.FirstLoadComplete = true;

        IsContentLoaded = true;

		_contentManager. OnContentLoaded();

		_subscriptionManager.Start();

        if (CommandUtil.PreSelectedProfile == _profileManager.CurrentProfile.Name)
        {
            _logger.Info($"[Command] Applying Profile ({_profileManager.CurrentProfile.Name})..");
            _profileManager.SetProfile(_profileManager.CurrentProfile);
        }

        ColossalOrderUtil.Start();

        if (CommandUtil.LaunchOnLoad)
        {
            _logger.Info($"[Command] Launching Cities..");
            _citiesManager.Launch();
        }

        if (CommandUtil.NoWindow)
        {
            _logger.Info($"[Command] Closing App..");
            return;
        }

        _logger.Info($"Starting Listeners..");

        ContentUtil.StartListeners();

        _logger.Info($"Listeners Started");

        if (ConnectionHandler.CheckConnection())
        {
            LoadDlcAndCR();
        }
        else
        {
            _logger.Warning("Not connected to the internet, delaying remaining loads.");

            ConnectionHandler.WhenConnected(() => new BackgroundAction(LoadDlcAndCR).Run());
        }

        WorkshopInfoUpdated?.Invoke();

        _logger.Info($"Finished.");
    }

    private void LoadDlcAndCR()
    {
        try
        { SteamUtil.LoadDlcs(); }
        catch { }

        _logger.Info($"Downloading compatibility data..");

        _compatibilityManager.DownloadData();

        _logger.Info($"Compatibility data downloaded");

		_compatibilityManager.CacheReport();

		_compatibilityManager.FirstLoadComplete = true;

        _logger.Info($"Compatibility report cached");
    }

    private void RunFirstTimeSetup()
    {
        _logger.Info("Running First Time Setup");

        _locationManager.RunFirstTimeSetup();

        _logger.Info("First Time Setup Completed");

        if (CrossIO.CurrentPlatform is Platform.Windows)
        {
            ContentUtil.CreateShortcut();
        }

        _settings.SessionSettings.FirstTimeSetupCompleted = true;
        _settings.SessionSettings.Save();

		_logger.Info("Saved Session Settings");

        Directory.CreateDirectory(_locationManager.SkyveAppDataPath);

        File.WriteAllText(CrossIO.Combine(_locationManager.SkyveAppDataPath, "SetupComplete.txt"), "Delete this file if your LOT hasn't been set up correctly and want to try again.\r\n\r\nLaunch the game, enable the mod and open Skyve from the main menu after deleting this file.");

        _profileManager.ConvertLegacyProfiles();
    }

	public void OnWorkshopInfoUpdated()
    {
        if (IsContentLoaded)
        {
            _delayedWorkshopInfoUpdated.Run();
        }
    }
}
