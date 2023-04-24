using System.Collections.Generic;

namespace LoadOrderToolTwo.Domain.Steam;
public class SteamUser
{
	public SteamUser(SteamUserEntry entry)
	{
		SteamId = entry.steamid;
		Name = entry.personaname;
		ProfileUrl = entry.profileurl;
		AvatarUrl = entry.avatarmedium;
	}

	public SteamUser()
	{

	}

	public string SteamId { get; set; }
	public string Name { get; set; }
	public string ProfileUrl { get; set; }
	public string AvatarUrl { get; set; }

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
		return -80009682 + EqualityComparer<string>.Default.GetHashCode(SteamId);
	}
}
