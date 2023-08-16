using System;
using System.Collections.Generic;

namespace Skyve.Domain.CS1.Utilities;
public class CachedSaveLibrary<TKey, TValue>
{
	internal readonly Dictionary<TKey, CachedSaveItem<TKey, TValue>> _dictionary = new();
	private readonly Func<TKey, TValue> _getter;
	private readonly Action<TKey, TValue> _setter;

	public int Count => _dictionary.Count;

	public CachedSaveLibrary(Func<TKey, TValue> getter, Action<TKey, TValue> setter)
	{
		_getter = getter;
		_setter = setter;
	}

	public void SetValue(TKey key, TValue value)
	{
		var entry = new CachedSaveItem<TKey, TValue>(key, value, _getter, _setter);

		if (entry.IsStateValid())
		{
			lock (_dictionary)
			{
				_dictionary[key] = entry;
			}
		}
		else
		{
			lock (_dictionary)
			{
				_dictionary.Remove(key);
			}
		}
	}

	public bool GetValue(TKey key, out TValue? value)
	{
		lock (_dictionary)
		{
			if (_dictionary.ContainsKey(key) && _dictionary[key].IsStateValid())
			{
				value = _dictionary[key].ValueToSave;
				return true;
			}
		}

		value = default;
		return false;
	}

	public void Save()
	{
		List<CachedSaveItem<TKey, TValue>> values;

		lock (_dictionary)
		{
			values = new(_dictionary.Values);

			_dictionary.Clear();
		}

		foreach (var item in values)
		{
			item.Save();
		}
	}

	public bool Any()
	{
		lock (_dictionary)
		{
			return _dictionary.Count > 0;
		}
	}
}
