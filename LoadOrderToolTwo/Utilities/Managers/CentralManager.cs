using Extensions;

using LoadOrderToolTwo.Domain;
using LoadOrderToolTwo.Domain.Enums;
using LoadOrderToolTwo.Domain.Interfaces;
using LoadOrderToolTwo.Domain.Utilities;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using System.Windows.Forms;

using static System.Environment;
using static System.Windows.Forms.AxHost;

namespace LoadOrderToolTwo.Utilities.Managers;
internal static class CentralManager
{
	private static List<Package>? packages;

	public static event Action? ContentLoaded;
	public static event Action? WorkshopInfoUpdated;
	public static event Action? PackageInformationUpdated;
	public static event Action? ModInformationUpdated;
	public static event Action? AssetInformationUpdated;

	private static readonly DelayedAction _delayedWorkshopInfoUpdated;
	private static readonly DelayedAction _delayedPackageInformationUpdated;
	private static readonly DelayedAction _delayedModInformationUpdated;
	private static readonly DelayedAction _delayedAssetInformationUpdated;

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
				if (package.IsPseudoMod && SessionSettings.UserSettings.HidePseudoMods)
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
				if (package.IsPseudoMod && SessionSettings.UserSettings.HidePseudoMods)
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
				if (package.IsPseudoMod && SessionSettings.UserSettings.HidePseudoMods)
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
		ISave.CustomSaveDirectory = Directory.GetParent(Application.ExecutablePath).FullName;

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

		_delayedWorkshopInfoUpdated = new(300, () => WorkshopInfoUpdated?.Invoke());
		_delayedPackageInformationUpdated = new(300, () => PackageInformationUpdated?.Invoke());
		_delayedModInformationUpdated = new(300, () => ModInformationUpdated?.Invoke());
		_delayedAssetInformationUpdated = new(300, () => AssetInformationUpdated?.Invoke());
	}

	public static void Start()
	{
		if (!SessionSettings.FirstTimeSetupCompleted)
		{
			try
			{ RunFirstTimeSetup(); }
			catch (Exception ex) { Log.Exception(ex, "Failed to complete the First Time Setup", true); }
		}

		if (LocationManager.Platform is Platform.MacOSX)
		{
			ConnectionHandler.Start("steamcommunity.com", 60000);
		}
		else
		{
			ConnectionHandler.Start();
		}

		Log.Info("Loading packages..");

		var content = ContentUtil.LoadContents();

		Log.Info($"Loaded {content.Count} packages");
		Log.Info($"Analyzing packages..");

		AnalyzePackages(content);

		Log.Info($"Finished analyzing packages..");

		packages = content;

		IsContentLoaded = true;

		ContentLoaded?.Invoke();

		if (CommandUtil.PreSelectedProfile == CurrentProfile.Name)
		{
			Log.Info($"[Command] Applying Profile ({CurrentProfile.Name})..");
			ProfileManager.SetProfile(CurrentProfile);
		}

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
		Log.Info($"Loading Steam Cache..");

		var cachedSteamInfo = SteamUtil.GetCachedInfo();

		if (cachedSteamInfo != null)
		{
			Log.Info($"Applying Steam Cache..");
			foreach (var package in Packages)
			{
				if (cachedSteamInfo.ContainsKey(package.SteamId))
				{
					package.SetSteamInformation(cachedSteamInfo[package.SteamId], true);
				}
			}

			_delayedWorkshopInfoUpdated.Run();

			Parallelism.ForEach(Packages.OrderBy(x => x.Mod == null).ToList(), (package) =>
			{
				package.Status = ModsUtil.GetStatus(package, out var reason);
				package.StatusReason = reason;

				InformationUpdate(package);
			});
		}
		else
			Log.Info($"No Steam Cache");

		if (!ConnectionHandler.WhenConnected(UpdateSteamInformation))
		{
			_delayedWorkshopInfoUpdated.Run();

			Log.Warning("Not connected to the internet");
		}
	}

	private static async void UpdateSteamInformation()
	{
		if (!ConnectionHandler.IsConnected)
		{
			return;
		}

		SteamUtil.LoadDlcs();

		Log.Info($"Loading Steam info from the web..");
		var result = await SteamUtil.GetWorkshopInfoAsync(Packages.Where(x => x.Workshop).Select(x => x.SteamId).ToArray());

		Log.Info($"Applying updated steam info..");
		foreach (var package in Packages)
		{
			if (result.ContainsKey(package.SteamId))
			{
				package.SetSteamInformation(result[package.SteamId], false);
			}
		}

		_delayedWorkshopInfoUpdated.Run();

		Log.Info($"Loading thumbnails..");
		Parallelism.ForEach(Packages.OrderBy(x => x.Mod == null).ThenBy(x => x.Name).ToList(), async (package) =>
		{
			if (!string.IsNullOrWhiteSpace(package.IconUrl))
			{
				if (await ImageManager.Ensure(package.IconUrl))
				{
					InformationUpdate(package);
				}
			}

			if (!string.IsNullOrWhiteSpace(package.Author?.AvatarUrl))
			{
				if (await ImageManager.Ensure(package.Author?.AvatarUrl))
				{
					InformationUpdate(package);
				}
			}
		});

		Log.Info($"Load Complete");
		_delayedWorkshopInfoUpdated.Run();

		new BackgroundAction(UpdateSteamInformation).RunIn((int)TimeSpan.FromHours(1).TotalMilliseconds);
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

		File.WriteAllText(Path.Combine(LocationManager.LotAppDataPath, "SetupComplete.txt"), "Delete this file if your LOT hasn't been set up correctly and want to try again.\r\n\r\nLaunch the game, enable the mod and open Load Order Tool from the main menu after deleting this file.");
	}

	private static void AnalyzePackages(List<Package> content)
	{
		var firstTime = UpdateManager.IsFirstTime();

		foreach (var package in content)
		{
			if (!firstTime)
			{
				HandleNewPackage(package);
			}

			if (package.Mod is not null)
			{
				if (!SessionSettings.UserSettings.AdvancedIncludeEnable)
				{
					if (package.Mod.IsIncluded && !package.Mod.IsEnabled)
					{
						package.Mod.IsIncluded = false;
					}
				}

				ModLogicManager.Analyze(package.Mod);
			}
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

	public static void InformationUpdate(IPackage iPackage)
	{
		if (iPackage is Package package)
		{
			_delayedPackageInformationUpdated.Run();

			if (package.Mod != null)
			{
				_delayedModInformationUpdated.Run();
			}

			if (package.Assets != null)
			{
				foreach (var asset in package.Assets)
				{
					_delayedAssetInformationUpdated.Run();
				}
			}
		}
		else if (iPackage is Mod mod)
		{
			_delayedModInformationUpdated.Run();
			_delayedPackageInformationUpdated.Run();
		}
		else if (iPackage is Asset asset)
		{
			_delayedAssetInformationUpdated.Run();
			_delayedPackageInformationUpdated.Run();
		}
	}

	internal static void AddPackage(Package package)
	{
		var cachedSteamInfo = SteamUtil.GetCachedInfo();

		if (cachedSteamInfo != null && cachedSteamInfo.ContainsKey(package.SteamId))
		{
			package.SetSteamInformation(cachedSteamInfo[package.SteamId], true);
		}

		if (packages is null)
		{
			packages = new List<Package>() { package };
		}
		else
		{
			packages.Add(package);
		}

		HandleNewPackage(package);

		if (package.Mod is not null)
		{
			ModLogicManager.Analyze(package.Mod);
		}

		RefreshSteamInfo(package);
		ContentLoaded?.Invoke();
	}

	internal static void RefreshSteamInfo(Package package)
	{
		if (!package.Workshop)
		{
			return;
		}

		ConnectionHandler.WhenConnected(async () =>
		{
			var result = await SteamUtil.GetWorkshopInfoAsync(new ulong[] { package.SteamId });

			if (result.ContainsKey(package.SteamId))
			{
				package.SetSteamInformation(result[package.SteamId], false);
			}

			_delayedWorkshopInfoUpdated.Run();

			if (!string.IsNullOrWhiteSpace(package.IconUrl))
			{
				if (await ImageManager.Ensure(package.IconUrl))
				{
					InformationUpdate(package);
				}
			}

			if (!string.IsNullOrWhiteSpace(package.Author?.AvatarUrl))
			{
				if (await ImageManager.Ensure(package.Author?.AvatarUrl))
				{
					InformationUpdate(package);
				}
			}

			_delayedWorkshopInfoUpdated.Run();
		});
	}

	internal static void RemovePackage(Package package)
	{
		packages?.Remove(package);

		if (package.Mod is not null)
		{
			ModLogicManager.ModRemoved(package.Mod);
		}

		package.Status = DownloadStatus.NotDownloaded;
		ContentLoaded?.Invoke();
		_delayedWorkshopInfoUpdated.Run();
	}
}
