﻿using Extensions;

using LoadOrderToolTwo.Domain.Enums;
using LoadOrderToolTwo.Domain.Interfaces;
using LoadOrderToolTwo.Domain.Steam;
using LoadOrderToolTwo.Utilities;
using LoadOrderToolTwo.Utilities.Managers;

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;

namespace LoadOrderToolTwo.Domain;
public class Asset : IPackage
{
	private readonly string[] _assetTags;

	public Asset(Package package, string crpPath)
	{
		FileName = crpPath.FormatPath();
		Package = package;
		FileSize = new FileInfo(FileName).Length;
		LocalTime = File.GetLastWriteTimeUtc(FileName);

		if (AssetsUtil.AssetInfoCache.TryGetValue(FileName, out var asset))
		{
			Name = asset.Name;
			Description = asset.Description;
			_assetTags = asset.Tags;
		}
		else
		{
			Name = Path.GetFileNameWithoutExtension(FileName).FormatWords();
			_assetTags = new string[0];
		}
	}

	public string FileName { get; }
	public Package Package { get; }
	public long FileSize { get; }
	public string Name { get; set; }
	public string? Description { get; }
	public DateTime LocalTime { get; }
	public bool IsIncluded { get => AssetsUtil.IsIncluded(this); set => AssetsUtil.SetIncluded(this, value); }

	public string Folder => ((IPackage)Package).Folder;
	public bool BuiltIn => ((IPackage)Package).BuiltIn;
	public ulong SteamId => ((IPackage)Package).SteamId;
	public string? SteamPage => ((IPackage)Package).SteamPage;
	public bool Workshop => ((IPackage)Package).Workshop;
	public SteamUser? Author { get => ((IPackage)Package).Author; set => ((IPackage)Package).Author = value; }
	public Bitmap? IconImage => AssetsUtil.GetIcon(this) ?? ((IPackage)Package).IconImage;
	public string? IconUrl { get => ((IPackage)Package).IconUrl; set => ((IPackage)Package).IconUrl = value; }
	public long LocalSize => ((IPackage)Package).LocalSize;
	public bool RemovedFromSteam { get => ((IPackage)Package).RemovedFromSteam; set => ((IPackage)Package).RemovedFromSteam = value; }
	public bool Private { get => ((IPackage)Package).Private; set => ((IPackage)Package).Private = value; }
	public long ServerSize { get => ((IPackage)Package).ServerSize; set => ((IPackage)Package).ServerSize = value; }
	public DateTime ServerTime { get => ((IPackage)Package).ServerTime; set => ((IPackage)Package).ServerTime = value; }
	public DownloadStatus Status { get => ((IPackage)Package).Status; set => ((IPackage)Package).Status = value; }
	public string? StatusReason { get => ((IPackage)Package).StatusReason; set => ((IPackage)Package).StatusReason = value; }
	public bool SteamInfoLoaded { get => ((IPackage)Package).SteamInfoLoaded; set => ((IPackage)Package).SteamInfoLoaded = value; }
	public string? SteamDescription { get => ((IPackage)Package).SteamDescription; set => ((IPackage)Package).SteamDescription = value; }
	public string? VirtualFolder => ((IPackage)Package).VirtualFolder;
	public Bitmap? AuthorIconImage => ((IPackage)Package).AuthorIconImage;
	public DateTime SubscribeTime => ((IPackage)Package).SubscribeTime;
	public bool IsPseudoMod { get => ((IPackage)Package).IsPseudoMod; set => ((IPackage)Package).IsPseudoMod = value; }
	public string[]? WorkshopTags { set => ((IPackage)Package).WorkshopTags = value; }
	public IEnumerable<TagItem> Tags
	{
		get
		{
			if (Package.WorkshopTags is not null)
			{
				foreach (var item in Package.WorkshopTags)
				{
					yield return new(TagSource.Workshop, item);
				}
			}

			foreach (var item in _assetTags)
			{
				yield return new(TagSource.InGame, item);
			}

			foreach (var item in AssetsUtil.GetFindItTags(this))
			{
				yield return new(TagSource.FindIt, item.ToCapital(false));
			}
		}
	}

	public override bool Equals(object? obj)
	{
		return obj is Asset asset &&
			   FileName == asset.FileName;
	}

	public override int GetHashCode()
	{
		return 901043656 + EqualityComparer<string>.Default.GetHashCode(FileName);
	}

	public override string ToString()
	{
		return Name;
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
