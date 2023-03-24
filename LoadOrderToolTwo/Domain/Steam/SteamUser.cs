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
}
