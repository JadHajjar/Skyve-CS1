using System.IO;
using System.Xml;
using System.Xml.Serialization;

#if SkyveApp
using Skyve.Domain;
using Skyve.Domain.Systems;
#endif

namespace SkyveShared;
internal class SharedUtil
{
#if SkyveApp
	internal static string LocalApplicationData => ServiceCenter.Get<ILocationManager>().AppDataPath;
	internal static string LocalLOMData => Path.Combine(LocalApplicationData, "Skyve");
#elif TOOL
	internal static string LocalApplicationData => CO.IO.DataLocation.localApplicationData;
	internal static string LocalLOMData => CO.IO.DataLocation.LocalLOMData;
#elif IPATCH
        internal static string LocalApplicationData => SkyveIPatch.Patches.Entry.GamePaths.AppDataPath;
        internal static string LocalLOMData => SkyveIPatch.Patches.Entry.LocalLOMData;
#else
	internal static string LocalApplicationData => ColossalFramework.IO.DataLocation.localApplicationData;
	internal static string LocalLOMData => Path.Combine(LocalApplicationData, "Skyve");
#endif
	internal static XmlWriterSettings Indented => new() { Indent = true };

	internal static XmlSerializerNamespaces NoNamespaces =>
		new(new[] { XmlQualifiedName.Empty });

	internal static void Serialize<T>(T obj, string filePath)
	{
		var serializer = new XmlSerializer(typeof(T));
		using var writer = new StreamWriter(filePath);
		var dirInfo = new FileInfo(filePath).Directory;
		if (!dirInfo.Exists)
		{
			dirInfo.Create();
		}

		using var xmlWriter = new XmlTextWriter(writer);
		xmlWriter.Formatting = Formatting.Indented;
		serializer.Serialize(xmlWriter, obj, NoNamespaces);
	}

	public static T? Deserialize<T>(string filePath) where T : class
	{
		var ser = new XmlSerializer(typeof(T));
		var dirInfo = new FileInfo(filePath).Directory;
		if (!dirInfo.Exists)
		{
			dirInfo.Create();
		}

		using var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read);
		return ser.Deserialize(fs) as T;
	}
}
