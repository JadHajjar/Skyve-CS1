using System;
using System.Collections.Generic;

namespace LoadOrderToolTwo.Domain.Utilities;
internal class CachedSaveLibrary<TItem, TKey, TValue> where TItem : CachedSaveItem<TKey, TValue>
{
	internal readonly Dictionary<TKey, TItem> _dictionary = new();

	public CachedSaveLibrary()
	{

	}

	public void SetValue(TKey key, TValue value)
	{
		var entry = (Activator.CreateInstance(typeof(TItem), key, value) as TItem)!;

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
		List<TItem> values;

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
