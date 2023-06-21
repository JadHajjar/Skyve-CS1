using Extensions;

using SkyveApp.Domain;
using SkyveApp.Domain.Interfaces;
using SkyveApp.Services.Interfaces;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SkyveApp.Services;
internal class UpdateManager : IUpdateManager
{
    private readonly Dictionary<string, DateTime> _previousPackages = new(new PathEqualityComparer());

    private readonly IContentManager _contentManager;

	UpdateManager(IContentManager contentManager)
	{
		_contentManager = contentManager;

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

		_contentManager.ContentLoaded += CentralManager_WorkshopInfoUpdated;
		_contentManager.WorkshopInfoUpdated += CentralManager_WorkshopInfoUpdated;
	}

	public bool IsPackageKnown(IPackage package)
    {
        return _previousPackages.ContainsKey(package.Folder);
    }

    public DateTime GetLastUpdateTime(IPackage package)
    {
        return _previousPackages.TryGet(package.Folder);
    }

    private void CentralManager_WorkshopInfoUpdated()
    {
        ISave.Save(_contentManager.Packages.Select(x => new KnownPackage(x)), "KnownPackages.json");
    }

    public bool IsFirstTime()
    {
        return _previousPackages.Count == 0;
    }
}
