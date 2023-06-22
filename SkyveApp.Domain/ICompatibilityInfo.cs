using SkyveApp.Domain.Enums;
using SkyveApp.Domain.Systems;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkyveApp.Domain;
public interface ICompatibilityInfo
{
	IPackage Package { get; }
	List<ILink> Links { get; }
	List<ICompatibilityItem> ReportItems { get; }
	NotificationType Notification { get; }
}
