using SkyveApp.Domain;
using SkyveApp.Domain.Systems;

namespace SkyveApp.Systems;
public class GenericPackageIdentity : IPackageIdentity
{
	public ulong Id { get; }
	public string Name => this.GetWorkshopInfo()?.Name ?? string.Empty;
	public string? Url => this.GetWorkshopInfo()?.Url;

	public GenericPackageIdentity(ulong id)
	{
		Id = id;
	}
}
