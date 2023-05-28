using ColossalFramework.IO;
using ColossalFramework.PlatformServices;

using KianCommons;

using SkyveApp.Domain.Utilities;

using SkyveShared;

using System;
using System.IO;
using System.Linq;
using System.Threading;

namespace SkyveMod.Util;
internal static class SubscriptionUtil
{
	private static readonly string _filePath = Path.Combine(DataLocation.localApplicationData, Path.Combine("Skyve", "SubscriptionTransfer.xml"));
	private static FileSystemWatcher? watcher;

	internal static void Start()
	{
		watcher = new FileSystemWatcher
		{
			Path = Path.Combine(DataLocation.localApplicationData, "Skyve"),
			NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName,
			Filter = "SubscriptionTransfer.xml"
		};

		watcher.Changed += new FileSystemEventHandler(SubscriptionFileChanged);
		watcher.Created += new FileSystemEventHandler(SubscriptionFileChanged);

		watcher.EnableRaisingEvents = true;

		Run();
	}

	internal static void Stop()
	{
		watcher?.Dispose();
	}

	private static void SubscriptionFileChanged(object sender, FileSystemEventArgs e)
	{
		Run();
	}

	internal static void Run()
	{
		try
		{
			if (!File.Exists(_filePath))
			{ return; }

			var data = SharedUtil.Deserialize<SubscriptionTransfer>(_filePath);

			if (data == null)
			{ return; }

			if (data.SubscribeTo is not null)
			{
				foreach (var item in data.SubscribeTo.Distinct())
				{
					if (item > 0)
					{
						for (var i = 0; i < 3; i++)
						{
							if (PlatformService.workshop.Subscribe(new(item)))
							{
								Thread.Sleep(15);

								break;
							}

							Thread.Sleep(50);
						}
					}
				}
			}

			if (data.UnsubscribingFrom is not null)
			{
				foreach (var item in data.UnsubscribingFrom.Distinct())
				{
					if (item > 0)
					{
						PlatformService.workshop.Unsubscribe(new(item));
					}

					Thread.Sleep(15);
				}
			}

			File.Delete(_filePath);

			File.WriteAllLines(Path.Combine(SharedUtil.LocalLOMData, "SubscriptionList.txt"), PlatformService.workshop.GetSubscribedItems().Select(x => x.AsUInt64.ToString()).ToArray());
		}
		catch (Exception ex) { ex.Log(false); }
	}
}
