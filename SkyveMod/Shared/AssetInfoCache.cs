using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SkyveShared;
public class AssetInfoCache
{
	public const string FILE_NAME = "AssetInfoCache.xml";
	public static string FilePath => Path.Combine(SharedUtil.LocalLOMData, FILE_NAME);

	public List<Asset> Assets { get; set; } = new();
	public uint[] AvailableDLCs { get; set; } = new uint[0];

	public void AddAsset(Asset item)
	{
		Assets.Add(item);
	}

	public Asset GetAsset(string path)
	{
		return Assets.FirstOrDefault(x => x.Path == path);
	}

	public void Serialize()
	{
		SharedUtil.Serialize(this, FilePath);
	}

	public static AssetInfoCache Deserialize()
	{
		try
		{
			return SharedUtil.Deserialize<AssetInfoCache>(FilePath) ?? new();
		}
		catch { }

		return new();
	}

#nullable disable
	public class Asset
	{
		public string Path { get; set; }
		public string FullName { get; set; }
		public string Name { get; set; }
		public string Description { get; set; }
		public string[] Tags { get; set; }
		public byte[] Thumbnail { get; set; }
	}
#nullable enable
}
