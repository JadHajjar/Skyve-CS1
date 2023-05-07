using LoadOrderToolTwo.Domain.Interfaces;

using System.Collections.Generic;
using System.Linq;

namespace LoadOrderToolTwo.Domain.Compatibility;
public class CompatibilityInfo
{
	public IPackage Package { get; }
	public IndexedPackage? Data { get; }
	public List<PackageLink> Links { get; }
	public List<ReportMessage> ReportMessages { get; }
	public NotificationType Notification => ReportMessages.Count > 0 ? ReportMessages.Max(x => x.Status.Notification) : NotificationType.None;

	public CompatibilityInfo(IPackage package, IndexedPackage? packageData)
	{
		Package = package;
		Data = packageData;
		Links = new();
		ReportMessages = new();
	}

	public void Add(ReportType type, IPackageStatus status, string message)
	{
		ReportMessages.Add(new ReportMessage
		{
			Type = type,
			Status = status,
			Message = message,
		});
	}
}
