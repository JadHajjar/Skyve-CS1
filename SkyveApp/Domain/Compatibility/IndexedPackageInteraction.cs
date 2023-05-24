using System.Collections.Generic;

namespace SkyveApp.Domain.Compatibility;

public class IndexedPackageInteraction
{
	public PackageInteraction Interaction { get; }
	public Dictionary<ulong, IndexedPackage> Packages { get; }

	public IndexedPackageInteraction(PackageInteraction interaction, Dictionary<ulong, IndexedPackage> packages)
	{
		Interaction = interaction;
		Packages = new();

		if (interaction.Packages is not null)
		{
			foreach (var item in interaction.Packages)
			{
				if (packages.ContainsKey(item))
				{
					Packages[item] = packages[item];
				}
			}
		}
	}

	public static implicit operator IndexedPackageInteraction(PackageInteraction status)
	{
		return new(status, new());
	}
}