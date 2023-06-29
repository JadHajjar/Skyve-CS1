using System;
using System.Collections.Generic;

namespace SkyveApp.Domain;
public interface IWorkshopInfo
{
	IUser? Author { get; }
	string? ThumbnailUrl { get; }
	string? Description { get; }
	DateTime ServerTime { get; }
	long ServerSize { get; }
	int Subscribers { get; }
	int Score { get; }
	int ScoreVoteCount { get; }
	bool IsMod { get; }
	bool IsRemoved { get; }
	bool IsIncompatible { get; }
	bool IsBanned { get; }
	string Title { get; }
	string[] Tags { get; }
	IEnumerable<IPackageRequirement> Requirements { get; }
	ulong Id { get; }
	string? Url { get; }
}
