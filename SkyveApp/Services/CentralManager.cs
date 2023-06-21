using Extensions;

using SkyveApp.Domain;
using SkyveApp.Services.Interfaces;
using SkyveApp.Utilities;

using SkyveShared;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SkyveApp.Services;
internal class CentralManager
{
	private readonly IModLogicManager _modLogicManager;
	private readonly ICompatibilityManager _compatibilityManager;
	private readonly IProfileManager _profileManager;
	private readonly ICitiesManager _citiesManager;
	private readonly ILocationManager _locationManager;
	private readonly IUpdateManager _updateManager;
	private readonly ISubscriptionsManager _subscriptionManager;
	private readonly IContentManager _contentManager;
	private readonly IContentUtil _contentUtil;
	private readonly IColossalOrderUtil _colossalOrderUtil;
	private readonly ISettings _settings;
	private readonly ILogger _logger;
	private readonly INotifier _notifier;

	public CentralManager(IModLogicManager modLogicManager, ICompatibilityManager compatibilityManager, IProfileManager profileManager, ICitiesManager citiesManager, ILocationManager locationManager, IUpdateManager updateManager, ISubscriptionsManager subscriptionManager, ILogger logger, ISettings settings, IContentManager contentManager, IContentUtil contentUtil, INotifier notifier, IColossalOrderUtil colossalOrderUtil)
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
		_contentUtil = contentUtil;
		_notifier = notifier;
		_colossalOrderUtil = colossalOrderUtil;
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

		var content = _contentUtil.LoadContents();

		_logger.Info($"Loaded {content.Count} packages");

		_contentManager.SetPackages(content);

		_logger.Info($"Loading and applying CR Data..");

		RequestDataUtil.Start(content);

		_compatibilityManager.LoadCachedData();

		_compatibilityManager.DoFirstCache(content);

		_logger.Info($"Analyzing packages..");

		try
		{ AnalyzePackages(content); }
		catch (Exception ex) { _logger.Exception(ex, "Failed to analyze packages"); }

		_logger.Info($"Finished analyzing packages..");

		_compatibilityManager.FirstLoadComplete = true;

		_notifier.OnContentLoaded();

		_subscriptionManager.Start();

		if (CommandUtil.PreSelectedProfile == _profileManager.CurrentProfile.Name)
		{
			_logger.Info($"[Command] Applying Profile ({_profileManager.CurrentProfile.Name})..");
			_profileManager.SetProfile(_profileManager.CurrentProfile);
		}

		_colossalOrderUtil.Start();

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

		_contentUtil.StartListeners();

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

		_notifier.OnWorkshopInfoUpdated();

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
			_locationManager.CreateShortcut();
		}

		_settings.SessionSettings.FirstTimeSetupCompleted = true;
		_settings.SessionSettings.Save();

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
				_contentManager.HandleNewPackage(package);
			}

			if (package.Mod is not null)
			{
				if (!_settings.SessionSettings.UserSettings.AdvancedIncludeEnable)
				{
					if (!package.Mod.IsEnabled && package.Mod.IsIncluded)
					{
						package.Mod.IsIncluded = false;
					}
				}

				if (_settings.SessionSettings.UserSettings.LinkModAssets && package.Assets is not null)
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
			_contentUtil.DeleteAll(item.Folder);
		}

		_logger.Info($"Applying analysis results..");

		_modLogicManager.ApplyRequiredStates();
	}
}
