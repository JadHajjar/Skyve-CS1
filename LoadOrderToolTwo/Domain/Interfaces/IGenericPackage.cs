using LoadOrderToolTwo.Domain.Steam;

using System.Drawing;

namespace LoadOrderToolTwo.Domain.Interfaces;
internal interface IGenericPackage
{
	string? Name { get; }
	bool IsMod { get; }
	string[]? Tags { get; }
	ulong SteamId { get; }
	Bitmap? Thumbnail { get; }
	string? ThumbnailUrl { get; }
	SteamUser? Author { get; }
}
