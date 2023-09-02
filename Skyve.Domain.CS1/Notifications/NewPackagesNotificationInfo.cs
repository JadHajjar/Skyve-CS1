using Skyve.Systems;

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Skyve.Domain.CS1.Notifications;
public class NewPackagesNotificationInfo : INotificationInfo
{
	public NewPackagesNotificationInfo(List<ILocalPackageWithContents> newPackages)
	{
		Time = newPackages.Max(x => x.LocalTime);
		Title = Locale.NewPackages;
		Description = Locale.NewPackagesSinceSession.FormatPlural(newPackages.Count, newPackages[0].CleanName());
		Icon = "I_New";
	}

	public DateTime Time { get; }
	public string Title { get; }
	public string? Description { get; }
	public string Icon { get; }
	public Color? Color { get; }
	public bool HasAction { get; }

	public void OnClick()
	{
	}

	public void OnRightClick()
	{
	}
}
