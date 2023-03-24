using Extensions;

using LoadOrderToolTwo.Domain.Utilities;
using LoadOrderToolTwo.Utilities;
using LoadOrderToolTwo.Utilities.Managers;

using Newtonsoft.Json;

using System;
using System.Collections.Generic;

namespace LoadOrderToolTwo.Domain;
public class Profile
{
	public static readonly Profile TemporaryProfile = new(Locale.TemporaryProfile) { Temporary = true, AutoSave = false };

	[JsonIgnore, CloneIgnore] public bool Temporary { get; private set; }
	[JsonIgnore, CloneIgnore] public bool IsMissingItems { get; set; }
	[CloneIgnore] public string? Name { get; set; }

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
		AutoSave = true;
	}

	public bool Save()
	{
		ProfileManager.GatherInformation(this);
		
		return ProfileManager.Save(this);
	}

	[CloneIgnore] public List<Asset> Assets { get; set; }
	[CloneIgnore] public List<Mod> Mods { get; set; }
	public LaunchSettings LaunchSettings { get; set; }
	public LsmSettings LsmSettings { get; internal set; }
	public string? LsmSkipFile { get; set; }
	public bool AutoSave { get; set; }
	public DateTime LastEditDate { get; set; }
	public bool ForAssetEditor { get; set; }
	public bool ForGameplay { get; set; }

	public class Asset
	{
		public string? Name { get; set; }
		public string? RelativePath { get; set; }
        public ulong SteamId { get; set; }

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
			SteamId = mod.SteamId;
			Name = mod.Name;
			Enabled = mod.IsEnabled;
			RelativePath = ProfileManager.ToRelativePath(mod.Folder);
		}

        public Mod()
        {
            
        }
    }
}
