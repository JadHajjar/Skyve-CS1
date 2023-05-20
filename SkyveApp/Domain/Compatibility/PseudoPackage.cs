using SkyveApp.Domain.Interfaces;
using SkyveApp.Utilities;
using SkyveApp.Utilities.Managers;

using System.Linq;

namespace SkyveApp.Domain.Compatibility;

public class PseudoPackage
{
    public ulong SteamId { get; set; }

	public PseudoPackage(ulong steamId)
	{
		SteamId = steamId;
	}

	private IPackage? IPackage;

	public PseudoPackage(IPackage iPackage)
	{
		SteamId = iPackage.SteamId;
		IPackage = iPackage;
	}

	public static implicit operator ulong(PseudoPackage pkg) => pkg.SteamId;

	public IPackage? Package => IPackage ?? SteamUtil.GetItem(SteamId);
}