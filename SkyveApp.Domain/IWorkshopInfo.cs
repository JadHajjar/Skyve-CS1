using System;
using System.Collections.Generic;

namespace SkyveApp.Domain;
public interface IWorkshopInfo
{
	IUser? Author { get; }
	string? IconUrl { get; }
	string? Description { get; }
	DateTime ServerTime { get; }
	long ServerSize { get; }
	int Subscriptions { get; }
	int Score { get; }
	int ScoreCount { get; }
	bool IsMod { get; }
	bool Removed { get; }
	bool Incompatible { get; }
	bool Banned { get; }
	string Title { get; }
	string[] Tags { get; }
	IEnumerable<IPackageRequirement> Requirements { get; }
	ulong Id { get; }
	string? Url { get; }
}
