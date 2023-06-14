using Extensions;

using SkyveApp.Domain;
using SkyveApp.Domain.Utilities;

using SkyveShared;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using static System.Environment;

namespace SkyveApp.Utilities.Managers;
internal static class CentralManager
{
	private static Dictionary<ulong, Package>? indexedPackages;
	private static List<Package>? packages;

	public static event Action? ContentLoaded;
	public static event Action? WorkshopInfoUpdated;
	public static event Action? PackageInformationUpdated;
	public static event Action? PackageInclusionUpdated;

	private static readonly DelayedAction _delayedWorkshopInfoUpdated;
	private static readonly DelayedAction _delayedPackageInformationUpdated;
	private static readonly DelayedAction _delayedPackageInclusionUpdated;
	private static readonly DelayedAction _delayedContentLoaded;

	public static Profile CurrentProfile => ProfileManager.CurrentProfile;
	public static bool IsContentLoaded { get; private set; }
	public static SessionSettings SessionSettings { get; }
	public static IEnumerable<Package> Packages
	{
		get
		{
			var currentPackages = packages is null ? new() : new List<Package>(packages);

			foreach (var package in currentPackages)
			{
				if (SessionSettings.UserSettings.HidePseudoMods && ModLogicManager.IsPseudoMod(package))
				{
					continue;
				}

				yield return package;
			}
		}
	}

	public static IEnumerable<Mod> Mods
	{
		get
		{
			var currentPackages = packages is null ? new() : new List<Package>(packages);

			foreach (var package in currentPackages)
			{
				if (SessionSettings.UserSettings.HidePseudoMods && ModLogicManager.IsPseudoMod(package))
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

	public static IEnumerable<Asset> Assets
	{
		get
		{
			var currentPackages = packages is null ? new() : new List<Package>(packages);

			foreach (var package in currentPackages)
			{
				if (SessionSettings.UserSettings.HidePseudoMods && ModLogicManager.IsPseudoMod(package))
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

	static CentralManager()
	{
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

	public static void Start()
	{
		if (!SessionSettings.FirstTimeSetupCompleted)
		{
			try
			{ RunFirstTimeSetup(); }
			catch (Exception ex) { Log.Exception(ex, "Failed to complete the First Time Setup", true); }
		}

		ConnectionHandler.AssumeInternetConnectivity = SessionSettings.UserSettings.AssumeInternetConnectivity;

		ConnectionHandler.Start();

		Log.Info("Loading packages..");

		var content = ContentUtil.LoadContents();

		Log.Info($"Loaded {content.Count} packages");

		indexedPackages = content.Where(x => x.SteamId != 0).ToDictionary(x => x.SteamId);
		packages = content;

		Log.Info($"Loading and applying CR Data..");

		CompatibilityManager.LoadCachedData();

		CompatibilityManager.DoFirstCache(packages);

		Log.Info($"Analyzing packages..");

		try
		{ AnalyzePackages(content); }
		catch (Exception ex) { Log.Exception(ex, "Failed to analyze packages"); }

		Log.Info($"Finished analyzing packages..");

		CompatibilityManager.FirstLoadComplete = true;

		IsContentLoaded = true;

		OnContentLoaded();

		SubscriptionsManager.Start();

		if (CommandUtil.PreSelectedProfile == CurrentProfile.Name)
		{
			Log.Info($"[Command] Applying Profile ({CurrentProfile.Name})..");
			ProfileManager.SetProfile(CurrentProfile);
		}

		ColossalOrderUtil.Start();

		if (CommandUtil.LaunchOnLoad)
		{
			Log.Info($"[Command] Launching Cities..");
			CitiesManager.Launch();
		}

		if (CommandUtil.NoWindow)
		{
			Log.Info($"[Command] Closing App..");
			return;
		}

		Log.Info($"Starting Listeners..");

		ContentUtil.StartListeners();

		Log.Info($"Listeners Started");

		if (ConnectionHandler.CheckConnection())
		{
			LoadDlcAndCR();
		}
		else
		{
			Log.Warning("Not connected to the internet, delaying remaining loads.");

			ConnectionHandler.WhenConnected(() => new BackgroundAction(LoadDlcAndCR).Run());
		}

		WorkshopInfoUpdated?.Invoke();

		Log.Info($"Finished.");
	}

	private static void LoadDlcAndCR()
	{
		try
		{ SteamUtil.LoadDlcs(); }
		catch { }

		Log.Info($"Downloading compatibility data..");

		CompatibilityManager.DownloadData();

		Log.Info($"Compatibility data downloaded");

		CompatibilityManager.CacheReport();

		CompatibilityManager.FirstLoadComplete = true;

		Log.Info($"Compatibility report cached");
	}

	private static void RunFirstTimeSetup()
	{
		Log.Info("Running First Time Setup");

		LocationManager.RunFirstTimeSetup();

		Log.Info("First Time Setup Completed");

		if (LocationManager.Platform is Platform.Windows)
		{
			ContentUtil.CreateShortcut();
		}

		SessionSettings.FirstTimeSetupCompleted = true;
		SessionSettings.Save();

		Log.Info("Saved Session Settings");

		Directory.CreateDirectory(LocationManager.SkyveAppDataPath);

		File.WriteAllText(LocationManager.Combine(LocationManager.SkyveAppDataPath, "SetupComplete.txt"), "Delete this file if your LOT hasn't been set up correctly and want to try again.\r\n\r\nLaunch the game, enable the mod and open Skyve from the main menu after deleting this file.");

		ProfileManager.ConvertLegacyProfiles();
	}

	private static void AnalyzePackages(List<Package> content)
	{
		var firstTime = UpdateManager.IsFirstTime();
		var blackList = new List<Package>();

		foreach (var package in content)
		{
			if (CompatibilityManager.IsBlacklisted(package))
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

				ModLogicManager.Analyze(package.Mod);
			}
		}

		content.RemoveAll(x => blackList.Contains(x));

		if (blackList.Count > 0)
		{
			BlackListTransfer.SendList(blackList.Select(x => x.SteamId), false);
		}
		else if (ExtensionClass.FileExists(BlackListTransfer.FilePath))
		{
			ExtensionClass.DeleteFile(BlackListTransfer.FilePath);
		}

		foreach (var item in blackList)
		{
			ContentUtil.DeleteAll(item.Folder);
		}

		Log.Info($"Applying analysis results..");

		ModLogicManager.ApplyRequiredStates();
	}

	private static void HandleNewPackage(Package package)
	{
		if (UpdateManager.IsPackageKnown(package))
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

	internal static void AddPackage(Package package)
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
			ModLogicManager.Analyze(package.Mod);
		}

		OnInformationUpdated();
		OnContentLoaded();
	}

	internal static void RemovePackage(Package package)
	{
		packages?.Remove(package);
		indexedPackages?.Remove(package.SteamId);

		if (package.Mod is not null)
		{
			ModLogicManager.ModRemoved(package.Mod);
		}

		OnContentLoaded();
		_delayedWorkshopInfoUpdated.Run();

		ContentUtil.DeleteAll(package.Folder);
	}

	internal static void OnInformationUpdated()
	{
		if (IsContentLoaded)
		{
			_delayedPackageInformationUpdated.Run();
		}
	}

	internal static void OnInclusionUpdated()
	{
		if (IsContentLoaded)
		{
			_delayedPackageInclusionUpdated.Run();
			_delayedPackageInformationUpdated.Run();
		}
	}

	internal static void OnWorkshopInfoUpdated()
	{
		if (IsContentLoaded)
		{
			_delayedWorkshopInfoUpdated.Run();
		}
	}

	internal static void OnContentLoaded()
	{
		AssetsUtil.BuildAssetIndex();

		_delayedContentLoaded.Run();
	}

	internal static Package? GetPackage(ulong steamId)
	{
		if (indexedPackages?.TryGetValue(steamId, out var package) ?? false)
		{
			return package;
		}

		return null;
	}
}
