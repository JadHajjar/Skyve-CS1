using Extensions;

using System;
using System.Collections.Generic;
using System.IO;

namespace SkyveApp.Domain;
public class Mod : IMod
{
	private readonly string _dllName;
	public Mod(Package package, string dllPath, Version version)
	{
		Package = package;
		FilePath = dllPath.FormatPath();
		Version = version;
		_dllName = Path.GetFileNameWithoutExtension(FilePath).FormatWords();
	}

	public Package Package { get; }
	public string FilePath { get; }
	public Version Version { get; }
	public bool IsMod { get; } = true;

	public ILocalPackage? LocalPackage => Package;
	public long LocalSize => ((ILocalPackage)Package).LocalSize;
	public DateTime LocalTime => ((ILocalPackage)Package).LocalTime;
	public string Folder => ((ILocalPackage)Package).Folder;
	public bool IsLocal => ((IPackage)Package).IsLocal;
	public bool IsBuiltIn => ((IPackage)Package).IsBuiltIn;
	public IEnumerable<IPackageRequirement> Requirements => ((IPackage)Package).Requirements;
	public IEnumerable<ITag> Tags => ((IPackage)Package).Tags;
	public ulong Id => ((IPackageIdentity)Package).Id;
	public string Name => ((IPackageIdentity)Package).Name;
	public string? Url => ((IPackageIdentity)Package).Url;

	public override bool Equals(object? obj)
	{
		return obj is Mod mod &&
			   FilePath == mod.FilePath;
	}

	public override int GetHashCode()
	{
		return 901043656 + EqualityComparer<string>.Default.GetHashCode(FilePath);
	}

	public IWorkshopInfo? GetWorkshopInfo()
	{
		return ((IPackage)Package).GetWorkshopInfo();
	}

	public override string ToString()
	{
		return Name;
	}

	public static bool operator ==(Mod? left, Mod? right)
	{
		return
			left is null ? right is null :
			right is null ? left is null :
			left.Equals(right);
	}

	public static bool operator !=(Mod? left, Mod? right)
	{
		return !(left == right);
	}
}
