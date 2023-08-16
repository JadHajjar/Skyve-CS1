using Extensions;

using Skyve.Domain.Systems;

using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Serialization;

namespace Skyve.Domain.CS1.Legacy;
public interface IProfileItem
{
	string? GetIncludedPath();
	string? GetDisplayText();
	string GetCategoryName(); // mod or asset

}

public class LoadOrderProfile
{
	private const string LOCAL_APP_DATA_PATH = "%LOCALAPPDATA%";
	private const string CITIES_PATH = "%CITIES%";
	private const string WS_CONTENT_PATH = "%WORKSHOP%";

	private static string? FromFinalPath(string? path)
	{
		var locationManager = ServiceCenter.Get<ILocationManager>();

		return path
			?.Replace(locationManager.AppDataPath, LOCAL_APP_DATA_PATH)
			?.Replace(locationManager.GamePath, CITIES_PATH)
			?.Replace(locationManager.WorkshopContentPath, WS_CONTENT_PATH);
	}

	private static string? ToFinalPath(string? path)
	{
		var locationManager = ServiceCenter.Get<ILocationManager>();

		return path
			?.Replace(LOCAL_APP_DATA_PATH, locationManager.AppDataPath)
			?.Replace(CITIES_PATH, locationManager.GamePath)
			?.Replace(WS_CONTENT_PATH, locationManager.WorkshopContentPath);
	}

	public class Mod : IProfileItem
	{

		[XmlIgnore]
		public string? IncludedPathFinal;
		public string? IncludedPath
		{
			get => FromFinalPath(IncludedPathFinal);
			set => IncludedPathFinal = ToFinalPath(value);
		}

		public bool IsEnabled;
		public bool IsIncluded;
		public int LoadOrder;
		public string? DisplayText;

		public Mod() { }

		public string? GetIncludedPath()
		{
			return IncludedPathFinal;
		}

		public string? GetDisplayText()
		{
			return DisplayText;
		}

		public string GetCategoryName()
		{
			return "Mod";
		}
	}

	public class Asset : IProfileItem
	{
		[XmlIgnore]
		public string? IncludedPathFinal;

		/// <summary>
		/// only for storage. use the final path instead
		/// </summary>
		public string? IncludedPath
		{
			get => FromFinalPath(IncludedPathFinal);
			set => IncludedPathFinal = ToFinalPath(value);
		}

		public bool IsIncluded;
		public string? DisplayText;

		public Asset() { }

		public string? GetIncludedPath()
		{
			return IncludedPathFinal;
		}

		public string? GetDisplayText()
		{
			return DisplayText;
		}

		public string GetCategoryName()
		{
			return "Asset";
		}
	}

	public Mod[] Mods = new Mod[0];
	public Asset[] Assets = new Asset[0];
	public DLC[] ExcludedDLCs = new DLC[0];

	[XmlIgnore]
	public string? SkipFilePathFinal;
	public string? SkipFilePath
	{
		get => FromFinalPath(SkipFilePathFinal);
		set => SkipFilePathFinal = ToFinalPath(value);
	}

	public bool LoadEnabled = true;
	public bool LoadUsed = true;

	public Mod GetMod(string includedPath)
	{
		return Mods.FirstOrDefault(m => m.IncludedPathFinal == includedPath);
	}

	public Asset GetAsset(string includedPath)
	{
		return Assets.FirstOrDefault(m => m.IncludedPathFinal == includedPath);
	}

	public static LoadOrderProfile? Deserialize(string path)
	{
		try
		{
			return SkyveShared.SharedUtil.Deserialize<LoadOrderProfile>(path);
		}
		catch
		{
			return null;
		}
	}

	public Playset ToSkyvePlayset(string name)
	{
		var profile = new Playset(name)
		{
			AutoSave = false
		};

		foreach (var asset in Assets)
		{
			if (asset.IsIncluded)
			{
				var rgx = Regex.Match(asset.IncludedPath, Regex.Escape(WS_CONTENT_PATH) + "[\\\\/](\\d{8,20})[\\\\/]?");
				profile.Assets.Add(new Playset.Asset
				{
					SteamId = rgx.Success ? ulong.Parse(rgx.Groups[1].Value) : 0,
					RelativePath = asset.IncludedPath
				});
			}
		}

		foreach (var mod in Mods)
		{
			if (mod.IsIncluded)
			{
				var rgx = Regex.Match(mod.IncludedPath, Regex.Escape(WS_CONTENT_PATH) + "[\\\\/](\\d{8,20})[\\\\/]?");
				profile.Mods.Add(new Playset.Mod
				{
					SteamId = rgx.Success ? ulong.Parse(rgx.Groups[1].Value) : 0,
					RelativePath = mod.IncludedPath,
					Enabled = mod.IsEnabled
				});
			}
		}

		profile.ExcludedDLCs = ExcludedDLCs.Select(x => (uint)x).ToList();

		profile.LsmSettings.LoadEnabled = LoadEnabled;
		profile.LsmSettings.LoadUsed = LoadUsed;
		profile.LsmSettings.SkipFile = SkipFilePathFinal;
		profile.LsmSettings.UseSkipFile = CrossIO.FileExists(SkipFilePathFinal);

		return profile;
	}
}
