using Extensions;

using LoadOrderToolTwo.Domain.Utilities;

using Microsoft.Win32;

using System;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace LoadOrderToolTwo.Utilities.Managers;
internal class LocationManager
{
	private static FolderSettings _folderSettings;

	// Base Folders
	public static string GamePath { get; set; }
	public static string AppDataPath { get; set; }
	public static string SteamPath { get; set; }
	public static string VirtualGamePath { get; set; }
	public static string VirtualAppDataPath { get; set; }

	public static string CurrentDirectory { get; }
	public static Platform Platform { get; private set; }

	public static string DataPath => Path.Combine(GamePath, "Cities_Data");
	public static string ManagedDLL => Path.Combine(DataPath, "Managed");
	public static string MonoPath => Path.Combine(DataPath, "Mono");
	public static string AddonsPath => Path.Combine(AppDataPath, "Addons");
	public static string LotAppDataPath => Path.Combine(AppDataPath, "LoadOrderTwo");
	public static string LotProfilesAppDataPath => Path.Combine(LotAppDataPath, "Profiles");
	public static string ModsPath => Path.Combine(AddonsPath, "Mods");
	public static string AssetsPath => Path.Combine(AddonsPath, "Assets");
	public static string MapThemesPath => Path.Combine(AddonsPath, "MapThemes");
	public static string StylesPath => Path.Combine(AddonsPath, "Styles");

	public static string WorkshopContentPath
	{
		get
		{
			if (string.IsNullOrWhiteSpace(GamePath))
			{
				return string.Empty;
			}

			var parent = Path.GetDirectoryName(Path.GetDirectoryName(GamePath));

			if (string.IsNullOrEmpty(parent))
			{
				return string.Empty;
			}

			return Path.Combine(parent, "workshop", "content", "255710");
		}
	}

	public static string VirtualWorkshopContentPath
	{
		get
		{
			if (string.IsNullOrWhiteSpace(VirtualGamePath))
			{
				return string.Empty;
			}

			var parent = Path.GetDirectoryName(Path.GetDirectoryName(VirtualGamePath));

			if (string.IsNullOrEmpty(parent))
			{
				return string.Empty;
			}

			return Path.Combine(parent, "workshop", "content", "255710");
		}
	}

	public static string GameContentPath
	{
		get
		{
			if (Platform == Platform.MacOSX)
			{
				return Path.Combine(GamePath, "Cities.app", "Contents", "Resources", "Files");
			}

			return Path.Combine(GamePath, "Files");
		}
	}

	public static string CitiesExe => Platform switch
	{
		Platform.MacOSX => "Cities_Loader.sh",
		Platform.Linux => "Cities.x64",
		Platform.Windows or _ => "Cities.exe",
	};

	public static string SteamExe => Platform switch
	{
		Platform.MacOSX => "steam_osx",
		Platform.Linux => "Steam",
		Platform.Windows or _ => "Steam.exe",
	};

	static LocationManager()
	{
		_folderSettings = ISave.Load<FolderSettings>(nameof(FolderSettings) + ".json");

		CurrentDirectory = Path.GetDirectoryName(Application.ExecutablePath);

		if (_folderSettings.GamePath is null)
		{
			GamePath = string.Empty;
			AppDataPath = string.Empty;
			SteamPath = string.Empty;
			VirtualGamePath = string.Empty;
			VirtualAppDataPath = string.Empty;
			return;
		}

		GamePath = _folderSettings.GamePath.TrimEnd('/', '\\');
		AppDataPath = _folderSettings.AppDataPath.TrimEnd('/', '\\');
		SteamPath = _folderSettings.SteamPath.TrimEnd('/', '\\');
		VirtualGamePath = _folderSettings.VirtualGamePath.TrimEnd('/', '\\');
		VirtualAppDataPath = _folderSettings.VirtualAppDataPath.TrimEnd('/', '\\');
		ISave.CurrentPlatform = Platform = _folderSettings.Platform;

		Log.Info("Folder Settings:\r\n" +
			$"Platform: {Platform}\r\n" +
			$"GamePath: {GamePath}\r\n" +
			$"AppDataPath: {AppDataPath}\r\n" +
			$"GameContentPath: {WorkshopContentPath}\r\n" +
			$"SteamPath: {SteamPath}\r\n" +
			$"WorkshopContentPath: {WorkshopContentPath}\r\n" +
			$"VirtualGamePath: {VirtualGamePath}\r\n" +
			$"VirtualAppDataPath: {VirtualAppDataPath}\r\n" +
			$"VirtualWorkshopContentPath: {VirtualWorkshopContentPath}");
	}

	internal static string Format(string path, bool @out) => Platform is Platform.Windows || !@out ? path.Replace("/", "\\") : path.Replace("/", "\\");

	internal static void RunFirstTimeSetup()
	{
		var settings = new FolderSettings
		{
			GamePath = ConfigurationManager.AppSettings[nameof(GamePath)],
			AppDataPath = ConfigurationManager.AppSettings[nameof(AppDataPath)],
			SteamPath = ConfigurationManager.AppSettings[nameof(SteamPath)],
			VirtualGamePath = ConfigurationManager.AppSettings[nameof(VirtualGamePath)],
			VirtualAppDataPath = ConfigurationManager.AppSettings[nameof(VirtualAppDataPath)],

			Platform = Enum.TryParse(ConfigurationManager.AppSettings[nameof(Platform)], out Platform platform) ? platform : Platform.Windows
		};

		ISave.CurrentPlatform = settings.Platform;

		Log.Info("FTS Folder Settings:\r\n" +
			$"Platform: {settings.Platform}\r\n" +
			$"GamePath: {settings.GamePath}\r\n" +
			$"AppDataPath: {settings.AppDataPath}\r\n" +
			$"SteamPath: {settings.SteamPath}");

		try
		{
			if (settings.Platform is Platform.Windows)
			{
				if (!Directory.Exists(settings.SteamPath))
				{
					const string steamPathSubKey_ = @"Software\Valve\Steam";
					const string steamPathKey_ = "SteamPath";

					using var key = Registry.CurrentUser.OpenSubKey(steamPathSubKey_);
					var path = key?.GetValue(steamPathKey_) as string;

					if (!string.IsNullOrWhiteSpace(path) && Directory.Exists(path))
					{
						settings.SteamPath = path!;
					}
				}

				return;
			}

			if (settings.Platform is Platform.MacOSX)
			{
				Log.Info("Matching macOS Paths");

				settings.SteamPath = "/Applications/Steam.app/Contents";
				settings.GamePath = Path.GetDirectoryName(Path.GetDirectoryName(settings.GamePath));
				settings.VirtualAppDataPath = settings.AppDataPath;
				settings.VirtualGamePath = settings.GamePath;

				if (Directory.Exists(settings.GamePath))
				{
					return;
				}
			}

			Log.Info("Checking Virtual Paths");

			if (settings.GamePath.StartsWith("/"))
			{
				Log.Info($"GamePath: {settings.GamePath}");
				foreach (var item in DriveInfo.GetDrives().Reverse())
				{
					var virtualPath = item.Name + settings.GamePath.Substring(1);

					if (Directory.Exists(virtualPath))
					{
						Log.Info($"GamePath Matched: {virtualPath}");
						settings.VirtualGamePath = settings.GamePath;
						settings.GamePath = virtualPath;

						break;
					}
					Log.Info($"GamePath Try Failed for: {virtualPath}");
				}
			}

			if (settings.AppDataPath.StartsWith("/"))
			{
				Log.Info($"AppDataPath: {settings.AppDataPath}");

				foreach (var item in DriveInfo.GetDrives().Reverse())
				{
					var virtualPath = item.Name + settings.AppDataPath.Substring(1);

					if (Directory.Exists(virtualPath))
					{
						Log.Info($"AppDataPath Matched: {virtualPath}");
						settings.VirtualAppDataPath = settings.AppDataPath;
						settings.AppDataPath = virtualPath;
						break;
					}

					Log.Info($"AppDataPath Try Failed for: {virtualPath}");
				}
			}

			if (settings.SteamPath.StartsWith("/"))
			{
				Log.Info($"SteamPath: {settings.SteamPath}");

				foreach (var item in DriveInfo.GetDrives().Reverse())
				{
					var virtualPath = item.Name + settings.SteamPath.Substring(1);

					if (Directory.Exists(virtualPath))
					{
						Log.Info($"SteamPath Matched: {virtualPath}");
						settings.SteamPath = virtualPath;
						break;
					}

					Log.Info($"SteamPath Try Failed for: {virtualPath}");
				}
			}
		}
		finally
		{
			_folderSettings = settings;

			settings.Save();

			GamePath = settings.GamePath.TrimEnd('/', '\\');
			AppDataPath = settings.AppDataPath.TrimEnd('/', '\\');
			SteamPath = settings.SteamPath.TrimEnd('/', '\\');
			VirtualGamePath = settings.VirtualGamePath.TrimEnd('/', '\\');
			VirtualAppDataPath = settings.VirtualAppDataPath.TrimEnd('/', '\\');
			ISave.CurrentPlatform = Platform = settings.Platform;

			var externalConfig = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
			var appSettings = externalConfig.AppSettings;

			appSettings.Settings[nameof(GamePath)].Value = string.Empty;
			appSettings.Settings[nameof(AppDataPath)].Value = string.Empty;
			appSettings.Settings[nameof(SteamPath)].Value = string.Empty;
			appSettings.Settings[nameof(VirtualGamePath)].Value = string.Empty;
			appSettings.Settings[nameof(VirtualAppDataPath)].Value = string.Empty;

			externalConfig.Save();

			if (File.Exists(Path.Combine(AppDataPath, "LoadOrder", "LoadOrderConfig.xml")) && !File.Exists(Path.Combine(LotAppDataPath, "LoadOrderConfig.xml")))
			{
				ExtensionClass.CopyFile(Path.Combine(AppDataPath, "LoadOrder", "LoadOrderConfig.xml"), Path.Combine(LotAppDataPath, "LoadOrderConfig.xml"), true);
			}
		}
	}

	internal static void SetPaths(string gamePath, string appDataPath, string steamPath, string virtualGamePath, string virtualAppDataPath)
	{
		_folderSettings.GamePath = gamePath;
		_folderSettings.AppDataPath = appDataPath;
		_folderSettings.SteamPath = steamPath;
		_folderSettings.VirtualAppDataPath = virtualAppDataPath;
		_folderSettings.VirtualGamePath = virtualGamePath;

		_folderSettings.Save();
	}
}
