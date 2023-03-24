using LoadOrderToolTwo.ColossalOrder;
using LoadOrderToolTwo.Domain;
using LoadOrderToolTwo.Domain.Utilities;
using LoadOrderToolTwo.Utilities.Managers;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.ConstrainedExecution;
using System.Text;
using System.Threading.Tasks;

namespace LoadOrderToolTwo.Utilities;
internal class ColossalOrderUtil
{
	private const string GAME_SETTINGS_FILE_NAME = "userGameState";
	private static SettingsFile _settingsFile;
	private static readonly Dictionary<Mod, SavedBool> _settingsDictionary = new();
	private static readonly FileSystemWatcher _watcher;
	private static readonly DelayedAction _delayedAction = new(500);

	static ColossalOrderUtil()
	{
		_settingsFile = new SettingsFile() { fileName = GAME_SETTINGS_FILE_NAME };
		_settingsFile.Load();

		_watcher = CreateWatcher();
	}

	private static FileSystemWatcher CreateWatcher()
	{
		var watcher = new FileSystemWatcher
		{
			Path = LocationManager.AppDataPath,
			NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName,
			Filter = GAME_SETTINGS_FILE_NAME + SettingsFile.extension
		};

		watcher.Changed += new FileSystemEventHandler(FileChanged);
		watcher.Created += new FileSystemEventHandler(FileChanged);
		watcher.Deleted += new FileSystemEventHandler(FileChanged);

		watcher.EnableRaisingEvents = true;

		return watcher;
	}

	private static void FileChanged(object sender, FileSystemEventArgs e)
	{
		_delayedAction.Run(SettingsFileChanged);
	}

	private static void SettingsFileChanged()
	{
		if (CentralManager.SessionSettings.OverrideGameChanges)
		{
			SaveSettings();
		}
		else
		{
			_settingsFile = new SettingsFile() { fileName = GAME_SETTINGS_FILE_NAME };
			_settingsFile.Load();
			_settingsDictionary.Clear();

			foreach (var mod in CentralManager.Mods)
			{
				CentralManager.InformationUpdate(mod);
			}
		}
	}

	public static bool IsEnabled(Mod mod)
	{
		if (!_settingsDictionary.ContainsKey(mod))
		{
			_settingsDictionary[mod] = GetEnabledSetting(mod);
		}

		return _settingsDictionary[mod].value;
	}

	public static void SetEnabled(Mod mod, bool value)
	{
		if (!_settingsDictionary.ContainsKey(mod))
		{
			_settingsDictionary[mod] = GetEnabledSetting(mod);
		}

		_settingsDictionary[mod].value = value;
	}

	public static void SaveSettings()
	{
		_watcher.EnableRaisingEvents = false;
		_settingsFile.Save();
		_watcher.EnableRaisingEvents = true;
	}

	private static SavedBool GetEnabledSetting(Mod mod)
	{
		var savedEnabledKey_ = $"{Path.GetFileNameWithoutExtension(mod.Folder)}{GetLegacyHashCode(mod.VirtualFolder ?? mod.Folder)}.enabled";

		return new SavedBool(savedEnabledKey_, GAME_SETTINGS_FILE_NAME, def: false, autoUpdate: false) { settingsFile = _settingsFile };
	}

	[ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
	public static unsafe int GetLegacyHashCode(string str)
	{
		fixed (char* ptr = str + (RuntimeHelpers.OffsetToStringData / 2))
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
