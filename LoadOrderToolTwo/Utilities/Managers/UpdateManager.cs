using Extensions;

using LoadOrderToolTwo.Domain;
using LoadOrderToolTwo.Domain.Interfaces;

using System;
using System.Collections.Generic;
using System.Linq;

namespace LoadOrderToolTwo.Utilities.Managers;
internal class UpdateManager
{
	private static readonly Dictionary<string, DateTime> _previousPackages = new(new PathEqualityComparer());

	static UpdateManager()
	{
		ISave.Load(out List<KnownPackage> packages, "KnownPackages.json");

		if (packages != null)
		{
			foreach (var package in packages)
			{
				if (package.Folder is not null or "")
				{
					_previousPackages[package.Folder] = package.UpdateTime;
				}
			}
		}

		CentralManager.WorkshopInfoUpdated += CentralManager_WorkshopInfoUpdated;
	}

	public static bool IsPackageKnown(IPackage package)
	{
		return _previousPackages.ContainsKey(package.Folder);
	}

	public static DateTime GetLastUpdateTime(IPackage package)
	{
		return _previousPackages.TryGet(package.Folder);
	}

	private static void CentralManager_WorkshopInfoUpdated()
	{
		ISave.Save(CentralManager.Packages.Select(x => new KnownPackage(x)), "KnownPackages.json");
	}

	internal static bool IsFirstTime()
	{
		return _previousPackages.Count == 0;
	}
}
