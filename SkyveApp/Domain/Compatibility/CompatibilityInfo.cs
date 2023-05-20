using Extensions;

using Newtonsoft.Json;

using SkyveApp.Domain.Interfaces;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace SkyveApp.Domain.Compatibility;
public class CompatibilityInfo
{
	[JsonIgnore] public IPackage Package { get; }
	[JsonIgnore] public IndexedPackage? Data { get; }
	public List<PackageLink> Links { get; }
	public List<ReportItem> ReportItems { get; }
	public NotificationType Notification => ReportItems.Count > 0 ? ReportItems.Max(x => x.Status.Notification) : NotificationType.None;

	public CompatibilityInfo(IPackage package, IndexedPackage? packageData)
	{
		Package = package;
		Data = packageData;
		Links = packageData?.Package.Links ?? new();
		ReportItems = new();
	}

	public void Add(ReportType type, IGenericPackageStatus status, string message, ulong[] packages)
	{
		ReportItems.Add(new ReportItem
		{
			Type = type,
			Status = status,
			Message = message,
			Packages = packages.Select(x => new PseudoPackage(x)).ToArray()
		});
	}

	public void Add(ReportType type, IGenericPackageStatus status, string message, PseudoPackage[] packages)
	{
		ReportItems.Add(new ReportItem
		{
			Type = type,
			Status = status,
			Message = message,
			Packages = packages
		});
	}
}
