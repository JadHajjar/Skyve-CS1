#nullable disable

namespace LoadOrderToolTwo.Domain.Steam;

public class SteamWorkshopItemRootResponse
{
	public SteamWorkshopItemResponse response { get; set; }
}

public class SteamWorkshopItemResponse
{
	public int result { get; set; }
	public int resultcount { get; set; }
	public SteamWorkshopItemEntry[] publishedfiledetails { get; set; }
}

public class SteamWorkshopItemEntry
{
	public string publishedfileid { get; set; }
	public int result { get; set; }
	public string creator { get; set; }
	public int creator_app_id { get; set; }
	public int consumer_app_id { get; set; }
	public string filename { get; set; }
	public long file_size { get; set; }
	public string file_url { get; set; }
	public string hcontent_file { get; set; }
	public string preview_url { get; set; }
	public string hcontent_preview { get; set; }
	public string title { get; set; }
	public string description { get; set; }
	public int time_created { get; set; }
	public int time_updated { get; set; }
	public int visibility { get; set; }
	public int banned { get; set; }
	public string ban_reason { get; set; }
	public int subscriptions { get; set; }
	public int favorited { get; set; }
	public int lifetime_subscriptions { get; set; }
	public int lifetime_favorited { get; set; }
	public int views { get; set; }
	public SteamWorkshopItemTag[] tags { get; set; }
}

public class SteamWorkshopItemTag
{
	public string tag { get; set; }
}
#nullable enable