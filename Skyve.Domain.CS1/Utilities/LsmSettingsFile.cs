using Extensions;

using Skyve.Domain.Systems;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace Skyve.Domain.CS1.Utilities;

[XmlRoot("LoadingScreenModRevisited")]
public class LsmSettingsFile
{
	private const string FILE_NAME = "LoadingScreenModRevisited.xml";

	private static string FILE_PATH => CrossIO.Combine(ServiceCenter.Get<ILocationManager>().AppDataPath, FILE_NAME);

	public static string DefaultSkipPath => CrossIO.Combine(CrossIO.Combine(ServiceCenter.Get<ILocationManager>().AppDataPath, "Maps"), "SkippedPrefabs");
	public static string DefaultSkipFile => CrossIO.Combine(DefaultSkipPath, "skip.txt");

	public bool loadEnabled = true;
	public bool loadUsed = true;
	public bool skipPrefabs;
	public string? skipFile = string.Empty;
	public string? reportDir = string.Empty;

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
			if (CrossIO.FileExists(FILE_PATH))
			{
				return SkyveShared.SharedUtil.Deserialize<LsmSettingsFile>(FILE_PATH);
			}
		}
		catch (Exception ex)
		{
			ServiceCenter.Get<ILogger>().Exception(ex, "Failed to deserialize LSM file");
		}

		return null;
	}

	private void Serialize()
	{
		try
		{
			SkyveShared.SharedUtil.Serialize(this, FILE_PATH);
		}
		catch (Exception ex)
		{
			ServiceCenter.Get<ILogger>().Exception(ex, "Failed to serialize LSM file");
		}
	}

	public void SyncAndSerialize()
	{
		try
		{
			var ret = Deserialize();

			if (ret != null)
			{
				ret.skipFile = skipFile;
				ret.skipPrefabs = skipPrefabs;
				ret.loadEnabled = loadEnabled;
				ret.loadUsed = loadUsed;

				ret.Serialize();
			}
		}
		catch (Exception ex)
		{
			ServiceCenter.Get<ILogger>().Exception(ex, "Failed to sync and serialize LSM file");
		}
	}
}