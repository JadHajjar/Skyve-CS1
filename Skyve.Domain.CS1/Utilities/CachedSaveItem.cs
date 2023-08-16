using System;

namespace Skyve.Domain.CS1.Utilities;
public class CachedSaveItem<TKey, TValue>
{
	private readonly TValue _currentValue;

	public TKey Key { get; }
	public TValue ValueToSave { get; }
	protected Func<TKey, TValue> Getter { get; }
	protected Action<TKey, TValue> Setter { get; }

	public CachedSaveItem(TKey key, TValue value, Func<TKey, TValue> getter, Action<TKey, TValue> setter)
	{
		Key = key;
		ValueToSave = value;
		Getter = getter;
		Setter = setter;
		_currentValue = CurrentValue;
	}

	public virtual TValue CurrentValue => Getter(Key);

	protected virtual void OnSave()
	{
		Setter(Key, ValueToSave);
	}

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
