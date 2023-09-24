using Extensions;

using Skyve.Domain;
using Skyve.Domain.CS1.Notifications;
using Skyve.Domain.Systems;
using Skyve.Systems.CS1.Utilities;

using SkyveShared;

using SlickControls;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Skyve.Systems.CS1.Managers;
internal class CentralManager : ICentralManager
{
	private readonly IModLogicManager _modLogicManager;
	private readonly ICompatibilityManager _compatibilityManager;
	private readonly IPlaysetManager _playsetManager;
	private readonly ICitiesManager _citiesManager;
	private readonly ILocationManager _locationManager;
	private readonly ISubscriptionsManager _subscriptionManager;
	private readonly IPackageManager _packageManager;
	private readonly IContentManager _contentManager;
	private readonly ColossalOrderUtil _colossalOrderUtil;
	private readonly ISettings _settings;
	private readonly ILogger _logger;
	private readonly INotifier _notifier;
	private readonly IModUtil _modUtil;
	private readonly IBulkUtil _bulkUtil;
	private readonly IVersionUpdateService _versionUpdateService;
	private readonly INotificationsService _notificationsService;
	private readonly IUpdateManager _updateManager;
	private readonly IAssetUtil _assetUtil;

	public CentralManager(IModLogicManager modLogicManager, ICompatibilityManager compatibilityManager, IPlaysetManager profileManager, ICitiesManager citiesManager, ILocationManager locationManager, ISubscriptionsManager subscriptionManager, IPackageManager packageManager, IContentManager contentManager, ColossalOrderUtil colossalOrderUtil, ISettings settings, ILogger logger, INotifier notifier, IModUtil modUtil, IBulkUtil bulkUtil, IVersionUpdateService versionUpdateService, INotificationsService notificationsService, IUpdateManager updateManager, IAssetUtil assetUtil)
	{
		_modLogicManager = modLogicManager;
		_compatibilityManager = compatibilityManager;
		_playsetManager = profileManager;
		_citiesManager = citiesManager;
		_locationManager = locationManager;
		_subscriptionManager = subscriptionManager;
		_packageManager = packageManager;
		_contentManager = contentManager;
		_colossalOrderUtil = colossalOrderUtil;
		_settings = settings;
		_logger = logger;
		_notifier = notifier;
		_modUtil = modUtil;
		_bulkUtil = bulkUtil;
		_versionUpdateService = versionUpdateService;
		_notificationsService = notificationsService;
		_updateManager = updateManager;
		_assetUtil = assetUtil;
	}

	public void Start()
	{
		if (!_settings.SessionSettings.FirstTimeSetupCompleted)
		{
			try
			{ RunFirstTimeSetup(); }
			catch (Exception ex)
			{
				_logger.Exception(ex, "Failed to complete the First Time Setup");

				MessagePrompt.Show(ex, "Failed to complete the First Time Setup", form: SystemsProgram.MainForm as SlickForm);
			}
		}

		ConnectionHandler.AssumeInternetConnectivity = _settings.UserSettings.AssumeInternetConnectivity;

		ConnectionHandler.Start();

		_logger.Info("Loading packages..");

		var content = _contentManager.LoadContents();

		_logger.Info($"Loaded {content.Count} packages");

		_versionUpdateService.Run(content);

		_packageManager.SetPackages(content);

		_logger.Info($"Loading and applying CR Data..");

		_compatibilityManager.Start(content);

		_logger.Info($"Analyzing packages..");

		try
		{ AnalyzePackages(content); }
		catch (Exception ex) { _logger.Exception(ex, "Failed to analyze packages"); }

		_logger.Info($"Finished analyzing packages..");

		_notifier.OnContentLoaded();

		if (_modLogicManager.AreMultipleSkyvesPresent(out var skyveInstances))
		{
			_notificationsService.SendNotification(new MultipleSkyvesNotification(skyveInstances));
		}

		_subscriptionManager.Start();

		if (CommandUtil.PreSelectedProfile == _playsetManager.CurrentPlayset.Name)
		{
			_logger.Info($"[Command] Applying Playset ({_playsetManager.CurrentPlayset.Name})..");
			_playsetManager.SetCurrentPlayset(_playsetManager.CurrentPlayset);
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

		_contentManager.StartListeners();

		_logger.Info($"Listeners Started");

		if (ConnectionHandler.CheckConnection())
		{
			LoadDlcAndCR();

			_notifier.OnWorkshopInfoUpdated();

			_updateManager.SendUpdateNotifications();
		}
		else
		{
			_logger.Warning("Not connected to the internet, delaying remaining loads.");

			_notifier.OnWorkshopInfoUpdated();
			
			_updateManager.SendUpdateNotifications();

			_logger.Info($"Compatibility report cached");

			_compatibilityManager.DoFirstCache();

			ConnectionHandler.WhenConnected(() => new BackgroundAction(LoadDlcAndCR).Run());
		}

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

		_compatibilityManager.DoFirstCache();

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

		_playsetManager.RunFirstTimeSetup();

		_settings.SessionSettings.FirstTimeSetupCompleted = true;
		_settings.SessionSettings.Save();

		_logger.Info("Saved Session Settings");

		Directory.CreateDirectory(_locationManager.SkyveAppDataPath);

		File.WriteAllText(CrossIO.Combine(_locationManager.SkyveAppDataPath, "SetupComplete.txt"), "Delete this file if your LOT hasn't been set up correctly and want to try again.\r\n\r\nLaunch the game, enable the mod and open Skyve from the main menu after deleting this file.");
	}

	private void AnalyzePackages(List<ILocalPackageWithContents> content)
	{
		var blackList = new List<ILocalPackageWithContents>();
		var firstTime = _updateManager.IsFirstTime();

		_notifier.BulkUpdating = true;

		foreach (var package in content)
		{
			if (_compatibilityManager.IsBlacklisted(package))
			{
				blackList.Add(package);
				continue;
			}

			if (package.Mod is not null)
			{
				if (!_settings.UserSettings.AdvancedIncludeEnable)
				{
					if (!firstTime && !_modUtil.IsEnabled(package.Mod) && _modUtil.IsIncluded(package.Mod))
					{
						_modUtil.SetIncluded(package.Mod, false);
					}
				}

				if (_settings.UserSettings.LinkModAssets && package.Assets is not null)
				{
					_bulkUtil.SetBulkIncluded(package.Assets, _modUtil.IsIncluded(package.Mod));
				}

				_modLogicManager.Analyze(package.Mod, _modUtil);

				if (!firstTime && !_updateManager.IsPackageKnown(package))
				{
					_modUtil.SetEnabled(package.Mod, _modUtil.IsIncluded(package.Mod));
				}
			}
		}

		_notifier.BulkUpdating = false;
		_modUtil.SaveChanges();
		_assetUtil.SaveChanges();

		content.RemoveAll(x => blackList.Contains(x));

		if (blackList.Count > 0)
		{
			BlackListTransfer.SendList(blackList.Select(x => x.Id), false);
		}
		else if (CrossIO.FileExists(BlackListTransfer.FilePath))
		{
			CrossIO.DeleteFile(BlackListTransfer.FilePath);
		}

		foreach (var item in blackList)
		{
			_packageManager.DeleteAll(item.Folder);
		}

		_logger.Info($"Applying analysis results..");

		_modLogicManager.ApplyRequiredStates(_modUtil);
	}
}
