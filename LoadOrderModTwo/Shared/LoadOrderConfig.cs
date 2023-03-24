using System;
using System.IO;
using System.Xml.Serialization;

namespace LoadOrderShared;
public class ItemInfo
{
	public string? Path; // included path
}

public class ModInfo : ItemInfo
{
	public int LoadOrder = 1000;
}

public class AssetInfo : ItemInfo
{
	public bool Excluded;
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

	public ModInfo[] Mods = new ModInfo[0];
	public AssetInfo[] Assets { get; set; } = new AssetInfo[0];
	public string[] ExcludedDLCs = new string[0];

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
