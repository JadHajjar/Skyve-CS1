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
internal class CentralManager : IContentManager
{
    private Dictionary<ulong, Package>? indexedPackages;
    private List<Package>? packages;

    public event Action? ContentLoaded;
    public event Action? WorkshopInfoUpdated;
    public event Action? PackageInformationUpdated;
    public event Action? PackageInclusionUpdated;

    private readonly DelayedAction _delayedWorkshopInfoUpdated;
    private readonly DelayedAction _delayedPackageInformationUpdated;
    private readonly DelayedAction _delayedPackageInclusionUpdated;
    private readonly DelayedAction _delayedContentLoaded;

    private readonly IModLogicManager _modLogicManager;
    private readonly ICompatibilityManager _compatibilityManager;
    private readonly IProfileManager _profileManager;
    private readonly ICitiesManager _citiesManager;
    private readonly ILocationManager _locationManager;
    private readonly IUpdateManager _updateManager;
    private readonly ISubscriptionsManager _subscriptionManager;
    private readonly ILogger _logger;

	public bool IsContentLoaded { get; private set; }
    public SessionSettings SessionSettings { get; }
    public IEnumerable<Package> Packages
    {
        get
        {
            var currentPackages = packages is null ? new() : new List<Package>(packages);

            foreach (var package in currentPackages)
            {
                if (SessionSettings.UserSettings.HidePseudoMods && _modLogicManager.IsPseudoMod(package))
                {
                    continue;
                }

                yield return package;
            }
        }
    }

    public IEnumerable<Mod> Mods
    {
        get
        {
            var currentPackages = packages is null ? new() : new List<Package>(packages);

            foreach (var package in currentPackages)
            {
                if (SessionSettings.UserSettings.HidePseudoMods && _modLogicManager.IsPseudoMod(package))
                {
                    continue;
                }

                if (package.Mod != null)
                {
                    yield return package.Mod;
                }
            }
        }
    }

    public IEnumerable<Asset> Assets
    {
        get
        {
            var currentPackages = packages is null ? new() : new List<Package>(packages);

            foreach (var package in currentPackages)
            {
                if (SessionSettings.UserSettings.HidePseudoMods && _modLogicManager.IsPseudoMod(package))
                {
                    continue;
                }

                if (package.Assets == null)
                {
                    continue;
                }

                foreach (var asset in package.Assets)
                {
                    yield return asset;
                }
            }
        }
    }

	internal CentralManager(IModLogicManager modLogicManager, ICompatibilityManager compatibilityManager, IProfileManager profileManager, ICitiesManager citiesManager, ILocationManager locationManager, IUpdateManager updateManager, ISubscriptionsManager subscriptionManager, ILogger logger)
	{
		_logger = logger;
		_subscriptionManager = subscriptionManager;
		_modLogicManager = modLogicManager;
		_compatibilityManager = compatibilityManager;
		_profileManager = profileManager;
		_citiesManager = citiesManager;
		_locationManager = locationManager;
		_updateManager = updateManager;

		ISave.CustomSaveDirectory = Program.CurrentDirectory;

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

		SessionSettings = ISave.Load<SessionSettings>(nameof(SessionSettings) + ".json");

		_delayedContentLoaded = new(300, () => ContentLoaded?.Invoke());
		_delayedWorkshopInfoUpdated = new(300, () => WorkshopInfoUpdated?.Invoke());
		_delayedPackageInformationUpdated = new(300, () => PackageInformationUpdated?.Invoke());
		_delayedPackageInclusionUpdated = new(300, () => PackageInclusionUpdated?.Invoke());
	}

	public void Start()
    {
        if (!SessionSettings.FirstTimeSetupCompleted)
        {
            try
            { RunFirstTimeSetup(); }
            catch (Exception ex) { _logger.Exception(ex, "Failed to complete the First Time Setup", true); }
        }

        ConnectionHandler.AssumeInternetConnectivity = SessionSettings.UserSettings.AssumeInternetConnectivity;

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
        { AnalyzePackages(content); }
        catch (Exception ex) { _logger.Exception(ex, "Failed to analyze packages"); }

        _logger.Info($"Finished analyzing packages..");

		_compatibilityManager.FirstLoadComplete = true;

        IsContentLoaded = true;

        OnContentLoaded();

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

        SessionSettings.FirstTimeSetupCompleted = true;
        SessionSettings.Save();

		_logger.Info("Saved Session Settings");

        Directory.CreateDirectory(_locationManager.SkyveAppDataPath);

        File.WriteAllText(CrossIO.Combine(_locationManager.SkyveAppDataPath, "SetupComplete.txt"), "Delete this file if your LOT hasn't been set up correctly and want to try again.\r\n\r\nLaunch the game, enable the mod and open Skyve from the main menu after deleting this file.");

        _profileManager.ConvertLegacyProfiles();
    }

	public void AnalyzePackages(List<Package> content)
    {
        var firstTime = _updateManager.IsFirstTime();
        var blackList = new List<Package>();

        foreach (var package in content)
        {
            if (_compatibilityManager.IsBlacklisted(package))
            {
                blackList.Add(package);
                continue;
            }

            if (!firstTime)
            {
                HandleNewPackage(package);
            }

            if (package.Mod is not null)
            {
                if (!SessionSettings.UserSettings.AdvancedIncludeEnable)
                {
                    if (!package.Mod.IsEnabled && package.Mod.IsIncluded)
                    {
                        package.Mod.IsIncluded = false;
                    }
                }

                if (SessionSettings.UserSettings.LinkModAssets && package.Assets is not null)
                {
                    foreach (var asset in package.Assets)
                    {
                        asset.IsIncluded = package.Mod.IsIncluded;
                    }
                }

				_modLogicManager.Analyze(package.Mod);
            }
        }

        content.RemoveAll(x => blackList.Contains(x));

        if (blackList.Count > 0)
        {
            BlackListTransfer.SendList(blackList.Select(x => x.SteamId), false);
        }
        else if (CrossIO.FileExists(BlackListTransfer.FilePath))
        {
			CrossIO.DeleteFile(BlackListTransfer.FilePath);
        }

        foreach (var item in blackList)
        {
            ContentUtil.DeleteAll(item.Folder);
        }

        _logger.Info($"Applying analysis results..");

        _modLogicManager.ApplyRequiredStates();
    }

	public void HandleNewPackage(Package package)
    {
        if (_updateManager.IsPackageKnown(package))
        {
            return;
        }

        if (package.Mod is not null)
        {
            package.Mod.IsIncluded = !SessionSettings.UserSettings.DisableNewModsByDefault;

            if (SessionSettings.UserSettings.AdvancedIncludeEnable)
            {
                package.Mod.IsEnabled = !SessionSettings.UserSettings.DisableNewModsByDefault;
            }
        }

        if (package.Assets is not null)
        {
            foreach (var asset in package.Assets)
            {
                asset.IsIncluded = !SessionSettings.UserSettings.DisableNewAssetsByDefault;
            }
        }
    }

	public void AddPackage(Package package)
    {
        var info = SteamUtil.GetItem(package.SteamId);

        if (packages is null)
        {
            packages = new List<Package>() { package };
            indexedPackages = packages.Where(x => x.SteamId != 0).ToDictionary(x => x.SteamId);
        }
        else
        {
            packages.Add(package);
            if (indexedPackages is not null && package.SteamId != 0)
            {
                indexedPackages[package.SteamId] = package;
            }
        }

        HandleNewPackage(package);

        if (package.Mod is not null)
        {
            _modLogicManager.Analyze(package.Mod);
        }

        OnInformationUpdated();
        OnContentLoaded();
    }

	public void RemovePackage(Package package)
    {
        packages?.Remove(package);
        indexedPackages?.Remove(package.SteamId);

        if (package.Mod is not null)
        {
            _modLogicManager.ModRemoved(package.Mod);
        }

        OnContentLoaded();
        _delayedWorkshopInfoUpdated.Run();

        ContentUtil.DeleteAll(package.Folder);
    }

	public void OnInformationUpdated()
    {
        if (IsContentLoaded)
        {
            _delayedPackageInformationUpdated.Run();
        }
    }

	public void OnInclusionUpdated()
    {
        if (IsContentLoaded)
        {
            _delayedPackageInclusionUpdated.Run();
            _delayedPackageInformationUpdated.Run();
        }
    }

	public void OnWorkshopInfoUpdated()
    {
        if (IsContentLoaded)
        {
            _delayedWorkshopInfoUpdated.Run();
        }
    }

	public void OnContentLoaded()
    {
        AssetsUtil.BuildAssetIndex();

        _delayedContentLoaded.Run();
    }

	public Package? GetPackage(ulong steamId)
    {
        if (indexedPackages?.TryGetValue(steamId, out var package) ?? false)
        {
            return package;
        }

        return null;
    }
}
