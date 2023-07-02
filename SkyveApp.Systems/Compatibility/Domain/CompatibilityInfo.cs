using Extensions;

using Newtonsoft.Json;

using SkyveApp.Domain;
using SkyveApp.Domain.Enums;
using SkyveApp.Domain.Systems;
using SkyveApp.Systems.Compatibility.Domain.Api;

using System.Collections.Generic;
using System.Linq;

namespace SkyveApp.Systems.Compatibility.Domain;
public class CompatibilityInfo : ICompatibilityInfo
{
	public GenericWorkshopPackage SteamItem { get => Package is GenericWorkshopPackage s ? s : new GenericWorkshopPackage(Package.Id); set => Package = value; }
	[JsonIgnore] public IPackage Package { get; set; }
	[JsonIgnore] public IndexedPackage? Data { get; }
	public List<PackageLink> Links { get; set; }
	public List<ReportItem> ReportItems { get; set; }
	//[JsonIgnore] public NotificationType Notification => ReportItems.Count > 0 ? ReportItems.Max(x => ServiceCenter.Get<ICompatibilityManager>().IsSnoozed(x) ? 0 : x.Status.Notification) : NotificationType.None;

	ILocalPackage ICompatibilityInfo.Package { get; }
	public IPackageCompatibilityInfo Info { get; }
	List<ICompatibilityItem> ICompatibilityInfo.ReportItems { get; }

	public CompatibilityInfo(IPackage package, IndexedPackage? packageData)
	{
		Package = package;
		Data = packageData;
		Links = packageData?.Package.Links ?? new();
		ReportItems = new();
	}

	public void Add(ReportType type, IGenericPackageStatus status, string message, ulong[] packages, IWorkshopService workshopService)
	{
		ReportItems.Add(new ReportItem
		{
			PackageId = Data?.Package.SteamId ?? 0,
			Type = type,
			Status = status,
			Message = message,
			Packages = packages.Select(x => new PseudoPackage(x, workshopService)).ToArray()
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
