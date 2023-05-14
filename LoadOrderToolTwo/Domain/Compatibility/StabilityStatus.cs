using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoadOrderToolTwo.Domain.Compatibility;
internal class StabilityStatus : IPackageStatus<PackageStability>
{
    public StabilityStatus(PackageStability type, string? note)
    {
        Type = type;
		Action = type is PackageStability.Broken ? StatusAction.UnsubscribeThis : StatusAction.NoAction;
		Note = note;
	}
    public PackageStability Type { get; set; }
	public StatusAction Action { get; set; }
	public ulong[]? Packages { get; set; }
	public string? Note { get; set; }
	public NotificationType Notification => CRNAttribute.GetNotification(Type);
}
