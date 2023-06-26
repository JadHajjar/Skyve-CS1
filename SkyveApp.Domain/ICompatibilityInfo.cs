using SkyveApp.Domain.Enums;

using System.Collections.Generic;

namespace SkyveApp.Domain;
public interface ICompatibilityInfo
{
	ILocalPackage Package { get; }
	IPackageCompatibilityInfo Info { get; }
	List<ICompatibilityItem> ReportItems { get; }
}
