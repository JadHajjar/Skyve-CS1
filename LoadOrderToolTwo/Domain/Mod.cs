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
public class Mod : IPackage
{
	private readonly string _dllName;
	public Mod(Package package, string dllPath, Version version)
	{
		Package = package;
		FileName = dllPath.FormatPath();
		Version = version;
		_dllName = Path.GetFileNameWithoutExtension(FileName).FormatWords();
	}

	public string FileName { get; }
	public Version Version { get; }
	public Package Package { get; }
	public string Name => Package.Name.IfEmpty(_dllName);

	public bool IsIncluded { get => ModsUtil.IsIncluded(this); set => ModsUtil.SetIncluded(this, value); }
	public bool IsEnabled { get => ModsUtil.IsEnabled(this); set => ModsUtil.SetEnabled(this, value); }

	public bool BuiltIn => ((IPackage)Package).BuiltIn;
	public string Folder => ((IPackage)Package).Folder;
	public long FileSize => ((IPackage)Package).FileSize;
	public bool IsPseudoMod { get => ((IPackage)Package).IsPseudoMod; set => ((IPackage)Package).IsPseudoMod = value; }
	public bool IsMod => ((IGenericPackage)Package).IsMod;
	public IEnumerable<TagItem> Tags => ((IGenericPackage)Package).Tags;
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
		return obj is Mod mod &&
			   FileName == mod.FileName;
	}

	public override int GetHashCode()
	{
		return 901043656 + EqualityComparer<string>.Default.GetHashCode(FileName);
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
