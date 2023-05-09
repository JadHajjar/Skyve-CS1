using System;

namespace LoadOrderToolTwo.Domain.Compatibility;

[Flags]
public enum PackageUsage
{
	CityBuilding = 1 << 0,
	AssetCreation = 1 << 1,
	MapCreation = 1 << 2,
}
