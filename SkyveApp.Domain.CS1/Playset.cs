using Extensions;

using Newtonsoft.Json;

using SkyveApp.Domain.Compatibility.Enums;
using SkyveApp.Domain.Enums;
using SkyveApp.Domain.Interfaces;
using SkyveApp.Domain.Steam;
using SkyveApp.Domain.Systems;
using SkyveApp.Services;
using SkyveApp.Systems;
using SkyveApp.Systems.Compatibility.Domain.Api;
using SkyveApp.Utilities;

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;

namespace SkyveApp.Domain;
public class Playset : IPlayset
{
	private Bitmap? _banner;
	private byte[]? _bannerBytes;
	public static readonly Playset TemporaryPlayset = new(Locale.TemporaryPlayset) { Temporary = true, AutoSave = false };

	[CloneIgnore] public string Name { get; set; }
	[CloneIgnore] public bool Temporary { get; set; }
	[JsonIgnore] public DateTime LastEditDate { get; set; }
	[JsonIgnore] public DateTime DateCreated { get; set; }
	[JsonIgnore, CloneIgnore] public bool IsMissingItems { get; set; }
	[JsonIgnore] public int AssetCount => Assets.Count;
	[JsonIgnore] public int ModCount => Mods.Count;
	[JsonIgnore] public IEnumerable<IPackage> Packages => Assets.Concat(Mods);

	public Playset(string name) : this()
	{
		Name = name;
	}

	public Playset()
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
		var playsetManager = Program.Services.GetService<IPlaysetManager>();

		playsetManager.GatherInformation(this);

		UnsavedChanges = false;

		return playsetManager.Save(this);
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

	IUser? IPlayset.Author => SteamUtil.GetUser(Author);
	string? IPlayset.BannerUrl { get; }
	DateTime IPlayset.DateUpdated { get; }
	DateTime IPlayset.DateUsed { get; }

	public class Asset : IPackage, ILocalPackageIdentity
	{
		private string? _name;

		public string Name { get => ToString(); set => _name = value; }
		public string? RelativePath { get; set; }
		public ulong SteamId { get; set; }

		[JsonIgnore] public bool IsMod { get; set; }
		[JsonIgnore, CloneIgnore] public bool IsLocal => SteamId == 0;
		[JsonIgnore, CloneIgnore] public bool IsBuiltIn { get; }
		[JsonIgnore, CloneIgnore] public virtual ILocalPackage? LocalPackage => Program.Services.GetService<IPlaysetManager>().GetAsset(this);
		[JsonIgnore, CloneIgnore] public IEnumerable<IPackageRequirement> Requirements => LocalPackage?.Requirements ?? Enumerable.Empty<IPackageRequirement>();
		[JsonIgnore, CloneIgnore] public IEnumerable<ITag> Tags => LocalPackage?.Tags ?? Enumerable.Empty<ITag>();
		[JsonIgnore, CloneIgnore] public ulong Id => SteamId;
		[JsonIgnore, CloneIgnore] public string? Url => SteamId == 0 ? null : $"https://steamcommunity.com/workshop/filedetails/?id={Id}";
		[JsonIgnore, CloneIgnore] public string FilePath => RelativePath!;

		public Asset(Domain.Asset asset)
		{
			SteamId = asset.Id;
			Name = asset.Name;
			RelativePath = Program.Services.GetService<ILocationManager>().ToRelativePath(asset.FilePath);
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
			var name = GetWorkshopInfo()?.Title;

			if (name is not null)
				return name;

			if (_name is not null)
				return _name;

			if (!string.IsNullOrEmpty(RelativePath))
				return Path.GetFileNameWithoutExtension(RelativePath);

			return Locale.UnknownPackage;
		}

		public virtual UserProfileContent AsProfileContent()
		{
			return new UserProfileContent
			{
				RelativePath = RelativePath,
				SteamId = SteamId,
			};
		}

		public IWorkshopInfo? GetWorkshopInfo()
		{
			return SteamUtil.GetItem(SteamId);
		}
	}

	public class Mod : Asset
	{
		public bool Enabled { get; set; }
		[JsonIgnore, CloneIgnore] public override ILocalPackage? LocalPackage => Program.Services.GetService<IPlaysetManager>().GetMod(this);

		public Mod(Domain.Mod mod)
		{
			IsMod = true;
			SteamId = mod.Id;
			Name = mod.Name;
			Enabled = Program.Services.GetService<IModUtil>().IsEnabled(mod);
			RelativePath = Program.Services.GetService<ILocationManager>().ToRelativePath(mod.Folder);
		}

		public Mod(IPackage package)
		{
			IsMod = true;
			SteamId = package.Id;
			Name = package.Name;
			Enabled = true;
			RelativePath = CrossIO.Combine(ProfileManager.WS_CONTENT_PATH, package.Id.ToString());
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
