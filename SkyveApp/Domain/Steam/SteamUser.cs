using Extensions;

using SkyveApp.Domain.Compatibility;
using SkyveApp.Utilities.Managers;
using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.Drawing;

namespace SkyveApp.Domain.Steam;
public class SteamUser : ITimestamped
{
	public SteamUser(SteamUserEntry entry)
	{
		SteamId = ulong.Parse(entry.steamid);
		Name = entry.personaname;
		ProfileUrl = entry.profileurl;
		AvatarUrl = entry.avatarmedium;
		Timestamp = DateTime.Now;
	}

#nullable disable
	public SteamUser() { }
#nullable enable

	public ulong SteamId { get; set; }
	public string Name { get; set; }
	public string ProfileUrl { get; set; }
	public string AvatarUrl { get; set; }
	public DateTime Timestamp { get; set; }

	[JsonIgnore] public Bitmap? AvatarImage => ImageManager.GetImage(AvatarUrl, true).Result;

	public override string ToString()
	{
		return Name;
	}

	public override bool Equals(object? obj)
	{
		return obj is SteamUser user &&
			   SteamId == user.SteamId;
	}

	public override int GetHashCode()
	{
		return 2139390487 + SteamId.GetHashCode();
	}
}
