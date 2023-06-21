using Extensions;

using Microsoft.Win32;

using SkyveApp.Domain.Utilities;
using SkyveApp.Services.Interfaces;
using SkyveApp.Utilities;

using System;
using System.Configuration;
using System.IO;
using System.Reflection;
using System.Text;

namespace SkyveApp.Services;
internal class LocationManager : ILocationManager
{
    private FolderSettings _folderSettings;
	private readonly ILogger _logger;

	// Base Folders
	public string GamePath { get; set; }
    public string AppDataPath { get; set; }
    public string SteamPath { get; set; }

    public Platform Platform { get; private set; }

    public string DataPath => CrossIO.Combine(GamePath, "Cities_Data");
    public string ManagedDLL => CrossIO.Combine(DataPath, "Managed");
    public string MonoPath => CrossIO.Combine(DataPath, "Mono");
    public string AddonsPath => CrossIO.Combine(AppDataPath, "Addons");
    public string SkyveAppDataPath => CrossIO.Combine(AppDataPath, "Skyve");
    public string SkyveProfilesAppDataPath => CrossIO.Combine(SkyveAppDataPath, "Profiles");
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
            if (Platform == Platform.MacOSX)
            {
                return CrossIO.Combine(GamePath, "Cities.app", "Contents", "Resources", "Files");
            }

            return CrossIO.Combine(GamePath, "Files");
        }
    }

    public string SteamPathWithExe => CrossIO.Combine(SteamPath, SteamExe);

    public string CitiesPathWithExe => CrossIO.Combine(GamePath, CitiesExe);

    private string CitiesExe => Platform switch
    {
        Platform.MacOSX => "Cities_Loader.sh",
        Platform.Linux => "Cities.x64",
        Platform.Windows or _ => "Cities.exe",
    };

    private string SteamExe => Platform switch
    {
        Platform.MacOSX => "steam_osx",
        Platform.Linux => string.Empty,
        Platform.Windows or _ => "Steam.exe",
    };

    private LocationManager(ILogger logger)
    {
		_logger = logger;
		_folderSettings = ISave.Load<FolderSettings>(nameof(FolderSettings) + ".json");

        if (_folderSettings.GamePath is null)
        {
            GamePath = string.Empty;
            AppDataPath = string.Empty;
            SteamPath = string.Empty;
            return;
        }

		CrossIO.CurrentPlatform = Platform = _folderSettings.Platform;
        GamePath = _folderSettings.GamePath.FormatPath();
        AppDataPath = _folderSettings.AppDataPath.FormatPath();
        SteamPath = _folderSettings.SteamPath.FormatPath();

        SetCorrectPathSeparator();

        _logger.Info("Folder Settings:\r\n" +
            $"Platform: {Platform}\r\n" +
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

                if (Platform is Platform.Windows)
                {
                    CreateShortcut();
                }
            }
        }
        catch (Exception ex) { _logger.Exception(ex, "Failed to copy previous settings over"); }
	}

    public void RunFirstTimeSetup()
    {
        var settings = new FolderSettings
        {
            GamePath = ConfigurationManager.AppSettings[nameof(GamePath)],
            AppDataPath = ConfigurationManager.AppSettings[nameof(AppDataPath)],
            SteamPath = ConfigurationManager.AppSettings[nameof(SteamPath)],

            Platform = Enum.TryParse(ConfigurationManager.AppSettings[nameof(Platform)], out Platform platform) ? platform : Platform.Windows
        };

		CrossIO.CurrentPlatform = Platform = settings.Platform;

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

            _folderSettings = settings;

            settings.Save();

            GamePath = settings.GamePath.TrimEnd('/', '\\');
            AppDataPath = settings.AppDataPath.TrimEnd('/', '\\');
            SteamPath = settings.SteamPath.TrimEnd('/', '\\');
			CrossIO.CurrentPlatform = Platform = settings.Platform;

            var externalConfig = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            var appSettings = externalConfig.AppSettings;

            appSettings.Settings[nameof(GamePath)].Value = string.Empty;
            appSettings.Settings[nameof(AppDataPath)].Value = string.Empty;
            appSettings.Settings[nameof(SteamPath)].Value = string.Empty;

            externalConfig.Save();

            SetCorrectPathSeparator();

            if (File.Exists(CrossIO.Combine(AppDataPath, "LoadOrder", "LoadOrderConfig.xml")) && !File.Exists(CrossIO.Combine(SkyveAppDataPath, "SkyveConfig.xml")))
            {
				CrossIO.CopyFile(CrossIO.Combine(AppDataPath, "LoadOrder", "LoadOrderConfig.xml"), CrossIO.Combine(SkyveAppDataPath, "SkyveConfig.xml"), true);
            }
        }
    }

    private string FindSteamPath(FolderSettings settings)
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

    internal void SetPaths(string gamePath, string appDataPath, string steamPath)
    {
        _folderSettings.GamePath = gamePath;
        _folderSettings.AppDataPath = appDataPath;
        _folderSettings.SteamPath = steamPath;

        _folderSettings.Save();
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
			ExtensionClass.CreateShortcut(CrossIO.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "Skyve CS-I.lnk"), Program.ExecutablePath);
		}
		catch (Exception ex)
		{
			_logger.Exception(ex, "Failed to create shortcut");
		}
	}
}
