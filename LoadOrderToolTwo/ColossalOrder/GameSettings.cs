using LoadOrderToolTwo.Utilities;

using System;
using System.Collections.Generic;
using System.Threading;


namespace LoadOrderToolTwo.ColossalOrder;

public class GameSettings : SingletonLite<GameSettings>
{
	public static readonly string extension = ".cgs";

	private readonly Dictionary<string, SettingsFile> m_SettingsFiles = new Dictionary<string, SettingsFile>();

	private static bool m_Run;

	private static Thread? m_SaveThread;

	private static readonly object m_LockObject = new object();

	public static void AddSettingsFile(params SettingsFile[] settingsFiles)
	{
		instance.InternalAddSettingsFile(settingsFiles);
	}

	private void InternalAddSettingsFile(params SettingsFile[] settingsFiles)
	{
		lock (m_SettingsFiles)
		{
			for (var i = 0; i < settingsFiles.Length; i++)
			{
				try
				{
					settingsFiles[i].Load();
					m_SettingsFiles.Add(settingsFiles[i].fileName, settingsFiles[i]);
					Log.Debug("Settings file added.");
				}
				catch (Exception ex)
				{
					new Exception($"could not load {settingsFiles[i]} (maybe try launching CS?)", ex).Log();
				}
			}
		}
	}

	public static SettingsFile FindSettingsFileByName(string name)
	{
		return instance.InternalFindSettingsFileByName(name);
	}

	internal SettingsFile InternalFindSettingsFileByName(string name)
	{
		if (!m_SettingsFiles.TryGetValue(name, out var result))
		{
			// auto add missing setting files.
			result = new SettingsFile() { fileName = name };
			InternalAddSettingsFile(result);
		}
		return result;
	}

	public static void SaveAll()
	{
		instance.InternalSaveAll();
	}

	public void InternalSaveAll()
	{
		lock (m_SettingsFiles)
		{
			foreach (var settingsFile in m_SettingsFiles.Values)
			{
				if (settingsFile.isDirty)
				{
					settingsFile.Save();
				}
			}
		}
	}

	public static void ClearAll()
	{
		ClearAll(false);
	}

	public static void ClearAll(bool systemToo)
	{
		instance.InternalClearAll(systemToo);
	}

	private void InternalClearAll(bool systemToo)
	{
		foreach (var settingsFile in m_SettingsFiles.Values)
		{
			if (!settingsFile.isSystem || systemToo)
			{
				settingsFile.Delete();
			}
		}
	}

	private static void MonitorSave()
	{
		try
		{
			Log.Info("GameSettings Monitor Started...", false);
			while (m_Run)
			{
				SaveAll();
				lock (m_LockObject)
				{
					Monitor.Wait(m_LockObject, 100);
				}
			}
			SaveAll();
			Log.Info("GameSettings Monitor Exiting...", false);
		}
		catch (Exception ex)
		{
			Log.Exception(ex);
		}
	}

	public override void Awake()
	{
		base.Awake();
		try
		{
			sInstance = this;
			Log.Info("Creating GameSettings Monitor ...", false);
			m_SaveThread = new Thread(new ThreadStart(MonitorSave))
			{
				Name = "SaveSettingsThread",
				IsBackground = true
			};
			m_Run = true;
			m_SaveThread.Start();
		}
		catch (Exception ex) { ex.Log(); }
	}

	~GameSettings() => Terminate();

	public void Terminate()
	{
		m_Run = false;
		lock (m_LockObject)
		{
			Monitor.Pulse(m_LockObject);
		}

		Log.Info("GameSettings terminated", false);
	}
}
