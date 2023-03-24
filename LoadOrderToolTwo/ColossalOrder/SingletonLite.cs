using LoadOrderToolTwo.Utilities;

using System;

namespace LoadOrderToolTwo.ColossalOrder;
public abstract class SingletonLite<T>
		where T : SingletonLite<T>, new()
{
	protected static T? sInstance;
	private static readonly object lockObject = new object(); // work around in case constructor was used by mistake.
	public static T instance
	{
		get
		{
			try
			{
				if (sInstance == null)
				{
					lock (lockObject)
					{
						sInstance = new T();
						Log.Debug("Created singleton of type " + typeof(T).Name + ". calling Awake() ...");
						sInstance.Awake();
						Log.Debug("Awake() finished for " + typeof(T).Name);
					}
				}
				return sInstance;
			}
			catch (Exception ex)
			{
				ex.Log();
				throw;
			}
		}
	}

	public static bool exists => sInstance != null;

	public static void Ensure()
	{
		_ = instance;
	}

	public virtual void Awake() { }
}

