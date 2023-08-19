using Extensions;

using Newtonsoft.Json;

using Skyve.Domain.Systems;
using Skyve.Systems;

using System;
using System.Collections.Generic;
using System.Linq;

namespace Skyve.Domain.CS1.Steam;
public class SteamWorkshopInfo : IWorkshopInfo, ITimestamped, IEquatable<SteamWorkshopInfo?>
{
	private static readonly DateTime _epoch = new(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);

	public string? ThumbnailUrl { get; set; }
	public ulong AuthorId { get; set; }
	public string? Description { get; set; }
	public DateTime ServerTime { get; set; }
	public long ServerSize { get; set; }
	public int Subscribers { get; set; }
	public bool IsCollection { get; set; }
	public int Score { get; set; }
	public int ScoreVoteCount { get; set; }
	public bool IsMod { get; set; }
	public ulong[]? RequiredPackageIds { get; set; }
	public bool IsRemoved { get; set; }
	public bool IsIncompatible { get; set; }
	public bool IsBanned { get; set; }
	public bool IsInvalid { get; set; }
	public string Name { get; set; }
	public string[] Tags { get; set; }
	public ulong Id { get; set; }
	public string? Url { get; set; }
	public DateTime Timestamp { get; set; }

	[JsonIgnore] public IEnumerable<IPackageRequirement> Requirements => RequiredPackageIds?.Select(x => new SteamPackageRequirement(ServiceCenter.Get<ICompatibilityManager>().GetFinalSuccessor(new GenericPackageIdentity(x)).Id, !IsMod)) ?? Enumerable.Empty<IPackageRequirement>();
	[JsonIgnore] public IUser? Author => ServiceCenter.Get<IWorkshopService>().GetUser(AuthorId) ?? (AuthorId > 0 ? new SteamUser { SteamId = AuthorId, Name = AuthorId.ToString() } : null);

	public SteamWorkshopInfo(SteamWorkshopItemEntry entry)
	{
		Timestamp = DateTime.Now;
		Name = entry.title;
		Id = ulong.TryParse(entry.publishedfileid, out var id) ? id : 0;
		ServerSize = entry.file_size;
		ThumbnailUrl = entry.preview_url;
		AuthorId = ulong.TryParse(entry.creator, out var aid) ? aid : 0;
		Description = entry.file_description;
		ServerTime = _epoch.AddSeconds(entry.time_updated);
		Score = CalculateScore(entry);
		ScoreVoteCount = (entry.vote_data?.votes_down ?? 0) + (entry.vote_data?.votes_up ?? 0);
		Subscribers = entry.subscriptions;
		IsRemoved = entry.result is not 1 and not 17;
		IsBanned = entry.banned || entry.result is 16 or 17;
		IsIncompatible = entry.incompatible;
		IsCollection = entry.file_type == 2;
		IsInvalid = entry.creator_appid != 255710;
		IsMod = entry.tags?.Any(x => x.display_name == "Mod") ?? false;
		Url = $"https://steamcommunity.com/workshop/filedetails/?id={Id}";

		RequiredPackageIds = entry.children is null ? new ulong[0] : entry.children.Where(x => x.file_type == 0 && ulong.TryParse(x.publishedfileid, out _)).Select(x => ulong.Parse(x.publishedfileid)).ToArray();
		Tags = (entry.tags
			?.Select(item => item.tag)
			?.Where(item => item.IndexOf("compatible", StringComparison.OrdinalIgnoreCase) == -1)
			?.ToArray()) ?? new string[0];
	}

	public SteamWorkshopInfo()
	{
		Name = string.Empty;
		Tags = new string[0];
	}

	public override string ToString()
	{
		return Name;
	}

	public static int CalculateScore(SteamWorkshopItemEntry entry)
	{
		var upvotes = entry.vote_data?.votes_up ?? 0;
		var downvotes = (entry.vote_data?.votes_down ?? 0) / 10 + entry.num_reports;

		if (upvotes + downvotes < 5)
		{
			return -1;
		}

		var subscribersFactor = -Math.Min(100000, entry.subscriptions) / 2000 - 10;
		var goal = 1.472 * (Math.Pow(subscribersFactor, 2) + Math.Pow(subscribersFactor, 3) / 100) - 120;

		if (!(entry.tags?.Any(x => x.display_name == "Mod") ?? false))
		{
			goal /= 3.5;
		}

		return ((int)(100 * (upvotes - downvotes) / goal)).Between(0, 100);
	}

	public override bool Equals(object? obj)
	{
		return Equals(obj as SteamWorkshopInfo);
	}

	public bool Equals(SteamWorkshopInfo? other)
	{
		return other is not null &&
			   Id == other.Id;
	}

	public override int GetHashCode()
	{
		return 2108858624 + Id.GetHashCode();
	}

	public static bool operator ==(SteamWorkshopInfo? left, SteamWorkshopInfo? right)
	{
		return left?.Equals(right) ?? right is null;
	}

	public static bool operator !=(SteamWorkshopInfo? left, SteamWorkshopInfo? right)
	{
		return !(left == right);
	}
}
