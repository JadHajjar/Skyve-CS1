using Extensions;

using LoadOrderToolTwo.Utilities;
using LoadOrderToolTwo.Utilities.Managers;

using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;

namespace LoadOrderToolTwo.Domain.Utilities;

public class CustomTagsLibrary
{
	public const string filename = "FindItCustomTags.xml";

	public Dictionary<string, string> assetTags = new Dictionary<string, string>();

	public struct TagEntry
	{
		[XmlAttribute]
		public string key;
		[XmlAttribute]
		public string value;
	}

	public void AddAndSave()
	{


		Serialize();
	}

	public void Serialize()
	{
		try
		{
			var path = LocationManager.Combine(LocationManager.AppDataPath, filename);

			if (assetTags.Count == 0)
			{
				if (LocationManager.FileExists(path))
				{
					ExtensionClass.DeleteFile(path);
				}
				return;
			}

			var tagsEntries = new TagEntry[assetTags.Count];

			var i = 0;
			foreach (var key in assetTags.Keys)
			{
				tagsEntries[i].key = key;
				tagsEntries[i].value = assetTags[key];
				i++;
			}

			using var stream = new FileStream(path, FileMode.OpenOrCreate);
			stream.SetLength(0); // Emptying the file !!!
			var xmlSerializer = new XmlSerializer(typeof(TagEntry[]));
			xmlSerializer.Serialize(stream, tagsEntries);
		}
		catch (Exception e)
		{
			Log.Exception(e, "Couldn't serialize custom find-it tags");
		}
	}

	public void Deserialize()
	{
		try
		{
			assetTags.Clear();

			var path = LocationManager.Combine(LocationManager.AppDataPath, filename);

			if (!LocationManager.FileExists(path))
			{
				return;
			}

			TagEntry[] tagsEntries;

			var xmlSerializer = new XmlSerializer(typeof(TagEntry[]));
			using (var stream = new FileStream(path, FileMode.Open))
			{
				tagsEntries = (TagEntry[])xmlSerializer.Deserialize(stream);
			}

			for (var i = 0; i < tagsEntries.Length; i++)
			{
				assetTags[tagsEntries[i].key] = tagsEntries[i].value;
			}
		}
		catch (Exception e)
		{
			Log.Exception(e, "Couldn't load custom find-it tags");
		}
	}
}