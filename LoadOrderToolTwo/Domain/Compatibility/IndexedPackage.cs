using Extensions;

using LoadOrderToolTwo.Utilities.Managers;

using System.Collections.Generic;
using System.Linq;

namespace LoadOrderToolTwo.Domain.Compatibility;

public class IndexedPackage
{
	public Package Package { get; }
	public Dictionary<ulong, IndexedPackage> Group { get; }
	public Dictionary<StatusType, List<IndexedPackageStatus>> Statuses { get; }
	public Dictionary<InteractionType, List<IndexedPackageInteraction>> Interactions { get; }

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
			foreach (var grp in Package.Statuses.GroupBy(x => x.Type))
			{
				Statuses[grp.Key] = grp.Select(x => new IndexedPackageStatus(x, packages)).ToList();
			}
		}

		if (Package.Interactions is not null)
		{
			foreach (var grp in Package.Interactions.GroupBy(x => x.Type))
			{
				Interactions[grp.Key] = grp.Select(x => new IndexedPackageInteraction(x, packages)).ToList();
			}
		}

		if (Package.GroupId != 0)
		{
			foreach (var item in packages.Values)
			{
				if (item.Package.GroupId == Package.GroupId)
				{
					Group[item.Package.SteamId] = item;
				}
			}
		}
	}

	public void SetUpInteractions(Dictionary<ulong, IndexedPackage> packages)
	{
		if (Interactions.ContainsKey(InteractionType.Successor))
		{
			RecursiveSetSuccessor();
		}

		if (Interactions.ContainsKey(InteractionType.Alternative))
		{
			foreach (var item in Interactions[InteractionType.Alternative])
			{
				foreach (var package in item.Packages.Values)
				{
					var replacedInteraction = item.Clone();

					replacedInteraction.Packages[Package.SteamId] = this;
					replacedInteraction.Packages.Remove(package.Package.SteamId);

					if (package.Interactions.ContainsKey(InteractionType.Alternative))
					{
						package.Interactions[InteractionType.Alternative].Add(replacedInteraction);
					}
					else
					{
						package.Interactions[InteractionType.Alternative] = new() { replacedInteraction };
					}
				}
			}
		}
	}

	private void RecursiveSetSuccessor()
	{
		foreach (var item in Interactions[InteractionType.Successor].SelectMany(x => x.Packages.Values))
		{
			if (!item.Interactions.ContainsKey(InteractionType.SucceededBy))
			{
				item.Interactions[InteractionType.SucceededBy] = new()
				{
					new IndexedPackageInteraction(new(){ Type = InteractionType.SucceededBy }, new())
				};
			}

			PackageInteraction packageInteraction;

			if (Interactions.ContainsKey(InteractionType.SucceededBy))
			{
				item.Interactions[InteractionType.SucceededBy][0].Packages.Remove(Package.SteamId);

				foreach (var package in Interactions[InteractionType.SucceededBy][0].Packages)
				{
					item.Interactions[InteractionType.SucceededBy][0].Packages[package.Key] = package.Value;
				}

				packageInteraction = Interactions[InteractionType.SucceededBy][0].Interaction;
			}
			else
			{
				item.Interactions[InteractionType.SucceededBy][0].Packages[Package.SteamId] = this;

				packageInteraction = Interactions[InteractionType.Successor][0].Interaction;
			}

			if (packageInteraction.Action is StatusAction.Switch)
			{
				item.Interactions[InteractionType.SucceededBy][0].Interaction.Action = StatusAction.Switch;
			}

			item.Interactions[InteractionType.SucceededBy][0].Interaction.Packages = item.Interactions[InteractionType.SucceededBy][0].Packages.Keys.ToArray();

			if (item.Interactions.ContainsKey(InteractionType.Successor))
			{
				item.RecursiveSetSuccessor();
			}
		}
	}

	public override bool Equals(object? obj)
	{
		return obj is IndexedPackage package && Package.SteamId == package.Package.SteamId;
	}

	public override int GetHashCode()
	{
		return Package.SteamId.GetHashCode();
	}

	public Domain.Package? LocalPackage => CompatibilityManager.FindPackage(Package);
}
