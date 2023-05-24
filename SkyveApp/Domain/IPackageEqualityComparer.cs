using SkyveApp.Domain.Interfaces;

using System.Collections.Generic;

namespace SkyveApp.Domain;

public class IPackageEqualityComparer : IEqualityComparer<IPackage>
{
	public bool Equals(IPackage x, IPackage y)
	{
		if (x is null)
		{
			return y is null;
		}

		if (y is null)
		{
			return x is null;
		}

		if (x.SteamId != 0)
		{
			return x.SteamId == y.SteamId;
		}

		return x.Folder == y.Folder;
	}

	public int GetHashCode(IPackage obj)
	{
		return -1586376059 + obj.SteamId.GetHashCode() + obj.Folder.GetHashCode();
	}
}
