using Extensions;

using SkyveApp.Domain.Interfaces;
using SkyveApp.Domain.Utilities;
using SkyveApp.Services.Interfaces;
using SkyveApp.Utilities;
using SkyveShared;

using SlickControls;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace SkyveApp.Services;
internal class SubscriptionsManager : ISubscriptionsManager
{
    private readonly string _filePath;
    private readonly List<ulong> _delayedDownloads = new();
    private readonly DelayedAction _delayedDownloadsAction;
    private FileSystemWatcher? SubscriptionListWatcher;
    private FileSystemWatcher? SubscriptionTransferWatcher;

    public List<ulong> SubscribingTo { get; private set; } = new();
    public List<ulong> UnsubscribingFrom { get; private set; } = new();
    public List<ulong> PendingSubscribingTo { get; private set; } = new();
    public List<ulong> PendingUnsubscribingFrom { get; private set; } = new();
    public bool Redownload { get; set; }
    public bool SubscriptionsPending => CrossIO.FileExists(_filePath) && !_citiesManager.IsRunning();

    private readonly IContentManager _contentManager;
    private readonly ILocationManager _locationManager;
    private readonly ICitiesManager _citiesManager;
    private readonly ISettings _settings;
    private readonly ILogger _logger;

	public SubscriptionsManager(IContentManager contentManager, ILocationManager locationManager, ICitiesManager citiesManager, ISettings settings, ILogger logger)
	{
		_contentManager = contentManager;
		_locationManager = locationManager;
		_citiesManager = citiesManager;
		_settings = settings;
		_logger = logger;

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
        return package.Package is null ? SubscribingTo.Contains(package.SteamId) || PendingSubscribingTo.Contains(package.SteamId) : UnsubscribingFrom.Contains(package.SteamId) || PendingUnsubscribingFrom.Contains(package.SteamId);
    }

    public bool Subscribe(IEnumerable<ulong> ids)
    {
        return SubscribePrivate(ids, false);
    }

    public bool UnSubscribe(IEnumerable<ulong> ids)
    {
        return SubscribePrivate(ids, true);
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
                MessagePrompt.Show(Locale.SubscribingRequiresGameToOpen, Locale.SubscribingRequiresGameToOpenTitle, PromptButtons.OK, PromptIcons.Info, Program.MainForm);

                _settings.SessionSettings.SubscribeFirstTimeShown = true;
				_settings.SessionSettings.Save();
            }

            if (_settings.SessionSettings.UserSettings.ForceDownloadAndDeleteAsSoonAsRequested)
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

    private void SubscriptionTransferFileChanged(object sender, FileSystemEventArgs e)
    {
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

        SubscriptionListWatcher = new FileSystemWatcher
        {
            Path = _locationManager.SkyveAppDataPath,
            NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName,
            Filter = "SubscriptionList.txt"
        };

        SubscriptionListWatcher.Changed += new FileSystemEventHandler(FileChanged);
        SubscriptionListWatcher.Created += new FileSystemEventHandler(FileChanged);

        SubscriptionListWatcher.EnableRaisingEvents = true;

        SubscriptionTransferWatcher = new FileSystemWatcher
        {
            Path = _locationManager.SkyveAppDataPath,
            NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName,
            Filter = "SubscriptionTransfer.xml"
        };

        SubscriptionTransferWatcher.Deleted += new FileSystemEventHandler(SubscriptionTransferFileChanged);

        SubscriptionTransferWatcher.EnableRaisingEvents = true;

        _contentManager.ContentLoaded += CentralManager_ContentLoaded;
    }

    private void CentralManager_ContentLoaded()
    {
        PendingSubscribingTo.RemoveAll(x => _contentManager.GetPackage(x) is not null);
        PendingUnsubscribingFrom.RemoveAll(x => _contentManager.GetPackage(x) is null);
    }

    private void FileChanged(object sender, FileSystemEventArgs e)
    {
        if (!_settings.SessionSettings.UserSettings.DisablePackageCleanup && CrossIO.FileExists(e.FullPath))
        {
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

            SteamUtil.Download(ids.Select(ulong.Parse));
        }
    }

    internal void CancelPendingItems()
    {
        if (CrossIO.FileExists(_filePath))
        {
            CrossIO.DeleteFile(_filePath);
        }
    }
}
