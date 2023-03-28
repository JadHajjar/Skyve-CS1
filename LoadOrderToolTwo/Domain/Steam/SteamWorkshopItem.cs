using LoadOrderToolTwo.Domain.Enums;
using LoadOrderToolTwo.Domain.Interfaces;
using LoadOrderToolTwo.Utilities.Managers;

using Newtonsoft.Json;

using System;
using System.Drawing;
using System.Linq;

namespace LoadOrderToolTwo.Domain.Steam;
public class SteamWorkshopItem : IGenericPackage
{
	private static readonly DateTime _epoch = new(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);

	public SteamUser? Author { get; set; }
	public string? Title { get; set; }
	public string? PublishedFileID { get; set; }
	public long Size { get; set; }
	public string? PreviewURL { get; set; }
	public string? AuthorID { get; set; }
	public string? Description { get; set; }
	public DateTime UpdatedUTC { get; set; }
	public string[]? Tags { get; set; }
	public string? Class { get; set; }
	public bool Removed { get; set; }
	public bool Private { get; set; }

	[JsonIgnore] public string? Name => Title;
	[JsonIgnore] public bool IsMod => Tags?.Contains("Mod") ?? false;
	[JsonIgnore] public ulong SteamId => ulong.TryParse(PublishedFileID, out var id) ? id : 0;
	[JsonIgnore] public Bitmap? Thumbnail => ImageManager.GetImage(PreviewURL, true).Result;
	[JsonIgnore] public string? ThumbnailUrl => PreviewURL;

	public SteamWorkshopItem(SteamWorkshopItemEntry entry)
	{
		Removed = entry.result is not 1 and not 9;
		Private = entry.result is 9;
		Title = entry.title ?? string.Empty;
		PublishedFileID = entry.publishedfileid;
		Size = entry.file_size;
		PreviewURL = entry.preview_url ?? string.Empty;
		AuthorID = entry.creator ?? string.Empty;
		Description = entry.description ?? string.Empty;
		UpdatedUTC = _epoch.AddSeconds((ulong)entry.time_updated);
		Tags = (entry.tags
			?.Select(item => item.tag)
			?.Where(item => item.IndexOf("compatible", StringComparison.OrdinalIgnoreCase) < 0)
			?.ToArray()) ?? new string[0];

		if (Tags.Any(tag => tag.ToLower() == "mod"))
		{
			Class = "Mod";
		}
		else if (Tags.Any())
		{
			Class = "Asset";
		}
		else
		{
			Class = "subscribed item";
		}
	}

	public SteamWorkshopItem()
	{

	}
}
