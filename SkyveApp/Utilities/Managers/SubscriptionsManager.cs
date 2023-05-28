using Extensions;

using SkyveApp.Domain.Interfaces;
using SkyveApp.Domain.Utilities;

using SkyveShared;

using SlickControls;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace SkyveApp.Utilities.Managers;
internal static class SubscriptionsManager
{
	private static readonly string _filePath = Path.Combine(LocationManager.SkyveAppDataPath, "SubscriptionTransfer.xml");
	private static readonly List<ulong> _delayedDownloads = new();
	private static readonly DelayedAction _delayedDownloadsAction = new(4000, RunDownload);

	public static List<ulong> SubscribingTo { get; private set; } = new();
	public static List<ulong> UnsubscribingFrom { get; private set; } = new();
	public static List<ulong> PendingSubscribingTo { get; private set; } = new();
	public static List<ulong> PendingUnsubscribingFrom { get; private set; } = new();
	public static bool Redownload { get; set; }
	public static bool SubscriptionsPending => ExtensionClass.FileExists(_filePath) && !CitiesManager.IsRunning();

	static SubscriptionsManager()
	{
		if (ExtensionClass.FileExists(_filePath))
		{
			var transferData = SharedUtil.Deserialize<SubscriptionTransfer>(_filePath) ?? new();

			UnsubscribingFrom = transferData.UnsubscribingFrom ?? new();

			SubscribingTo = transferData.SubscribeTo ?? new();
		}
	}

	public static bool IsSubscribing(this IPackage package)
	{
		return package.Package is null ? (SubscribingTo.Contains(package.SteamId) || PendingSubscribingTo.Contains(package.SteamId)) : (UnsubscribingFrom.Contains(package.SteamId) || PendingUnsubscribingFrom.Contains(package.SteamId));
	}

	public static bool Subscribe(IEnumerable<ulong> ids)
	{
		return SubscribePrivate(ids, false);
	}

	public static bool UnSubscribe(IEnumerable<ulong> ids)
	{
		return SubscribePrivate(ids, true);
	}

	private static bool SubscribePrivate(IEnumerable<ulong> ids, bool unsub)
	{
		try
		{
			if (!ids.Any(x => x != 0))
			{
				return false;
			}

			if (!CitiesManager.IsRunning() && !CentralManager.SessionSettings.SubscribeFirstTimeShown)
			{
				MessagePrompt.Show(Locale.SubscribingRequiresGameToOpen, Locale.SubscribingRequiresGameToOpenTitle, PromptButtons.OK, PromptIcons.Info, Program.MainForm);

				CentralManager.SessionSettings.SubscribeFirstTimeShown = true;
				CentralManager.SessionSettings.Save();
			}

			if (CentralManager.SessionSettings.UserSettings.ForceDownloadAndDeleteAsSoonAsRequested)
			{
				if (unsub)
				{
					ContentUtil.DeleteAll(ids);
				}
				else
				{
					_delayedDownloads.AddRange(ids);
					_delayedDownloadsAction.Run();
				}
			}

			var transferData = new SubscriptionTransfer();

			if (ExtensionClass.FileExists(_filePath))
			{
				transferData = SharedUtil.Deserialize<SubscriptionTransfer>(_filePath) ?? new();
			}

			if (unsub)
			{
				transferData.UnsubscribingFrom ??= new();

				foreach (var item in ids)
				{
					if (transferData.UnsubscribingFrom.Contains(item))
					{
						transferData.UnsubscribingFrom.Remove(item);
					}
					else
					{
						transferData.UnsubscribingFrom.Add(item);
					}
				}

				UnsubscribingFrom = transferData.UnsubscribingFrom ?? new();
			}
			else
			{
				transferData.SubscribeTo ??= new();

				foreach (var item in ids)
				{
					if (transferData.SubscribeTo.Contains(item))
					{
						transferData.SubscribeTo.Remove(item);
					}
					else
					{
						transferData.SubscribeTo.Add(item);
					}
				}

				SubscribingTo = transferData.SubscribeTo ?? new();
			}

			if ((transferData.UnsubscribingFrom?.Any() ?? false) || (transferData.SubscribeTo?.Any() ?? false))
			{
				SharedUtil.Serialize(transferData, _filePath);
			}
			else
			{
				ExtensionClass.DeleteFile(_filePath);
			}
		}
		catch (Exception ex) { Log.Exception(ex, "Failed to send subscription transfer data"); return false; }

		return true;
	}

	private static void SubscriptionTransferFileChanged(object sender, FileSystemEventArgs e)
	{
		var transferData = new SubscriptionTransfer();

		if (ExtensionClass.FileExists(_filePath))
		{
			transferData = SharedUtil.Deserialize<SubscriptionTransfer>(_filePath) ?? new();
		}

		PendingUnsubscribingFrom = UnsubscribingFrom;
		PendingSubscribingTo = SubscribingTo;

		UnsubscribingFrom = transferData.UnsubscribingFrom ?? new();

		SubscribingTo = transferData.SubscribeTo ?? new();
	}

	private static void RunDownload()
	{
		var ids = new List<ulong>(_delayedDownloads);

		_delayedDownloads.Clear();

		if (LocationManager.Platform is Platform.MacOSX)
		{
			SteamUtil.ReDownload(ids);
		}

		SteamUtil.ReDownload(ids);
	}

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

		var watcher2 = new FileSystemWatcher
		{
			Path = LocationManager.SkyveAppDataPath,
			NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName,
			Filter = "SubscriptionTransfer.xml"
		};

		watcher2.Deleted += new FileSystemEventHandler(SubscriptionTransferFileChanged);

		watcher2.EnableRaisingEvents = true;
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

			if (!ids.Contains(name) && (!ExtensionClass.FileExists(_filePath) || !SubscribingTo.Any(x => x.ToString() == name)))
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

			SteamUtil.ReDownload(ids.Select(ulong.Parse));
		}
	}

	internal static void CancelPendingItems()
	{
		if (ExtensionClass.FileExists(_filePath))
		{
			ExtensionClass.DeleteFile(_filePath);
		}
	}
}
