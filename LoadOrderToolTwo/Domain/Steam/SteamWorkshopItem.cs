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
	public DateTime CreatedUTC { get; set; }
	public DateTime UpdatedUTC { get; set; }
	public string[]? Tags { get; set; }
	public bool Removed { get; set; }
	public SteamVisibility Visibility { get; set; }
	public bool Incompatible { get; set; }
	public int Reports { get; set; }
	public ulong[]? Children { get; set; }
    public int PositiveVotes { get; set; }
	public bool Banned { get; set; }
	public int NegativeVotes { get; set; }
	public int Subscriptions { get; set; }

	[JsonIgnore] public string? Name => Title;
	[JsonIgnore] public bool IsMod => Tags?.Contains("Mod") ?? false;
	[JsonIgnore] public ulong SteamId => ulong.TryParse(PublishedFileID, out var id) ? id : 0;
	[JsonIgnore] public Bitmap? Thumbnail => ImageManager.GetImage(PreviewURL, true).Result;
	[JsonIgnore] public string? ThumbnailUrl => PreviewURL;

	public SteamWorkshopItem(SteamWorkshopItemEntry entry)
	{
		Removed = entry.result is not 1 and not 9;
		Visibility = entry.visibility;
		Title = entry.title ?? string.Empty;
		PublishedFileID = entry.publishedfileid;
		Size = entry.file_size;
		PreviewURL = entry.preview_url ?? string.Empty;
		AuthorID = entry.creator ?? string.Empty;
		Description = entry.file_description ?? string.Empty;
		UpdatedUTC = _epoch.AddSeconds((ulong)entry.time_updated);
		CreatedUTC = _epoch.AddSeconds((ulong)entry.time_created);
		NegativeVotes = entry.vote_data?.votes_down ?? 0;
		PositiveVotes = entry.vote_data?.votes_up ?? 0;
		Banned = entry.banned;
		Incompatible = entry.incompatible;
		Subscriptions = entry.subscriptions;
		Reports = entry.num_reports;
		Children = entry.children?.Where(x => x.file_type == 0 && ulong.TryParse(x.publishedfileid, out _)).Select(x => ulong.Parse(x.publishedfileid)).ToArray() ?? new ulong[0];
		Tags = (entry.tags
			?.Select(item => item.tag)
			?.Where(item => item.IndexOf("compatible", StringComparison.OrdinalIgnoreCase) == -1)
			?.ToArray()) ?? new string[0];
	}

	public SteamWorkshopItem()
	{

	}
}
