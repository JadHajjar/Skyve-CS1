using Extensions;

using System;
using System.Collections.Generic;
using System.IO;

namespace SkyveApp.Domain;
public class Asset : IAsset
{
	public Asset(ILocalPackageWithContents package, string crpPath, SkyveShared.CSCache.Asset? asset)
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
	public ILocalPackageWithContents LocalPackage { get; }
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

	public override bool Equals(object? obj)
	{
		return obj is IAsset asset &&
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
