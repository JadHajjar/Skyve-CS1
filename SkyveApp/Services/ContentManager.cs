using Extensions;

using SkyveApp.Domain;
using SkyveApp.Domain.Interfaces;
using SkyveApp.Domain.Utilities;
using SkyveApp.Services.Interfaces;
using SkyveApp.Utilities;

using SkyveShared;

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

	private readonly IModLogicManager _modLogicManager;
	private readonly ISettings _settings;
	private readonly ILogger _logger;
	private readonly IUpdateManager _updateManager;
	private readonly INotifier _notifier;

	public ContentManager(IModLogicManager modLogicManager, ISettings settings, ILogger logger, IUpdateManager updateManager, INotifier notifier)
	{
		_modLogicManager = modLogicManager;
		_settings = settings;
		_logger = logger;
		_updateManager = updateManager;
		_notifier = notifier;
	}

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

		_notifier.OnInformationUpdated();
		_notifier.OnContentLoaded();
	}

	public void RemovePackage(Package package)
	{
		packages?.Remove(package);
		indexedPackages?.Remove(package.SteamId);

		if (package.Mod is not null)
		{
			_modLogicManager.ModRemoved(package.Mod);
		}

		_notifier.OnContentLoaded();
		_notifier.OnWorkshopInfoUpdated();

		Program.Services.GetService<IContentUtil>().DeleteAll(package.Folder);
	}

	public Package? GetPackage(ulong steamId)
	{
		if (indexedPackages?.TryGetValue(steamId, out var package) ?? false)
		{
			return package;
		}

		return null;
	}

	public void SetPackages(List<Package> content)
	{
		packages = content;
		indexedPackages = content.Where(x => x.SteamId != 0).ToDictionary(x => x.SteamId);
	}
}
