using Extensions;

using Skyve.Domain.Systems;
using Skyve.Systems;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Skyve.Domain.CS1;

public class Package : ILocalPackageWithContents
{
	private ulong id;

	public Package(string folder, bool builtIn, bool workshop, long localSize, DateTime localTime)
	{
		Folder = folder.FormatPath();
		LocalSize = localSize;
		LocalTime = localTime;
		IsBuiltIn = builtIn;
		IsLocal = !workshop;
		Assets = new Asset[0];

		if (workshop)
		{
			Id = ulong.Parse(Path.GetFileName(folder));
			Url = $"https://steamcommunity.com/workshop/filedetails/?id={Id}";
		}
	}

	public IAsset[] Assets { get; set; }
	public IMod? Mod { get; set; }
	public string Folder { get; set; }
	public bool IsBuiltIn { get; }
	public bool IsLocal { get; }
	public long LocalSize { get; set; }
	public DateTime LocalTime { get; set; }
	public string Name => ToString();
	public bool IsMod => Mod is not null;
	public string? Url { get; }
	public IEnumerable<IPackageRequirement> Requirements => this.GetWorkshopInfo()?.Requirements ?? Enumerable.Empty<IPackageRequirement>();

	public ulong Id
	{
		get
		{
			if (id == 0)
			{
				if (Mod is not null)
				{
					id = ServiceCenter.Get<ICompatibilityManager>().GetIdFromModName(Path.GetFileName(Mod.FilePath));
				}

				if (id == 0 && ulong.TryParse(Path.GetFileName(Folder), out var folderId))
				{
					if (folderId > 2_000_000_000)
					{
						id = folderId;
					}
				}
			}

			return id;
		}

		set => id = value;
	}

	ILocalPackageWithContents? IPackage.LocalParentPackage => this;
	ILocalPackageWithContents ILocalPackage.LocalParentPackage => this;
	ILocalPackage? IPackage.LocalPackage => this;
	string ILocalPackageIdentity.FilePath => Folder;

	public override string ToString()
	{
		var workshopInfo = this.GetWorkshopInfo();

		return workshopInfo is not null
			? workshopInfo.Name
			: Mod is not null ? Path.GetFileNameWithoutExtension(Mod!.FilePath).FormatWords() : Path.GetFileNameWithoutExtension(Folder);
	}

	public override bool Equals(object? obj)
	{
		return obj is Package package && Folder == package.Folder;
	}

	public override int GetHashCode()
	{
		return -1486376059 + EqualityComparer<string>.Default.GetHashCode(Folder);
	}

	public static bool operator ==(Package? left, Package? right)
	{
		return
			left is null ? right is null :
			right is null ? left is null :
			EqualityComparer<Package>.Default.Equals(left, right);
	}

	public static bool operator !=(Package? left, Package? right)
	{
		return !(left == right);
	}
}