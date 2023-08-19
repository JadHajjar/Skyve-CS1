using System;
using System.Collections.Generic;
using System.IO;

namespace Skyve.Domain.CS1.Utilities;
public class ModCollection
{
	private readonly Dictionary<string, List<IMod>> _modList = new(StringComparer.OrdinalIgnoreCase);
	private readonly Dictionary<string, CollectionInfo> _collectionList;

	public ModCollection(Dictionary<string, CollectionInfo> collectionList)
	{
		_collectionList = collectionList;
	}

	public void AddMod(IMod mod)
	{
		var key = Path.GetFileName(mod.FilePath);

		if (_modList.ContainsKey(key))
		{
			_modList[key].Add(mod);
		}
		else
		{
			_modList.Add(key, new() { mod });
		}
	}

	public void RemoveMod(IMod mod)
	{
		var key = Path.GetFileName(mod.FilePath);

		if (_modList.ContainsKey(key))
		{
			_modList[key].Remove(mod);
		}
	}

	public List<IMod>? GetCollection(IMod mod, out CollectionInfo? collection)
	{
		var key = Path.GetFileName(mod.FilePath);

		return GetCollection(key, out collection);
	}

	public List<IMod>? GetCollection(string key, out CollectionInfo? collection)
	{
		if (_modList.ContainsKey(key))
		{
			collection = _collectionList[key];

			return _modList[key];
		}

		collection = null;
		return null;
	}

	public void CheckAndAdd(IMod mod)
	{
		var fileName = Path.GetFileName(mod.FilePath);

		if (_collectionList.ContainsKey(fileName))
		{
			AddMod(mod);
		}
	}

	public IEnumerable<List<IMod>> Collections => _modList.Values;
}

public class CollectionInfo
{
	public bool Required { get; set; }
	public bool Forbidden { get; set; }
}
