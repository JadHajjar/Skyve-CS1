using Extensions;

using SkyveApp.Domain;
using SkyveApp.Domain.Compatibility.Enums;
using SkyveApp.Domain.Enums;
using SkyveApp.Domain.Interfaces;
using SkyveApp.Domain.Utilities;
using SkyveApp.Services;
using SkyveApp.Services.Interfaces;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;

namespace SkyveApp.Utilities;
internal class ContentUtil : IContentUtil
{
	private const string CACHE_FILENAME = "ModDllCache.json";
	public const string EXCLUDED_FILE_NAME = ".excluded";

	private readonly object _contentUpdateLock = new();

	private readonly Dictionary<string, ModDllCache> _dllCache = new(StringComparer.OrdinalIgnoreCase);

	public bool BulkUpdating { get; set; }

	private readonly IContentManager _contentManager;
	private readonly ILocationManager _locationManager;
	private readonly ICompatibilityManager _compatibilityManager;
	private readonly IProfileManager _profileManager;
	private readonly IModUtil _modUtil;
	private readonly IAssetUtil _assetUtil;
	private readonly ILogger _logger;
	private readonly INotifier _notifier;

	public ContentUtil(IContentManager contentManager, ILocationManager locationManager, ICompatibilityManager compatibilityManager, IProfileManager profileManager, ILogger logger, INotifier notifier, IModUtil modUtil, IAssetUtil assetUtil)
	{
		_contentManager = contentManager;
		_locationManager = locationManager;
		_compatibilityManager = compatibilityManager;
		_profileManager = profileManager;
		_modUtil = modUtil;
		_assetUtil = assetUtil;
		_logger = logger;
		_notifier = notifier;

		ISave.Load(out List<ModDllCache> cache, CACHE_FILENAME);

		if (cache != null)
		{
			foreach (var dll in cache)
			{
				if (dll.Path is not null or "")
				{
					_dllCache[dll.Path] = dll;
				}
			}
		}

		_notifier.ContentLoaded += SaveDllCache;
	}

	public IEnumerable<string> GetSubscribedItemPaths()
	{
		if (!Directory.Exists(_locationManager.WorkshopContentPath))
		{
			_logger.Warning($"Folder not found: '{_locationManager.WorkshopContentPath}'");
			yield break;
		}

		_logger.Info($"Looking for packages in: '{_locationManager.WorkshopContentPath}'");
		foreach (var path in Directory.EnumerateDirectories(_locationManager.WorkshopContentPath))
		{
			if (!ulong.TryParse(Path.GetFileName(path), out _))
			{
				continue;
			}

			var files = Directory.GetFiles(path);
			if (files.Length == 1 && files[0].EndsWith(EXCLUDED_FILE_NAME))
			{
				DeleteAll(path);
				continue;
			}

			yield return path;
		}
	}

	public IEnumerable<IPackage> GetReferencingPackage(ulong steamId, bool includedOnly)
	{
		foreach (var item in _contentManager.Packages)
		{
			if (includedOnly && !item.IsIncluded)
			{
				continue;
			}

			var crData = _compatibilityManager.CompatibilityData.Packages.TryGet(item.SteamId);

			if (crData == null)
			{
				if (item.RequiredPackages?.Contains(steamId) ?? false)
				{
					yield return item;
				}
			}
			else if (crData.Interactions.ContainsKey(InteractionType.RequiredPackages))
			{
				if (crData.Interactions[InteractionType.RequiredPackages].Any(x => x.Interaction.Packages?.Contains(steamId) ?? false))
				{
					yield return item;
				}
			}
		}
	}

	public string GetSubscribedItemPath(ulong id)
	{
		return CrossIO.Combine(_locationManager.WorkshopContentPath, id.ToString());
	}

	public DateTime GetLocalUpdatedTime(string path)
	{
		var dateTime = DateTime.MinValue;

		try
		{
			if (Directory.Exists(path))
			{
				foreach (var filePAth in Directory.GetFiles(path, "*", SearchOption.AllDirectories))
				{
					if (Path.GetFileName(filePAth) != EXCLUDED_FILE_NAME)
					{
						var lastWriteTimeUtc = File.GetLastWriteTimeUtc(filePAth);

						if (lastWriteTimeUtc > dateTime)
						{
							dateTime = lastWriteTimeUtc;
						}
					}
				}
			}
		}
		catch (Exception ex) { _logger.Exception(ex, $"Failed to get the local update time for '{path}'"); }

		return dateTime;
	}

	public DateTime GetLocalSubscribeTime(string path)
	{
		var dateTime = DateTime.MaxValue;

		if (Directory.Exists(path))
		{
			foreach (var filePAth in Directory.GetFiles(path, "*", SearchOption.AllDirectories))
			{
				if (Path.GetFileName(filePAth) != EXCLUDED_FILE_NAME)
				{
					var lastWriteTimeUtc = File.GetCreationTimeUtc(filePAth);

					if (lastWriteTimeUtc < dateTime)
					{
						dateTime = lastWriteTimeUtc;
					}
				}
			}
		}

		return dateTime;
	}

	public long GetTotalSize(string path)
	{
		try
		{
			if (Directory.Exists(path))
			{
				return new DirectoryInfo(path).GetFiles("*", SearchOption.AllDirectories).Sum(f => f.Length);
			}
		}
		catch { }

		return 0;
	}

	public List<Package> LoadContents()
	{
		var packages = new List<Package>();
		var gameModsPath = CrossIO.Combine(_locationManager.GameContentPath, "Mods");
		var addonsModsPath = _locationManager.ModsPath;
		var addonsAssetsPath = new[]
		{
			_locationManager.AssetsPath,
			_locationManager.StylesPath,
			_locationManager.MapThemesPath
		};

		foreach (var folder in addonsAssetsPath)
		{
			_logger.Info($"Looking for packages in: '{folder}'");

			if (Directory.Exists(folder))
			{
				foreach (var subFolder in Directory.GetDirectories(folder))
				{
					getPackage(subFolder, false, false, true);
				}
			}

			getPackage(folder, false, false, true, false);
		}

		if (Directory.Exists(gameModsPath))
		{
			_logger.Info($"Looking for packages in: '{gameModsPath}'");
			foreach (var folder in Directory.GetDirectories(gameModsPath))
			{
				getPackage(folder, true, false, false);
			}
		}
		else
		{
			_logger.Warning($"Folder not found: '{gameModsPath}'");
		}

		if (Directory.Exists(addonsModsPath))
		{
			_logger.Info($"Looking for packages in: '{addonsModsPath}'");
			foreach (var folder in Directory.GetDirectories(addonsModsPath))
			{
				getPackage(folder, false, false, false);
			}
		}
		else
		{
			_logger.Warning($"Folder not found: '{addonsModsPath}'");
		}

		var subscribedItems = GetSubscribedItemPaths().ToList();

		Parallelism.ForEach(subscribedItems, (folder) =>
		{
			getPackage(folder, false, true, false);
		});

		return packages;

		void getPackage(string folder, bool builtIn, bool workshop, bool expectAssets, bool withSubDirectories = true)
		{
			if (!Directory.Exists(folder))
			{
				_logger.Warning($"Package folder not found: '{folder}'");

				return;
			}

			var package = new Package(folder, builtIn, workshop);

			package.Assets = _assetUtil.GetAssets(package, withSubDirectories).ToArray();
			package.Mod = expectAssets ? null : _modUtil.GetMod(package);
			package.FileSize = GetTotalSize(package.Folder);
			package.LocalTime = GetLocalUpdatedTime(package.Folder);

			if (package.Assets.Length != 0 || package.Mod != null)
			{
				lock (packages)
				{
					packages.Add(package);
				}
			}
		}
	}

	public void ContentUpdated(string path, bool builtIn, bool workshop, bool self)
	{
		lock (_contentUpdateLock)
		{
			if ((!workshop &&
				!path.PathContains(_locationManager.AssetsPath) &&
				!path.PathContains(_locationManager.StylesPath) &&
				!path.PathContains(_locationManager.MapThemesPath) &&
				!path.PathContains(_locationManager.ModsPath)) ||
				path.PathEquals(_locationManager.ModsPath))
			{
				return;
			}

			var existingPackage = _contentManager.Packages.FirstOrDefault(x => x.Folder.PathEquals(path));

			if (existingPackage != null)
			{
				RefreshPackage(existingPackage, self);
			}
			else
			{
				AddNewPackage(path, builtIn, workshop, self);
			}
		}
	}

	private void AddNewPackage(string path, bool builtIn, bool workshop, bool self)
	{
		if (workshop && !ulong.TryParse(Path.GetFileName(path), out _))
		{ return; }

		var package = new Package(path, builtIn, workshop);

		package.Assets = _assetUtil.GetAssets(package, !self).ToArray();
		package.Mod = _modUtil.GetMod(package);
		package.FileSize = GetTotalSize(package.Folder);
		package.LocalTime = GetLocalUpdatedTime(package.Folder);

		_contentManager.AddPackage(package);
	}

	public void RefreshPackage(Package package, bool self)
	{
		if (IsDirectoryEmpty(package.Folder))
		{
			_contentManager.RemovePackage(package);
			return;
		}

		package.Assets = _assetUtil.GetAssets(package, !self).ToArray();
		package.Mod = _modUtil.GetMod(package);
		package.FileSize = GetTotalSize(package.Folder);
		package.LocalTime = GetLocalUpdatedTime(package.Folder);

		if (!package.Workshop && package.Mod is null)
		{
			_notifier.OnContentLoaded();
		}

		_notifier.OnInformationUpdated();
	}

	private bool IsDirectoryEmpty(string path)
	{
		if (!Directory.Exists(path))
			return true;

		var files = Directory.GetFiles(path);

		if (files.Length == 1 && files[0].EndsWith(EXCLUDED_FILE_NAME))
		{
			return true;
		}

		return false;
	}

	public void StartListeners()
	{
		PackageWatcher.Dispose();

		var addonsAssetsPath = new[]
		{
			_locationManager.AssetsPath,
			_locationManager.StylesPath,
			_locationManager.MapThemesPath
		};

		foreach (var folder in addonsAssetsPath)
		{
			PackageWatcher.Create(folder, true, false);
		}

		PackageWatcher.Create(_locationManager.ModsPath, false, false);

		PackageWatcher.Create(_locationManager.WorkshopContentPath, false, true);
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
		var package = _contentManager.Packages.FirstOrDefault(x => x.Folder.PathEquals(folder));

		if (package != null)
		{
			_contentManager.RemovePackage(package);
		}

		PackageWatcher.Pause();
		try
		{ CrossIO.DeleteFolder(folder); }
		catch (Exception ex) { _logger.Exception(ex, $"Failed to delete the folder '{folder}'"); }
		PackageWatcher.Resume();
	}

	public void MoveToLocalFolder<T>(T item) where T : IPackage
	{
		if (item is Asset asset)
		{
			CrossIO.CopyFile(asset.FileName, CrossIO.Combine(_locationManager.AssetsPath, Path.GetFileName(asset.FileName)), true);
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

	public GenericPackageState GetGenericPackageState(IPackage item)
	{
		return GetGenericPackageState(item, out _);
	}

	public GenericPackageState GetGenericPackageState(IPackage item, out Package? package)
	{
		if (item.SteamId == 0)
		{
			package = null;
			return GenericPackageState.Local;
		}

		package = _contentManager.GetPackage(item.SteamId);

		if (package == null)
		{
			return GenericPackageState.Unsubscribed;
		}

		if (!package.IsIncluded)
		{
			return GenericPackageState.Excluded;
		}

		if (package.Mod is null || package.Mod.IsEnabled)
		{
			return GenericPackageState.Enabled;
		}

		return GenericPackageState.Disabled;
	}

	public bool? GetDllModCache(string path, out Version? version)
	{
		if (_dllCache.TryGetValue(path, out var dll))
		{
			var currentDate = File.GetLastWriteTimeUtc(path);

			if (currentDate == dll.Date)
			{
				version = dll.Version;

				return dll.IsMod;
			}
		}

		version = null;
		return null;
	}

	public void SetDllModCache(string path, bool isMod, Version? version)
	{
		try
		{
			lock (_dllCache)
			{
				_dllCache[path] = new()
				{
					Path = path,
					Date = File.GetLastWriteTimeUtc(path),
					Version = version,
					IsMod = isMod,
				};
			}
		}
		catch (Exception ex) { _logger.Exception(ex, "Failed to save DLL cache"); }
	}

	public void SaveDllCache()
	{
		ISave.Save(_dllCache.Values, CACHE_FILENAME);
	}

	public void ClearDllCache()
	{
		_dllCache.Clear();

		try
		{
			CrossIO.DeleteFile(ISave.GetPath(CACHE_FILENAME)); }
		catch (Exception ex) { _logger.Exception(ex, "Failed to clear DLL cache"); }
	}

	public void SetBulkIncluded(IEnumerable<IPackage> packages, bool value)
	{
		var packageList = packages.ToList();

		if (packageList.Count == 0)
		{
			return;
		}

		if (packageList[0] is Package)
		{
			packageList = packageList.Cast<Package>().SelectMany(getPackageContents).ToList();
		}

		if (packageList.Count == 0)
		{
			return;
		}

		BulkUpdating = true;

		foreach (var package in packageList)
		{
			package.IsIncluded = value;
		}

		BulkUpdating = false;

		_notifier.OnInformationUpdated();
		_modUtil.SavePendingValues();
		_assetUtil.SaveChanges();
		_notifier.TriggerAutoSave();

		IEnumerable<IPackage> getPackageContents(Package package)
		{
			if (package.Mod is not null)
			{
				yield return package.Mod;
			}

			if (package.Assets is not null)
			{
				foreach (var asset in package.Assets)
				{
					yield return asset;
				}
			}
		}
	}

	public void SetBulkEnabled(IEnumerable<Mod> mods, bool value)
	{
		BulkUpdating = true;

		var modList = mods.ToList();

		if (modList.Count == 0)
		{
			BulkUpdating = false;
			return;
		}

		foreach (var package in modList.Skip(1))
		{
			package.IsEnabled = value;
		}

		BulkUpdating = false;

		modList[0].IsEnabled = value;
	}
}
