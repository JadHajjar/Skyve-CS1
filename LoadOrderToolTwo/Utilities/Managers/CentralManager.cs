using Extensions;

using LoadOrderToolTwo.Domain;
using LoadOrderToolTwo.Domain.Interfaces;
using LoadOrderToolTwo.Domain.Utilities;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace LoadOrderToolTwo.Utilities.Managers;
internal static class CentralManager
{

	private static List<Package>? packages;

	public static event Action? ContentLoaded;
	public static event Action? WorkshopInfoUpdated;
	public static event Action? PackageInformationUpdated;
	public static event Action? ModInformationUpdated;
	public static event Action? AssetInformationUpdated;

	private static readonly DelayedAction _delayedWorkshopInfoUpdated = new(300, () => WorkshopInfoUpdated?.Invoke());
	private static readonly DelayedAction _delayedPackageInformationUpdated = new(300, () => PackageInformationUpdated?.Invoke());
	private static readonly DelayedAction _delayedModInformationUpdated = new(300, () => ModInformationUpdated?.Invoke());
	private static readonly DelayedAction _delayedAssetInformationUpdated = new(300, () => AssetInformationUpdated?.Invoke());

	public static Profile CurrentProfile => ProfileManager.CurrentProfile;
	public static bool IsContentLoaded { get; private set; }
	public static SessionSettings SessionSettings { get; } = ISave.Load<SessionSettings>(nameof(SessionSettings) + ".json");
	public static IEnumerable<Package> Packages => packages ?? new();

	public static IEnumerable<Mod> Mods
	{
		get
		{
			var currentPackages = packages ?? new();

			foreach (var package in currentPackages)
			{
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
			var currentPackages = packages ?? new();

			foreach (var package in currentPackages)
			{
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

	public static void Start()
	{
		if (!SessionSettings.FirstTimeSetupCompleted)
		{
			try
			{ RunFirstTimeSetup(); }
			catch (Exception ex) { Log.Exception(ex, "Failed to complete the First Time Setup", true); }
		}

		var content = ContentUtil.LoadContents();

		AnalyzePackages(content);

		packages = content;

		IsContentLoaded = true;

		ContentLoaded?.Invoke();

		if (CommandUtil.PreSelectedProfile == CurrentProfile.Name)
		{
			ProfileManager.SetProfile(CurrentProfile, null);
		}

		if (CommandUtil.LaunchOnLoad)
		{
			CitiesManager.Launch();
		}

		if (CommandUtil.NoWindow)
		{
			return;
		}

		ContentUtil.StartListeners();

		var cachedSteamInfo = SteamUtil.GetCachedInfo();

		if (cachedSteamInfo != null)
		{
			foreach (var package in Packages)
			{
				if (cachedSteamInfo.ContainsKey(package.SteamId))
				{
					package.SetSteamInformation(cachedSteamInfo[package.SteamId], true);
				}
			}

			_delayedWorkshopInfoUpdated.Run();

			Parallel.ForEach(Packages.OrderBy(x => x.Mod == null), (package, state) =>
			{
				package.Status = ModsUtil.GetStatus(package, out var reason);
				package.StatusReason = reason;

				InformationUpdate(package);
			});
		}

		ConnectionHandler.WhenConnected(async () =>
		{
			SteamUtil.LoadDlcs();

			var result = await SteamUtil.GetWorkshopInfoAsync(Packages.Where(x => x.Workshop).Select(x => x.SteamId).ToArray());

			foreach (var package in Packages)
			{
				if (result.ContainsKey(package.SteamId))
				{
					package.SetSteamInformation(result[package.SteamId], false);
				}
			}

			_delayedWorkshopInfoUpdated.Run();

			Parallel.ForEach(Packages.OrderBy(x => x.Mod == null).ThenBy(x => x.Name), (package, state) =>
			{
				if (!string.IsNullOrWhiteSpace(package.IconUrl))
				{
					if (ImageManager.Ensure(package.IconUrl))
					{
						InformationUpdate(package);
					}
				}

				if (!string.IsNullOrWhiteSpace(package.Author?.AvatarUrl))
				{
					if (ImageManager.Ensure(package.Author?.AvatarUrl))
					{
						InformationUpdate(package);
					}
				}
			});

			_delayedWorkshopInfoUpdated.Run();
		});
	}

	private static void RunFirstTimeSetup()
	{
		LocationManager.RunFirstTimeSetup();

		if (LocationManager.Platform is Platform.Windows)
		{
			ContentUtil.CreateShortcut();
		}

		SessionSettings.FirstTimeSetupCompleted = true;
		SessionSettings.Save();

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

			if (!SessionSettings.AdvancedIncludeEnable && package.Mod is not null)
			{
				if (package.Mod.IsIncluded && !package.Mod.IsEnabled)
				{
					package.Mod.IsEnabled = true;
				}
			}

			if (package.Mod is not null)
			{
				ModLogicManager.Analyze(package.Mod);
			}
		}
	}

	private static void HandleNewPackage(Package package)
	{
		if (UpdateManager.IsPackageKnown(package))
		{
			return;
		}

		if (package.Mod is not null)
		{
			package.Mod.IsIncluded = !SessionSettings.DisableNewModsByDefault;

			if (SessionSettings.AdvancedIncludeEnable)
			{
				package.Mod.IsEnabled = !SessionSettings.DisableNewModsByDefault;
			}
		}

		if (package.Assets is not null)
		{
			foreach (var asset in package.Assets)
			{
				asset.IsIncluded = !SessionSettings.DisableNewAssetsByDefault;
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
				if (ImageManager.Ensure(package.IconUrl))
				{
					InformationUpdate(package);
				}
			}

			if (!string.IsNullOrWhiteSpace(package.Author?.AvatarUrl))
			{
				if (ImageManager.Ensure(package.Author?.AvatarUrl))
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
