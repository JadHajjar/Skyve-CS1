using Extensions;

using LoadOrderToolTwo.Domain.Interfaces;
using LoadOrderToolTwo.Domain.Steam;
using LoadOrderToolTwo.Utilities;
using LoadOrderToolTwo.Utilities.Managers;

using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;

namespace LoadOrderToolTwo.Domain;
public class Profile
{
	public static readonly Profile TemporaryProfile = new(Locale.TemporaryProfile) { Temporary = true, AutoSave = false };

	[CloneIgnore] public string? Name { get; set; }
	[CloneIgnore] public bool Temporary { get; set; }
	[JsonIgnore] public DateTime LastEditDate { get; set; }
	[JsonIgnore] public DateTime DateCreated { get; set; }
	[JsonIgnore, CloneIgnore] public bool IsMissingItems { get; set; }

	public Profile(string name) : this()
	{
		Name = name;
	}

	public Profile()
	{
		LaunchSettings = new();
		LsmSettings = new();
		Assets = new();
		Mods = new();
		ExcludedDLCs = new();
		AutoSave = true;
	}

	public bool Save()
	{
		ProfileManager.GatherInformation(this);

		return ProfileManager.Save(this);
	}

	public override string ToString()
	{
		return Name ?? base.ToString();
	}

	[CloneIgnore] public List<Asset> Assets { get; set; }
	[CloneIgnore] public List<Mod> Mods { get; set; }
	[CloneIgnore] public List<uint> ExcludedDLCs { get; set; }
	public LaunchSettings LaunchSettings { get; set; }
	public LsmSettings LsmSettings { get; set; }
	public bool AutoSave { get; set; }
	public bool ForAssetEditor { get; set; }
	public bool ForGameplay { get; set; }
	public bool IsFavorite { get; set; }
	public Color? Color { get; set; }
	public DateTime LastUsed { get; set; }

	public class Asset : IPackage
	{
		public string? Name { get; set; }
		public string? RelativePath { get; set; }
		public ulong SteamId { get; set; }

		[JsonIgnore, CloneIgnore] public bool IsMod { get; protected set; }
		[JsonIgnore, CloneIgnore] public bool Workshop => SteamId != 0;
		[JsonIgnore, CloneIgnore] public string Folder => RelativePath is null ? string.Empty : IsMod ? ProfileManager.ToLocalPath(RelativePath) : Path.GetDirectoryName(ProfileManager.ToLocalPath(RelativePath));
		[JsonIgnore, CloneIgnore] public Package? Package => SteamId == 0 ? null : CentralManager.Packages.FirstOrDefault(x => x.SteamId == SteamId);
		[JsonIgnore, CloneIgnore] public SteamWorkshopItem? WorkshopInfo { get; set; }
		[JsonIgnore, CloneIgnore] public IEnumerable<TagItem> Tags => WorkshopInfo?.Tags ?? new[] { new TagItem(Enums.TagSource.InGame, IsMod ? "Mod" : "Asset") };
		[JsonIgnore, CloneIgnore] public Bitmap? IconImage => WorkshopInfo?.IconImage;
		[JsonIgnore, CloneIgnore] public Bitmap? AuthorIconImage => WorkshopInfo?.AuthorIconImage;
		[JsonIgnore, CloneIgnore] public SteamUser? Author => WorkshopInfo?.Author;
		[JsonIgnore, CloneIgnore] public int Subscriptions => WorkshopInfo?.Subscriptions ?? 0;
		[JsonIgnore, CloneIgnore] public int PositiveVotes => WorkshopInfo?.PositiveVotes ?? 0;
		[JsonIgnore, CloneIgnore] public int NegativeVotes => WorkshopInfo?.NegativeVotes ?? 0;
		[JsonIgnore, CloneIgnore] public int Reports => WorkshopInfo?.Reports ?? 0;
		[JsonIgnore, CloneIgnore] public bool IsIncluded { get => WorkshopInfo?.IsIncluded ?? false; set { if (WorkshopInfo is not null) { WorkshopInfo.IsIncluded = value; } } }
		[JsonIgnore, CloneIgnore] public long FileSize => WorkshopInfo?.FileSize ?? 0;
		[JsonIgnore, CloneIgnore] public DateTime ServerTime => WorkshopInfo?.ServerTime ?? DateTime.MinValue;
		[JsonIgnore, CloneIgnore] public SteamVisibility Visibility => WorkshopInfo?.Visibility ?? SteamVisibility.Local;
		[JsonIgnore, CloneIgnore] public ulong[]? RequiredPackages => WorkshopInfo?.RequiredPackages;
		[JsonIgnore, CloneIgnore] public string? IconUrl => WorkshopInfo?.IconUrl;
		[JsonIgnore, CloneIgnore] public long ServerSize => WorkshopInfo?.ServerSize ?? 0;
		[JsonIgnore, CloneIgnore] public string? SteamDescription => WorkshopInfo?.SteamDescription;
		[JsonIgnore, CloneIgnore] public bool RemovedFromSteam => WorkshopInfo?.RemovedFromSteam ?? false;
		[JsonIgnore, CloneIgnore] public string[]? WorkshopTags => WorkshopInfo?.WorkshopTags;

		public Asset(Domain.Asset asset)
		{
			SteamId = asset.SteamId;
			Name = asset.Name;
			RelativePath = ProfileManager.ToRelativePath(asset.FileName);
		}

		public Asset()
		{

		}

		public override string ToString()
		{
			return WorkshopInfo?.Title ?? Name ?? Locale.UnknownPackage;
		}
	}

	public class Mod : Asset
	{
		public bool Enabled { get; set; }

		public Mod(Domain.Mod mod)
		{
			IsMod = true;
			SteamId = mod.SteamId;
			Name = mod.Name;
			Enabled = mod.IsEnabled;
			RelativePath = ProfileManager.ToRelativePath(mod.Folder);
		}

		public Mod(IPackage package)
		{
			IsMod = true;
			SteamId = package.SteamId;
			Name = package.Name;
			Enabled = true;
			RelativePath = LocationManager.Combine(ProfileManager.WS_CONTENT_PATH, package.SteamId.ToString());
		}

		public Mod()
		{
			IsMod = true;
		}
	}
}
