using Newtonsoft.Json;
using SkyveApp.Domain.Compatibility.Enums;

namespace SkyveApp.Domain.Compatibility;

public class ReportItem
{
	public ulong PackageId { get; set; }
	[JsonIgnore] public IGenericPackageStatus? Status { get; set; }
	public ReportType Type { get; set; }
	public string? Message { get; set; }
	public PseudoPackage[]? Packages { get; set; }


	public GenericPackageStatus? StatusDTO { get => Status is null ? null : new GenericPackageStatus(Status); set => Status = value?.ToGenericPackage(); }
}
