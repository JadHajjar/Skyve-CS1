using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoadOrderToolTwo.Domain.Utilities;
internal abstract class CachedSaveItem<TKey, TValue>
{
    private readonly TValue _currentValue;

	public TKey Key { get; }
	public TValue ValueToSave { get; }

    public CachedSaveItem(TKey key, TValue value)
    {
        Key = key;
        ValueToSave = value;
        _currentValue = CurrentValue;
	}

    public abstract TValue CurrentValue { get; }
	protected abstract void OnSave();

	public void Save()
	{
		if (IsStateValid())
		{
			OnSave();
		}
	}

	public bool IsStateValid()
	{
		return !(_currentValue?.Equals(ValueToSave) ?? false);
	}
}
