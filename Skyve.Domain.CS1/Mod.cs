using Extensions;

using Skyve.Systems;

using System;
using System.Collections.Generic;

namespace Skyve.Domain.CS1;
public class Mod : IMod
{
	public Mod(ILocalPackageWithContents package, string dllPath, Version version)
	{
		LocalParentPackage = package;
		FilePath = dllPath.FormatPath();
		Version = version;
	}

	public string FilePath { get; }
	public Version Version { get; }
	public bool IsMod { get; } = true;

	public ILocalPackageWithContents LocalParentPackage { get; }
	public long LocalSize => LocalParentPackage.LocalSize;
	public DateTime LocalTime => LocalParentPackage.LocalTime;
	public string Folder => LocalParentPackage.Folder;
	public bool IsLocal => LocalParentPackage.IsLocal;
	public bool IsBuiltIn => LocalParentPackage.IsBuiltIn;
	public IEnumerable<IPackageRequirement> Requirements => LocalParentPackage.Requirements;
	public ulong Id => LocalParentPackage.Id;
	public string Name => LocalParentPackage.Name;
	public string? Url => LocalParentPackage.Url;
	ILocalPackage? IPackage.LocalPackage => this;

	public override bool Equals(object? obj)
	{
		return obj is IMod mod &&
			   FilePath == mod.FilePath;
	}

	public override int GetHashCode()
	{
		return 901043656 + EqualityComparer<string>.Default.GetHashCode(FilePath);
	}

	public IWorkshopInfo? GetWorkshopInfo()
	{
		return LocalParentPackage.GetWorkshopInfo();
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
