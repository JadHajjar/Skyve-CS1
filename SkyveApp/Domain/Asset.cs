using Extensions;

using SkyveApp.Domain.Enums;
using SkyveApp.Domain.Interfaces;
using SkyveApp.Domain.Steam;
using SkyveApp.Utilities;
using SkyveApp.Utilities.Managers;

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;

namespace SkyveApp.Domain;
public class Asset : IPackage
{
	private readonly string[] _assetTags;

	public Asset(Package package, string crpPath)
	{
		FileName = crpPath.FormatPath();
		Package = package;
		FileSize = new FileInfo(FileName).Length;
		LocalTime = File.GetLastWriteTimeUtc(FileName);

		if (AssetsUtil.AssetInfoCache.TryGetValue(FileName, out var asset))
		{
			Name = asset.Name;
			Description = asset.Description;
			_assetTags = asset.Tags;
		}
		else
		{
			Name = Path.GetFileNameWithoutExtension(FileName).FormatWords();
			_assetTags = new string[0];
		}
	}

	public string FileName { get; }
	public Package Package { get; }
	public long FileSize { get; }
	public string Name { get; }
	public string? Description { get; }
	public DateTime LocalTime { get; }
	public string[] AssetTags => _assetTags;
	public bool IsIncluded { get => AssetsUtil.IsIncluded(this); set => AssetsUtil.SetIncluded(this, value); }
	public IEnumerable<TagItem> Tags => Package.Tags.Concat(GetAssetTags());
	public string Folder => ((IPackage)Package).Folder;
	public bool IsMod => ((IPackage)Package).IsMod;
	public ulong SteamId => ((IPackage)Package).SteamId;
	public Bitmap? IconImage => ((IPackage)Package).IconImage;
	public Bitmap? AuthorIconImage => ((IPackage)Package).AuthorIconImage;
	public SteamUser? Author => ((IPackage)Package).Author;
	public int Subscriptions => ((IPackage)Package).Subscriptions;
	public int PositiveVotes => ((IPackage)Package).PositiveVotes;
	public int NegativeVotes => ((IPackage)Package).NegativeVotes;
	public int Reports => ((IPackage)Package).Reports;
	public bool Workshop => ((IPackage)Package).Workshop;
	public DateTime ServerTime => ((IPackage)Package).ServerTime;
	public SteamVisibility Visibility => ((IPackage)Package).Visibility;
	public ulong[]? RequiredPackages => ((IPackage)Package).RequiredPackages;
	public string? IconUrl => ((IPackage)Package).IconUrl;
	public long ServerSize => ((IPackage)Package).ServerSize;
	public string? SteamDescription => ((IPackage)Package).SteamDescription;
	public string[]? WorkshopTags => ((IPackage)Package).WorkshopTags;
	public bool RemovedFromSteam => ((IPackage)Package).RemovedFromSteam;
	public bool Incompatible => ((IPackage)Package).Incompatible;

	public bool IsCollection => ((IPackage)Package).IsCollection;

	public IEnumerable<TagItem> GetAssetTags()
	{
		foreach (var item in _assetTags)
		{
			yield return new(TagSource.InGame, item);
		}

		foreach (var item in AssetsUtil.GetFindItTags(this))
		{
			yield return new(TagSource.FindIt, item.ToCapital(false));
		}
	}

	public override bool Equals(object? obj)
	{
		return obj is Asset asset &&
			   FileName == asset.FileName;
	}

	public override int GetHashCode()
	{
		return 901043656 + EqualityComparer<string>.Default.GetHashCode(FileName);
	}

	public override string ToString()
	{
		return Name;
	}

	public static bool operator ==(Asset? left, Asset? right)
	{
		return
			left is null ? right is null :
			right is null ? left is null :
			EqualityComparer<Asset>.Default.Equals(left, right);
	}

	public static bool operator !=(Asset? left, Asset? right)
	{
		return !(left == right);
	}
}
