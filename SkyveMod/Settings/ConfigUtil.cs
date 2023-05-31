using ColossalFramework.IO;
using ColossalFramework.Packaging;
using ColossalFramework.PlatformServices;

using KianCommons;

using SkyveShared;

using System;
using System.Collections;
using System.IO;
using System.Threading;

using static KianCommons.ReflectionHelpers;

namespace SkyveMod.Settings;
extern alias Injections;
public static class ConfigUtil
{
	public static string LocalLoadOrderPath => Path.Combine(DataLocation.localApplicationData, "Skyve");

	private static Hashtable assetsTable_;

	private static SkyveConfig config_;
	public static SkyveConfig Config
	{
		get
		{
			try
			{
				Init();
				return config_;
			}
			catch (Exception ex)
			{
				Log.Exception(ex);
				return null;
			}
		}
	}

	private static void Init()
	{
		if (config_ != null)
		{
			return; // already initialized.
		}

		LogCalled();
		config_ =
			SkyveConfig.Deserialize()
			?? new SkyveConfig();

		var n = Math.Max(PlatformService.workshop.GetSubscribedItemCount(), config_.Assets.Length);
		assetsTable_ = new Hashtable(n * 10);
		foreach (var assetInfo in config_.Assets)
		{
			assetsTable_[assetInfo.Path] = assetInfo;
		}

		SaveThread.Init();
	}

	public static void Terminate()
	{
		LogCalled();
		SaveThread.Terminate();
		config_ = null;
	}

	public static void SaveConfig()
	{
		try
		{
			//LogCalled();
			SaveThread.Dirty = false;
			if (config_ == null)
			{
				return;
			}

			lock (SaveThread.LockObject)
			{
				config_.Serialize();
			}
		}
		catch (Exception ex)
		{
			Log.Exception(ex);
		}
	}


	// this is useful to store author details after call back.
	internal static class SaveThread
	{
		const int INTERVAL_MS = 1000;
		static bool isRunning_;
		private static Thread thread_;

		public static bool Dirty = false;
		public static readonly object LockObject = new object();

		static SaveThread()
		{
			Init();
		}

		internal static void Init()
		{
			if (isRunning_ != null)
			{
				return; // already running.
			}

			thread_ = new Thread(RunThread)
			{
				Name = "SaveThread",
				IsBackground = true
			};
			isRunning_ = true;
			thread_.Start();
		}

		internal static void Terminate()
		{
			Flush();
			isRunning_ = false;
			thread_ = null;
		}

		private static void RunThread()
		{
			try
			{
				while (isRunning_)
				{
					Thread.Sleep(INTERVAL_MS);
					Flush();
				}
				Log.Info("Save Thread Exiting...");
			}
			catch (Exception ex)
			{
				Log.Exception(ex);
			}
		}
		public static void Flush()
		{
			if (Dirty)
			{
				SaveConfig();
			}
		}
	}

	internal static AssetInfo GetAssetConfig(this Package.Asset a)
	{
		return assetsTable_[a.GetPath()] as AssetInfo;
	}

	internal static string GetPath(this Package.Asset a)
	{
		return a.package.packagePath;
	}
}
