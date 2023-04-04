using Extensions;
using LoadOrderToolTwo.Domain.Enums;
using LoadOrderToolTwo.Domain.Interfaces;
using LoadOrderToolTwo.Domain.Steam;
using LoadOrderToolTwo.Utilities;

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
		FileName = dllPath;
		Version = version;
		_dllName = Path.GetFileNameWithoutExtension(dllPath).FormatWords();
	}

	public string FileName { get; }
	public Version Version { get; }
	public Package Package { get; }
	public string Name { get => Package.Name.IfEmpty(_dllName); set => Package.Name = value; }
	public long FileSize => ContentUtil.GetTotalSize(Folder);

	public bool IsIncluded { get => ModsUtil.IsIncluded(this); set => ModsUtil.SetIncluded(this, value); }
	public bool IsEnabled { get => ModsUtil.IsEnabled(this); set => ModsUtil.SetEnabled(this, value); }

	public string Folder => ((IPackage)Package).Folder;
	public string? VirtualFolder => ((IPackage)Package).VirtualFolder;
	public bool BuiltIn => ((IPackage)Package).BuiltIn;
	public ulong SteamId => ((IPackage)Package).SteamId;
	public string? SteamPage => ((IPackage)Package).SteamPage;
	public bool Workshop => ((IPackage)Package).Workshop;
	public SteamUser? Author { get => ((IPackage)Package).Author; set => ((IPackage)Package).Author = value; }
	public Bitmap? IconImage => ((IPackage)Package).IconImage;
	public string? IconUrl { get => ((IPackage)Package).IconUrl; set => ((IPackage)Package).IconUrl = value; }
	public long LocalSize => ((IPackage)Package).LocalSize;
	public DateTime LocalTime => ((IPackage)Package).LocalTime;
	public bool RemovedFromSteam { get => ((IPackage)Package).RemovedFromSteam; set => ((IPackage)Package).RemovedFromSteam = value; }
	public bool Private { get => ((IPackage)Package).Private; set => ((IPackage)Package).Private = value; }
	public long ServerSize { get => ((IPackage)Package).ServerSize; set => ((IPackage)Package).ServerSize = value; }
	public DateTime ServerTime { get => ((IPackage)Package).ServerTime; set => ((IPackage)Package).ServerTime = value; }
	public DownloadStatus Status { get => ((IPackage)Package).Status; set => ((IPackage)Package).Status = value; }
	public string? StatusReason { get => ((IPackage)Package).StatusReason; set => ((IPackage)Package).StatusReason = value; }
	public bool SteamInfoLoaded { get => ((IPackage)Package).SteamInfoLoaded; set => ((IPackage)Package).SteamInfoLoaded = value; }
	public string? SteamDescription { get => ((IPackage)Package).SteamDescription; set => ((IPackage)Package).SteamDescription = value; }
	public IEnumerable<TagItem> Tags => ((IPackage)Package).Tags;
	public string[] WorkshopTags { set => ((IPackage)Package).WorkshopTags = value; }
	public Bitmap? AuthorIconImage => ((IPackage)Package).AuthorIconImage;
	public DateTime SubscribeTime => ((IPackage)Package).SubscribeTime;
	public bool IsPseudoMod { get => ((IPackage)Package).IsPseudoMod; set => ((IPackage)Package).IsPseudoMod = value; }

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
			EqualityComparer<Mod>.Default.Equals(left, right);
	}

	public static bool operator !=(Mod? left, Mod? right)
	{
		return !(left == right);
	}
}
