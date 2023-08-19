using Skyve.Domain.CS1.Steam;

using System;
using System.Collections.Generic;
using System.Drawing;

namespace Skyve.Domain.CS1.Interfaces;
public interface IPackage_
{
	string? Name { get; }
	bool IsMod { get; }
	IEnumerable<TagItem> Tags { get; }
	ulong SteamId { get; }
	Bitmap? IconImage { get; }
	Bitmap? AuthorIconImage { get; }
	SteamUser? Author { get; }
	int Subscriptions { get; }
	int PositiveVotes { get; }
	int NegativeVotes { get; }
	int Reports { get; }
	bool IsIncluded { get; set; }
	bool Workshop { get; }
	bool IsCollection { get; }
	Package? Package { get; }
	long FileSize { get; }
	DateTime ServerTime { get; }
	SteamVisibility Visibility { get; }
	ulong[]? RequiredPackages { get; }
	string? IconUrl { get; }
	long ServerSize { get; }
	string? SteamDescription { get; }
	string[]? WorkshopTags { get; }
	string Folder { get; }
	bool RemovedFromSteam { get; }
	bool Incompatible { get; }
	bool Banned { get; }
}