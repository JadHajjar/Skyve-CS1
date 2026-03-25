using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Xml.Serialization;

#if SkyveApp
using Extensions;
#endif

namespace SkyveShared;

public class DlcConfig
{
	public const string FILE_NAME = "DlcConfig.xml";
	public static string FilePath => Path.Combine(SharedUtil.LocalLOMData, FILE_NAME);

	public List<uint> RemovedDLCs { get; set; } = new();
	public List<uint> AvailableDLCs { get; set; } = new();
	public List<uint> OwnedDLCs { get; set; } = new();

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

	[EditorBrowsable(EditorBrowsableState.Never), Browsable(false)]
	public List<SavedModInfo> SavedModsInfo { get; set; } = new();
	public Dictionary<string, ModInfo> GetModsInfo()
	{
		var dictionary = new Dictionary<string, ModInfo>(new PathEqualityComparer());

		foreach (var item in SavedModsInfo)
		{
			dictionary[item.Path ?? string.Empty] = item;
		}

		return dictionary;
	}
#if SkyveApp
	public void SetModsInfo(Dictionary<string, ModInfo> value)
	{
		var list = new List<SavedModInfo>();

		foreach (var item in value)
		{
			list.Add(new()
			{
				Path = item.Key,
				Excluded = item.Value.Excluded,
				LoadOrder = item.Value.LoadOrder,
			});
		}

		SavedModsInfo = list;
	}
#endif

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

	public class SavedModInfo : ModInfo
	{
		public string? Path { get; set; }
	}

	public class ModInfo
	{
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

public class SkyveConfig
{
	public const string FILE_NAME = "SkyveConfig.xml";

	public bool HidePanels { get; set; }
	public bool FastContentManager { get; set; } = true;
	public bool LogAssetLoadingTimes { get; set; } = true;
	public bool LogPerModAssetLoadingTimes { get; set; }
	public bool LogPerModOnCreatedTimes { get; set; }
	public bool IgnoranceIsBliss { get; set; }
	public bool UGCCache { get; set; } = true;

	public float StatusX { get; set; } = 1000;
	public float StatusY { get; set; } = 10;

	public static string FilePath => Path.Combine(SharedUtil.LocalLOMData, FILE_NAME);

	public void Serialize()
	{
		SharedUtil.Serialize(this, FilePath);
	}

	public static SkyveConfig? Deserialize()
	{
		try
		{
			return SharedUtil.Deserialize<SkyveConfig>(FilePath);
		}
		catch { }

		return null;
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

	//public ModInfo[] Mods = new ModInfo[0];
	public AssetInfo[] Assets { get; set; } = new AssetInfo[0];
	
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