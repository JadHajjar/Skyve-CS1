using Extensions;

using System.Collections.Generic;
using System.Linq;

namespace SkyveApp.Domain.Compatibility;

public class IndexedCompatibilityData
{
	public IndexedCompatibilityData(CompatibilityData? data)
	{
		Packages = data?.Packages?.ToDictionary(x => x.SteamId, x => NewMethod(x, data.Packages)) ?? new();
		Authors = data?.Authors?.ToDictionary(x => x.SteamId) ?? new();
		BlackListedIds = data?.BlackListedIds ?? new();
		BlackListedNames = data?.BlackListedNames ?? new();

		foreach (var item in Packages.Values)
		{
			item.Load(Packages);
		}

		foreach (var item in Packages.Values)
		{
			item.SetUpInteractions(Packages);
		}
	}

	private static IndexedPackage NewMethod(Package package, List<Package> packages)
	{
		var nonTest = package.Statuses?.FirstOrDefault(x => x.Type == StatusType.TestVersion && (x.Packages?.Any() ?? false));

		if (nonTest is not null)
		{
			var id = nonTest.Packages![0];
			var stable = packages.FirstOrDefault(x => x.SteamId == id);

			if (stable is not null)
			{
				package.Links = stable.Links;
				package.RequiredDLCs = stable.RequiredDLCs;
				package.Tags = stable.Tags;
				package.Note = stable.Note;
				package.Usage = stable.Usage;
				package.Type = stable.Type;
				package.Statuses ??= new();
				package.Statuses.AddRange(stable.Statuses ?? new());
				package.Interactions ??= new();
				package.Interactions.AddRange(stable.Interactions?.Where(x => x.Type > InteractionType.Alternative) ?? Enumerable.Empty<PackageInteraction>());
			}
		}

		return new IndexedPackage(package);
	}

	public Dictionary<ulong, IndexedPackage> Packages { get; }
	public Dictionary<ulong, Author> Authors { get; }
	public List<ulong> BlackListedIds { get; }
	public List<string> BlackListedNames { get; }
}
