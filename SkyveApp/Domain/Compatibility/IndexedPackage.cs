using Extensions;

using SkyveApp.Utilities;
using SkyveApp.Utilities.Managers;

using System.Collections.Generic;
using System.Linq;

namespace SkyveApp.Domain.Compatibility;

public class IndexedPackage
{
	public Package Package { get; }
	public Dictionary<ulong, IndexedPackage> Group { get; }
	public Dictionary<ulong, IndexedPackage> RequirementAlternatives { get; }
	public Dictionary<StatusType, List<IndexedPackageStatus>> Statuses { get; }
	public Dictionary<InteractionType, List<IndexedPackageInteraction>> Interactions { get; }

	public IndexedPackage(Package package)
	{
		Package = package;
		Statuses = new();
		Group = new();
		RequirementAlternatives = new();
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

		if (Statuses.ContainsKey(StatusType.TestVersion))
		{
			foreach (var item in Statuses[StatusType.TestVersion].SelectMany(x => x.Packages))
			{
				Group[item.Key] = item.Value;

				item.Value.Group[Package.SteamId] = this;
			}
		}

		if (Statuses.ContainsKey(StatusType.Deprecated))
		{
			foreach (var item in Statuses[StatusType.Deprecated].SelectMany(x => x.Packages))
			{
				RequirementAlternatives[item.Key] = item.Value;
			}
		}

		if (Statuses.ContainsKey(StatusType.Reupload))
		{
			foreach (var item in Statuses[StatusType.Reupload].SelectMany(x => x.Packages))
			{
				RequirementAlternatives[item.Key] = item.Value;
			}
		}

		if (Interactions.ContainsKey(InteractionType.RequirementAlternative))
		{
			foreach (var item in Interactions[InteractionType.RequirementAlternative].SelectMany(x => x.Packages))
			{
				RequirementAlternatives[item.Key] = item.Value;
			}
		}

		if (Interactions.ContainsKey(InteractionType.Alternative))
		{
			foreach (var item in Interactions[InteractionType.Alternative].SelectMany(x => x.Packages))
			{
				RequirementAlternatives[item.Key] = item.Value;
			}
		}

		if (Statuses.ContainsKey(StatusType.Deprecated) && Statuses[StatusType.Deprecated].Any(x => x.Packages.Any()))
		{
			var interaction = new IndexedPackageInteraction(new() { Type = InteractionType.SucceededBy, Packages = Statuses[StatusType.Deprecated].SelectMany(x => x.Packages.Keys).ToArray() }, packages);

			if (!Interactions.ContainsKey(InteractionType.SucceededBy))
			{
				Interactions[InteractionType.SucceededBy] = new() { interaction };
			}
			else
			{
				Interactions[InteractionType.SucceededBy].Add(interaction);
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
				var linkedPackages = item.Interaction.Packages.ToList();

				linkedPackages.Add(Package.SteamId);

				foreach (var package in item.Packages.Values)
				{
					var replacedInteraction = item.Interaction.Clone();

					replacedInteraction.Packages = linkedPackages.Where(x => x != package.Package.SteamId).ToArray();

					if (package.Interactions.ContainsKey(InteractionType.Alternative))
					{
						package.Interactions[InteractionType.Alternative][0].Interaction.Packages = linkedPackages.Concat(package.Interactions[InteractionType.Alternative][0].Interaction.Packages ?? new ulong[0]).Distinct().ToArray();
					}
					else
					{
						package.Interactions[InteractionType.Alternative] = new() { new(replacedInteraction, packages) };
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

				foreach (var package in Interactions[InteractionType.SucceededBy][0].Packages.ToList())
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
}
