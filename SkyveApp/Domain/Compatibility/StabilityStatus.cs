using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkyveApp.Domain.Compatibility;
internal class StabilityStatus : IPackageStatus<PackageStability>
{
    public StabilityStatus(PackageStability type, string? note, bool review)
    {
        Type = type;
		Action = type is PackageStability.Broken ? StatusAction.UnsubscribeThis : review?StatusAction.RequestReview: StatusAction.NoAction;
		Note = note;
	}

    public PackageStability Type { get; set; }
	public StatusAction Action { get; set; }
	public ulong[]? Packages { get; set; }
	public string? Note { get; set; }
	public NotificationType Notification => CRNAttribute.GetNotification(Type);
	public int IntType => (int)Type;
}
