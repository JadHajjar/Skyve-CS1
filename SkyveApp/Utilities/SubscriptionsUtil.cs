using Extensions;

using SkyveApp.Utilities.Managers;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace SkyveApp.Utilities;
internal class SubscriptionsUtil
{
	public static bool Redownload { get; set; }

	internal static void Start()
	{
		var watcher = new FileSystemWatcher
		{
			Path = LocationManager.SkyveAppDataPath,
			NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName,
			Filter = "SubscriptionList.txt"
		};

		watcher.Changed += new FileSystemEventHandler(FileChanged);
		watcher.Created += new FileSystemEventHandler(FileChanged);

		watcher.EnableRaisingEvents = true;
	}

	private static void FileChanged(object sender, FileSystemEventArgs e)
	{
		if (!CentralManager.SessionSettings.UserSettings.DisablePackageCleanup && ExtensionClass.FileExists(e.FullPath))
		{
			var date = File.GetLastWriteTime(e.FullPath);

			if (DateTime.Now - date < TimeSpan.FromSeconds(5))
			{
				Thread.Sleep(500);

				var list = File.ReadAllLines(e.FullPath).ToList();

				try
				{ ExtensionClass.DeleteFile(e.FullPath); }
				catch { Thread.Sleep(500); }

				HandleSubscriptions(list);
			}
		}
	}

	private static void HandleSubscriptions(List<string> ids)
	{
		foreach (var folder in Directory.EnumerateDirectories(LocationManager.WorkshopContentPath))
		{
			var name = Path.GetFileName(folder);

			if (!ids.Contains(name))
			{
				ContentUtil.DeleteAll(folder);
			}
			else
			{
				ids.Remove(name);
			}
		}

		if (Redownload)
		{
			Redownload = false;

			SteamUtil.ReDownload(ids.Select(ulong.Parse).ToArray());
		}
	}
}
