using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoadOrderToolTwo.Domain.Utilities;
internal class CachedSaveLibrary<TItem, TKey, TValue> where TItem : CachedSaveItem<TKey, TValue>
{
	private readonly Dictionary<TKey, TItem> _dictionary = new();

	public CachedSaveLibrary()
	{

	}

	public void SetValue(TKey key, TValue value)
	{
		var entry = (Activator.CreateInstance(typeof(TItem), key, value) as TItem)!;

		if (entry.IsStateValid())
		{
			lock (_dictionary)
				_dictionary[key] = entry;
		}
		else
		{
			lock (_dictionary)
				_dictionary.Remove(key);
		}
	}

	public bool GetValue(TKey key, out TValue? value)
	{
		if (_dictionary.ContainsKey(key) && _dictionary[key].IsStateValid())
		{
			value = _dictionary[key].ValueToSave;
			return true;
		}

		value = default;
		return false;
	}

	public void Save()
	{
		lock (_dictionary)
		{
			foreach (var item in _dictionary.Values)
			{
				item.Save();
			}

			_dictionary.Clear();
		}
	}

	public bool Any()
	{
		return _dictionary.Count > 0;
	}
}
