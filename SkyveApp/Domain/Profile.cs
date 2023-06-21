using Extensions;

using Newtonsoft.Json;

using SkyveApp.Domain.Compatibility.Api;
using SkyveApp.Domain.Compatibility.Enums;
using SkyveApp.Domain.Interfaces;
using SkyveApp.Domain.Steam;
using SkyveApp.Services;
using SkyveApp.Services.Interfaces;
using SkyveApp.Utilities;

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;

namespace SkyveApp.Domain;
public class Profile : IProfile
{
	private Bitmap? _banner;
	private byte[]? _bannerBytes;
	public static readonly Profile TemporaryProfile = new(Locale.TemporaryProfile) { Temporary = true, AutoSave = false };

	[CloneIgnore] public string Name { get; set; }
	[CloneIgnore] public bool Temporary { get; set; }
	[JsonIgnore] public DateTime LastEditDate { get; set; }
	[JsonIgnore] public DateTime DateCreated { get; set; }
	[JsonIgnore, CloneIgnore] public bool IsMissingItems { get; set; }
	[JsonIgnore] public int AssetCount => Assets.Count;
	[JsonIgnore] public int ModCount => Mods.Count;
	[JsonIgnore] public IEnumerable<IPackage> Packages => Assets.Concat(Mods);

	public Profile(string name) : this()
	{
		Name = name;
	}

	public Profile()
	{
		Name = string.Empty;
		LaunchSettings = new();
		LsmSettings = new();
		Assets = new();
		Mods = new();
		ExcludedDLCs = new();
		AutoSave = true;
	}

	public bool Save()
	{
		var profileManager = Program.Services.GetService<IProfileManager>();
	
		profileManager.GatherInformation(this);

		UnsavedChanges = false;

		return profileManager.Save(this);
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
	public bool IsFavorite { get; set; }
	public Color? Color { get; set; }
	public DateTime LastUsed { get; set; }
	public PackageUsage Usage { get; set; }
	public ulong Author { get; set; }
	public int ProfileId { get; set; }
	public bool UnsavedChanges { get; set; }
	public byte[]? BannerBytes { get => _bannerBytes; set { _bannerBytes = value; _banner?.Dispose(); _banner = null; } }
	public bool Public { get; set; }

	public bool ForAssetEditor { set { if (value) { Usage = PackageUsage.AssetCreation; } } }
	public bool ForGameplay { set { if (value) { Usage = PackageUsage.CityBuilding; } } }
	[JsonIgnore] public int Downloads { get; }
	[JsonIgnore] public Bitmap? Banner
	{
		get
		{
			if (_banner is not null) return _banner;
			
			if (BannerBytes is null || BannerBytes.Length == 0)
				return null;

			using var ms = new MemoryStream(BannerBytes);

			return _banner = new Bitmap(ms);
		}
	}

	public class Asset : IPackage
	{
		private string? _name;

		public string? Name { get => WorkshopInfo?.Name ?? _name ?? Path.GetFileNameWithoutExtension(RelativePath); set => _name = value; }
		public string? RelativePath { get; set; }
		public ulong SteamId { get; set; }

		[JsonIgnore, CloneIgnore] public bool IsMod { get; protected set; }
		[JsonIgnore, CloneIgnore] public bool Workshop => SteamId != 0;
		[JsonIgnore, CloneIgnore] public string Folder => RelativePath is null ? string.Empty : IsMod ? Program.Services.GetService<IProfileManager>().ToLocalPath(RelativePath) : Path.GetDirectoryName(Program.Services.GetService<IProfileManager>().ToLocalPath(RelativePath));
		[JsonIgnore, CloneIgnore] public Package? Package => Program.Services.GetService<IContentManager>().GetPackage(SteamId);
		[JsonIgnore, CloneIgnore] public SteamWorkshopItem? WorkshopInfo => SteamUtil.GetItem(SteamId);
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
		[JsonIgnore, CloneIgnore] public bool Incompatible => WorkshopInfo?.Incompatible ?? false;
		[JsonIgnore, CloneIgnore] public bool Banned => WorkshopInfo?.Banned ?? false;
		[JsonIgnore, CloneIgnore] public bool IsCollection => WorkshopInfo?.IsCollection ?? false;
		[JsonIgnore, CloneIgnore] public string[]? WorkshopTags => WorkshopInfo?.WorkshopTags;

		public Asset(Domain.Asset asset)
		{
			SteamId = asset.SteamId;
			Name = asset.Name;
			RelativePath = Program.Services.GetService<IProfileManager>().ToRelativePath(asset.FileName);
		}

		public Asset()
		{

		}

		public Asset(UserProfileContent content)
		{
			RelativePath = content.RelativePath;
			SteamId = content.SteamId;
		}

		public override string ToString()
		{
			return WorkshopInfo?.Title ?? Name ?? Locale.UnknownPackage;
		}

		public virtual UserProfileContent AsProfileContent()
		{
			return new UserProfileContent
			{
				RelativePath = RelativePath,
				SteamId = SteamId,
			};
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
			RelativePath = Program.Services.GetService<IProfileManager>().ToRelativePath(mod.Folder);
		}

		public Mod(IPackage package)
		{
			IsMod = true;
			SteamId = package.SteamId;
			Name = package.Name;
			Enabled = true;
			RelativePath = CrossIO.Combine(ProfileManager.WS_CONTENT_PATH, package.SteamId.ToString());
		}

		public Mod()
		{
			IsMod = true;
		}

		public Mod(UserProfileContent content)
		{
			IsMod = true;
			Enabled = content.Enabled;
			RelativePath = content.RelativePath;
			SteamId = content.SteamId;
		}

		public override UserProfileContent AsProfileContent()
		{
			return new UserProfileContent
			{
				Enabled = Enabled,
				IsMod = true,
				RelativePath = RelativePath,
				SteamId = SteamId,
			};
		}
	}
}
