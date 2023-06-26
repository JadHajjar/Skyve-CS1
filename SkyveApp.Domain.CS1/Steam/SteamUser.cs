using Extensions;

using Newtonsoft.Json;

using SkyveApp.Domain.Systems;
using SkyveApp.Services;
using SkyveApp.Systems;
using SkyveApp.Utilities;

using System;
using System.Drawing;

namespace SkyveApp.Domain.Steam;
public class SteamUser : IUser, ITimestamped
{
	public SteamUser(SteamUserEntry entry)
	{
		SteamId = ulong.Parse(entry.steamid);
		Name = entry.personaname;
		ProfileUrl = entry.profileurl;
		AvatarUrl = entry.avatarfull;
		Timestamp = DateTime.Now;
	}

	public SteamUser() 
	{
		Name = string.Empty;
		ProfileUrl = string.Empty;
		AvatarUrl = string.Empty;
	}

	public ulong SteamId { get; set; }
	public string Name { get; set; }
	public string ProfileUrl { get; set; }
	public string AvatarUrl { get; set; }
	public DateTime Timestamp { get; set; }

	[JsonIgnore] public Bitmap? AvatarImage => Program.Services.GetService<IImageService>().GetImage(AvatarUrl, true).Result;

	public override string ToString()
	{
		return Name ?? Locale.UnknownUser;
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
