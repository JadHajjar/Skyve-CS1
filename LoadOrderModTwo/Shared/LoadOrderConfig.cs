using System.IO;

namespace LoadOrderShared;
public class ItemInfo
{
	public string? Path; // included path
}

public class ModInfo : ItemInfo
{
	//public int LoadOrder = 1000;
}

public class AssetInfo : ItemInfo
{
	//public bool Excluded;
}

public class LoadOrderConfig
{
	public const int DefaultLoadOrder = 1000;
	public const string FILE_NAME = "LoadOrderConfig.xml";

	public string? WorkShopContentPath;
	public string? GamePath;
	public string? SteamPath;

	public bool TurnOffSteamPanels = true;
	public bool FastContentManager = true;
	//public bool SoftDLLDependancy = false;
	public bool DeleteUnsubscribedItemsOnLoad = false;
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

	public static string FilePath => Path.Combine(SharedUtil.LocalLOMData, FILE_NAME);

	public void Serialize()
	{
		SharedUtil.Serialize(this, FilePath);
	}

	public static LoadOrderConfig? Deserialize()
	{
		try
		{
			return SharedUtil.Deserialize<LoadOrderConfig>(FilePath);
		}
		catch { }
		return null;
	}
}
