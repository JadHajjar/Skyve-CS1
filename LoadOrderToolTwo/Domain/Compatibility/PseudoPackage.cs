using LoadOrderToolTwo.Domain.Interfaces;
using LoadOrderToolTwo.Utilities;
using LoadOrderToolTwo.Utilities.Managers;

using System.Linq;

namespace LoadOrderToolTwo.Domain.Compatibility;

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
		IPackage = iPackage;
	}

	public static implicit operator ulong(PseudoPackage pkg) => pkg.SteamId;

	public IPackage? Package => IPackage ?? SteamUtil.GetItem(SteamId);
}