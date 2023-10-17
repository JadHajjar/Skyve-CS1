using Extensions;

using Microsoft.Win32;

using Skyve.Domain;
using Skyve.Domain.CS1.Notifications;
using Skyve.Domain.Systems;

using System;
using System.Configuration;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

namespace Skyve.Systems.CS1.Managers;
internal class LocationManager : ILocationManager
{
	internal const string LOCAL_APP_DATA_PATH = "%LOCALAPPDATA%";
	internal const string CITIES_PATH = "%CITIES%";
	internal const string WS_CONTENT_PATH = "%WORKSHOP%";

	private readonly ILogger _logger;
	private readonly ISettings _settings;

	// Base Folders
	public string GamePath { get; set; }
	public string AppDataPath { get; set; }
	public string SteamPath { get; set; }

	public string DataPath => CrossIO.Combine(GamePath, "Cities_Data");
	public string ManagedDLL => CrossIO.Combine(DataPath, "Managed");
	public string MonoPath => CrossIO.Combine(DataPath, "Mono");
	public string AddonsPath => CrossIO.Combine(AppDataPath, "Addons");
	public string SkyveAppDataPath => CrossIO.Combine(AppDataPath, "Skyve");
	public string SkyvePlaysetsAppDataPath => CrossIO.Combine(SkyveAppDataPath, "Profiles");
	public string ModsPath => CrossIO.Combine(AddonsPath, "Mods");
	public string AssetsPath => CrossIO.Combine(AddonsPath, "Assets");
	public string MapThemesPath => CrossIO.Combine(AddonsPath, "MapThemes");
	public string MapsPath => CrossIO.Combine(AppDataPath, "Maps");
	public string StylesPath => CrossIO.Combine(AddonsPath, "Styles");

	public string WorkshopContentPath
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

			return CrossIO.Combine(parent, "workshop", "content", "255710").FormatPath();
		}
	}

	public string GameContentPath
	{
		get
		{
			if (CrossIO.CurrentPlatform == Platform.MacOSX)
			{
				return CrossIO.Combine(GamePath, "Cities.app", "Contents", "Resources", "Files");
			}

			return CrossIO.Combine(GamePath, "Files");
		}
	}

	public string SteamPathWithExe => CrossIO.Combine(SteamPath, SteamExe);

	public string CitiesPathWithExe => CrossIO.Combine(GamePath, CitiesExe);

	private string CitiesExe => CrossIO.CurrentPlatform switch
	{
		Platform.MacOSX => "Cities_Loader.sh",
		Platform.Linux => "Cities.x64",
		Platform.Windows or _ => "Cities.exe",
	};

	private string SteamExe => CrossIO.CurrentPlatform switch
	{
		Platform.MacOSX => "steam_osx",
		Platform.Linux => string.Empty,
		Platform.Windows or _ => "Steam.exe",
	};

	public LocationManager(ILogger logger, ISettings settings, INotificationsService notificationsService)
	{
		_logger = logger;
		_settings = settings;

		if (_settings.FolderSettings.GamePath is null)
		{
			GamePath = string.Empty;
			AppDataPath = string.Empty;
			SteamPath = string.Empty;
			return;
		}

		GamePath = _settings.FolderSettings.GamePath?.FormatPath() ?? string.Empty;
		AppDataPath = _settings.FolderSettings.AppDataPath?.FormatPath() ?? string.Empty;
		SteamPath = _settings.FolderSettings.SteamPath?.FormatPath() ?? string.Empty;

		SetCorrectPathSeparator();

		if (_settings.SessionSettings.FirstTimeSetupCompleted)
		{
			if (!CrossIO.FileExists(CitiesPathWithExe) || !Directory.Exists(AppDataPath) || (!string.IsNullOrEmpty(SteamPath) && !CrossIO.FileExists(SteamPathWithExe)))
			{
				notificationsService.SendNotification(new IncorrectLocationSettingsNotification());
			}
		}

		_logger.Info("Folder Settings:\r\n" +
			$"Platform: {CrossIO.CurrentPlatform}\r\n" +
			$"GamePath: {GamePath}\r\n" +
			$"AppDataPath: {AppDataPath}\r\n" +
			$"GameContentPath: {GameContentPath}\r\n" +
			$"SteamPath: {SteamPath}\r\n" +
			$"WorkshopContentPath: {WorkshopContentPath}");

		try
		{
			if (Directory.Exists(CrossIO.Combine(AppDataPath, "LoadOrderTwo")))
			{
				CrossIO.MoveFolder(CrossIO.Combine(AppDataPath, "LoadOrderTwo"), SkyveAppDataPath, false);

				if (CrossIO.FileExists(CrossIO.Combine(SkyveAppDataPath, "LoadOrderConfig.xml")) && !CrossIO.FileExists(CrossIO.Combine(SkyveAppDataPath, "SkyveConfig.xml")))
				{
					File.Move(CrossIO.Combine(SkyveAppDataPath, "LoadOrderConfig.xml"), CrossIO.Combine(SkyveAppDataPath, "SkyveConfig.xml"));
				}

				CrossIO.DeleteFolder(CrossIO.Combine(AppDataPath, "LoadOrderTwo"));

				if (CrossIO.CurrentPlatform is Platform.Windows)
				{
					CreateShortcut();
				}
			}
		}
		catch (Exception ex) { _logger.Exception(ex, "Failed to copy previous settings over"); }
	}

	public void RunFirstTimeSetup()
	{
		var settings = _settings.FolderSettings;

		settings.GamePath = ConfigurationManager.AppSettings[nameof(GamePath)] ?? string.Empty;
		settings.AppDataPath = ConfigurationManager.AppSettings[nameof(AppDataPath)] ?? string.Empty;
		settings.SteamPath = ConfigurationManager.AppSettings[nameof(SteamPath)] ?? string.Empty;
		settings.Platform = Enum.TryParse(ConfigurationManager.AppSettings[nameof(Platform)], out Platform platform) ? platform : Platform.Windows;

		CrossIO.CurrentPlatform = settings.Platform;

		_logger.Info("FTS Folder Settings:\r\n" +
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
				_logger.Info("Matching macOS Paths");

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
			catch (Exception ex) { _logger.Exception(ex, "Failed to find steam's installation folder"); }

			if (settings.Platform is not Platform.Windows)
			{
				settings.GamePath = settings.GamePath.Replace('\\', '/');
				settings.AppDataPath = settings.AppDataPath.Replace('\\', '/');
				settings.SteamPath = settings.SteamPath.Replace('\\', '/');
			}

			settings.Save();

			GamePath = settings.GamePath.TrimEnd('/', '\\');
			AppDataPath = settings.AppDataPath.TrimEnd('/', '\\');
			SteamPath = settings.SteamPath.TrimEnd('/', '\\');

			var externalConfig = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
			var appSettings = externalConfig.AppSettings;

			appSettings.Settings[nameof(GamePath)].Value = string.Empty;
			appSettings.Settings[nameof(AppDataPath)].Value = string.Empty;
			appSettings.Settings[nameof(SteamPath)].Value = string.Empty;

			externalConfig.Save();

			SetCorrectPathSeparator();

			Directory.CreateDirectory(SkyveAppDataPath);

			if (File.Exists(CrossIO.Combine(AppDataPath, "LoadOrder", "LoadOrderConfig.xml")) && !File.Exists(CrossIO.Combine(SkyveAppDataPath, "SkyveConfig.xml")))
			{
				CrossIO.CopyFile(CrossIO.Combine(AppDataPath, "LoadOrder", "LoadOrderConfig.xml"), CrossIO.Combine(SkyveAppDataPath, "SkyveConfig.xml"), true);
			}
		}
	}

	private string FindSteamPath(IFolderSettings settings)
	{
		_logger.Info("Finding steam's path");

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

	public void SetPaths(string gamePath, string appDataPath, string steamPath)
	{
		_settings.FolderSettings.GamePath = gamePath;
		_settings.FolderSettings.AppDataPath = appDataPath;
		_settings.FolderSettings.SteamPath = steamPath;

		_settings.FolderSettings.Save();
	}

	private void SetCorrectPathSeparator()
	{
		var field = typeof(Path).GetField(nameof(Path.DirectorySeparatorChar), BindingFlags.Static | BindingFlags.Public);
		field.SetValue(null, CrossIO.PathSeparator[0]);
	}

	public void CreateShortcut()
	{
		try
		{
			ExtensionClass.CreateShortcut(CrossIO.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "Skyve CS-I.lnk"), Application.ExecutablePath);
		}
		catch (Exception ex)
		{
			_logger.Exception(ex, "Failed to create shortcut");
		}
	}

	public string ToRelativePath(string? localPath)
	{
		if (localPath is null or "")
		{
			return string.Empty;
		}

		return localPath
			.Replace(AppDataPath, LOCAL_APP_DATA_PATH)
			.Replace(GamePath, CITIES_PATH)
			.Replace(WorkshopContentPath, WS_CONTENT_PATH)
			.FormatPath();
	}

	public string ToLocalPath(string? relativePath)
	{
		if (relativePath is null or "")
		{
			return string.Empty;
		}

		return relativePath
			.Replace(LOCAL_APP_DATA_PATH, AppDataPath)
			.Replace(CITIES_PATH, GamePath)
			.Replace(WS_CONTENT_PATH, WorkshopContentPath)
			.FormatPath();
	}
}
