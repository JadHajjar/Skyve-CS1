using Extensions;

using LoadOrderToolTwo.Domain.Enums;
using LoadOrderToolTwo.Domain.Interfaces;
using LoadOrderToolTwo.Domain.Steam;
using LoadOrderToolTwo.Utilities;
using LoadOrderToolTwo.Utilities.Managers;

using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.Drawing;

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

	public class Asset : IGenericPackage
	{
		public string? Name { get; set; }
		public string? RelativePath { get; set; }
		public ulong SteamId { get; set; }

		[JsonIgnore, CloneIgnore] public bool IsMod { get; protected set; }
		[JsonIgnore, CloneIgnore] public SteamWorkshopItem? WorkshopInfo { get; set; }
		[JsonIgnore, CloneIgnore] public string[]? Tags => WorkshopInfo?.Tags;
		[JsonIgnore, CloneIgnore] public Bitmap? Thumbnail => WorkshopInfo?.Thumbnail;
		[JsonIgnore, CloneIgnore] public string? ThumbnailUrl => WorkshopInfo?.ThumbnailUrl;
		[JsonIgnore, CloneIgnore] public SteamUser? Author => WorkshopInfo?.Author;

		public Asset(Domain.Asset asset)
		{
			SteamId = asset.SteamId;
			Name = asset.Name;
			RelativePath = ProfileManager.ToRelativePath(asset.FileName);
		}

		public Asset()
		{

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

		public Mod()
		{
			IsMod = true;
		}
	}
}
