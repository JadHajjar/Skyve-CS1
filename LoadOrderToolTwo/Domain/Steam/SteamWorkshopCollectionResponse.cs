#nullable disable

namespace LoadOrderToolTwo.Domain.Steam;

public class SteamWorkshopCollectionRootResponse
{
	public SteamWorkshopCollectionResponse response { get; set; }
}

public class SteamWorkshopCollectionResponse
{
	public int result { get; set; }
	public int resultcount { get; set; }
	public SteamWorkshopCollectionDetails[] collectiondetails { get; set; }
}

public class SteamWorkshopCollectionDetails
{
	public string publishedfileid { get; set; }
	public int result { get; set; }
	public SteamWorkshopCollectionItem[] children { get; set; }
}

public class SteamWorkshopCollectionItem
{
	public string publishedfileid { get; set; }
	public int sortorder { get; set; }
	public int filetype { get; set; }
}
#nullable enable