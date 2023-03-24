#nullable disable

namespace LoadOrderToolTwo.Domain.Steam;

public class SteamUserRootResponse
{
	public SteamUserResponse response { get; set; }
}

public class SteamUserResponse
{
	public SteamUserEntry[] players { get; set; }
}

public class SteamUserEntry
{
	public string steamid { get; set; }
	public string personaname { get; set; }
	public string profileurl { get; set; }
	public string avatarmedium { get; set; }
}
#nullable enable