using SkyveApp.Domain;
using SkyveApp.Domain.Systems;

namespace SkyveApp;
public static class ServiceExtensions
{
	private static IWorkshopService? _workshopService;
	private static IWorkshopService WorkshopService => _workshopService ??= ServiceCenter.Get<IWorkshopService>();

	public static IWorkshopInfo? GetWorkshopInfo(this IPackageIdentity identity)
	{
		return WorkshopService.GetInfo(identity);
	}

	public static IPackage? GetWorkshopPackage(this IPackageIdentity identity)
	{
		return WorkshopService.GetPackage(identity);
	}
}
