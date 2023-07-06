using System;
using System.Collections.Generic;
using System.Threading;

namespace SkyveShared;
public class DelayedAction<TKey>
{
	private readonly Dictionary<TKey, Timer> _timers;
	private readonly int _delayMilliseconds;
	private readonly Action<TKey> _defaultAction;

	public DelayedAction(int delayMilliseconds, Action<TKey> defaultAction = null)
	{
		_timers = new Dictionary<TKey, Timer>();
		_delayMilliseconds = delayMilliseconds;
		_defaultAction = defaultAction;
	}

	public void Run(TKey key)
	{
		if (_defaultAction != null)
		{
			Run(key, _defaultAction);
		}
	}

	public void Run(TKey key, Action<TKey> action)
	{
		lock (_timers)
		{
			if (_timers.TryGetValue(key, out var timer))
			{
				_ = timer.Change(_delayMilliseconds, Timeout.Infinite);
			}
			else
			{
				timer = new Timer(_ =>
				{
					action(key);

					lock (_timers)
					{
						_ = _timers.Remove(key);
					}
				}, null, _delayMilliseconds, Timeout.Infinite);

				_timers.Add(key, timer);
			}
		}
	}
}

public class DelayedAction
{
	private Timer _timer;
	private readonly int _delayMilliseconds;
	private readonly Action _defaultAction;

	public DelayedAction(int delayMilliseconds, Action defaultAction = null)
	{
		_delayMilliseconds = delayMilliseconds;
		_defaultAction = defaultAction;
	}

	public void Run()
	{
		if (_defaultAction != null)
		{
			Run(_defaultAction);
		}
	}

	public void Run(Action action)
	{
		if (_timer != null)
		{
			_ = _timer.Change(_delayMilliseconds, Timeout.Infinite);
		}
		else
		{
			_timer = new Timer(_ =>
			{
				action();

				_timer = null;
			}, null, _delayMilliseconds, Timeout.Infinite);
		}
	}
}