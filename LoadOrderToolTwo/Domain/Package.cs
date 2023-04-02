using Extensions;
using LoadOrderToolTwo.Domain.Enums;
using LoadOrderToolTwo.Domain.Interfaces;
using LoadOrderToolTwo.Domain.Steam;
using LoadOrderToolTwo.Utilities;
using LoadOrderToolTwo.Utilities.Managers;

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;

namespace LoadOrderToolTwo.Domain;

public class Package : IPackage
{
	public Package(string folder, bool builtIn, bool workshop)
	{
		Name = string.Empty;
		Folder = folder;
		BuiltIn = builtIn;
		Workshop = workshop;

		if (workshop)
		{
			SteamId = ulong.Parse(Path.GetFileName(folder));
			SteamPage = $"https://steamcommunity.com/workshop/filedetails/?id={SteamId}";
		}

		if (!string.IsNullOrWhiteSpace(LocationManager.VirtualGamePath) && Folder.Contains(LocationManager.GamePath))
		{
			VirtualFolder = Folder.Replace(LocationManager.GamePath, LocationManager.VirtualGamePath).Replace("\\", "/");
		}

		if (!string.IsNullOrWhiteSpace(LocationManager.VirtualWorkshopContentPath) && Folder.Contains(LocationManager.WorkshopContentPath))
		{
			VirtualFolder = Folder.Replace(LocationManager.WorkshopContentPath, LocationManager.VirtualWorkshopContentPath).Replace("\\", "/");
		}

		if (!string.IsNullOrWhiteSpace(LocationManager.VirtualAppDataPath) && Folder.Contains(LocationManager.AppDataPath))
		{
			VirtualFolder = Folder.Replace(LocationManager.AppDataPath, LocationManager.VirtualAppDataPath).Replace("\\", "/");
		}
	}

	public Asset[]? Assets { get; set; }
	public Mod? Mod { get; set; }

	public long LocalSize => ContentUtil.GetTotalSize(Folder);
	public DateTime LocalTime => ContentUtil.GetLocalUpdatedTime(Folder).ToUniversalTime();
	public DateTime SubscribeTime => ContentUtil.GetLocalSubscribeTime(Folder).ToUniversalTime();

	public string? VirtualFolder { get; set; }
	public string Folder { get; set; }
	public ulong SteamId { get; set; }
	public bool BuiltIn { get; set; }
	public bool Workshop { get; set; }
	public string? SteamPage { get; set; }
	public SteamUser? Author { get; set; }
	public Bitmap? IconImage => ImageManager.GetImage(IconUrl, true).Result;
	public Bitmap? AuthorIconImage => ImageManager.GetImage(Author?.AvatarUrl, true).Result;
	public string? IconUrl { get; set; }
	public string Name { get; set; }
	public bool RemovedFromSteam { get; set; }
	public bool Private { get; set; }
	public long ServerSize { get; set; }
	public DateTime ServerTime { get; set; }
	public DownloadStatus Status { get; set; }
	public string? StatusReason { get; set; }
	public bool SteamInfoLoaded { get; set; }
	public string[]? Tags { get; set; }
	public string? SteamDescription { get; set; }
	public bool IsPseudoMod { get; set; }
	public bool IsIncluded { get => (Mod?.IsIncluded ?? true) && (Assets?.All(x => x.IsIncluded) ?? true); set => SetIncluded(value); }

	Package IPackage.Package => this;

	internal CompatibilityManager.ReportInfo? CompatibilityReport => CompatibilityManager.GetCompatibilityReport(this);
	internal bool? ForAssetEditor => CompatibilityManager.IsForAssetEditor(this);
	internal bool? ForNormalGame => CompatibilityManager.IsForNormalGame(this);
	public long FileSize => Mod?.FileSize ?? Assets?.Sum(x => x.FileSize) ?? 0;

	private void SetIncluded(bool value)
	{
		if (Mod is not null)
		{
			Mod.IsIncluded = value;
		}

		if (Assets is not null)
		{
			foreach (var asset in Assets)
			{
				asset.IsIncluded = value;
			}
		}
	}

	public override string ToString()
	{
		if (!string.IsNullOrEmpty(Name))
		{ return Name; }

		if (!string.IsNullOrEmpty(Mod?.Name))
		{ return Mod!.Name; }

		return Path.GetFileNameWithoutExtension(Folder).FormatWords();
	}

	public override bool Equals(object? obj)
	{
		return obj is Package package &&
			   Folder == package.Folder;
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