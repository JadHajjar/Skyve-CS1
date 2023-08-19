using Extensions;

using Skyve.Domain.Systems;

using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;

namespace Skyve.Domain.CS1.Utilities;

public class CustomTagsLibrary
{
	public const string filename = "FindItCustomTags.xml";

	public Dictionary<string, string> assetTags = new();

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
			var path = CrossIO.Combine(ServiceCenter.Get<ILocationManager>().AppDataPath, filename);

			if (assetTags.Count == 0)
			{
				if (CrossIO.FileExists(path))
				{
					CrossIO.DeleteFile(path);
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
			ServiceCenter.Get<ILogger>().Exception(e, "Couldn't serialize custom find-it tags");
		}
	}

	public static CustomTagsLibrary Deserialize()
	{
		try
		{
			var instance = new CustomTagsLibrary();
			instance.assetTags.Clear();

			var path = CrossIO.Combine(ServiceCenter.Get<ILocationManager>().AppDataPath, filename);

			if (!CrossIO.FileExists(path))
			{
				return new();
			}

			TagEntry[] tagsEntries;

			var xmlSerializer = new XmlSerializer(typeof(TagEntry[]));
			using (var stream = new FileStream(path, FileMode.Open))
			{
				tagsEntries = (TagEntry[])xmlSerializer.Deserialize(stream);
			}

			for (var i = 0; i < tagsEntries.Length; i++)
			{
				instance.assetTags[tagsEntries[i].key] = tagsEntries[i].value;
			}

			return instance;
		}
		catch (Exception e)
		{
			ServiceCenter.Get<ILogger>().Exception(e, "Couldn't load custom find-it tags");
		}

		return new();
	}
}