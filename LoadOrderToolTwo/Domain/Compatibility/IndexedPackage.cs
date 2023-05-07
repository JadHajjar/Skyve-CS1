using LoadOrderToolTwo.Utilities.Managers;

using System.Collections.Generic;

namespace LoadOrderToolTwo.Domain.Compatibility;

public class IndexedPackage
{
	public Package Package { get; }
	public Dictionary<ulong, IndexedPackage> Group { get; }
	public Dictionary<StatusType, IndexedPackageStatus> Statuses { get; }
	public Dictionary<InteractionType, IndexedPackageInteraction> Interactions { get; }

	public IndexedPackage(Package package)
	{
		Package = package;
		Statuses = new();
		Group = new();
		Interactions = new();
	}

	public void Load(Dictionary<ulong, IndexedPackage> packages)
	{
		if (Package.Statuses is not null)
		{
			foreach (var item in Package.Statuses)
			{
				Statuses[item.Type] = new(item, packages);
			}
		}

		if (Package.Interactions is not null)
		{
			foreach (var item in Package.Interactions)
			{
				Interactions[item.Type] = new(item, packages);
			}
		}

		foreach (var item in packages.Values)
		{
			if (item.Package.GroupId == Package.GroupId)
			{
				Group[item.Package.SteamId] = item;
			}
		}
	}

	public Domain.Package? LocalPackage => CompatibilityManager.FindPackage(Package);
}
