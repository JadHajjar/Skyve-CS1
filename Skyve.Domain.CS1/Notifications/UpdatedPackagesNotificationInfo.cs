using Skyve.Domain.Enums;
using Skyve.Systems;

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Skyve.Domain.CS1.Notifications;
public class UpdatedPackagesNotificationInfo : INotificationInfo
{
	public UpdatedPackagesNotificationInfo(List<ILocalPackageWithContents> updatedPackages)
	{
		Time = updatedPackages.Max(x => x.LocalTime);
		Title = Locale.PackageUpdates;
		Description = Locale.PackagesUpdatedSinceSession.FormatPlural(updatedPackages.Count, updatedPackages[0].CleanName());
		Icon = "I_ReDownload";
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
