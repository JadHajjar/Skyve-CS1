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
	public string Name { get; }
	public string? Description { get; }
	public DateTime LocalTime { get; }
	public string[] AssetTags => _assetTags;
	public bool IsIncluded { get => AssetsUtil.IsIncluded(this); set => AssetsUtil.SetIncluded(this, value); }
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

	public bool BuiltIn => ((IPackage)Package).BuiltIn;
	public string Folder => ((IPackage)Package).Folder;
	public bool IsPseudoMod { get => ((IPackage)Package).IsPseudoMod; set => ((IPackage)Package).IsPseudoMod = value; }
	public bool IsMod => ((IGenericPackage)Package).IsMod;
	public ulong SteamId => ((IGenericPackage)Package).SteamId;
	public Bitmap? IconImage => ((IGenericPackage)Package).IconImage;
	public Bitmap? AuthorIconImage => ((IGenericPackage)Package).AuthorIconImage;
	public SteamUser? Author => ((IGenericPackage)Package).Author;
	public int Subscriptions => ((IGenericPackage)Package).Subscriptions;
	public int PositiveVotes => ((IGenericPackage)Package).PositiveVotes;
	public int NegativeVotes => ((IGenericPackage)Package).NegativeVotes;
	public int Reports => ((IGenericPackage)Package).Reports;
	public bool Workshop => ((IGenericPackage)Package).Workshop;
	public DateTime ServerTime => ((IGenericPackage)Package).ServerTime;
	public CompatibilityManager.ReportInfo? CompatibilityReport => ((IGenericPackage)Package).CompatibilityReport;
	public SteamVisibility Visibility => ((IGenericPackage)Package).Visibility;
	public ulong[]? RequiredPackages => ((IGenericPackage)Package).RequiredPackages;
	public string? IconUrl => ((IGenericPackage)Package).IconUrl;
	public long ServerSize => ((IGenericPackage)Package).ServerSize;
	public string? SteamDescription => ((IGenericPackage)Package).SteamDescription;
	public string[]? WorkshopTags => ((IGenericPackage)Package).WorkshopTags;
	public bool RemovedFromSteam => ((IGenericPackage)Package).RemovedFromSteam;

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
