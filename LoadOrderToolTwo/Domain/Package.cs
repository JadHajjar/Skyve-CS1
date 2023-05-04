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
		Folder = folder.FormatPath();
		BuiltIn = builtIn;
		Workshop = workshop;
		Assets = new Asset[0];

		if (workshop)
		{
			SteamId = ulong.Parse(Path.GetFileName(folder));
		}
	}

	public Asset[] Assets { get; set; }
	public Mod? Mod { get; set; }

	public bool BuiltIn { get; }
	public bool Workshop { get; }
	public string Folder { get; }
	public bool IsPseudoMod { get; set; }
	public long FileSize => ContentUtil.GetTotalSize(Folder);

	//public long LocalSize => ContentUtil.GetTotalSize(Folder);
	public DateTime LocalTime => ContentUtil.GetLocalUpdatedTime(Folder);
	//public DateTime SubscribeTime => ContentUtil.GetLocalSubscribeTime(Folder);

	//public string? VirtualFolder { get; set; }
	//public string Folder { get; set; }
	//public ulong SteamId { get; set; }
	//public bool BuiltIn { get; set; }
	//public bool Workshop { get; set; }
	//public string? SteamPage { get; set; }
	//public SteamUser? Author { get; set; }
	//public Bitmap? IconImage => ImageManager.GetImage(IconUrl, true).Result;
	//public Bitmap? AuthorIconImage => ImageManager.GetImage(Author?.AvatarUrl, true).Result;
	//public string? IconUrl { get; set; }
	//public string Name { get; set; }
	//public bool RemovedFromSteam { get; set; }
	//public bool Private { get; set; }
	//public long ServerSize { get; set; }
	//public DateTime ServerTime { get; set; }
	public DownloadStatus Status { get; set; }
	public string? StatusReason { get; set; }
	//public bool SteamInfoLoaded { get; set; }
	//public string[]? WorkshopTags { get; set; }
	//public string? SteamDescription { get; set; }
	//public bool IsPseudoMod { get; set; }
	//public int Stars { get; set; }
	//public int Subscriptions { get; set; }
	//public int PositiveVotes { get; set; }
	//public int NegativeVotes { get; set; }
	//public ulong[] RequiredPackages { get; set; }
	//public SteamVisibility Visibility { get; set; }
	public bool IsIncluded { get => (Mod?.IsIncluded ?? true) && (Assets?.All(x => x.IsIncluded) ?? true); set => ContentUtil.SetBulkIncluded(new[] { this }, value); }
	//public IEnumerable<TagItem> Tags => WorkshopTags?.Select(x => new TagItem(TagSource.Workshop, x)) ?? Enumerable.Empty<TagItem>();

	//Package IPackage.Package => this;

	public CompatibilityManager.ReportInfo? CompatibilityReport => CompatibilityManager.GetCompatibilityReport(this);
	internal bool? ForAssetEditor => CompatibilityManager.IsForAssetEditor(this);
	internal bool? ForNormalGame => CompatibilityManager.IsForNormalGame(this);
	Package? IPackage.Package => this;

	public SteamWorkshopItem? WorkshopInfo { get; internal set; }
	public ulong SteamId { get; }

	public string? Name => WorkshopInfo?.Name;
	public bool IsMod => Mod is not null || (WorkshopInfo?.IsMod ?? false);
	public IEnumerable<TagItem> Tags => WorkshopTags?.Select(x => new TagItem(TagSource.Workshop, x)) ?? Enumerable.Empty<TagItem>();
	public Bitmap? IconImage => WorkshopInfo?.IconImage;
	public Bitmap? AuthorIconImage => WorkshopInfo?.AuthorIconImage;
	public string? IconUrl => WorkshopInfo?.IconUrl;
	public SteamUser? Author => WorkshopInfo?.Author;
	public int Subscriptions => WorkshopInfo?.Subscriptions ?? 0;
	public int PositiveVotes => WorkshopInfo?.PositiveVotes ?? 0;
	public int NegativeVotes => WorkshopInfo?.NegativeVotes ?? 0;
	public int Reports => WorkshopInfo?.Reports ?? 0;
	public DateTime ServerTime => WorkshopInfo?.ServerTime ?? DateTime.MinValue;
	public SteamVisibility Visibility => WorkshopInfo?.Visibility ?? SteamVisibility.Local;
	public ulong[]? RequiredPackages => WorkshopInfo?.RequiredPackages;
	public bool RemovedFromSteam => WorkshopInfo?.RemovedFromSteam ?? false;
	public long ServerSize => WorkshopInfo?.ServerSize??0;
	public string? SteamDescription => WorkshopInfo?.SteamDescription;
	public string[]? WorkshopTags => WorkshopInfo?.WorkshopTags;

	public bool IsPartiallyIncluded()
	{
		var included = false;
		var excluded = false;

		if (Mod is not null)
		{
			if (Mod.IsIncluded)
			{
				included = true;
			}
			else
			{
				excluded = true;
			}
		}

		if (Assets != null)
		{
			foreach (var item in Assets)
			{
				if (item.IsIncluded)
				{
					included = true;
				}
				else
				{
					excluded = true;
				}

				if (included && excluded)
					return true;
			}
		}

		return false;
	}

	public override string ToString()
	{
		if (!string.IsNullOrEmpty(Name))
		{ return Name!; }

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