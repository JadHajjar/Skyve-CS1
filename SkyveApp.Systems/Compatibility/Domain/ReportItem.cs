using Newtonsoft.Json;

using SkyveApp.Domain;

using SkyveApp.Domain.Enums;

using System.Linq;

namespace SkyveApp.Systems.Compatibility.Domain;

public class ReportItem : ICompatibilityItem
{
	public ulong PackageId { get; set; }
	[JsonIgnore] public IGenericPackageStatus Status { get; set; }
	public ReportType Type { get; set; }
	public string? Message { get; set; }
	public PseudoPackage[]? Packages { get; set; }

	public GenericPackageStatus? StatusDTO { get => Status is null ? null : new GenericPackageStatus(Status); set => Status = value?.ToGenericPackage(); }

	IPackage[] ICompatibilityItem.Packages => Packages.Select(x => x.Package).ToArray() ?? new IPackage[0];
}
