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
		if (CompatibilityManager.CompatibilityData.Packages.TryGetValue(steamId, out var package) && package.Interactions.ContainsKey(InteractionType.SucceededBy))
		{
			steamId = package.Interactions[InteractionType.SucceededBy]
					.SelectMany(x => x.Packages.Values)
					.OrderByDescending(x => x.Package.ReviewDate)
					.FirstOrDefault()?
					.Package.SteamId ?? steamId;
		}

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