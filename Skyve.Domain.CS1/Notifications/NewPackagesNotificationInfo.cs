using Skyve.Domain.Systems;
using Skyve.Systems;

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace Skyve.Domain.CS1.Notifications;
public class NewPackagesNotificationInfo : INotificationInfo
{
	public NewPackagesNotificationInfo(List<ILocalPackageWithContents> newPackages)
	{
		_packages = newPackages;
		Time = newPackages.Max(x => x.LocalTime);
		Title = Locale.NewPackages;
		Description = Locale.NewPackagesSinceSession.FormatPlural(newPackages.Count, newPackages[0].CleanName());
		Icon = "I_New";
		HasAction = true;
	}

	private readonly List<ILocalPackageWithContents> _packages;

	public DateTime Time { get; }
	public string Title { get; }
	public string? Description { get; }
	public string Icon { get; }
	public Color? Color { get; }
	public bool HasAction { get; }

	public void OnClick()
	{
		ServiceCenter.Get<IInterfaceService>().ViewSpecificPackages(_packages, Title);
	}

	public void OnRightClick()
	{
	}
}
