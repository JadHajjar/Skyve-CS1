using SkyveApp.Domain.Enums;

using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkyveApp.Domain;
public interface ICompatibilityItem
{
	ulong PackageId { get; }
	IGenericPackageStatus? Status { get; }
	ReportType Type { get; }
	string? Message { get; }
	IPackageIdentity[] Packages { get; }
}
