using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;

namespace SkyveShared;

public class DlcConfig
{
	public const string FILE_NAME = "DlcConfig.xml";
	public static string FilePath => Path.Combine(SharedUtil.LocalLOMData, FILE_NAME);

	public List<uint> AvailableDLCs { get; set; } = new() { 2148900, 2224691, 2224690, 2148902, 2225940, 2225941, 2148901, 2148903, 2148904, 2144480, 2144481, 2144482, 2144483, 2008400, 1992290, 1992291, 1992293, 1992292, 1726380, 1726382, 1726381, 1726384, 1726383, 1531471, 1531470, 1531473, 1531472, 1146930, 1148022, 1196100, 1148020, 1148021, 944071, 1059820, 1065491, 1065490, 715194, 944070, 715191, 614580, 547502, 515191, 420610, 369150, 715190, 547500, 515190, 815380, 715193, 614581, 614582, 547501, 346791, 715192, 456200, 563850 };
	public List<uint> RemovedDLCs { get; set; } = new();

	public void Serialize()
	{
		lock (this)
		{
			SharedUtil.Serialize(this, FilePath);
		}
	}

	public static DlcConfig Deserialize()
	{
		try
		{
			return SharedUtil.Deserialize<DlcConfig>(FilePath) ?? new DlcConfig();
		}
		catch { }

		return new DlcConfig();
	}
}

public class ModConfig
{
	public const string FILE_NAME = "ModConfig.xml";
	public static string FilePath => Path.Combine(SharedUtil.LocalLOMData, FILE_NAME);

	public List<ModInfo> ModsInfo { get; set; } = new();

	public void Serialize()
	{
		lock (this)
		{
			SharedUtil.Serialize(this, FilePath);
		}
	}

	public static ModConfig Deserialize()
	{
		try
		{
			return SharedUtil.Deserialize<ModConfig>(FilePath) ?? new ModConfig();
		}
		catch { }

		return new ModConfig();
	}

	public class ModInfo
	{
		public string? Path { get; set; }
		public bool Excluded { get; set; }
		public int LoadOrder { get; set; }
	}
}

public class AssetConfig
{
	public const string FILE_NAME = "AssetConfig.xml";
	public static string FilePath => Path.Combine(SharedUtil.LocalLOMData, FILE_NAME);

	public List<string> ExcludedAssets { get; set; } = new();

	public void Serialize()
	{
		lock (this)
		{
			SharedUtil.Serialize(this, FilePath);
		}
	}

	public static AssetConfig Deserialize()
	{
		try
		{
			return SharedUtil.Deserialize<AssetConfig>(FilePath) ?? new AssetConfig();
		}
		catch { }

		return new AssetConfig();
	}
}

[XmlRoot("SkyveConfig")]
public class SkyveConfigOld
{
	public const int DefaultLoadOrder = 1000;
	public const string FILE_NAME = "SkyveConfig.xml";

	public string? WorkShopContentPath;
	public string? GamePath;
	public string? SteamPath;

	public bool HidePanels;
	public bool FastContentManager = true;
	//public bool SoftDLLDependancy = false;
	//public bool DeleteUnsubscribedItemsOnLoad = false;
	public bool AddHarmonyResolver = true;
	public bool LogAssetLoadingTimes = true;
	public bool LogPerModAssetLoadingTimes = false;
	public bool LogPerModOnCreatedTimes = false;
	public bool IgnoranceIsBliss = false; // turn off steam warnings.
	public bool UGCCache = true;

	public float StatusX = 1000;
	public float StatusY = 10;

	//public ModInfo[] Mods = new ModInfo[0];
	public AssetInfo[] Assets { get; set; } = new AssetInfo[0];
	public uint[] AvailableDLCs = new uint[] { 2148900, 2224691, 2224690, 2148902, 2225940, 2225941, 2148901, 2148903, 2148904, 2144480, 2144481, 2144482, 2144483, 2008400, 1992290, 1992291, 1992293, 1992292, 1726380, 1726382, 1726381, 1726384, 1726383, 1531471, 1531470, 1531473, 1531472, 1146930, 1148022, 1196100, 1148020, 1148021, 944071, 1059820, 1065491, 1065490, 715194, 944070, 715191, 614580, 547502, 515191, 420610, 369150, 715190, 547500, 515190, 815380, 715193, 614581, 614582, 547501, 346791, 715192, 456200, 563850 };
	public uint[] RemovedDLCs = new uint[0];
	public class AssetInfo
	{
		public string? Path { get; set; }
	}

	public static string FilePath => Path.Combine(SharedUtil.LocalLOMData, FILE_NAME);

	public void Serialize()
	{
		SharedUtil.Serialize(this, FilePath);
	}

	public static SkyveConfigOld? Deserialize()
	{
		try
		{
			return SharedUtil.Deserialize<SkyveConfigOld>(FilePath);
		}
		catch { }

		return null;
	}
}