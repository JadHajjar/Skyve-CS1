using System.Collections.Generic;
using System.Linq;

namespace LoadOrderToolTwo.Domain.Compatibility;

public class IndexedCompatibilityData
{
	private readonly List<Package> _packages;
	private readonly List<Author> _authors;

	public IndexedCompatibilityData(CompatibilityData? data)
	{
		_packages = data?.Packages ?? new();
		_authors = data?.Authors ?? new();

		Packages = _packages.ToDictionary(x => x.SteamId, x => new IndexedPackage(x));
		Authors = _authors.ToDictionary(x => x.SteamId);
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
