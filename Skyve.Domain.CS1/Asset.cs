using Extensions;

using Skyve.Domain.Enums;
using Skyve.Systems;

using System;
using System.Collections.Generic;
using System.IO;

namespace Skyve.Domain.CS1;
public class Asset : IAsset
{
	public Asset(ILocalPackageData package, string crpPath, SkyveShared.AssetInfoCache.Asset? asset)
	{
		FilePath = crpPath;
		LocalParentPackage = package;
		FileSize = new FileInfo(FilePath).Length;
		LocalTime = File.GetLastWriteTimeUtc(FilePath);

		if (asset is not null)
		{
			Name = asset.Name;
			AssetTags = asset.Tags;
			FullName = asset.FullName;
		}
		else
		{
			Name = Path.GetFileNameWithoutExtension(FilePath).FormatWords();
			FullName = (package.IsLocal() ? "" : $"{Id}.") + Path.GetFileNameWithoutExtension(FilePath).RemoveDoubleSpaces().Replace(' ', '_');
			AssetTags = new string[0];
		}
	}

	public string FilePath { get; }
	public ILocalPackageData LocalParentPackage { get; }
	public long FileSize { get; }
	public DateTime LocalTime { get; }
	public string Name { get; }
	public string[] AssetTags { get; }
	public string FullName { get; }
	public string Folder => LocalParentPackage.Folder;
	public bool IsBuiltIn => LocalParentPackage.IsBuiltIn;
	public ulong Id => LocalParentPackage.Id;
	public string? Url => LocalParentPackage.Url;
	public IPackage? Package { get; set; }
	public AssetType AssetType { get; set; }
	public string[] Tags { get; set; }
	public string? Version { get; set; }

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
		return LocalParentPackage.GetWorkshopInfo();
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
