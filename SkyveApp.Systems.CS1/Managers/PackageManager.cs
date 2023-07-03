using Extensions;

using SkyveApp.Domain;
using SkyveApp.Domain.CS1;
using SkyveApp.Domain.CS1.Utilities;
using SkyveApp.Domain.Systems;
using SkyveApp.Systems.CS1.Utilities;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SkyveApp.Systems.CS1.Managers;
internal class PackageManager : IPackageManager
{
	private Dictionary<ulong, ILocalPackageWithContents>? indexedPackages;
	private List<ILocalPackageWithContents>? packages;

	private readonly IModLogicManager _modLogicManager;
	private readonly ISettings _settings;
	private readonly ILogger _logger;
	private readonly INotifier _notifier;
	private readonly ILocationManager _locationManager;

	public PackageManager(IModLogicManager modLogicManager, ISettings settings, ILogger logger, INotifier notifier, ILocationManager locationManager)
	{
		_modLogicManager = modLogicManager;
		_settings = settings;
		_logger = logger;
		_notifier = notifier;
		_locationManager = locationManager;
	}

	public IEnumerable<ILocalPackageWithContents> Packages
	{
		get
		{
			var currentPackages = packages is null ? new() : new List<ILocalPackageWithContents>(packages);

			foreach (var package in currentPackages)
			{
				if (_settings.UserSettings.HidePseudoMods && _modLogicManager.IsPseudoMod(package))
				{
					continue;
				}

				yield return package;
			}
		}
	}

	public IEnumerable<IMod> Mods
	{
		get
		{
			var currentPackages = packages is null ? new() : new List<ILocalPackageWithContents>(packages);

			foreach (var package in currentPackages)
			{
				if (_settings.UserSettings.HidePseudoMods && _modLogicManager.IsPseudoMod(package))
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

	public IEnumerable<IAsset> Assets
	{
		get
		{
			var currentPackages = packages is null ? new() : new List<ILocalPackageWithContents>(packages);

			foreach (var package in currentPackages)
			{
				if (_settings.UserSettings.HidePseudoMods && _modLogicManager.IsPseudoMod(package))
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

	public void AddPackage(ILocalPackageWithContents package)
	{
		var info = SteamUtil.GetItem(package.Id);

		if (packages is null)
		{
			packages = new() { package };
			indexedPackages = packages.Where(x => x.Id != 0).ToDictionary(x => x.Id);
		}
		else
		{
			packages.Add(package);
			if (indexedPackages is not null && package.Id != 0)
			{
				indexedPackages[package.Id] = package;
			}
		}

		_notifier.OnInformationUpdated();
		_notifier.OnContentLoaded();
	}

	public void RemovePackage(ILocalPackageWithContents package)
	{
		packages?.Remove(package);
		indexedPackages?.Remove(package.Id);

		if (package.Mod is not null)
		{
			_modLogicManager.ModRemoved(package.Mod);
		}

		_notifier.OnContentLoaded();
		_notifier.OnWorkshopInfoUpdated();

		DeleteAll(package.Folder);
	}

	public ILocalPackageWithContents? GetPackageById(IPackageIdentity identity)
	{
		if (indexedPackages?.TryGetValue(identity.Id, out var package) ?? false)
		{
			return package;
		}

		return null;
	}

	public void SetPackages(List<ILocalPackageWithContents> content)
	{
		packages = content;
		indexedPackages = content.Where(x => x.Id != 0).ToDictionary(x => x.Id);
	}

	public void DeleteAll(IEnumerable<ulong> ids)
	{
		foreach (var id in ids)
		{
			DeleteAll(CrossIO.Combine(_locationManager.WorkshopContentPath, id.ToString()));
		}
	}

	public void DeleteAll(string folder)
	{
		var package = Packages.FirstOrDefault(x => x.Folder.PathEquals(folder));

		if (package != null)
		{
			RemovePackage(package);
		}

		PackageWatcher.Pause();
		try
		{ CrossIO.DeleteFolder(folder); }
		catch (Exception ex) { _logger.Exception(ex, $"Failed to delete the folder '{folder}'"); }
		PackageWatcher.Resume();
	}

	public void MoveToLocalFolder(ILocalPackage item)
	{
		if (item is Asset asset)
		{
			CrossIO.CopyFile(asset.FilePath, CrossIO.Combine(_locationManager.AssetsPath, Path.GetFileName(asset.FilePath)), true);
			return;
		}

		if (item.LocalParentPackage?.Assets?.Any() ?? false)
		{
			var target = new DirectoryInfo(CrossIO.Combine(_locationManager.AssetsPath, Path.GetFileName(item.Folder)));

			new DirectoryInfo(item.Folder).CopyAll(target, x => Path.GetExtension(x).Equals(".crp", StringComparison.CurrentCultureIgnoreCase));

			target.RemoveEmptyFolders();
		}

		if (item.LocalParentPackage?.Mod is not null)
		{
			var target = new DirectoryInfo(CrossIO.Combine(_locationManager.ModsPath, Path.GetFileName(item.Folder)));

			new DirectoryInfo(item.Folder).CopyAll(target, x => !Path.GetExtension(x).Equals(".crp", StringComparison.CurrentCultureIgnoreCase));

			target.RemoveEmptyFolders();
		}
	}
}
