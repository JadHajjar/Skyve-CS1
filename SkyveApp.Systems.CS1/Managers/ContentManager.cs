using Extensions;

using SkyveApp.Domain;
using SkyveApp.Domain.Interfaces;
using SkyveApp.Domain.Systems;
using SkyveApp.Domain.Utilities;
using SkyveApp.Services.Interfaces;
using SkyveApp.Utilities;

using SkyveShared;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkyveApp.Systems.CS1.Managers;
internal class ContentManager : IContentManager
{
    private Dictionary<ulong, Package>? indexedPackages;
    private List<Package>? packages;

    private readonly IModLogicManager _modLogicManager;
    private readonly ISettings _settings;
    private readonly ILogger _logger;
    private readonly IUpdateManager _updateManager;
    private readonly INotifier _notifier;
    private readonly ILocationManager _locationManager;

    public ContentManager(IModLogicManager modLogicManager, ISettings settings, ILogger logger, IUpdateManager updateManager, INotifier notifier, Interfaces.ILocationManager locationManager)
    {
        _modLogicManager = modLogicManager;
        _settings = settings;
        _logger = logger;
        _updateManager = updateManager;
        _notifier = notifier;
        _locationManager = locationManager;
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

        ServiceCenter.Get<IContentUtil>().DeleteAll(package.Folder);
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

    public void MoveToLocalFolder(IPackage item)
    {
        if (item is Asset asset)
        {
            CrossIO.CopyFile(asset.FilePath, CrossIO.Combine(_locationManager.AssetsPath, Path.GetFileName(asset.FilePath)), true);
            return;
        }

        if (item.Package?.Assets?.Any() ?? false)
        {
            var target = new DirectoryInfo(CrossIO.Combine(_locationManager.AssetsPath, Path.GetFileName(item.Folder)));

            new DirectoryInfo(item.Folder).CopyAll(target, x => Path.GetExtension(x).Equals(".crp", StringComparison.CurrentCultureIgnoreCase));

            target.RemoveEmptyFolders();
        }

        if (item.Package?.Mod is not null)
        {
            var target = new DirectoryInfo(CrossIO.Combine(_locationManager.ModsPath, Path.GetFileName(item.Folder)));

            new DirectoryInfo(item.Folder).CopyAll(target, x => !Path.GetExtension(x).Equals(".crp", StringComparison.CurrentCultureIgnoreCase));

            target.RemoveEmptyFolders();
        }
    }
}
