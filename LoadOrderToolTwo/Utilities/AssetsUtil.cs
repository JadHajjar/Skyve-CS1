using Extensions;

using LoadOrderShared;

using LoadOrderToolTwo.Domain;
using LoadOrderToolTwo.Utilities.Managers;

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;

namespace LoadOrderToolTwo.Utilities;
internal class AssetsUtil
{
	private static readonly LoadOrderConfig _config;

	public static HashSet<string> ExcludedHashSet { get; }
	public static Dictionary<string, CSCache.Asset> AssetInfoCache { get; }

	static AssetsUtil()
	{
		_config = LoadOrderConfig.Deserialize() ?? new();
		var cache = CSCache.Deserialize()?.Assets.ToDictionary(x => x.IncludedPath, x => x, StringComparer.InvariantCultureIgnoreCase);

		AssetInfoCache = cache ?? new();
		ExcludedHashSet = new HashSet<string>(_config.Assets.Where(x => x.Excluded).Select(x => x.Path?.ToLower() ?? string.Empty));
	}

	public static IEnumerable<Asset> GetAssets(Package package)
	{
		if (!Directory.Exists(package.Folder))
		{
			yield break;
		}

		var files = Directory.GetFiles(package.Folder, $"*.crp", SearchOption.AllDirectories);

		foreach (var file in files)
		{
			yield return new Asset(package, file);
		}
	}

	internal static bool IsIncluded(Asset asset)
	{
		return !ExcludedHashSet.Contains(asset.FileName.ToLower());
	}

	internal static void SetIncluded(Asset asset, bool value)
	{
		if (value)
		{
			ExcludedHashSet.Remove(asset.FileName.ToLower());
		}
		else
		{
			ExcludedHashSet.Add(asset.FileName.ToLower());
		}

		CentralManager.InformationUpdate(asset);
		ProfileManager.TriggerAutoSave();

		SaveChanges();
	}

	internal static void SetIncluded(IEnumerable<Asset> assets, bool value)
	{
		var list = assets.ToList();

		for (var i = 0; i < list.Count; i++)
		{
			if (value)
			{
				ExcludedHashSet.Remove(list[i].FileName.ToLower());
			}
			else
			{
				ExcludedHashSet.Add(list[i].FileName.ToLower());
			}

			CentralManager.InformationUpdate(list[i]);
		}

		if (!ProfileManager.ApplyingProfile && !CitiesManager.IsRunning())
		{
			SaveChanges();
		}

		ProfileManager.TriggerAutoSave();
	}

	public static void SaveChanges()
	{
		if (ProfileManager.ApplyingProfile)
		{
			return;
		}

		_config.Assets = ExcludedHashSet
				.Select(x => new AssetInfo { Excluded = true, Path = x })
				.ToArray();

		_config.Serialize();
	}

	internal static Asset GetAsset(string? v)
	{
		return CentralManager.Assets.FirstOrDefault(x => x.FileName.Equals(v, StringComparison.InvariantCultureIgnoreCase));
	}

	internal static Bitmap? GetIcon(Asset asset)
	{
		var fileName = Path.Combine(LocationManager.LotAppDataPath, "AssetPictures");

		if (asset.SteamId > 0)
		{
			fileName = Path.Combine(fileName, asset.SteamId.ToString());
		}

		fileName = Path.Combine(fileName, Path.GetFileNameWithoutExtension(asset.FileName).Trim().Replace(' ', '_') + ".png");

		if (File.Exists(fileName))
		{
			return (Bitmap)Image.FromFile(fileName);
		}

		return null;
	}

	internal static bool IsDlcExcluded(uint dlc)
	{
		return _config.RemovedDLCs.Contains(dlc);
	}

	internal static void SetDlcExcluded(uint dlc, bool excluded)
	{
		var list = new List<uint>(_config.RemovedDLCs);

		if (excluded)
		{
			list.Add(dlc);
		}
		else
		{
			list.Remove(dlc);
		}

		_config.RemovedDLCs = list.ToArray();

		SaveChanges();
	}
}
