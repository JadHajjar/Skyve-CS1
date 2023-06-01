using Extensions;

using SkyveApp.Domain.Interfaces;
using SkyveApp.Domain.Steam;
using SkyveApp.Utilities;
using SkyveApp.Utilities.Managers;

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;

namespace SkyveApp.Domain;
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

	public string Folder => ((IPackage)Package).Folder;
	public long FileSize => ((IPackage)Package).FileSize;
	public bool IsMod => ((IPackage)Package).IsMod;
	public IEnumerable<TagItem> Tags => ((IPackage)Package).Tags;
	public ulong SteamId => ((IPackage)Package).SteamId;
	public Bitmap? IconImage => ((IPackage)Package).IconImage;
	public Bitmap? AuthorIconImage => ((IPackage)Package).AuthorIconImage;
	public SteamUser? Author => ((IPackage)Package).Author;
	public int Subscriptions => ((IPackage)Package).Subscriptions;
	public int PositiveVotes => ((IPackage)Package).PositiveVotes;
	public int NegativeVotes => ((IPackage)Package).NegativeVotes;
	public int Reports => ((IPackage)Package).Reports;
	public bool Workshop => ((IPackage)Package).Workshop;
	public DateTime ServerTime => ((IPackage)Package).ServerTime;
	public SteamVisibility Visibility => ((IPackage)Package).Visibility;
	public ulong[]? RequiredPackages => ((IPackage)Package).RequiredPackages;
	public string? IconUrl => ((IPackage)Package).IconUrl;
	public long ServerSize => ((IPackage)Package).ServerSize;
	public string? SteamDescription => ((IPackage)Package).SteamDescription;
	public string[]? WorkshopTags => ((IPackage)Package).WorkshopTags;
	public bool RemovedFromSteam => ((IPackage)Package).RemovedFromSteam;
	public bool Incompatible => ((IPackage)Package).Incompatible;
	public bool Banned => ((IPackage)Package).Banned;

	public bool IsCollection => ((IPackage)Package).IsCollection;

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
