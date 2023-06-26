using Newtonsoft.Json;

using SkyveApp.Domain;

using SkyveApp.Domain.Enums;

namespace SkyveApp.Systems.Compatibility.Domain;

public class ReportItem : ICompatibilityItem
{
	public ulong PackageId { get; set; }
	[JsonIgnore] public IGenericPackageStatus Status { get; set; }
	public ReportType Type { get; set; }
	public string? Message { get; set; }
	public PseudoPackage[]? Packages { get; set; }

	public GenericPackageStatus? StatusDTO { get => Status is null ? null : new GenericPackageStatus(Status); set => Status = value?.ToGenericPackage(); }

	IPackageIdentity[] ICompatibilityItem.Packages => Packages ?? new IPackageIdentity[0];
}
