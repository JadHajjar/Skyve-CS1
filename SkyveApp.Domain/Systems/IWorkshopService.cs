namespace SkyveApp.Domain.Systems;
public interface IWorkshopService
{
	IWorkshopInfo? GetInfo(object id);
	IWorkshopInfo? GetInfo(IPackageIdentity identity);
	IPackage? GetPackage(IPackageIdentity identity);
	IUser? GetUser(object authorId);
}
