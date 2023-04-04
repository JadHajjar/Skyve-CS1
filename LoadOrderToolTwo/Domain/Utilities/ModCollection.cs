using System.Collections.Generic;
using System.IO;

namespace LoadOrderToolTwo.Domain.Utilities;
internal class ModCollection
{
	private readonly Dictionary<string, List<Mod>> _modList = new();

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

	internal List<Mod>? GetCollection(Mod mod)
	{
		var key = Path.GetFileName(mod.FileName);

		return GetCollection(key);
	}

	internal List<Mod>? GetCollection(string key)
	{
		if (_modList.ContainsKey(key))
		{
			return _modList[key];
		}

		return null;
	}

	internal IEnumerable<List<Mod>> Collections => _modList.Values;
}
