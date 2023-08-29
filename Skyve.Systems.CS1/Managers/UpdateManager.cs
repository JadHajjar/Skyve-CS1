using Extensions;

using Skyve.Domain;
using Skyve.Domain.CS1;
using Skyve.Domain.Systems;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Skyve.Systems.CS1.Managers;
internal class UpdateManager : IUpdateManager
{
	private readonly Dictionary<string, DateTime> _previousPackages = new(new PathEqualityComparer());
	private readonly INotifier _notifier;

	public UpdateManager(INotifier notifier)
	{
		_notifier = notifier;

		ISave.Load(out List<KnownPackage> packages, "KnownPackages.json");

		if (packages != null)
		{
			if (File.GetLastWriteTimeUtc(ISave.GetPath("KnownPackages.json")) < new DateTime(2023, 4, 15, 18, 0, 0, DateTimeKind.Utc))
			{
				CrossIO.DeleteFile(ISave.GetPath("KnownPackages.json"));
				packages.Clear();
			}

			foreach (var package in packages)
			{
				if (package.Folder is not null or "")
				{
					_previousPackages[package.Folder] = package.UpdateTime;
				}
			}
		}

		_notifier.ContentLoaded += CentralManager_WorkshopInfoUpdated;
		_notifier.WorkshopInfoUpdated += CentralManager_WorkshopInfoUpdated;
	}

	public bool IsPackageKnown(ILocalPackage package)
	{
		return _previousPackages.ContainsKey(package.Folder);
	}

	public DateTime GetLastUpdateTime(ILocalPackage package)
	{
		return _previousPackages.TryGet(package.Folder);
	}

	private void CentralManager_WorkshopInfoUpdated()
	{
		ISave.Save(ServiceCenter.Get<IPackageManager>().Packages.Select(x => new KnownPackage(x)), "KnownPackages.json");
	}

	public bool IsFirstTime()
	{
		return _previousPackages.Count == 0;
	}

	public IEnumerable<ILocalPackage> GetNewPackages()
	{
		return new List<ILocalPackage>();
	}
}
