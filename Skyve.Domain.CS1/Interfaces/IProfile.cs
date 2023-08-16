using Skyve.Domain.Enums;

using System;
using System.Collections.Generic;
using System.Drawing;

namespace Skyve.Domain.CS1.Interfaces;
public interface IProfile_
{
	ulong Author { get; }
	Bitmap? Banner { get; }
	Color? Color { get; set; }
	bool IsFavorite { get; set; }
	bool IsMissingItems { get; }
	DateTime LastEditDate { get; }
	DateTime LastUsed { get; }
	string? Name { get; }
	PackageUsage Usage { get; }
	DateTime DateCreated { get; }
	bool Temporary { get; }
	int AssetCount { get; }
	int ModCount { get; }
	IEnumerable<IPackage> Packages { get; }
	int ProfileId { get; }
	int Downloads { get; }
	bool Public { get; }
}
