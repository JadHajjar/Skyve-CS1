using Extensions;

using Newtonsoft.Json;

using Skyve.Domain.Enums;
using Skyve.Domain.Systems;
using Skyve.Systems;
using Skyve.Systems.Compatibility.Domain.Api;

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;

namespace Skyve.Domain.CS1;
public class Playset : ICustomPlayset
{
	private Bitmap? _banner;
	private byte[]? _bannerBytes;
	public static readonly Playset TemporaryPlayset = new(ServiceCenter.Get<ILocale>().Get("TemporaryPlayset")) { Temporary = true, AutoSave = false };

	[CloneIgnore] public string? Name { get; set; }
	[CloneIgnore] public bool Temporary { get; set; }
	[JsonIgnore] public DateTime LastEditDate { get; set; }
	[JsonIgnore] public DateTime DateCreated { get; set; }
	[JsonIgnore, CloneIgnore] public bool IsMissingItems { get; set; }
	[JsonIgnore] public int AssetCount => Assets.Count;
	[JsonIgnore] public int ModCount => Mods.Count;
	[JsonIgnore] public IEnumerable<IPlaysetEntry> Entries => Assets.Concat(Mods);
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
		var playsetManager = ServiceCenter.Get<IPlaysetManager>();

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
	public byte[]? BannerBytes
	{
		get => _bannerBytes; set
		{
			_bannerBytes = value;
			_banner?.Dispose();
			_banner = null;
		}
	}
	public bool Public { get; set; }

	public bool ForAssetEditor
	{
		set
		{
			if (value)
			{
				Usage = PackageUsage.AssetCreation;
			}
		}
	}
	public bool ForGameplay
	{
		set
		{
			if (value)
			{
				Usage = PackageUsage.CityBuilding;
			}
		}
	}
	[JsonIgnore] public int Downloads { get; }
	[JsonIgnore]
	public Bitmap? Banner
	{
		get
		{
			if (_banner is not null)
			{
				return _banner;
			}

			if (BannerBytes is null || BannerBytes.Length == 0)
			{
				return null;
			}

			using var ms = new MemoryStream(BannerBytes);

			return _banner = new Bitmap(ms);
		}
		set
		{
			if (value is null)
			{
				BannerBytes = null;
			}
			else
			{
				BannerBytes = (byte[])new ImageConverter().ConvertTo(value, typeof(byte[]));
			}
		}
	}

	IUser? IPlayset.Author => ServiceCenter.Get<IWorkshopService>().GetUser(Author);
	string? IPlayset.BannerUrl { get; }
	DateTime IPlayset.DateUpdated => LastEditDate;
	DateTime IPlayset.DateUsed => LastUsed;
	bool ICustomPlayset.DisableWorkshop => LaunchSettings.NoWorkshop;

	public class Asset : IPackage, IPlaysetEntry
	{
		private string? _name;

		[JsonIgnore] public string Name { get => ToString(); protected set => _name = value; }
		public string? RelativePath { get; set; }
		public ulong SteamId { get; set; }

		[JsonIgnore] public bool IsMod { get; set; }
		[JsonIgnore, CloneIgnore] public bool IsLocal => SteamId == 0;
		[JsonIgnore, CloneIgnore] public bool IsBuiltIn { get; }
		[JsonIgnore, CloneIgnore] public virtual ILocalPackageWithContents? LocalParentPackage => ServiceCenter.Get<IPlaysetManager>().GetAsset(this)?.LocalParentPackage;
		[JsonIgnore, CloneIgnore] public virtual ILocalPackage? LocalPackage => ServiceCenter.Get<IPlaysetManager>().GetAsset(this);
		[JsonIgnore, CloneIgnore] public IEnumerable<IPackageRequirement> Requirements => LocalParentPackage?.Requirements ?? Enumerable.Empty<IPackageRequirement>();
		[JsonIgnore, CloneIgnore] public ulong Id => SteamId;
		[JsonIgnore, CloneIgnore] public string? Url => SteamId == 0 ? null : $"https://steamcommunity.com/workshop/filedetails/?id={Id}";
		[JsonIgnore, CloneIgnore] public string FilePath => RelativePath!;

		public Asset(IAsset asset)
		{
			SteamId = asset.Id;
			Name = asset.Name;
			RelativePath = ServiceCenter.Get<ILocationManager>().ToRelativePath(asset.FilePath);
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
			var name = this.GetWorkshopInfo()?.Name;

			return name is not null
				? name
				: _name is not null
				? _name
				: !string.IsNullOrEmpty(RelativePath)
				? Path.GetFileNameWithoutExtension(RelativePath)
				: (string)LocaleHelper.GetGlobalText("UnknownPackage");
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
		[JsonIgnore, CloneIgnore] public override ILocalPackageWithContents? LocalParentPackage => ServiceCenter.Get<IPlaysetManager>().GetMod(this)?.LocalParentPackage;
		[JsonIgnore, CloneIgnore] public override ILocalPackage? LocalPackage => ServiceCenter.Get<IPlaysetManager>().GetMod(this);

		public Mod(IMod mod)
		{
			IsMod = true;
			SteamId = mod.Id;
			Name = mod.Name;
			Enabled = ServiceCenter.Get<IModUtil>().IsEnabled(mod);
			RelativePath = ServiceCenter.Get<ILocationManager>().ToRelativePath(mod.Folder);
		}

		public Mod(IPackage package)
		{
			IsMod = true;
			SteamId = package.Id;
			Name = package.Name;
			Enabled = true;
			RelativePath = CrossIO.Combine("%WORKSHOP%", package.Id.ToString());
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
