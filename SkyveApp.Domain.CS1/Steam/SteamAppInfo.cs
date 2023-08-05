namespace SkyveApp.Domain.CS1.Steam;

public class SteamAppInfo
{
	public bool success { get; set; }
	public SteamAppData? data { get; set; }
}

public class SteamAppData
{
	public string? name { get; set; }
	public string? short_description { get; set; }
	public uint[]? dlc { get; set; }
	public SteamAppReleaseDate? release_date { get; set; }
}

public class SteamAppReleaseDate
{
	public bool coming_soon { get; set; }
	public string? date { get; set; }
}