using LoadOrderToolTwo.Utilities;
using LoadOrderToolTwo.Utilities.Managers;

using System;
using System.Collections.Generic;
using System.IO;

namespace LoadOrderToolTwo.ColossalOrder;

public class SettingsFile
{
	public static readonly string extension = ".cgs";

	private readonly char[] settingsIdentifier = new char[]
	{
					'C',
					'G',
					'S',
					'F'
	};

	private readonly ushort settingsVersion = 3;
	private readonly Dictionary<string, int> m_SettingsIntValues = new Dictionary<string, int>();

	private readonly Dictionary<string, bool> m_SettingsBoolValues = new Dictionary<string, bool>();

	private readonly Dictionary<string, float> m_SettingsFloatValues = new Dictionary<string, float>();

	private readonly Dictionary<string, string> m_SettingsStringValues = new Dictionary<string, string>();

	private readonly Dictionary<string, InputKey> m_SettingsInputKeyValues = new Dictionary<string, InputKey>();

	private string? m_PathName;
	private bool m_DontSave;
	private readonly object m_Saving = new object();

	public ushort version { get; private set; }

	public bool isDirty { get; private set; }

	public bool isSystem { get; private set; }

	public bool dontSave
	{
		get => m_DontSave;
		set => m_DontSave = true;
	}

	public string fileName
	{
		get => Path.GetFileNameWithoutExtension(m_PathName);
		set => m_PathName = Path.Combine(LocationManager.AppDataPath, Path.ChangeExtension(value, extension));
	}

	public string systemFileName
	{
		get => Path.GetFileNameWithoutExtension(m_PathName);
		set
		{
			m_PathName = Path.Combine(LocationManager.GamePath, Path.ChangeExtension(value, extension));
			isSystem = true;
		}
	}

	public string? pathName
	{
		get => m_PathName;
		set => m_PathName = Path.ChangeExtension(value, extension);
	}

	//public string cloudName
	//{
	//	get
	//	{
	//		return this.m_PathName;
	//	}
	//	set
	//	{
	//		this.m_PathName = PathUtils.AddExtension(value, GameSettings.extension);
	//		this.m_UseCloud = true;
	//	}
	//}

	internal Stream? CreateReadStream()
	{
		if (pathName is null)
		{
			return null;
		}

		return new FileStream(pathName, FileMode.Open, FileAccess.Read);
	}

	internal Stream? CreateWriteStream()
	{
		if (pathName is null)
		{
			return null;
		}

		return new SafeFileStream(pathName, FileMode.Create);
	}

	public void DeleteEntry(string key)
	{
		m_SettingsInputKeyValues.Remove(key);
		m_SettingsIntValues.Remove(key);
		lock(m_SettingsBoolValues)
			m_SettingsBoolValues.Remove(key);
		m_SettingsFloatValues.Remove(key);
		m_SettingsStringValues.Remove(key);
		MarkDirty();
	}

	public string[] ListKeys()
	{
		var keys = m_SettingsInputKeyValues.Keys;
		var keys2 = m_SettingsIntValues.Keys;
		var keys3 = m_SettingsBoolValues.Keys;
		var keys4 = m_SettingsFloatValues.Keys;
		var keys5 = m_SettingsStringValues.Keys;
		var array = new string[keys.Count + keys2.Count + keys3.Count + keys4.Count + keys5.Count];
		var num = 0;
		keys.CopyTo(array, num);
		num += keys.Count;
		keys2.CopyTo(array, num);
		num += keys2.Count;
		keys3.CopyTo(array, num);
		num += keys3.Count;
		keys4.CopyTo(array, num);
		num += keys4.Count;
		keys5.CopyTo(array, num);
		num += keys5.Count;
		return array;
	}

	public bool IsValid()
	{
		if (string.IsNullOrEmpty(pathName))
		{
			return false;
		}
		//if (!this.m_UseCloud)
		{
			return File.Exists(pathName);
		}
		//return PlatformService.cloud.Exists(this.pathName);
	}

	public void Delete()
	{
		if (IsValid())
		{
			//if (this.m_UseCloud)
			//{
			//	PlatformService.cloud.Delete(this.pathName);
			//	return;
			//}
			File.Delete(pathName);
		}
	}

	private void Serialize(Stream stream)
	{
		try
		{
			Dictionary<string, int> dictionary;
			lock (m_SettingsIntValues)
			{
				dictionary = new Dictionary<string, int>(m_SettingsIntValues);
			}
			Dictionary<string, bool> dictionary2;
			lock (m_SettingsBoolValues)
			{
				dictionary2 = new Dictionary<string, bool>(m_SettingsBoolValues);
			}
			Dictionary<string, float> dictionary3;
			lock (m_SettingsFloatValues)
			{
				dictionary3 = new Dictionary<string, float>(m_SettingsFloatValues);
			}
			Dictionary<string, string> dictionary4;
			lock (m_SettingsStringValues)
			{
				dictionary4 = new Dictionary<string, string>(m_SettingsStringValues);
			}
			Dictionary<string, InputKey> dictionary5;
			lock (m_SettingsInputKeyValues)
			{
				dictionary5 = new Dictionary<string, InputKey>(m_SettingsInputKeyValues);
			}
			using var binaryWriter = new BinaryWriter(stream);
			binaryWriter.Write(settingsIdentifier);
			binaryWriter.Write(settingsVersion);
			binaryWriter.Write(dictionary.Count);
			foreach (var keyValuePair in dictionary)
			{
				binaryWriter.Write(keyValuePair.Key);
				binaryWriter.Write(keyValuePair.Value);
			}
			binaryWriter.Write(dictionary2.Count);
			foreach (var keyValuePair2 in dictionary2)
			{
				binaryWriter.Write(keyValuePair2.Key);
				binaryWriter.Write(keyValuePair2.Value);
			}
			binaryWriter.Write(dictionary3.Count);
			foreach (var keyValuePair3 in dictionary3)
			{
				binaryWriter.Write(keyValuePair3.Key);
				binaryWriter.Write(keyValuePair3.Value);
			}
			binaryWriter.Write(dictionary4.Count);
			foreach (var keyValuePair4 in dictionary4)
			{
				binaryWriter.Write(keyValuePair4.Key);
				binaryWriter.Write(keyValuePair4.Value);
			}
			binaryWriter.Write(dictionary5.Count);
			foreach (var keyValuePair5 in dictionary5)
			{
				binaryWriter.Write(keyValuePair5.Key);
				binaryWriter.Write(keyValuePair5.Value);
			}
			binaryWriter.Flush();
		}
		catch (Exception ex) { ex.Log(); }
	}

	private bool ValidateID(char[] id)
	{
		if (id.Length != 4)
		{
			return false;
		}

		for (var i = 0; i < 4; i++)
		{
			if (id[i] != settingsIdentifier[i])
			{
				return false;
			}
		}

		return true;
	}

	private void Deserialize(Stream stream)
	{
		try
		{
			using var binaryReader = new BinaryReader(stream);
			if (ValidateID(binaryReader.ReadChars(4)))
			{
				version = binaryReader.ReadUInt16();
				if (version < 2)
				{
					throw new Exception("Setting file '" + fileName + "' version is incompatible. The internal format of settings files has changed and your settings will be reset.");
				}
				lock (m_SettingsIntValues)
				{
					m_SettingsIntValues.Clear();
					var num = binaryReader.ReadInt32();
					for (var i = 0; i < num; i++)
					{
						var key = binaryReader.ReadString();
						var value = binaryReader.ReadInt32();
						m_SettingsIntValues.Add(key, value);
					}
				}
				lock (m_SettingsBoolValues)
				{
					m_SettingsBoolValues.Clear();
					var num2 = binaryReader.ReadInt32();
					for (var j = 0; j < num2; j++)
					{
						var key2 = binaryReader.ReadString();
						var value2 = binaryReader.ReadBoolean();
						m_SettingsBoolValues.Add(key2, value2);
					}
				}
				lock (m_SettingsFloatValues)
				{
					m_SettingsFloatValues.Clear();
					var num3 = binaryReader.ReadInt32();
					for (var k = 0; k < num3; k++)
					{
						var key3 = binaryReader.ReadString();
						var value3 = binaryReader.ReadSingle();
						m_SettingsFloatValues.Add(key3, value3);
					}
				}
				lock (m_SettingsStringValues)
				{
					m_SettingsStringValues.Clear();
					var num4 = binaryReader.ReadInt32();
					for (var l = 0; l < num4; l++)
					{
						var key4 = binaryReader.ReadString();
						var value4 = binaryReader.ReadString();
						m_SettingsStringValues.Add(key4, value4);
					}
				}
				lock (m_SettingsInputKeyValues)
				{
					m_SettingsInputKeyValues.Clear();
					var num5 = binaryReader.ReadInt32();
					for (var m = 0; m < num5; m++)
					{
						var key5 = binaryReader.ReadString();
						InputKey value5 = binaryReader.ReadInt32();
						m_SettingsInputKeyValues.Add(key5, value5);
					}
				}
				return;
			}
			throw new Exception("Setting file '" + fileName + "' header mismatch. The internal format of settings files has changed.");
		}
		catch (Exception ex) { ex.Log(); }
	}

	internal void Load()
	{
		try
		{
			if (IsValid())
			{
				Log.Info("Loading " + m_PathName, false);
				using var stream = CreateReadStream();

				if (stream != null)
				{
					Deserialize(stream);
					Log.Info(message: "Loaded " + m_PathName, false);
				}
				else
				{
					Log.Warning(message: "Failed to load " + m_PathName, false);
				}
			}
		}
		catch (Exception ex) { ex.Log(); }
	}

	internal void Save()
	{
		try
		{
			if (!dontSave && !string.IsNullOrEmpty(pathName))
			{
				Log.Info(message: "Saving " + m_PathName, false);
				lock (m_Saving)
				{
					using var stream = CreateWriteStream();
					if (stream != null)
					{
						Serialize(stream);
						Log.Info(message: "Saved " + m_PathName);
					}
					else
					{
						Log.Warning(message: "Failed to save " + m_PathName, false);
					}
				}
			}
		}
		catch (Exception ex) { ex.Log(); }
		finally
		{
			isDirty = false;
		}
	}

	public void MarkDirty()
	{
		isDirty = true;
	}

	internal bool GetValue(string name, out object? v)
	{
		if (m_SettingsInputKeyValues.TryGetValue(name, out var inputKey))
		{
			v = inputKey;
			return true;
		}
		if (m_SettingsIntValues.TryGetValue(name, out var num))
		{
			v = num;
			return true;
		}
		if (m_SettingsBoolValues.TryGetValue(name, out var flag))
		{
			v = flag;
			return true;
		}
		if (m_SettingsStringValues.TryGetValue(name, out var text))
		{
			v = text;
			return true;
		}
		if (m_SettingsFloatValues.TryGetValue(name, out var num2))
		{
			v = num2;
			return true;
		}
		v = null;
		return false;
	}

	internal bool GetValue(string name, ref string val)
	{
		if (m_SettingsStringValues.TryGetValue(name, out var text))
		{
			val = text;
			return true;
		}
		return false;
	}

	internal void SetValue(string name, string val)
	{
		if (!m_SettingsStringValues.TryGetValue(name, out var a) || a != val)
		{
			Log.Debug("Setting " + name + " updated to " + val);
			m_SettingsStringValues[name] = val;
			MarkDirty();
		}
	}

	internal bool GetValue(string name, ref bool val)
	{
		lock(m_SettingsBoolValues)
		if (m_SettingsBoolValues.TryGetValue(name, out var flag))
		{
			val = flag;
			return true;
		}
		return false;
	}

	internal void SetValue(string name, bool val)
	{
		lock(m_SettingsBoolValues)
		if (!m_SettingsBoolValues.TryGetValue(name, out var flag) || flag != val)
		{
			Log.Debug("Setting " + name + " updated to " + val);
			m_SettingsBoolValues[name] = val;
			MarkDirty();
		}
	}

	internal bool GetValue(string name, ref int val)
	{
		if (m_SettingsIntValues.TryGetValue(name, out var num))
		{
			val = num;
			return true;
		}
		return false;
	}

	internal void SetValue(string name, int val)
	{
		if (!m_SettingsIntValues.TryGetValue(name, out var num) || num != val)
		{
			Log.Debug("Setting " + name + " updated to " + val);
			m_SettingsIntValues[name] = val;
			MarkDirty();
		}
	}

	internal bool GetValue(string name, ref InputKey val)
	{
		if (m_SettingsInputKeyValues.TryGetValue(name, out var inputKey))
		{
			val = inputKey;
			return true;
		}
		return false;
	}

	internal void SetValue(string name, InputKey val)
	{
		if (!m_SettingsInputKeyValues.TryGetValue(name, out var value) || value != val)
		{
			Log.Debug("Setting " + name + " updated to " + val);
			m_SettingsInputKeyValues[name] = val;
			MarkDirty();
		}
	}

	internal bool GetValue(string name, ref float val)
	{
		if (m_SettingsFloatValues.TryGetValue(name, out var num))
		{
			val = num;
			return true;
		}
		return false;
	}

	internal void SetValue(string name, float val)
	{
		if (!m_SettingsFloatValues.TryGetValue(name, out _) ||
				Math.Abs(m_SettingsFloatValues[name] - val) > float.Epsilon)
		{
			Log.Debug("Setting " + name + " updated to " + val);
			m_SettingsFloatValues[name] = val;
			MarkDirty();
		}
	}
}
