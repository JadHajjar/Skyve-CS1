using System.Collections.Generic;
using System.Linq;

namespace SkyveApp.Domain.Compatibility;

public class IndexedCompatibilityData
{
	public IndexedCompatibilityData(CompatibilityData? data)
	{
		Packages = data?.Packages?.ToDictionary(x => x.SteamId, x => new IndexedPackage(x)) ?? new();
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

	public Dictionary<ulong, IndexedPackage> Packages { get; }
	public Dictionary<ulong, Author> Authors { get; }
	public List<ulong> BlackListedIds { get; }
	public List<string> BlackListedNames { get; }
}
