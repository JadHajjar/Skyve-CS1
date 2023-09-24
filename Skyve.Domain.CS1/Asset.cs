using Extensions;

using Skyve.Systems;

using System;
using System.Collections.Generic;
using System.IO;

namespace Skyve.Domain.CS1;
public class Asset : IAsset
{
	public Asset(ILocalPackageWithContents package, string crpPath, SkyveShared.AssetInfoCache.Asset? asset)
	{
		FilePath = crpPath;
		LocalParentPackage = package;
		LocalSize = new FileInfo(FilePath).Length;
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
			FullName = (IsLocal ? "" : $"{Id}.") + Path.GetFileNameWithoutExtension(FilePath).RemoveDoubleSpaces().Replace(' ', '_');
			AssetTags = new string[0];
		}
	}

	public string FilePath { get; }
	public ILocalPackageWithContents LocalParentPackage { get; }
	public long LocalSize { get; }
	public DateTime LocalTime { get; }
	public string Name { get; }
	public string[] AssetTags { get; }
	public string FullName { get; }
	public bool IsMod => LocalParentPackage.IsMod;
	public string Folder => LocalParentPackage.Folder;
	public bool IsLocal => LocalParentPackage.IsLocal;
	public bool IsBuiltIn => LocalParentPackage.IsBuiltIn;
	public IEnumerable<IPackageRequirement> Requirements => LocalParentPackage.Requirements;
	public ulong Id => LocalParentPackage.Id;
	public string? Url => LocalParentPackage.Url;
	ILocalPackage? IPackage.LocalPackage => this;

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
