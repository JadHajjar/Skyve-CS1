using SkyveApp.ColossalOrder;
using SkyveApp.Domain;
using SkyveApp.Domain.Utilities;
using SkyveApp.Services;
using SkyveApp.Services.Interfaces;

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.ConstrainedExecution;

namespace SkyveApp.Utilities;
internal class ColossalOrderUtil : IColossalOrderUtil
{
	private const string GAME_SETTINGS_FILE_NAME = "userGameState";
	private SettingsFile _settingsFile;
	private bool _initialized;
	private readonly Dictionary<Mod, SavedBool> _settingsDictionary = new();
	private readonly FileSystemWatcher _watcher;
	private readonly DelayedAction _delayedAction = new(500);

	private readonly ILocationManager _locationManager;
	private readonly ISettings _settings;
	private readonly INotifier _notifier;

	ColossalOrderUtil(ILocationManager locationManager, INotifier notifier, ISettings settings)
	{
		_locationManager = locationManager;
		_notifier = notifier;
		_settings = settings;
		_settingsFile = new SettingsFile() { fileName = GAME_SETTINGS_FILE_NAME };
		_settingsFile.Load();

		_watcher = CreateWatcher();
	}

	private FileSystemWatcher CreateWatcher()
	{
		var watcher = new FileSystemWatcher
		{
			Path = _locationManager.AppDataPath,
			NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName,
			Filter = GAME_SETTINGS_FILE_NAME + SettingsFile.extension
		};

		watcher.Changed += new FileSystemEventHandler(FileChanged);
		watcher.Created += new FileSystemEventHandler(FileChanged);
		watcher.Deleted += new FileSystemEventHandler(FileChanged);

		return watcher;
	}

	public void Start()
	{
		_initialized = true;
		SaveSettings();
	}

	private void FileChanged(object sender, FileSystemEventArgs e)
	{
		_delayedAction.Run(SettingsFileChanged);
	}

	private void SettingsFileChanged()
	{
		if (_settings.SessionSettings.UserSettings.OverrideGameChanges)
		{
			var currentState = _settingsDictionary.ToDictionary(x => x.Key, x => x.Value.value);

			_settingsFile = new SettingsFile() { fileName = GAME_SETTINGS_FILE_NAME };
			_settingsFile.Load();
			_settingsDictionary.Clear();

			foreach (var kvp in currentState)
			{
				SetEnabled(kvp.Key, kvp.Value);
			}

			SaveSettings();
		}
		else
		{
			_settingsFile = new SettingsFile() { fileName = GAME_SETTINGS_FILE_NAME };
			_settingsFile.Load();
			_settingsDictionary.Clear();

			_notifier.OnInformationUpdated();
		}
	}

	public bool IsEnabled(Mod mod)
	{
		if (!_settingsDictionary.ContainsKey(mod))
		{
			_settingsDictionary[mod] = GetEnabledSetting(mod);
		}

		return _settingsDictionary[mod].value;
	}

	public void SetEnabled(Mod mod, bool value)
	{
		if (!_settingsDictionary.ContainsKey(mod))
		{
			_settingsDictionary[mod] = GetEnabledSetting(mod);
		}

		_settingsDictionary[mod].value = value;
	}

	public void SaveSettings()
	{
		if (_initialized)
		{
			_watcher.EnableRaisingEvents = false;
			_settingsFile.Save();
			_watcher.EnableRaisingEvents = true;
		}
	}

	private SavedBool GetEnabledSetting(Mod mod)
	{
		var savedEnabledKey_ = $"{Path.GetFileNameWithoutExtension(mod.Folder)}{GetLegacyHashCode(mod.Folder)}.enabled";

		return new SavedBool(savedEnabledKey_, GAME_SETTINGS_FILE_NAME, def: false, autoUpdate: false) { settingsFile = _settingsFile };
	}

	[ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
	private unsafe int GetLegacyHashCode(string str)
	{
		//fixed (char* ptr = str + (RuntimeHelpers.OffsetToStringData / 2))
		fixed (char* ptr = str + (12 / 2))
		{
			var ptr2 = ptr;
			var ptr3 = ptr2 + str.Length - 1;
			var num = 0;
			while (ptr2 < ptr3)
			{
				num = (num << 5) - num + *ptr2;
				num = (num << 5) - num + ptr2[1];
				ptr2 += 2;
			}
			ptr3++;
			if (ptr2 < ptr3)
			{
				num = (num << 5) - num + *ptr2;
			}
			return num;
		}
	}
}
