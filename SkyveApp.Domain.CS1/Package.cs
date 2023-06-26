using Extensions;

using Microsoft.Extensions.DependencyInjection;

using SkyveApp.Services;
using SkyveApp.Utilities;

using System;
using System.Collections.Generic;
using System.IO;

namespace SkyveApp.Domain;

public class Package : ILocalPackageWithContents
{
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

	public Asset[] Assets { get; set; }
	public Mod? Mod { get; set; }
	public string Folder { get; set; }
	public ulong Id { get; }
	public bool IsBuiltIn { get; }
	public bool IsLocal { get; }
	public long LocalSize { get; set; }
	public DateTime LocalTime { get; set; }
	public string Name => ToString();
	public bool IsMod => Mod is not null;
	public string? Url { get; }
	public IEnumerable<ITag> Tags
	{
		get
		{

		}
	}
	public IEnumerable<IPackageRequirement> Requirements 
	{
		get 
		{

		}
	}

	public IWorkshopInfo? GetWorkshopInfo()
	{
		var id = Id;

		if (id == 0)
		{
			if (ulong.TryParse(Path.GetFileName(Folder), out var folderId))
			{
				id = folderId;
			}
			else if (Mod is not null)
			{
				var crId = Program.Services.GetService<CompatibilityManager>()?.CompatibilityData.PackageNames.TryGet(Path.GetFileName(Mod.FilePath));

				if (crId.HasValue)
				{
					id = crId.Value;
				}
			}
		}

		return SteamUtil.GetItem(id);
	}

	IAsset[] ILocalPackageWithContents.Assets => Assets;
	IMod? ILocalPackageWithContents.Mod => Mod;
	ILocalPackage? IPackage.LocalPackage => this;
	string ILocalPackageIdentity.FilePath => Folder;

	public override string ToString()
	{
		var workshopInfo = GetWorkshopInfo();

		if (workshopInfo is not null)
		{
			return workshopInfo.Title;
		}

		if (Mod is not null)
		{
			return Path.GetFileNameWithoutExtension(Mod!.FilePath).FormatWords();
		}

		return Path.GetFileNameWithoutExtension(Folder);
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