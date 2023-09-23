using Extensions;

using Skyve.Domain;
using Skyve.Domain.CS1;
using Skyve.Domain.CS1.Notifications;
using Skyve.Domain.Systems;

using System;
using System.Collections.Generic;
using System.Linq;

namespace Skyve.Systems.CS1.Managers;
internal class UpdateManager : IUpdateManager
{
	private readonly Dictionary<string, DateTime> _previousPackages = new(new PathEqualityComparer());
	private readonly INotificationsService _notificationsService;
	private readonly IPackageManager _packageManager;

	public UpdateManager(INotificationsService notificationsService, IPackageManager packageManager)
	{
		_notificationsService = notificationsService;
		_packageManager = packageManager;

		try
		{
			ISave.Load(out List<KnownPackage> packages, "LastPackages.json");

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
		}
		catch { }
	}

	public void SendUpdateNotifications()
	{
		if (IsFirstTime())
		{
			return;
		}

		var newPackages = new List<ILocalPackageWithContents>();
		var updatedPackages = new List<ILocalPackageWithContents>();

		foreach (var package in _packageManager.Packages)
		{
			var date = _previousPackages.TryGet(package.Folder);

			if (date == default)
			{
				newPackages.Add(package);
			}
			else if (package.LocalTime > date)
			{
				updatedPackages.Add(package);
			}
		}

		if (newPackages.Count > 0)
		{
			_notificationsService.SendNotification(new NewPackagesNotificationInfo(newPackages));
		}

		if (updatedPackages.Count > 0)
		{
			_notificationsService.SendNotification(new UpdatedPackagesNotificationInfo(updatedPackages));
		}

		ISave.Save(ServiceCenter.Get<IPackageManager>().Packages.Select(x => new KnownPackage(x)), "LastPackages.json");
	}

	public bool IsPackageKnown(ILocalPackage package)
	{
		return _previousPackages.ContainsKey(package.Folder);
	}

	public DateTime GetLastUpdateTime(ILocalPackage package)
	{
		return _previousPackages.TryGet(package.Folder);
	}

	public bool IsFirstTime()
	{
		return _previousPackages.Count == 0;
	}

	public IEnumerable<ILocalPackageWithContents> GetNewOrUpdatedPackages()
	{
		if (IsFirstTime())
		{
			yield break;
		}

		foreach (var package in _packageManager.Packages)
		{
			var date = _previousPackages.TryGet(package.Folder);

			if (package.LocalTime > date)
			{
				yield return package;
			}
		}
	}
}
