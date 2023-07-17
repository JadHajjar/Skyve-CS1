using Newtonsoft.Json;

using SkyveApp.Domain;

using SkyveApp.Domain.Systems;

namespace SkyveApp.Systems.Compatibility.Domain;

public class PseudoPackage : IPackageIdentity
{
	private readonly IWorkshopService? _workshopService;

	private readonly IPackage? _iPackage;

	public PseudoPackage(IWorkshopService workshopService)
	{
		_workshopService = workshopService;
	}

	public PseudoPackage(ulong steamId, IWorkshopService workshopService)
	{
		Id = steamId;
		_workshopService = workshopService;
	}

	public PseudoPackage(IPackage iPackage)
	{
		Id = iPackage.Id;
		_iPackage = iPackage;
	}

	public ulong Id { get; set; }
	[JsonIgnore] public string Name => Package?.Name ?? string.Empty;
	[JsonIgnore] public IPackage Package => _iPackage ?? _workshopService?.GetPackage(new GenericPackageIdentity(Id)) ?? new GenericWorkshopPackage(new GenericPackageIdentity(Id));
	[JsonIgnore] public string? Url => Package?.Url;

	public IWorkshopInfo? GetWorkshopInfo()
	{
		return Package?.GetWorkshopInfo();
	}

	public static implicit operator ulong(PseudoPackage pkg)
	{
		return pkg.Id;
	}
}