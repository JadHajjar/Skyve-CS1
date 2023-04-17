using Extensions;

using LoadOrderToolTwo.Domain.Utilities;

using Microsoft.Win32;

using System;
using System.Configuration;
using System.IO;
using System.Reflection;
using System.Text;
using System.Windows.Forms;

namespace LoadOrderToolTwo.Utilities.Managers;
internal static class LocationManager
{
	private static FolderSettings _folderSettings;

	// Base Folders
	public static string GamePath { get; set; }
	public static string AppDataPath { get; set; }
	public static string SteamPath { get; set; }

	public static Platform Platform { get; private set; }

	public static string DataPath => Combine(GamePath, "Cities_Data");
	public static string ManagedDLL => Combine(DataPath, "Managed");
	public static string MonoPath => Combine(DataPath, "Mono");
	public static string AddonsPath => Combine(AppDataPath, "Addons");
	public static string LotAppDataPath => Combine(AppDataPath, "LoadOrderTwo");
	public static string LotProfilesAppDataPath => Combine(LotAppDataPath, "Profiles");
	public static string ModsPath => Combine(AddonsPath, "Mods");
	public static string AssetsPath => Combine(AddonsPath, "Assets");
	public static string MapThemesPath => Combine(AddonsPath, "MapThemes");
	public static string MapsPath => Combine(AppDataPath, "Maps");
	public static string StylesPath => Combine(AddonsPath, "Styles");

	public static string PathSeparator => Platform is Platform.Windows ? "\\" : "/";
	public static string InvalidPathSeparator => Platform is not Platform.Windows ? "\\" : "/";

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

			return Combine(parent, "workshop", "content", "255710").FormatPath();
		}
	}

	public static string GameContentPath
	{
		get
		{
			if (Platform == Platform.MacOSX)
			{
				return Combine(GamePath, "Cities.app", "Contents", "Resources", "Files");
			}

			return Combine(GamePath, "Files");
		}
	}

	public static string SteamPathWithExe => Combine(SteamPath, SteamExe);

	public static string CitiesPathWithExe => Combine(GamePath, CitiesExe);

	private static string CitiesExe => Platform switch
	{
		Platform.MacOSX => "Cities_Loader.sh",
		Platform.Linux => "Cities.x64",
		Platform.Windows or _ => "Cities.exe",
	};

	private static string SteamExe => Platform switch
	{
		Platform.MacOSX => "steam_osx",
		Platform.Linux => string.Empty,
		Platform.Windows or _ => "Steam.exe",
	};

	static LocationManager()
	{
		_folderSettings = ISave.Load<FolderSettings>(nameof(FolderSettings) + ".json");

		if (_folderSettings.GamePath is null)
		{
			GamePath = string.Empty;
			AppDataPath = string.Empty;
			SteamPath = string.Empty;
			return;
		}

		ISave.CurrentPlatform = Platform = _folderSettings.Platform;
		GamePath = _folderSettings.GamePath.FormatPath();
		AppDataPath = _folderSettings.AppDataPath.FormatPath();
		SteamPath = _folderSettings.SteamPath.FormatPath();

		SetCorrectPathSeparator();

		Log.Info("Folder Settings:\r\n" +
			$"Platform: {Platform}\r\n" +
			$"GamePath: {GamePath}\r\n" +
			$"AppDataPath: {AppDataPath}\r\n" +
			$"GameContentPath: {GameContentPath}\r\n" +
			$"SteamPath: {SteamPath}\r\n" +
			$"WorkshopContentPath: {WorkshopContentPath}");
	}

	internal static string FormatPath(this string path)
	{
		return path.TrimEnd('/', '\\').Replace(InvalidPathSeparator, PathSeparator);
	}

	internal static string Combine(params string[] paths)
	{
		if (paths.Length == 0)
		{
			return string.Empty;
		}

		var sb = new StringBuilder(paths[0].TrimEnd('/', '\\'));

		for (var i = 1; i < paths.Length; i++)
		{
			if (!string.IsNullOrWhiteSpace(paths[i]))
			{
				sb.Append(PathSeparator);

				sb.Append(paths[i].TrimEnd('/', '\\'));
			}
		}

		return sb.ToString();
	}

	internal static bool FileExists(string? path)
	{
		if (Platform is not Platform.Windows)
		{
			try
			{ return Directory.GetFiles(Path.GetDirectoryName(path).FormatPath(), Path.GetFileName(path)).Length != 0; }
			catch { }
		}

		return File.Exists(path);
	}

	internal static void RunFirstTimeSetup()
	{
		var settings = new FolderSettings
		{
			GamePath = ConfigurationManager.AppSettings[nameof(GamePath)],
			AppDataPath = ConfigurationManager.AppSettings[nameof(AppDataPath)],
			SteamPath = ConfigurationManager.AppSettings[nameof(SteamPath)],

			Platform = Enum.TryParse(ConfigurationManager.AppSettings[nameof(Platform)], out Platform platform) ? platform : Platform.Windows
		};

		ISave.CurrentPlatform = Platform = settings.Platform;

		Log.Info("FTS Folder Settings:\r\n" +
			$"Platform: {settings.Platform}\r\n" +
			$"GamePath: {settings.GamePath}\r\n" +
			$"AppDataPath: {settings.AppDataPath}\r\n" +
			$"SteamPath: {settings.SteamPath}");

		try
		{
			if (settings.Platform is Platform.Windows)
			{
				return;
			}

			if (settings.Platform is Platform.MacOSX)
			{
				Log.Info("Matching macOS Paths");

				settings.GamePath = Path.GetDirectoryName(Path.GetDirectoryName(settings.GamePath)).Replace('\\', '/');
				settings.AppDataPath = settings.AppDataPath.Replace('\\', '/');

				if (Directory.Exists(settings.GamePath))
				{
					return;
				}
			}
		}
		finally
		{
			try
			{ settings.SteamPath = FindSteamPath(settings); }
			catch (Exception ex) { Log.Exception(ex, "Failed to find steam's installation folder"); }

			if (settings.Platform is not Platform.Windows)
			{
				settings.GamePath = settings.GamePath.Replace('\\', '/');
				settings.AppDataPath = settings.AppDataPath.Replace('\\', '/');
				settings.SteamPath = settings.SteamPath.Replace('\\', '/');
			}

			_folderSettings = settings;

			settings.Save();

			GamePath = settings.GamePath.TrimEnd('/', '\\');
			AppDataPath = settings.AppDataPath.TrimEnd('/', '\\');
			SteamPath = settings.SteamPath.TrimEnd('/', '\\');
			ISave.CurrentPlatform = Platform = settings.Platform;

			var externalConfig = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
			var appSettings = externalConfig.AppSettings;

			appSettings.Settings[nameof(GamePath)].Value = string.Empty;
			appSettings.Settings[nameof(AppDataPath)].Value = string.Empty;
			appSettings.Settings[nameof(SteamPath)].Value = string.Empty;

			externalConfig.Save();

			SetCorrectPathSeparator();

			if (File.Exists(Combine(AppDataPath, "LoadOrder", "LoadOrderConfig.xml")) && !File.Exists(Combine(LotAppDataPath, "LoadOrderConfig.xml")))
			{
				ExtensionClass.CopyFile(Combine(AppDataPath, "LoadOrder", "LoadOrderConfig.xml"), Combine(LotAppDataPath, "LoadOrderConfig.xml"), true);
			}
		}
	}

	private static string FindSteamPath(FolderSettings settings)
	{
		Log.Info("Finding steam's path");

		if (settings.Platform is Platform.Windows)
		{
			const string steamPathSubKey_ = @"Software\Valve\Steam";
			const string steamPathKey_ = "SteamPath";

			using var key = Registry.CurrentUser.OpenSubKey(steamPathSubKey_);
			var path = key?.GetValue(steamPathKey_) as string;

			if (!string.IsNullOrWhiteSpace(path) && Directory.Exists(path))
			{
				return path![0].ToString().ToUpper() + path!.FormatPath().Substring(1);
			}
		}

		if (settings.Platform is Platform.MacOSX)
		{
			var basePath = "/Applications/Steam.app";

			if (Directory.Exists(basePath))
			{
				var file = Directory.GetFiles(basePath, "steam_osx", SearchOption.AllDirectories);

				if (file.Length > 0)
				{
					return Path.GetDirectoryName(file[0]).FormatPath();
				}
			}
		}

		return settings.SteamPath.FormatPath();
	}

	internal static void SetPaths(string gamePath, string appDataPath, string steamPath)
	{
		_folderSettings.GamePath = gamePath;
		_folderSettings.AppDataPath = appDataPath;
		_folderSettings.SteamPath = steamPath;

		_folderSettings.Save();
	}

	private static void SetCorrectPathSeparator()
	{
		var field = typeof(Path).GetField(nameof(Path.DirectorySeparatorChar), BindingFlags.Static | BindingFlags.Public);
		field.SetValue(null, PathSeparator[0]);
	}
}
