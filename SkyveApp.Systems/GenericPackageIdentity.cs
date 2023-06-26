using SkyveApp.Domain;
using SkyveApp.Domain.Systems;

namespace SkyveApp.Systems;
public class GenericPackageIdentity : IPackageIdentity
{
	private readonly IWorkshopInfo? _workshopInfo;
	private readonly IWorkshopService? _workshopService;

	public ulong Id { get; }
	public string Name => GetWorkshopInfo()?.Title ?? string.Empty;
	public string? Url => GetWorkshopInfo()?.Url;

	public IWorkshopInfo? GetWorkshopInfo()
	{
		return _workshopInfo ?? _workshopService?.GetInfo(Id);
	}

	public GenericPackageIdentity(IWorkshopInfo workshopInfo)
	{
		_workshopInfo = workshopInfo;
		Id = workshopInfo.Id;
	}

	public GenericPackageIdentity(ulong id, IWorkshopService workshopService)
	{
		_workshopService = workshopService;
		Id = id;
	}
}
