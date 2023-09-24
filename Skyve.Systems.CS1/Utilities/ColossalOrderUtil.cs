using Extensions;

using Skyve.Domain;
using Skyve.Domain.CS1.ColossalOrder;
using Skyve.Domain.Systems;
using Skyve.Systems.CS1.Systems;

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.ConstrainedExecution;

namespace Skyve.Systems.CS1.Utilities;
internal class ColossalOrderUtil
{
	private const string GAME_SETTINGS_FILE_NAME = "userGameState";
	private SettingsFile _settingsFile;
	private bool _initialized;
	private readonly Dictionary<IMod, SavedBool> _settingsDictionary = new();
	private readonly FileWatcher _watcher;
	private readonly DelayedAction _delayedAction = new(500);

	private readonly SettingsService _settings;
	private readonly ILocationManager _locationManager;
	private readonly INotifier _notifier;
	private readonly ILogger _logger;

	public ColossalOrderUtil(ILocationManager locationManager, INotifier notifier, ISettings settings, ILogger logger)
	{
		_locationManager = locationManager;
		_notifier = notifier;
		_logger = logger;
		_settings = (settings as SettingsService)!;
		_settingsFile = new SettingsFile() { fileName = GAME_SETTINGS_FILE_NAME };
		_settingsFile.Load();

		_watcher = CreateWatcher();
	}

	private FileWatcher CreateWatcher()
	{
		var watcher = new FileWatcher
		{
			Path = _locationManager.AppDataPath,
			NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName,
			Filter = GAME_SETTINGS_FILE_NAME + SettingsFile.extension
		};

		watcher.Changed += FileChanged;
		watcher.Created += FileChanged;
		watcher.Deleted += FileChanged;

		return watcher;
	}

	public void Start()
	{
		_initialized = true;
		SaveSettings();
	}

	private void FileChanged(object sender, FileWatcherEventArgs e)
	{
		_delayedAction.Run(SettingsFileChanged);
	}

	private void SettingsFileChanged()
	{
		_logger.Info($"[Auto] UserGameState update triggered, override [{_settings.SessionSettings.UserSettings.OverrideGameChanges}]");

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

	public bool IsEnabled(IMod mod)
	{
		if (!_settingsDictionary.ContainsKey(mod))
		{
			_settingsDictionary[mod] = GetEnabledSetting(mod);
		}

		return _settingsDictionary[mod].value;
	}

	public void SetEnabled(IMod mod, bool value)
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

	private SavedBool GetEnabledSetting(IMod mod)
	{
		var savedEnabledKey_ = $"{Path.GetFileNameWithoutExtension(mod.Folder)}{GetLegacyHashCode(mod.Folder)}.enabled";

		return new SavedBool(savedEnabledKey_, GAME_SETTINGS_FILE_NAME, def: false, autoUpdate: false) { settingsFile = _settingsFile };
	}

	[ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
	private unsafe int GetLegacyHashCode(string str)
	{
		//fixed (char* ptr = str + (RuntimeHelpers.OffsetToStringData / 2))
		fixed (char* ptr = str + 12 / 2)
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
