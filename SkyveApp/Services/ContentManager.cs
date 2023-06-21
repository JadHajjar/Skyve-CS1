using Extensions;

using SkyveApp.Domain;
using SkyveApp.Domain.Interfaces;
using SkyveApp.Domain.Utilities;
using SkyveApp.Services.Interfaces;
using SkyveApp.Utilities;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkyveApp.Services;
internal class ContentManager : IContentManager
{
	private Dictionary<ulong, Package>? indexedPackages;
	private List<Package>? packages;

	private readonly DelayedAction _delayedPackageInformationUpdated;
	private readonly DelayedAction _delayedPackageInclusionUpdated;
	private readonly DelayedAction _delayedContentLoaded;

	private readonly IModLogicManager _modLogicManager;
	private readonly ICompatibilityManager _compatibilityManager;
	private readonly ISettings _settings;
	private readonly ILogger _logger;
	private readonly IUpdateManager _updateManager;

	public ContentManager(IModLogicManager modLogicManager, ICompatibilityManager compatibilityManager, ISettings settings, ILogger logger, IUpdateManager updateManager)
	{
		_delayedContentLoaded = new(300, () => ContentLoaded?.Invoke());
		_delayedPackageInformationUpdated = new(300, () => PackageInformationUpdated?.Invoke());
		_delayedPackageInclusionUpdated = new(300, () => PackageInclusionUpdated?.Invoke());

		_modLogicManager = modLogicManager;
		_compatibilityManager = compatibilityManager;
		_settings = settings;
		_logger = logger;
		_updateManager = updateManager;
	}

	public bool IsContentLoaded { get; private set; }
	public IEnumerable<Package> Packages
	{
		get
		{
			var currentPackages = packages is null ? new() : new List<Package>(packages);

			foreach (var package in currentPackages)
			{
				if (_settings.SessionSettings.UserSettings.HidePseudoMods && _modLogicManager.IsPseudoMod(package))
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
				if (_settings.SessionSettings.UserSettings.HidePseudoMods && _modLogicManager.IsPseudoMod(package))
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
				if (_settings.SessionSettings.UserSettings.HidePseudoMods && _modLogicManager.IsPseudoMod(package))
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

	public event Action? ContentLoaded;
	public event Action? PackageInformationUpdated;
	public event Action? PackageInclusionUpdated;

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
			package.Mod.IsIncluded = !_settings.SessionSettings.UserSettings.DisableNewModsByDefault;

			if (_settings.SessionSettings.UserSettings.AdvancedIncludeEnable)
			{
				package.Mod.IsEnabled = !_settings.SessionSettings.UserSettings.DisableNewModsByDefault;
			}
		}

		if (package.Assets is not null)
		{
			foreach (var asset in package.Assets)
			{
				asset.IsIncluded = !_settings.SessionSettings.UserSettings.DisableNewAssetsByDefault;
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
