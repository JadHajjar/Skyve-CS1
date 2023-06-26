namespace SkyveApp.Domain.Systems;
public interface IWorkshopService
{
	IWorkshopInfo? GetInfo(ulong id);
	IPackage? GetPackage(ulong id);
}
