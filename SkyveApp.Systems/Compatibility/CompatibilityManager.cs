using Extensions;

using SkyveApp.Domain;
using SkyveApp.Domain.Enums;
using SkyveApp.Domain.Systems;
using SkyveApp.Systems.Compatibility.Domain;
using SkyveApp.Systems.Compatibility.Domain.Api;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace SkyveApp.Systems.Compatibility;

public class CompatibilityManager : ICompatibilityManager
{
	private const string DATA_CACHE_FILE = "CompatibilityDataCache.json";
	private const string SNOOZE_FILE = "CompatibilitySnoozed.json";

	private readonly Dictionary<IPackage, CompatibilityInfo> _cache = new(new IPackageEqualityComparer());
	private readonly List<SnoozedItem> _snoozedItems = new();
	private readonly Regex _bracketsRegex = new(@"[\[\(](.+?)[\]\)]", RegexOptions.Compiled);
	private readonly Regex _urlRegex = new(@"(https?):\/\/(?:([\w-]+)\.)?([\w-]+)\.(\w+)((?:\/[\w-]+)*\/)([\w-]+)+\.([\w]+)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

	private readonly ILocale _locale;
	private readonly ILogger _logger;
	private readonly INotifier _notifier;
	private readonly IPackageManager _contentManager;
	private readonly ICompatibilityUtil _compatibilityUtil;
	private readonly IPackageUtil _contentUtil;
	private readonly IPackageNameUtil _packageUtil;
	private readonly IWorkshopService _workshopService;
	private readonly IDlcManager _dlcManager;
	private readonly SkyveApiUtil _skyveApiUtil;
	private readonly CompatibilityHelper _compatibilityHelper;

	private bool firstLoadComplete;

	public IndexedCompatibilityData CompatibilityData { get; private set; }
	public bool FirstLoadComplete => firstLoadComplete;

	public CompatibilityManager(IPackageManager contentManager, ILogger logger, INotifier notifier, ICompatibilityUtil compatibilityUtil, IPackageUtil contentUtil, ILocale locale, IPackageNameUtil packageUtil, IWorkshopService workshopService, SkyveApiUtil skyveApiUtil, IDlcManager dlcManager)
	{
		_contentManager = contentManager;
		_logger = logger;
		_notifier = notifier;
		_compatibilityUtil = compatibilityUtil;
		_contentUtil = contentUtil;
		_locale = locale;
		_packageUtil = packageUtil;
		_workshopService = workshopService;
		_skyveApiUtil = skyveApiUtil;
		_dlcManager = dlcManager;
		_compatibilityHelper = new CompatibilityHelper(this, contentManager, contentUtil, packageUtil, workshopService, locale);

		CompatibilityData = new(null);

		LoadSnoozedData();

		ConnectionHandler.WhenConnected(() => new BackgroundAction(DownloadData).Run());

		_notifier.ContentLoaded += () => new BackgroundAction(CacheReport).Run();
		_notifier.PackageInclusionUpdated += () => new BackgroundAction(CacheReport).Run();
	}

	public void CacheReport()
	{
		CacheReport(_contentManager.Packages);
	}

	internal void CacheReport(IEnumerable<IPackage> content)
	{
		if (!firstLoadComplete)
		{
			return;
		}

		foreach (var package in content)
		{
			_ = GetCompatibilityInfo(package, true);
		}

		_notifier.OnInformationUpdated();

		_notifier.OnCompatibilityReportProcessed();
	}

	internal void LoadSnoozedData()
	{
		try
		{
			var path = ISave.GetPath(SNOOZE_FILE);

			ISave.Load(out List<SnoozedItem>? data, SNOOZE_FILE);

			if (data is not null)
			{
				_snoozedItems.AddRange(data);
			}
		}
		catch { }
	}

	public void Start(List<ILocalPackageWithContents> packages)
	{
		try
		{
			var path = ISave.GetPath(DATA_CACHE_FILE);

			ISave.Load(out CompatibilityData? data, DATA_CACHE_FILE);

			CompatibilityData = new IndexedCompatibilityData(data);

			foreach (var package in packages)
			{
				_cache[package] = GenerateCompatibilityInfo(package);
			}

			firstLoadComplete = true;
		}
		catch { }
	}

	public async void DownloadData()
	{
		try
		{
			var data = await _skyveApiUtil.Catalogue();

			if (data is not null)
			{
				ISave.Save(data, DATA_CACHE_FILE);

				CompatibilityData = new IndexedCompatibilityData(data);

				CacheReport();
#if DEBUG
				if (System.Diagnostics.Debugger.IsAttached)
				{
					var dic = await _skyveApiUtil.Translations();

					if (dic is not null)
					{
						File.WriteAllText("../../../Properties/CompatibilityNotes.json", Newtonsoft.Json.JsonConvert.SerializeObject(dic, Newtonsoft.Json.Formatting.Indented));
					}
				}
#endif
				return;
			}
		}
		catch (Exception ex)
		{
			_logger.Exception(ex, "Failed to get compatibility data");
		}

		CompatibilityData ??= new IndexedCompatibilityData(new());
	}

	public void ResetSnoozes()
	{
		_snoozedItems.Clear();

		try
		{
			CrossIO.DeleteFile(ISave.GetPath(SNOOZE_FILE));
		}
		catch (Exception ex)
		{
			_logger.Exception(ex, "Failed to clear Snoozes");
		}
	}

	public bool IsSnoozed(ICompatibilityItem reportItem)
	{
		return _snoozedItems.Any(x => x.Equals(reportItem));
	}

	public void ToggleSnoozed(ICompatibilityItem reportItem)
	{
		if (IsSnoozed(reportItem))
		{
			_ = _snoozedItems.RemoveAll(x => x.Equals(reportItem));
		}
		else
		{
			_snoozedItems.Add(new SnoozedItem(reportItem));
		}

		ISave.Save(_snoozedItems, SNOOZE_FILE);

		_notifier.OnRefreshUI();
	}

	public bool IsBlacklisted(IPackageIdentity package)
	{
		return CompatibilityData.BlackListedIds.Contains(package.Id)
			|| CompatibilityData.BlackListedNames.Contains(package.Name ?? string.Empty)
			|| (package.GetWorkshopInfo()?.IsIncompatible ?? false);
	}

	public ICompatibilityInfo GetCompatibilityInfo(IPackage package, bool noCache = false)
	{
		return !firstLoadComplete
			? new CompatibilityInfo(package, null)
			: !noCache && _cache.TryGetValue(package, out var info) ? info : (_cache[package] = GenerateCompatibilityInfo(package));
	}

	public CompatibilityPackageData GetAutomatedReport(IPackage package)
	{
		var info = new CompatibilityPackageData
		{
			Stability = package.IsMod ? PackageStability.NotReviewed : PackageStability.AssetNotReviewed,
			SteamId = package.Id,
			Name = package.Name,
			FileName = package.LocalParentPackage?.Mod?.FilePath,
			Links = new(),
			Interactions = new(),
			Statuses = new(),
		};

		var workshopInfo = package.GetWorkshopInfo();

		if (workshopInfo?.Requirements.Any() ?? false)
		{
			info.Interactions.AddRange(workshopInfo.Requirements.GroupBy(x => x.Optional).Select(o =>
				new PackageInteraction
				{
					Type = o.Key ? InteractionType.OptionalPackages : InteractionType.RequiredPackages,
					Action = StatusAction.SubscribeToPackages,
					Packages = o.ToArray(x => x.Id)
				}));
		}

		var tagMatches = _bracketsRegex.Matches(workshopInfo?.Name ?? string.Empty);

		foreach (Match match in tagMatches)
		{
			var tag = match.Value.ToLower();

			if (tag.ToLower() is "broken")
			{
				info.Stability = PackageStability.Broken;
			}
			else if (tag.ToLower() is "obsolete" or "deprecated" or "abandoned")
			{
				info.Statuses.Add(new(StatusType.Deprecated));
			}
			else if (tag.ToLower() is "alpha" or "experimental" or "beta" or "test" or "testing")
			{
				info.Statuses.Add(new(StatusType.TestVersion));
			}
		}

		_compatibilityUtil.PopulateAutomaticPackageInfo(info, package, workshopInfo);

		if (workshopInfo?.Description is not null)
		{
			var matches = _urlRegex.Matches(workshopInfo.Description);

			foreach (Match match in matches)
			{
				var type = (match.Groups[3].Value.ToLower() + match.Groups[4].Value.ToLower()) switch
				{
					"github.com" => LinkType.Github,
					"discord.com" or "discord.gg" => LinkType.Discord,
					"crowdin.com" => LinkType.Crowdin,
					"buymeacoffee.com" or "patreon.com" or "ko-fi.com" or "paypal.com" => LinkType.Donation,
					_ => LinkType.Other
				};

				if (type is not LinkType.Other)
				{
					info.Links.Add(new PackageLink
					{
						Url = match.Groups[1].Value,
						Type = type,
					});
				}
			}
		}

		return info;
	}

	private CompatibilityInfo GenerateCompatibilityInfo(IPackage package)
	{
#if DEBUG
		var sw = new System.Diagnostics.Stopwatch();
		sw.Start();
#endif

		var packageData = _compatibilityHelper.GetPackageData(package);
		var info = new CompatibilityInfo(package, packageData);
		var workshopInfo = package.GetWorkshopInfo();

		if (package.LocalParentPackage?.Mod is IMod mod)
		{
			var modName = Path.GetFileName(mod.FilePath);
			var duplicate = _contentManager.Mods.AllWhere(x => modName == Path.GetFileName(x.FilePath));

			if (duplicate.Count > 1 && duplicate.Count(_contentUtil.IsIncluded) > 1)
			{
				info.Add(ReportType.Compatibility
					, new PackageInteraction { Type = InteractionType.Identical, Action = StatusAction.SelectOne }
					, _locale.Get($"Interaction_{InteractionType.Identical}")
					, duplicate.Select(x => new PseudoPackage(x)).ToArray());
			}
		}

		if (workshopInfo?.IsIncompatible == true)
		{
			info.Add(ReportType.Stability, new StabilityStatus(PackageStability.Incompatible, null, false), _locale.Get($"Stability_{PackageStability.Incompatible}"), new PseudoPackage[0]);
		}

		if (packageData is null)
		{
			return info;
		}

		_compatibilityUtil.PopulatePackageReport(packageData, info);

		var author = CompatibilityData.Authors.TryGet(packageData.Package.AuthorId) ?? new();

		if (packageData.Package.Stability is not PackageStability.Stable && workshopInfo?.IsIncompatible != true && !author.Malicious)
		{
			info.Add(ReportType.Stability, new StabilityStatus(packageData.Package.Stability, null, false), _locale.Get($"Stability_{packageData.Package.Stability}"), new PseudoPackage[0]);
		}

		foreach (var status in packageData.Statuses)
		{
			foreach (var item in status.Value)
			{
				_compatibilityHelper.HandleStatus(info, item);
			}
		}

		if (!package.IsLocal && package.IsMod && packageData.Package.Type is PackageType.GenericPackage)
		{
			if (!packageData.Statuses.ContainsKey(StatusType.TestVersion) && !packageData.Statuses.ContainsKey(StatusType.SourceAvailable) && !info.Links.Any(x => x.Type is LinkType.Github))
			{
				_compatibilityHelper.HandleStatus(info, new PackageStatus { Type = StatusType.SourceCodeNotAvailable, Action = StatusAction.NoAction });
			}

			if (!packageData.Statuses.ContainsKey(StatusType.TestVersion) && workshopInfo?.Description is not null && workshopInfo.Description.GetWords().Length <= 30)
			{
				_compatibilityHelper.HandleStatus(info, new PackageStatus { Type = StatusType.IncompleteDescription, Action = StatusAction.UnsubscribeThis });
			}

			if (!author.Malicious && workshopInfo?.ServerTime.Date < _compatibilityUtil.MinimumModDate && DateTime.UtcNow - workshopInfo?.ServerTime > TimeSpan.FromDays(365) && !packageData.Statuses.ContainsKey(StatusType.Deprecated))
			{
				_compatibilityHelper.HandleStatus(info, new PackageStatus(StatusType.AutoDeprecated));
			}
		}

		foreach (var interaction in packageData.Interactions)
		{
			foreach (var item in interaction.Value)
			{
				_compatibilityHelper.HandleInteraction(info, item);
			}
		}

		if (packageData.Package.RequiredDLCs?.Any() ?? false)
		{
			var missing = packageData.Package.RequiredDLCs.Where(x => !_dlcManager.IsAvailable(x));

			if (missing.Any())
			{
				_compatibilityHelper.HandleStatus(info, new PackageStatus
				{
					Type = StatusType.MissingDlc,
					Action = StatusAction.NoAction,
					Packages = missing.Select(x => (ulong)x).ToArray()
				});
			}
		}

		if (author.Malicious)
		{
			info.Add(ReportType.Stability, new StabilityStatus(PackageStability.Broken, null, false) { Action = StatusAction.UnsubscribeThis }, _locale.Get($"AuthorMalicious").Format(_packageUtil.CleanName(package, true), (workshopInfo?.Author?.Name).IfEmpty(author.Name)), new PseudoPackage[0]);
		}
		else if (package.IsMod && author.Retired)
		{
			info.Add(ReportType.Stability, new StabilityStatus(PackageStability.AuthorRetired, null, false), _locale.Get($"AuthorRetired").Format(_packageUtil.CleanName(package, true), (workshopInfo?.Author?.Name).IfEmpty(author.Name)), new PseudoPackage[0]);
		}

		if (!string.IsNullOrEmpty(packageData.Package.Note))
		{
			info.Add(ReportType.Stability, new StabilityStatus(PackageStability.NotReviewed, packageData.Package.Note, false), string.Empty, new PseudoPackage[0]);
		}

		if (package.IsLocal)
		{
			info.Add(ReportType.Stability, new StabilityStatus(PackageStability.Local, null, false), _locale.Get($"Stability_{PackageStability.Local}").Format(_packageUtil.CleanName(_workshopService.GetPackage(new GenericPackageIdentity(packageData.Package.SteamId)), true)), new PseudoPackage[] { new(packageData.Package.SteamId, _workshopService) });
		}

		if (!package.IsLocal && !author.Malicious && workshopInfo?.IsIncompatible != true)
		{
			info.Add(ReportType.Stability, new StabilityStatus(PackageStability.Stable, string.Empty, true), (packageData.Package.Stability is not PackageStability.NotReviewed and not PackageStability.AssetNotReviewed ? _locale.Get("LastReviewDate").Format(packageData.Package.ReviewDate.ToReadableString(packageData.Package.ReviewDate.Year != DateTime.Now.Year, ExtensionClass.DateFormat.TDMY)) + "\r\n\r\n" : string.Empty) + _locale.Get("RequestReviewInfo"), new PseudoPackage[0]);
		}

#if DEBUG
		sw.Stop();
		if (sw.ElapsedMilliseconds > 100)
		{
			_logger.Debug($"CR ({sw.Elapsed.ToReadableString()}) for {package.Name}");
		}
#endif

		return info;
	}

	public void ResetCache()
	{
		_cache.Clear();

		try
		{
			CrossIO.DeleteFile(ISave.GetPath(DATA_CACHE_FILE));
		}
		catch (Exception ex)
		{
			_logger.Exception(ex, "Failed to clear CR cache");
		}

		CacheReport();
	}

	public IPackageIdentity GetFinalSuccessor(IPackageIdentity package)
	{
		return _compatibilityHelper.GetFinalSuccessor(package);
	}

	public IPackageCompatibilityInfo? GetPackageInfo(IPackageIdentity package)
	{
		return _compatibilityHelper.GetPackageData(package)?.Package;
	}

	public NotificationType GetNotification(ICompatibilityInfo info)
	{
		return info.ReportItems.Count == 0 ? NotificationType.None : info.ReportItems.Max(x => IsSnoozed(x) ? 0 : x.Status.Notification);
	}

	public ulong GetIdFromModName(string fileName)
	{
		return CompatibilityData.PackageNames.TryGet(fileName);
	}

	public bool IsUserVerified(IUser author)
	{
		return CompatibilityData.Authors.TryGet(ulong.Parse(author.Id.ToString()))?.Verified ?? false;
	}
}
