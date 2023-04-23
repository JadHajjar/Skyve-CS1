using Extensions;

using LoadOrderToolTwo.Domain;
using LoadOrderToolTwo.Domain.Enums;
using LoadOrderToolTwo.Domain.Interfaces;
using LoadOrderToolTwo.Domain.Utilities;
using LoadOrderToolTwo.Utilities.Managers;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace LoadOrderToolTwo.Utilities;
internal class ContentUtil
{
	public const string EXCLUDED_FILE_NAME = ".excluded";

	private static readonly object _contentUpdateLock = new();

	private static readonly Dictionary<string, ModDllCache> _dllCache = new(StringComparer.OrdinalIgnoreCase);

	internal static bool BulkUpdating { get; set; }

	static ContentUtil()
	{
		ISave.Load(out List<ModDllCache> cache, "ModDllCache.json");

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

		CentralManager.ContentLoaded += SaveDllCache;
	}

	public static IEnumerable<string> GetSubscribedItemPaths()
	{
		if (!Directory.Exists(LocationManager.WorkshopContentPath))
		{
			Log.Warning($"Folder not found: '{LocationManager.WorkshopContentPath}'");
			yield break;
		}

		Log.Info($"Looking for packages in: '{LocationManager.WorkshopContentPath}'");
		foreach (var path in Directory.EnumerateDirectories(LocationManager.WorkshopContentPath))
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

	public static string GetSubscribedItemPath(ulong id)
	{
		return LocationManager.Combine(LocationManager.WorkshopContentPath, id.ToString());
	}

	public static DateTime GetLocalUpdatedTime(string path)
	{
		var dateTime = DateTime.MinValue;

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

		return dateTime;
	}

	public static DateTime GetLocalSubscribeTime(string path)
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

	public static long GetTotalSize(string path)
	{
		return new DirectoryInfo(path).GetFiles("*", SearchOption.AllDirectories).Sum(f => f.Length);
	}

	internal static List<Package> LoadContents()
	{
		var packages = new List<Package>();
		var gameModsPath = LocationManager.Combine(LocationManager.GameContentPath, "Mods");
		var addonsModsPath = LocationManager.ModsPath;
		var addonsAssetsPath = new[]
		{
			LocationManager.AssetsPath,
			LocationManager.StylesPath,
			LocationManager.MapThemesPath
		};

		foreach (var folder in addonsAssetsPath)
		{
			Log.Info($"Looking for packages in: '{folder}'");

			if (Directory.Exists(folder))
			{
				foreach (var subFolder in Directory.GetDirectories(folder))
				{
					getPackage(subFolder, false, false);
				}
			}

			getPackage(folder, false, false, false);
		}

		if (Directory.Exists(gameModsPath))
		{
			Log.Info($"Looking for packages in: '{gameModsPath}'");
			foreach (var folder in Directory.GetDirectories(gameModsPath))
			{
				getPackage(folder, true, false);
			}
		}
		else
		{
			Log.Warning($"Folder not found: '{gameModsPath}'");
		}

		if (Directory.Exists(addonsModsPath))
		{
			Log.Info($"Looking for packages in: '{addonsModsPath}'");
			foreach (var folder in Directory.GetDirectories(addonsModsPath))
			{
				getPackage(folder, false, false);
			}
		}
		else
		{
			Log.Warning($"Folder not found: '{addonsModsPath}'");
		}

		var subscribedItems = GetSubscribedItemPaths().ToList();

		Parallelism.ForEach(subscribedItems, (folder) =>
		{
			getPackage(folder, false, true);
		});

		return packages;

		void getPackage(string folder, bool builtIn, bool workshop, bool withSubDirectories = true)
		{
			if (!Directory.Exists(folder))
			{
				Log.Warning($"Package folder not found: '{folder}'");

				return;
			}

			var package = new Package(folder, builtIn, workshop);

			package.Assets = AssetsUtil.GetAssets(package, withSubDirectories).ToArray();
			package.Mod = ModsUtil.GetMod(package);

			if (package.Assets.Length != 0 || package.Mod != null)
			{
				lock (packages)
				{
					packages.Add(package);
				}
			}
		}
	}

	internal static void ContentUpdated(string path, bool builtIn, bool workshop, bool self)
	{
		lock (_contentUpdateLock)
		{
			if ((!workshop &&
				!path.PathContains(LocationManager.AssetsPath) &&
				!path.PathContains(LocationManager.StylesPath) &&
				!path.PathContains(LocationManager.MapThemesPath) &&
				!path.PathContains(LocationManager.ModsPath)) ||
				path.PathEquals(LocationManager.ModsPath))
			{
				return;
			}

			var existingPackage = CentralManager.Packages.FirstOrDefault(x => x.Folder.PathEquals(path));

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

	private static void AddNewPackage(string path, bool builtIn, bool workshop, bool self)
	{
		if (workshop && !ulong.TryParse(Path.GetFileName(path), out _))
		{ return; }

		var package = new Package(path, builtIn, workshop);

		package.Assets = AssetsUtil.GetAssets(package, !self).ToArray();
		package.Mod = ModsUtil.GetMod(package);

		CentralManager.AddPackage(package);
	}

	internal static void RefreshPackage(Package package, bool self)
	{
		if (!Directory.Exists(package.Folder))
		{
			CentralManager.RemovePackage(package);
			return;
		}

		package.Assets = AssetsUtil.GetAssets(package, !self).ToArray();
		package.Mod = ModsUtil.GetMod(package);

		if (!package.Workshop && package.Mod is null)
		{
			CentralManager.OnContentLoaded();
		}

		CentralManager.InformationUpdate(package);
		CentralManager.RefreshSteamInfo(package);
	}

	internal static void StartListeners()
	{
		var addonsAssetsPath = new[]
		{
			LocationManager.AssetsPath,
			LocationManager.StylesPath,
			LocationManager.MapThemesPath
		};

		foreach (var folder in addonsAssetsPath)
		{
			PackageWatcher.Create(folder, true, false);
		}

		PackageWatcher.Create(LocationManager.ModsPath, false, false);

		PackageWatcher.Create(LocationManager.WorkshopContentPath, false, true);
	}

	internal static void CreateShortcut()
	{
		try
		{
			ExtensionClass.CreateShortcut(LocationManager.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "LOT 2.lnk"), Program.ExecutablePath);
		}
		catch (Exception ex)
		{
			Log.Exception(ex, "Failed to create shortcut");
		}
	}

	internal static void DeleteAll(IEnumerable<ulong> ids)
	{
		foreach (var id in ids)
		{
			DeleteAll(LocationManager.Combine(LocationManager.WorkshopContentPath, id.ToString()));
		}
	}

	internal static void DeleteAll(string folder)
	{
		var package = CentralManager.Packages.FirstOrDefault(x => x.Folder.PathEquals(folder));

		if (package != null)
		{
			CentralManager.RemovePackage(package);
		}

		PackageWatcher.Pause();
		try
		{ ExtensionClass.DeleteFolder(folder); }
		catch (Exception ex) { Log.Exception(ex, $"Failed to delete the folder '{folder}'"); }
		PackageWatcher.Resume();
	}

	internal static void MoveToLocalFolder<T>(T item) where T : IPackage
	{
		if (item is Asset asset)
		{
			ExtensionClass.CopyFile(asset.FileName, LocationManager.Combine(LocationManager.AssetsPath, Path.GetFileName(asset.FileName)), true);
			return;
		}

		if (item.Package.Assets?.Any() ?? false)
		{
			var target = new DirectoryInfo(LocationManager.Combine(LocationManager.AssetsPath, Path.GetFileName(item.Folder)));

			new DirectoryInfo(item.Folder).CopyAll(target, x => Path.GetExtension(x).Equals(".crp", StringComparison.CurrentCultureIgnoreCase));

			target.RemoveEmptyFolders();
		}

		if (item.Package.Mod is not null)
		{
			var target = new DirectoryInfo(LocationManager.Combine(LocationManager.ModsPath, Path.GetFileName(item.Folder)));

			new DirectoryInfo(item.Folder).CopyAll(target, x => !Path.GetExtension(x).Equals(".crp", StringComparison.CurrentCultureIgnoreCase));

			target.RemoveEmptyFolders();
		}
	}

	internal static GenericPackageState GetGenericPackageState(IGenericPackage item)
	{
		return GetGenericPackageState(item, out _);
	}

	internal static GenericPackageState GetGenericPackageState(IGenericPackage item, out Package? package)
	{
		if (item.SteamId == 0)
		{
			package = null;
			return GenericPackageState.Local;
		}

		package = CentralManager.Packages.FirstOrDefault(x => x.SteamId == item.SteamId);

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

	internal static bool? GetDllModCache(string path, out Version? version)
	{
		if (_dllCache.TryGetValue(path, out var dll))
		{
			var currentLength = new FileInfo(path).Length;

			if (currentLength == dll.Length)
			{
				version = dll.Version;

				return dll.IsMod;
			}
		}

		version = null;
		return null;
	}

	internal static void SetDllModCache(string path, bool isMod, Version? version)
	{
		try
		{
			lock (_dllCache)
			{
				_dllCache[path] = new()
				{
					Path = path,
					Length = new FileInfo(path).Length,
					Version = version,
					IsMod = isMod,
				};
			}
		}
		catch (Exception ex) { Log.Exception(ex, "Failed to save DLL cache"); }
	}

	internal static void SaveDllCache()
	{
		ISave.Save(_dllCache.Values, "ModDllCache.json");
	}

	internal static void SetBulkIncluded(IEnumerable<IPackage> packages, bool value)
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

		BulkUpdating = true;

		foreach (var package in packageList)
		{
			package.IsIncluded = value;
		}

		BulkUpdating = false;

		CentralManager.InformationUpdate(packageList[0]);
		ModsUtil.SavePendingValues();
		AssetsUtil.SaveChanges();
		ProfileManager.TriggerAutoSave();

		static IEnumerable<IPackage> getPackageContents(Package package)
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

	internal static void SetBulkEnabled(IEnumerable<Mod> mods, bool value)
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
