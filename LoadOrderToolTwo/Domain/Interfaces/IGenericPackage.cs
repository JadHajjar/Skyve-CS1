using LoadOrderToolTwo.Domain.Enums;
using LoadOrderToolTwo.Domain.Steam;

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
