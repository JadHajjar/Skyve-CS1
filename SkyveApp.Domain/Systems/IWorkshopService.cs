namespace SkyveApp.Domain.Systems;
public interface IWorkshopService
{
	IWorkshopInfo? GetInfo(IPackageIdentity identity);
	IPackage? GetPackage(IPackageIdentity identity);
	IUser? GetUser(object authorId);
}
