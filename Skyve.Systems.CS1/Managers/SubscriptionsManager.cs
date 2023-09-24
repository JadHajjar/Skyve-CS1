using Extensions;

using Skyve.Domain;
using Skyve.Domain.CS1.Utilities;
using Skyve.Domain.Systems;
using Skyve.Systems.CS1.Systems;
using Skyve.Systems.CS1.Utilities;

using SkyveShared;

using SlickControls;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace Skyve.Systems.CS1.Managers;
internal class SubscriptionsManager : ISubscriptionsManager
{
	private readonly string _filePath;
	private readonly List<ulong> _delayedDownloads = new();
	private readonly DelayedAction _delayedDownloadsAction;
	private FileWatcher? SubscriptionListWatcher;
	private FileWatcher? SubscriptionTransferWatcher;

	public List<ulong> SubscribingTo { get; private set; } = new();
	public List<ulong> UnsubscribingFrom { get; private set; } = new();
	public List<ulong> PendingSubscribingTo { get; private set; } = new();
	public List<ulong> PendingUnsubscribingFrom { get; private set; } = new();
	public bool Redownload { get; set; }
	public bool SubscriptionsPending => CrossIO.FileExists(_filePath) && !_citiesManager.IsRunning();

	private readonly IPackageManager _contentManager;
	private readonly ILocationManager _locationManager;
	private readonly ICitiesManager _citiesManager;
	private readonly ILogger _logger;
	private readonly INotifier _notifier;
	private readonly SettingsService _settings;

	public SubscriptionsManager(IPackageManager contentManager, ILocationManager locationManager, ICitiesManager citiesManager, ISettings settings, ILogger logger, INotifier notifier)
	{
		_contentManager = contentManager;
		_locationManager = locationManager;
		_citiesManager = citiesManager;
		_settings = (settings as SettingsService)!;
		_logger = logger;
		_notifier = notifier;

		_filePath = Path.Combine(_locationManager.SkyveAppDataPath, "SubscriptionTransfer.xml");
		_delayedDownloadsAction = new(4000, RunDownload);

		if (CrossIO.FileExists(_filePath))
		{
			var transferData = SharedUtil.Deserialize<SubscriptionTransfer>(_filePath) ?? new();

			UnsubscribingFrom = transferData.UnsubscribingFrom ?? new();

			SubscribingTo = transferData.SubscribeTo ?? new();
		}
	}

	public bool IsSubscribing(IPackage package)
	{
		return package.LocalParentPackage is null ? SubscribingTo.Contains(package.Id) || PendingSubscribingTo.Contains(package.Id) : UnsubscribingFrom.Contains(package.Id) || PendingUnsubscribingFrom.Contains(package.Id);
	}

	public bool Subscribe(IEnumerable<IPackageIdentity> ids)
	{
		return SubscribePrivate(ids.Select(x => x.Id), false);
	}

	public bool UnSubscribe(IEnumerable<IPackageIdentity> ids)
	{
		return SubscribePrivate(ids.Select(x => x.Id), true);
	}

	private bool SubscribePrivate(IEnumerable<ulong> ids, bool unsub)
	{
		try
		{
			if (!ids.Any(x => x != 0))
			{
				return false;
			}

			if (!_citiesManager.IsRunning() && !_settings.SessionSettings.SubscribeFirstTimeShown)
			{
				MessagePrompt.Show(Locale.SubscribingRequiresGameToOpen, Locale.SubscribingRequiresGameToOpenTitle, PromptButtons.OK, PromptIcons.Info, SystemsProgram.MainForm as SlickForm);

				_settings.SessionSettings.SubscribeFirstTimeShown = true;
				_settings.SessionSettings.Save();
			}

			if (_settings.SessionSettings.UserSettings.ForceDownloadAndDeleteAsSoonAsRequested)
			{
				if (unsub)
				{
					_contentManager.DeleteAll(ids);
				}
				else
				{
					_delayedDownloads.AddRange(ids);
					_delayedDownloadsAction.Run();
				}
			}

			var transferData = new SubscriptionTransfer();

			if (CrossIO.FileExists(_filePath))
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
				CrossIO.DeleteFile(_filePath);
			}
		}
		catch (Exception ex) { _logger.Exception(ex, "Failed to send subscription transfer data"); return false; }

		return true;
	}

	private void SubscriptionTransferFileChanged(object sender, FileWatcherEventArgs e)
	{
		_logger.Info($"[Auto] Subscription Transfer triggered from '{e.FullPath}'");

		var transferData = new SubscriptionTransfer();

		if (CrossIO.FileExists(_filePath))
		{
			transferData = SharedUtil.Deserialize<SubscriptionTransfer>(_filePath) ?? new();
		}

		PendingUnsubscribingFrom = UnsubscribingFrom;
		PendingSubscribingTo = SubscribingTo;

		UnsubscribingFrom = transferData.UnsubscribingFrom ?? new();

		SubscribingTo = transferData.SubscribeTo ?? new();
	}

	private void RunDownload()
	{
		var ids = new List<ulong>(_delayedDownloads);

		_delayedDownloads.Clear();

		if (CrossIO.CurrentPlatform is Platform.MacOSX)
		{
			SteamUtil.Download(ids);
		}

		SteamUtil.Download(ids);
	}

	public void Start()
	{
		SubscriptionListWatcher?.Dispose();
		SubscriptionTransferWatcher?.Dispose();

		SubscriptionListWatcher = new FileWatcher
		{
			Path = _locationManager.SkyveAppDataPath,
			NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName,
			Filter = "SubscriptionList.txt"
		};

		SubscriptionListWatcher.Changed += FileChanged;
		SubscriptionListWatcher.Created += FileChanged;

		SubscriptionListWatcher.EnableRaisingEvents = true;

		SubscriptionTransferWatcher = new FileWatcher
		{
			Path = _locationManager.SkyveAppDataPath,
			NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName,
			Filter = "SubscriptionTransfer.xml"
		};

		SubscriptionTransferWatcher.Deleted += SubscriptionTransferFileChanged;

		SubscriptionTransferWatcher.EnableRaisingEvents = true;

		_notifier.ContentLoaded += CentralManager_ContentLoaded;
	}

	private void CentralManager_ContentLoaded()
	{
		PendingSubscribingTo.RemoveAll(x => _contentManager.GetPackageById(new GenericPackageIdentity(x)) is not null);
		PendingUnsubscribingFrom.RemoveAll(x => _contentManager.GetPackageById(new GenericPackageIdentity(x)) is null);
	}

	private void FileChanged(object sender, FileWatcherEventArgs e)
	{
		if (!_settings.SessionSettings.UserSettings.DisablePackageCleanup && CrossIO.FileExists(e.FullPath))
		{
			_logger.Info($"[Auto] Subscription List triggered from '{e.FullPath}'");

			var date = File.GetLastWriteTime(e.FullPath);

			if (DateTime.Now - date < TimeSpan.FromSeconds(5))
			{
				Thread.Sleep(500);

				var list = File.ReadAllLines(e.FullPath).ToList();

				try
				{ CrossIO.DeleteFile(e.FullPath); }
				catch { Thread.Sleep(500); }

				HandleSubscriptions(list);
			}
		}
	}

	private void HandleSubscriptions(List<string> ids)
	{
		foreach (var folder in Directory.EnumerateDirectories(_locationManager.WorkshopContentPath))
		{
			var name = Path.GetFileName(folder);

			if (!ids.Contains(name) && (!CrossIO.FileExists(_filePath) || !SubscribingTo.Any(x => x.ToString() == name)))
			{
				_contentManager.DeleteAll(folder);
			}
			else
			{
				ids.Remove(name);
			}
		}

		if (Redownload)
		{
			Redownload = false;

			SteamUtil.Download(ids.Select(ulong.Parse));
		}
	}

	public void CancelPendingItems()
	{
		if (CrossIO.FileExists(_filePath))
		{
			CrossIO.DeleteFile(_filePath);
		}
	}
}
