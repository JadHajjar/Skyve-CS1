using SkyveApp.Domain;
using SkyveApp.Domain.Enums;
using SkyveApp.Domain.Systems;

namespace SkyveApp;
public static class ServiceExtensions
{
	private static IWorkshopService? _workshopService;
	private static ICompatibilityManager? _compatibilityManager;

	private static IWorkshopService WorkshopService => _workshopService ??= ServiceCenter.Get<IWorkshopService>();
	private static ICompatibilityManager CompatibilityManager => _compatibilityManager ??= ServiceCenter.Get<ICompatibilityManager>();

	public static IWorkshopInfo? GetWorkshopInfo(this IPackageIdentity identity)
	{
		return WorkshopService.GetInfo(identity);
	}

	public static IPackage? GetWorkshopPackage(this IPackageIdentity identity)
	{
		return WorkshopService.GetPackage(identity);
	}

	public static ICompatibilityInfo GetCompatibilityInfo(this IPackage package)
	{
		return CompatibilityManager.GetCompatibilityInfo(package);
	}

	public static NotificationType GetNotification(this ICompatibilityInfo compatibilityInfo)
	{
		return CompatibilityManager.GetNotification(compatibilityInfo);
	}
}
