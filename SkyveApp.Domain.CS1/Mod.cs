using Extensions;

using System;
using System.Collections.Generic;

namespace SkyveApp.Domain.CS1;
public class Mod : IMod
{
	public Mod(ILocalPackageWithContents package, string dllPath, Version version)
	{
		LocalPackage = package;
		FilePath = dllPath.FormatPath();
		Version = version;
	}

	public string FilePath { get; }
	public Version Version { get; }
	public bool IsMod { get; } = true;

	public ILocalPackageWithContents LocalPackage { get; }
	public long LocalSize => LocalPackage.LocalSize;
	public DateTime LocalTime => LocalPackage.LocalTime;
	public string Folder => LocalPackage.Folder;
	public bool IsLocal => LocalPackage.IsLocal;
	public bool IsBuiltIn => LocalPackage.IsBuiltIn;
	public IEnumerable<IPackageRequirement> Requirements => LocalPackage.Requirements;
	public ulong Id => LocalPackage.Id;
	public string Name => LocalPackage.Name;
	public string? Url => LocalPackage.Url;

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
		return LocalPackage.GetWorkshopInfo();
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
