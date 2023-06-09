﻿using SkyveApp.Domain.Compatibility.Enums;

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkyveApp.Domain.Interfaces;
public interface IProfile
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
