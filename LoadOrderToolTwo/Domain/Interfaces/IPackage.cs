using LoadOrderToolTwo.Domain.Enums;
using LoadOrderToolTwo.Domain.Steam;
using LoadOrderToolTwo.Utilities.Managers;

using System;
using System.Collections.Generic;
using System.Drawing;

namespace LoadOrderToolTwo.Domain.Interfaces;
public interface IPackage
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
	Package? Package { get; }
	long FileSize { get; }
	DateTime ServerTime { get; }
	CompatibilityManager.ReportInfo? CompatibilityReport { get; }
	SteamVisibility Visibility { get; }
	ulong[]? RequiredPackages { get; }
	string? IconUrl { get; }
	long ServerSize { get; }
	string? SteamDescription { get; }
	string[]? WorkshopTags { get; }
	string Folder { get; }
	bool RemovedFromSteam { get; }
}