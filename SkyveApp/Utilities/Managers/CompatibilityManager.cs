using Extensions;

using SkyveApp.Domain.Compatibility;
using SkyveApp.Domain.Interfaces;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace SkyveApp.Utilities.Managers;

public static class CompatibilityManager
{
	private const ulong MUSIC_MOD_ID = 2474585115;
	private const ulong IMT_MOD_ID = 2140418403;
	private const ulong THEMEMIXER_MOD_ID = 2954236385;
	private const ulong RENDERIT_MOD_ID = 1794015399;
	private const ulong PO_MOD_ID = 1094334744;
	private const string DATA_CACHE_FILE = "CompatibilityDataCache.json";
	private const string SNOOZE_FILE = "CompatibilitySnoozed.json";
	private static readonly Dictionary<IPackage, CompatibilityInfo> _cache = new(new Domain.IPackageEqualityComparer());
	private static readonly List<SnoozedItem> _snoozedItems = new();

	public static IndexedCompatibilityData CompatibilityData { get; private set; }
	public static Author User { get; private set; }
	public static bool FirstLoadComplete { get; set; }

	public static event Action? ReportProcessed;

	static CompatibilityManager()
	{
		User = new();
		CompatibilityData = new(null);

		LoadSnoozedData();

		LoadCachedData();

		ConnectionHandler.WhenConnected(() => new BackgroundAction(DownloadData).Run());

		CentralManager.ContentLoaded += () => new BackgroundAction(CacheReport).Run();
		CentralManager.PackageInclusionUpdated += () => new BackgroundAction(CacheReport).Run();

		new BackgroundAction(RefreshUserState).RunEvery(60000, true);
	}

	internal static void CacheReport()
	{
		CacheReport(CentralManager.Packages);
	}

	internal static void CacheReport(IEnumerable<Domain.Package> content)
	{
		foreach (var package in content)
		{
			GetCompatibilityInfo(package, true);
		}

		CentralManager.OnInformationUpdated();

		ReportProcessed?.Invoke();
	}

	internal static void LoadSnoozedData()
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

	internal static void LoadCachedData()
	{
		try
		{
			var path = ISave.GetPath(DATA_CACHE_FILE);

			ISave.Load(out CompatibilityData? data, DATA_CACHE_FILE);

			CompatibilityData = new IndexedCompatibilityData(data);
		}
		catch { }
	}

	internal static async void DownloadData()
	{
		try
		{
			var data = await CompatibilityApiUtil.Catalogue();

			if (data is not null)
			{
				ISave.Save(data, DATA_CACHE_FILE);

				CompatibilityData = new IndexedCompatibilityData(data);

				CacheReport();
#if DEBUG
				if (System.Diagnostics.Debugger.IsAttached)
				{
					var dic = await CompatibilityApiUtil.Translations();

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
			Log.Exception(ex, "Failed to get compatibility data", System.Diagnostics.Debugger.IsAttached);
		}

		CompatibilityData ??= new IndexedCompatibilityData(new());
	}

	internal static bool IsSnoozed(ReportItem reportItem)
	{
		return _snoozedItems.Any(x => x.Equals(reportItem));
	}

	internal static void ToggleSnoozed(ReportItem reportItem)
	{
		if (IsSnoozed(reportItem))
		{
			_snoozedItems.RemoveAll(x => x.Equals(reportItem));
		}
		else
		{
			_snoozedItems.Add(new SnoozedItem(reportItem));
		}

		ISave.Save(_snoozedItems, SNOOZE_FILE);

		Program.MainForm?.TryInvoke(() => Program.MainForm.Invalidate(true));
	}

	internal static bool IsBlacklisted(IPackage package)
	{
		return CompatibilityData.BlackListedIds.Contains(package.SteamId)
			|| CompatibilityData.BlackListedNames.Contains(package.Name ?? string.Empty)
			|| package.Incompatible;
	}

	internal static CompatibilityInfo GetCompatibilityInfo(this IPackage package, bool noCache = false)
	{
		if (!FirstLoadComplete && !noCache)
		{
			return new CompatibilityInfo(package, null);
		}

		if (!noCache && _cache.TryGetValue(package, out var info))
		{
			return info;
		}

		return _cache[package] = GenerateCompatibilityInfo(package);
	}

	internal static Package GetAutomatedReport(IPackage package)
	{
		var info = new Package
		{
			Stability = package.IsMod ? PackageStability.NotReviewed : PackageStability.AssetNotReviewed,
			SteamId = package.SteamId,
			Name = package.Name,
			FileName = package.Package?.Mod?.FileName,
			Links = new(),
			Interactions = new(),
			Statuses = new(),
		};

		if (package.RequiredPackages?.Any() ?? false)
		{
			info.Interactions.Add(new PackageInteraction { Type = package.IsMod ? InteractionType.RequiredPackages : InteractionType.OptionalPackages, Action = StatusAction.SubscribeToPackages, Packages = package.RequiredPackages });
		}

		var tagMatches = Regex.Matches(package.ToString(), @"[\[\(](.+?)[\]\)]");

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

		if (package.RequiredPackages?.Contains(MUSIC_MOD_ID) ?? false)
		{
			info.Statuses.Add(new(StatusType.MusicCanBeCopyrighted));
		}

		if (package.SteamDescription is not null)
		{
			var matches = Regex.Matches(package.SteamDescription, @"\[url\=(https://(?:www\.)?(.+?)/.*?)\]");

			foreach (Match match in matches)
			{
				var type = match.Groups[2].Value.ToLower() switch
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

	private static CompatibilityInfo GenerateCompatibilityInfo(IPackage package)
	{
		var packageData = CompatibilityUtil.GetPackageData(package);
		var info = new CompatibilityInfo(package, packageData);

		if (package.Package?.Mod is not null)
		{
			var modName = Path.GetFileName(package.Package.Mod.FileName);
			var duplicate = CentralManager.Mods.AllWhere(x => x.IsIncluded && modName == Path.GetFileName(x.FileName));

			if (duplicate.Count > 1)
			{
				info.Add(ReportType.Compatibility
					, new PackageInteraction { Type = InteractionType.Identical, Action = StatusAction.SelectOne }
					, LocaleCR.Get($"Interaction_{InteractionType.Identical}")
					, duplicate.Select(x => new PseudoPackage(x)).ToArray());
			}
		}

		if (package.Workshop && package.Incompatible)
		{
			info.Add(ReportType.Stability, new StabilityStatus(PackageStability.Incompatible, null, false), LocaleCR.Get($"Stability_{PackageStability.Incompatible}"), new PseudoPackage[0]);
		}

		if (packageData is null)
		{
			return info;
		}

		var author = CompatibilityData.Authors[packageData.Package.AuthorId];

		if ((package.Workshop || packageData.Package.Stability is not PackageStability.Stable) && !package.Incompatible && !author.Malicious)
		{
			info.Add(ReportType.Stability, new StabilityStatus(packageData.Package.Stability, null, false), LocaleCR.Get($"Stability_{packageData.Package.Stability}"), new PseudoPackage[0]);
		}

		foreach (var status in packageData.Statuses)
		{
			foreach (var item in status.Value)
			{
				CompatibilityUtil.HandleStatus(info, item);
			}
		}

		if (packageData.Package.Type is PackageType.GenericPackage)
		{
			if (package.IsMod && !packageData.Statuses.ContainsKey(StatusType.TestVersion) && !packageData.Statuses.ContainsKey(StatusType.SourceAvailable) && !info.Links.Any(x => x.Type is LinkType.Github))
			{
				CompatibilityUtil.HandleStatus(info, new PackageStatus { Type = StatusType.SourceCodeNotAvailable, Action = StatusAction.UnsubscribeThis });
			}

			if (package.IsMod && !packageData.Statuses.ContainsKey(StatusType.TestVersion) && package.SteamDescription is not null && package.SteamDescription.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries).Length <= 3)
			{
				CompatibilityUtil.HandleStatus(info, new PackageStatus { Type = StatusType.IncompleteDescription, Action = StatusAction.UnsubscribeThis });
			}

			if (package.IsMod && !author.Malicious && DateTime.UtcNow - package.ServerTime > TimeSpan.FromDays(365) && !packageData.Statuses.ContainsKey(StatusType.Deprecated))
			{
				CompatibilityUtil.HandleStatus(info, new PackageStatus(StatusType.Deprecated));
			}
		}

		if (packageData.Package.Type is PackageType.CSM && !packageData.Statuses.ContainsKey(StatusType.Reupload))
		{
			CompatibilityUtil.HandleStatus(info, new PackageStatus(StatusType.Reupload, StatusAction.Switch) { Packages = new ulong[] { 1558438291 } });
		}

		foreach (var interaction in packageData.Interactions)
		{
			foreach (var item in interaction.Value)
			{
				CompatibilityUtil.HandleInteraction(info, item);
			}
		}

		if (packageData.Package.Type is PackageType.MusicPack or PackageType.ThemeMix or PackageType.IMTMarkings or PackageType.RenderItPreset or PackageType.POFont or PackageType.ContentPackage && !packageData.Interactions.ContainsKey(InteractionType.RequiredPackages))
		{
			CompatibilityUtil.HandleInteraction(info, new PackageInteraction(InteractionType.RequiredPackages, StatusAction.SubscribeToPackages)
			{
				Packages = new ulong[]
				{
					packageData.Package.Type switch
					{
						PackageType.MusicPack => MUSIC_MOD_ID,
						PackageType.ThemeMix => THEMEMIXER_MOD_ID,
						PackageType.IMTMarkings => IMT_MOD_ID,
						PackageType.RenderItPreset => RENDERIT_MOD_ID,
						PackageType.POFont => PO_MOD_ID,
						_ => 0
					}
				}
			});
		}

		if (packageData.Package.RequiredDLCs?.Any() ?? false)
		{
			var missing = packageData.Package.RequiredDLCs.Where(x => !SteamUtil.IsDlcInstalledLocally(x));

			if (missing.Any())
			{
				CompatibilityUtil.HandleStatus(info, new PackageStatus
				{
					Type = StatusType.MissingDlc,
					Action = StatusAction.NoAction,
					Packages = missing.Select(x => (ulong)x).ToArray()
				});
			}
		}

		if (author.Malicious)
		{
			info.Add(ReportType.Stability, new StabilityStatus(PackageStability.Broken, null, false) { Action = StatusAction.UnsubscribeThis }, LocaleCR.Get($"AuthorMalicious").Format(package.CleanName(), (package.Author?.Name).IfEmpty(author.Name)), new PseudoPackage[0]);
		}
		else if (package.IsMod && author.Retired)
		{
			info.Add(ReportType.Stability, new StabilityStatus(PackageStability.AuthorRetired, null, false), LocaleCR.Get($"AuthorRetired").Format(package.CleanName(), (package.Author?.Name).IfEmpty(author.Name)), new PseudoPackage[0]);
		}

		if (!string.IsNullOrEmpty(packageData.Package.Note))
		{
			info.Add(ReportType.Stability, new StabilityStatus(PackageStability.NotReviewed, packageData.Package.Note, false), string.Empty, new ulong[0]);
		}

		if (!package.Workshop)
		{
			info.Add(ReportType.Stability, new StabilityStatus(PackageStability.Local, null, false), LocaleCR.Get($"Stability_{PackageStability.Local}").Format(SteamUtil.GetItem(packageData.Package.SteamId)?.CleanName()), new PseudoPackage[] { new(packageData.Package.SteamId) });
		}
		else if (info.Notification < NotificationType.Unsubscribe)
		{
			info.Add(ReportType.Stability, new StabilityStatus(PackageStability.Stable, string.Empty, true), ((packageData.Package.Stability is not PackageStability.NotReviewed and not PackageStability.AssetNotReviewed) ? (LocaleCR.LastReviewDate.Format(packageData.Package.ReviewDate.ToReadableString(packageData.Package.ReviewDate.Year != DateTime.Now.Year, ExtensionClass.DateFormat.TDMY)) + "\r\n\r\n") : string.Empty) + LocaleCR.RequestReviewInfo, new PseudoPackage[0]);
		}

		return info;
	}

	private static async void RefreshUserState()
	{
		var steamId = SteamUtil.GetLoggedInSteamId();

		if (steamId == 0)
		{
			User = new();
		}

		User = CompatibilityData.Authors.TryGet(steamId) ?? new Author { SteamId = steamId };

		try
		{ User.Manager = await CompatibilityApiUtil.IsCommunityManager(); }
		catch
		{ User.Manager = false; }
	}
}
