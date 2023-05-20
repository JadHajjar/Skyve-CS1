using System;
using System.Collections.Generic;
using System.IO;

namespace SkyveApp.Domain.Utilities;
internal class ModCollection
{
	private readonly Dictionary<string, List<Mod>> _modList = new(StringComparer.OrdinalIgnoreCase);
	private readonly Dictionary<string, CollectionInfo> _collectionList;

	public ModCollection(Dictionary<string, CollectionInfo> collectionList)
	{
		_collectionList = collectionList;
	}

	public void AddMod(Mod mod)
	{
		var key = Path.GetFileName(mod.FileName);

		if (_modList.ContainsKey(key))
		{
			_modList[key].Add(mod);
		}
		else
		{
			_modList.Add(key, new() { mod });
		}
	}

	public void RemoveMod(Mod mod)
	{
		var key = Path.GetFileName(mod.FileName);

		if (_modList.ContainsKey(key))
		{
			_modList[key].Remove(mod);
		}
	}

	internal List<Mod>? GetCollection(Mod mod, out CollectionInfo? collection)
	{
		var key = Path.GetFileName(mod.FileName);

		return GetCollection(key, out collection);
	}

	internal List<Mod>? GetCollection(string key, out CollectionInfo? collection)
	{
		if (_modList.ContainsKey(key))
		{
			collection = _collectionList[key];

			return _modList[key];
		}

		collection = null;
		return null;
	}

	internal void CheckAndAdd(Mod mod)
	{
		var fileName = Path.GetFileName(mod.FileName);

		if (_collectionList.ContainsKey(fileName))
		{
			AddMod(mod);
		}
	}

	internal IEnumerable<List<Mod>> Collections => _modList.Values;
}

internal class CollectionInfo
{
	public bool Required { get; set; }
	public bool Forbidden { get; set; }
}
