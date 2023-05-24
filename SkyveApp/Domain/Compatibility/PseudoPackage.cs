using Newtonsoft.Json;

using SkyveApp.Domain.Interfaces;
using SkyveApp.Utilities;

namespace SkyveApp.Domain.Compatibility;

public class PseudoPackage
{
	public PseudoPackage()
	{

	}
	public ulong SteamId { get; set; }

	public PseudoPackage(ulong steamId)
	{
		SteamId = steamId;
	}

	private readonly IPackage? IPackage;

	public PseudoPackage(IPackage iPackage)
	{
		SteamId = iPackage.SteamId;
		IPackage = iPackage;
	}

	public static implicit operator ulong(PseudoPackage pkg)
	{
		return pkg.SteamId;
	}

	[JsonIgnore] public IPackage? Package => IPackage ?? SteamUtil.GetItem(SteamId);
}