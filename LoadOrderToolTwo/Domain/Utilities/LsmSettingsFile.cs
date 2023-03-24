using LoadOrderToolTwo.Utilities;
using LoadOrderToolTwo.Utilities.Managers;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace LoadOrderToolTwo.Domain.Utilities;

[XmlRoot("LoadingScreenModRevisited")]
public class LsmSettingsFile
{
	const string FILE_NAME = "LoadingScreenModRevisited.xml";
	static string FILE_PATH => Path.Combine(LocationManager.AppDataPath, FILE_NAME);

	public static string DefaultSkipPath => Path.Combine(Path.Combine(LocationManager.AppDataPath, "Maps"), "SkippedPrefabs");
	public static string DefaultSkipFile => Path.Combine(DefaultSkipPath, "skip.txt");

	public bool loadEnabled = true;
	public bool loadUsed = true;
	public bool skipPrefabs;
	public string? skipFile = string.Empty;

	[XmlAnyElement]
	public List<XElement> Elements { get; } = new List<XElement>();

	public string this[XName name]
	{
		get => Elements.Where(e => e.Name == name).Select(e => e.Value).FirstOrDefault();
		set
		{
			var element = Elements.Where(e => e.Name == name).FirstOrDefault();
			if (element == null)
			{
				Elements.Add(element = new XElement(name));
			}

			element.Value = value;
		}
	}

	public override string ToString()
	{
		var ret = $"loadEnabled={loadEnabled}, loadUsed={loadUsed}, skipPrefabs={skipPrefabs}, skipFile={skipFile}";
		foreach (var item in Elements)
		{
			ret += $", {item.Name}={item.Value}";
		}
		return ret;
	}

	public static LsmSettingsFile? Deserialize()
	{
		try
		{
			if (File.Exists(FILE_PATH))
			{
				return LoadOrderShared.SharedUtil.Deserialize<LsmSettingsFile>(FILE_PATH);
			}
		}
		catch (Exception ex)
		{
			ex.Log();
		}

		return null;
	}

	private void Serialize()
	{
		try
		{
			LoadOrderShared.SharedUtil.Serialize(this, FILE_PATH);
		}
		catch (Exception ex)
		{
			ex.Log();
		}
	}

	public void SyncAndSerialize()
	{
		try
		{
			var ret = Deserialize();

			if (ret != null)
			{
				ret.skipFile = this.skipFile;
				ret.skipPrefabs = this.skipPrefabs;
				ret.loadEnabled = this.loadEnabled;
				ret.loadUsed = this.loadUsed;

				ret.Serialize();
			}
		}
		catch (Exception ex)
		{
			ex.Log();
		}
	}
}