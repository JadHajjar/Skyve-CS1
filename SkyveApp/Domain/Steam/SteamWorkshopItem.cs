using Extensions;

using SkyveApp.Domain.Enums;
using SkyveApp.Domain.Interfaces;
using SkyveApp.Utilities;
using SkyveApp.Utilities.Managers;

using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace SkyveApp.Domain.Steam;
public class SteamWorkshopItem : IPackage, ITimestamped
{
	private static readonly DateTime _epoch = new(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);

	public DateTime Timestamp { get; set; }
	public string? Title { get; set; }
	public string? PublishedFileID { get; set; }
	public long ServerSize { get; set; }
	public string? PreviewURL { get; set; }
	public ulong AuthorID { get; set; }
	public string? SteamDescription { get; set; }
	public DateTime CreatedUTC { get; set; }
	public DateTime ServerTime { get; set; }
	public string[]? WorkshopTags { get; set; }
	public bool RemovedFromSteam { get; set; }
	public SteamVisibility Visibility { get; set; }
	public bool Incompatible { get; set; }
	public int Reports { get; set; }
	public bool IsCollection { get; set; }
	public ulong[]? RequiredPackages { get; set; }
	public int PositiveVotes { get; set; }
	public bool Banned { get; set; }
	public int NegativeVotes { get; set; }
	public int Subscriptions { get; set; }

	[JsonIgnore] public SteamUser? Author => AuthorID == 0 ? null : SteamUtil.GetUser(AuthorID);
	[JsonIgnore] public string? Name => Title;
	[JsonIgnore] public bool IsMod => WorkshopTags?.Contains("Mod") ?? false;
	[JsonIgnore] public ulong SteamId => ulong.TryParse(PublishedFileID, out var id) ? id : 0;
	[JsonIgnore] public string? IconUrl => PreviewURL;
	[JsonIgnore] public Bitmap? IconImage => ImageManager.GetImage(IconUrl, true).Result;
	[JsonIgnore] public Bitmap? AuthorIconImage => ImageManager.GetImage(Author?.AvatarUrl, true).Result;
	[JsonIgnore] public bool IsIncluded { get => Package?.IsIncluded ?? false; set { if (Package is not null) { Package.IsIncluded = value; } } }
	[JsonIgnore] public bool Workshop => true;
	[JsonIgnore] public Package? Package => SteamId == 0 ? null : CentralManager.Packages.FirstOrDefault(x => x.SteamId == SteamId);
	[JsonIgnore] public long FileSize => ServerSize;
	[JsonIgnore] public string Folder => Package?.Folder ?? string.Empty;
	[JsonIgnore] public IEnumerable<TagItem> Tags
	{
		get
		{
			if (WorkshopTags is not null)
			{
				foreach (var item in WorkshopTags)
				{
					yield return new TagItem(TagSource.Workshop, item);
				}
			}

			var crTags = this.GetCompatibilityInfo().Data?.Package.Tags;

			if (crTags is not null)
			{
				foreach (var item in crTags)
				{
					yield return new TagItem(TagSource.Global, item);
				}
			}

			var findItTags =AssetsUtil.GetFindItTags(this);

			foreach (var item in findItTags)
			{
				yield return new TagItem(TagSource.FindIt, item);
			}
		}
	}

	public SteamWorkshopItem(SteamWorkshopItemEntry entry)
	{
		Timestamp = DateTime.Now;
		RemovedFromSteam = entry.result is not 1 and not 17;
		Visibility = entry.visibility;
		Title = entry.title;
		PublishedFileID = entry.publishedfileid;
		ServerSize = entry.file_size;
		PreviewURL = entry.preview_url;
		AuthorID = ulong.TryParse(entry.creator, out var id) ? id : 0;
		SteamDescription = entry.file_description;
		ServerTime = _epoch.AddSeconds(entry.time_updated);
		CreatedUTC = _epoch.AddSeconds(entry.time_created);
		NegativeVotes = entry.vote_data?.votes_down ?? 0;
		PositiveVotes = entry.vote_data?.votes_up ?? 0;
		Banned = entry.banned || entry.result is 17;
		Incompatible = entry.incompatible;
		Subscriptions = entry.subscriptions;
		Reports = entry.num_reports;
		IsCollection = entry.file_type == 2;
		RequiredPackages = entry.children?.Where(x => x.file_type == 0 && ulong.TryParse(x.publishedfileid, out _)).Select(x => ulong.Parse(x.publishedfileid)).ToArray();
		WorkshopTags = (entry.tags
			?.Select(item => item.tag)
			?.Where(item => item.IndexOf("compatible", StringComparison.OrdinalIgnoreCase) == -1)
			?.ToArray()) ?? new string[0];
	}

	public SteamWorkshopItem()
	{

	}

	public override string ToString()
	{
		return Name ?? string.Empty;
	}
}
