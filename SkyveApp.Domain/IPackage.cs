using System;
using System.Collections.Generic;

namespace SkyveApp.Domain;
public interface IPackage : IPackageIdentity
{
	ulong SteamId { get; }
	IUser? Author { get; }
	string? IconUrl { get; }
	string? Description { get; }
	DateTime ServerTime { get; }
	long ServerSize { get; }
	bool IsMod { get; }
	int Subscriptions { get; }
	int Score { get; }
	int ScoreCount { get; }
	bool Removed { get; }
	bool Incompatible { get; }
	bool Banned { get; }
	ILocalPackage? LocalPackage { get; }
	IEnumerable<IPackageIdentity> Requirements { get; }
	IEnumerable<ITag> Tags { get; }
}