using Extensions;

using SkyveApp.Domain.Enums;
using SkyveApp.Domain.Interfaces;
using SkyveApp.Domain.Steam;
using SkyveApp.Services;
using SkyveApp.Utilities;

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;

namespace SkyveApp.Domain;
public class Asset : IAsset
{
	public Asset(ILocalPackage package, string crpPath, SkyveShared.CSCache.Asset? asset)
	{
		FilePath = crpPath;
		LocalPackage = package;
		LocalSize = new FileInfo(FilePath).Length;
		LocalTime = File.GetLastWriteTimeUtc(FilePath);

		if (asset is not null)
		{
			Name = asset.Name;
			AssetTags = asset.Tags;
		}
		else
		{
			Name = Path.GetFileNameWithoutExtension(FilePath).FormatWords();
			AssetTags = new string[0];
		}
	}

	public string FilePath { get; }
	public ILocalPackage LocalPackage { get; }
	public long LocalSize { get; }
	public DateTime LocalTime { get; }
	public string Name { get; }
	public string[] AssetTags { get; }
	public bool IsMod { get; }
	public string Folder => LocalPackage.Folder;
	public bool IsLocal => LocalPackage.IsLocal;
	public bool IsBuiltIn => LocalPackage.IsBuiltIn;
	public IEnumerable<IPackageRequirement> Requirements => LocalPackage.Requirements;
	public ulong Id => LocalPackage.Id;
	public string? Url => LocalPackage.Url;

	public IEnumerable<ITag> Tags
	{
		get
		{
			foreach (var item in AssetTags)
			{
				yield return new TagItem(TagSource.InGame, item);
			}

			foreach (var item in Program.Services.GetService<IAssetUtil>().GetFindItTags(this))
			{
				yield return new TagItem(TagSource.FindIt, item.ToCapital(false));
			}
		}
	}

	public override bool Equals(object? obj)
	{
		return obj is Asset asset &&
			   FilePath == asset.FilePath;
	}

	public override int GetHashCode()
	{
		return 901043656 + EqualityComparer<string>.Default.GetHashCode(FilePath);
	}

	public override string ToString()
	{
		return Name;
	}

	public IWorkshopInfo? GetWorkshopInfo()
	{
		return LocalPackage.GetWorkshopInfo();
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
