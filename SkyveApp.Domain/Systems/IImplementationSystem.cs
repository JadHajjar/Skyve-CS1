namespace SkyveApp.Domain.Systems;
public interface IImplementationSystem
{
	IAsset CreateAsset(ILocalPackage package, string file);

}
