using Extensions;

using Newtonsoft.Json;
using SkyveApp.Domain.Compatibility.Api;
using SkyveApp.Domain.Compatibility.Enums;
using SkyveApp.Domain.Interfaces;
using SkyveApp.Domain.Steam;
using SkyveApp.Services;
using SkyveApp.Services.Interfaces;

using System.Collections.Generic;
using System.Linq;

namespace SkyveApp.Domain.Compatibility;
public class CompatibilityInfo
{
	public SteamWorkshopItem SteamItem { get => Package is SteamWorkshopItem s ? s : Package.Package?.WorkshopInfo; set => Package = value; }
	[JsonIgnore] public IPackage Package { get; set; }
	[JsonIgnore] public IndexedPackage? Data { get; }
	public List<PackageLink> Links { get; set; }
	public List<ReportItem> ReportItems { get; set; }
	[JsonIgnore] public NotificationType Notification => ReportItems.Count > 0 ? ReportItems.Max(x => Program.Services.GetService<ICompatibilityManager>().IsSnoozed(x) ? 0 : x.Status.Notification) : NotificationType.None;

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
			PackageId = Data?.Package.SteamId ?? 0,
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
			PackageId = Data?.Package.SteamId ?? 0,
			Type = type,
			Status = status,
			Message = message,
			Packages = packages
		});
	}
}
