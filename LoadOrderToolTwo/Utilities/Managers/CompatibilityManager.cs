using Extensions;

using LoadOrderToolTwo.Domain.Compatibility;
using LoadOrderToolTwo.Domain.Interfaces;

using SlickControls;

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace LoadOrderToolTwo.Utilities.Managers;
public static class CompatibilityManager
{
	private const string DATA_CACHE_FILE = "CompatibilityDataCache.json";
	private static readonly Dictionary<IPackage, CompatibilityInfo> _cache = new();

	public static IndexedCompatibilityData CompatibilityData { get; private set; }

	static CompatibilityManager()
	{
		CompatibilityData = new(null);

		LoadDataCached();

		ConnectionHandler.WhenConnected(DownloadData);

		CentralManager.ContentLoaded += () => new BackgroundAction(CacheReport).Run();
		CentralManager.PackageInformationUpdated += () => new BackgroundAction(CacheReport).Run();
	}

	internal static void CacheReport()
	{
		CacheReport(CentralManager.Packages);
	}

	internal static void CacheReport(IEnumerable<Domain.Package> content)
	{
		foreach (var package in content)
		{
			GetCompatibilityReport(package);
		}
	}

	private static void LoadDataCached()
	{
		try
		{
			var path = ISave.GetPath(DATA_CACHE_FILE);

			ISave.Load(out CompatibilityData? data, DATA_CACHE_FILE);

			CompatibilityData = new IndexedCompatibilityData(data);
		}
		catch { }
	}

	private static async void DownloadData()
	{
		try
		{
			var data = await CompatibilityApiUtil.Get<CompatibilityData>("/Catalogue");

			if (data is not null)
			{
				CompatibilityData = new IndexedCompatibilityData(data);

				ISave.Save(data, DATA_CACHE_FILE);

				CacheReport();

				return;
			}
		}
		catch (Exception ex)
		{
			Log.Exception(ex, "Failed to get compatibility data");
		}

		CompatibilityData ??= new IndexedCompatibilityData(new());
	}

	public static bool IsBlacklisted(IPackage package)
	{
		return CompatibilityData.BlackListedIds.Contains(package.SteamId)
			|| CompatibilityData.BlackListedNames.Contains(package.Name ?? string.Empty);
	}

	internal static Domain.Package? FindPackage(Package package)
	{
		var localPackage = CentralManager.Packages.FirstOrDefault(x => x.SteamId == package.SteamId);

		if (localPackage is not null)
		{
			return localPackage;
		}

		return CentralManager.Mods.FirstOrDefault(x => Path.GetFileName(x.FileName) == package.FileName)?.Package;
	}

	internal static CompatibilityInfo GetCompatibilityInfo(this IPackage package)
	{
		if (_cache.TryGetValue(package, out var info))
		{
			return info;
		}

		return _cache[package] = GetCompatibilityReport(package);
	}

	private static CompatibilityInfo GetCompatibilityReport(IPackage package)
	{
		var packageData = GetPackageData(package);
		var info = new CompatibilityInfo(package, packageData);

		if (packageData is null)
		{
			return info;
		}

		info.Add(ReportType.Stability, new StabilityStatus(packageData.Package.Stability, packageData.Package.Note), LocaleCR.Get($"Stability_{packageData.Package.Stability}"), new ulong[0]);

		foreach (var status in packageData.Statuses)
		{
			foreach (var item in status.Value)
			{
				HandleStatus(info, status.Key, item);
			}
		}

		foreach (var interaction in packageData.Interactions)
		{
			foreach (var item in interaction.Value)
			{
				HandleInteraction(info, interaction.Key, item);
			}
		}

		if (packageData.Package.RequiredDLCs?.Any() ?? false)
		{
			var missing = packageData.Package.RequiredDLCs.Where(x => !SteamUtil.IsDlcInstalledLocally(x));
			
			if (missing.Any())
			{
				HandleStatus(info, StatusType.MissingDlc, new IndexedPackageStatus(new PackageStatus
				{
					Type = StatusType.MissingDlc,
					Action = StatusAction.NoAction,
				}, missing.ToDictionary(x => (ulong)x, x => packageData)));
			}
		}

		return info;
	}

	private static IndexedPackage? GetPackageData(IPackage package)
	{
		if (package.Workshop)
		{
			var packageData = CompatibilityData.Packages.TryGet(package.SteamId);

			if (packageData is null)
			{
				packageData = new IndexedPackage(GetAutomatedReport(package));

				packageData.Load(CompatibilityData.Packages);
			}

			return packageData;
		}

		if (package.Package?.Mod is not null)
		{
			return CompatibilityData.Packages.Values.FirstOrDefault(x => x.Package.FileName == Path.GetFileName(package.Package.Mod.FileName));
		}

		return null;
	}

	private static void HandleStatus(CompatibilityInfo info, StatusType key, IndexedPackageStatus status)
	{
		if (key == StatusType.DependencyMod && ContentUtil.GetReferencingPackage(info.Package.SteamId, true).Any())
		{
			return;
		}

		var type = key switch
		{
			StatusType.Deprecated or StatusType.CausesIssues or StatusType.SavesCantLoadWithoutIt => ReportType.Stability,
			StatusType.DependencyMod or StatusType.TestVersion or StatusType.MusicCanBeCopyrighted => ReportType.Status,
			StatusType.SourceCodeNotAvailable or StatusType.IncompleteDescription or StatusType.Reupload => ReportType.Ambiguous,
			StatusType.MissingDlc => ReportType.DlcMissing,
			_ => ReportType.Status,
		};

		var translation = LocaleCR.Get($"Status_{key}");
		var action = LocaleCR.Get($"Action_{status.Status.Action}");
		var text = (status.Status.Packages?.Length ?? 0) switch { 0 => translation.Zero, 1 => translation.One, _ => translation.Plural } ?? translation.One;
		var actionText = (status.Status.Packages?.Length ?? 0) switch { 0 => action.Zero, 1 => action.One, _ => action.Plural } ?? action.One;
		var message = string.Format($"{text}\r\n\r\n{actionText}", info.Package.CleanName(), status.Status.Packages?.FirstOrDefault().Value?.Package.Name?.RemoveVersionText(out _)).Trim();

		info.Add(type, status.Status, message, status.Status.Packages ?? new ulong[0]);
	}

	private static void HandleInteraction(CompatibilityInfo info, InteractionType key, IndexedPackageInteraction interaction)
	{
		if (key is InteractionType.Successor or InteractionType.RequirementAlternative)
		{
			return;
		}

		var packages = interaction.Interaction.Packages?.ToList() ?? new();

		if (key is InteractionType.SameFunctionality or InteractionType.CausesIssuesWith or InteractionType.IncompatibleWith)
		{
			packages.RemoveAll(x => GetPackage(x) is null);
		}
		else if (key is InteractionType.RequiredPackages)
		{
			packages.RemoveAll(x => GetPackage(x) is not null);
		}

		if (packages.Count == 0)
			return;

		var type = key switch
		{
			InteractionType.SucceededBy => ReportType.Successors,
			InteractionType.Alternative => ReportType.Alternatives,
			InteractionType.SameFunctionality or InteractionType.CausesIssuesWith or InteractionType.IncompatibleWith => ReportType.Compatibility,
			InteractionType.RequiredPackages => ReportType.RequiredPackages,
			_ => ReportType.Note
		};

		var translation = LocaleCR.Get($"Interaction_{key}");
		var action = LocaleCR.Get($"Action_{interaction.Interaction.Action}");
		var text = packages.Count switch { 0 => translation.Zero, 1 => translation.One, _ => translation.Plural } ?? translation.One;
		var actionText = packages.Count switch { 0 => action.Zero, 1 => action.One, _ => action.Plural } ?? action.One;
		var message = string.Format($"{text}\r\n\r\n{actionText}", info.Package.CleanName(), packages.FirstOrDefault().Value?.Package.Name?.RemoveVersionText(out _)).Trim();

		info.Add(type, interaction.Interaction, message, packages.ToArray());
	}

	private static IPackage? GetPackage(ulong steamId)
	{
		var package = CompatibilityData.Packages.TryGet(steamId);

		if (package is not null)
		{
			return FindPackage(package.Package);
		}

		return CentralManager.Packages.FirstOrDefault(x => x.SteamId == steamId);
	}

	internal static Package GetAutomatedReport(IPackage package)
	{
		var info = new Package
		{
			Stability = PackageStability.NotReviewed,
			SteamId = package.SteamId,
			Name = package.Name,
			FileName = package.Package?.Mod?.FileName,
			Links = new(),
			Interactions = new(),
			Statuses = new(),
		};

		if (package.RequiredPackages?.Any() ?? false)
		{
			info.Interactions.Add(new PackageInteraction { Type = InteractionType.RequiredPackages, Action = StatusAction.SubscribeToPackages, Packages = package.RequiredPackages });
		}

		package.ToString().RemoveVersionText(out var titleTags);

		foreach (var tag in titleTags)
		{
			if (tag.ToLower() is "obsolete" or "deprecated" or "abandoned" or "broken")
			{
				info.Stability = PackageStability.Broken;
			}
			else if (tag.ToLower() is "alpha" or "experimental" or "beta" or "test" or "testing")
			{
				info.Statuses.Add(new PackageStatus { Type = StatusType.TestVersion });
			}
		}

		const ulong MUSIC_MOD_ID = 2474585115;

		if (package.RequiredPackages?.Contains(MUSIC_MOD_ID) ?? false)
		{
			info.Statuses.Add(new PackageStatus { Type = StatusType.MusicCanBeCopyrighted });
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

		if (package.IsMod && DateTime.UtcNow - package.ServerTime > TimeSpan.FromDays(365))
		{
			info.Statuses.Add(new PackageStatus { Type = StatusType.Deprecated });
		}

		if (package.IsMod && !info.Links.Any(x => x.Type is LinkType.Github))
		{
			info.Statuses.Add(new PackageStatus { Type = StatusType.SourceCodeNotAvailable, Action = StatusAction.UnsubscribeThis });
		}

		if (package.IsMod && (package.SteamDescription is null || package.SteamDescription.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries).Length <= 3))
		{
			info.Statuses.Add(new PackageStatus { Type = StatusType.IncompleteDescription, Action = StatusAction.UnsubscribeThis });
		}

		return info;
	}

	public static DynamicIcon GetIcon(this LinkType link)
	{
		return link switch
		{
			LinkType.Website => "I_Globe",
			LinkType.Github => "I_Github",
			LinkType.Crowdin => "I_Translate",
			LinkType.Donation => "I_Donate",
			LinkType.Discord => "I_Discord",
			_ => "I_Share",
		};
	}

	public static DynamicIcon GetIcon(this NotificationType notification, bool status)
	{
		return notification switch
		{
			NotificationType.Info => "I_Info",
			NotificationType.MissingDependency => "I_MissingMod",
			NotificationType.Caution => "I_Remarks",
			NotificationType.Warning => "I_MinorIssues",
			NotificationType.AttentionRequired => "I_MajorIssues",
			NotificationType.Switch => "I_Switch",
			NotificationType.Unsubscribe => "I_Broken",
			NotificationType.None or _ => status ? "I_Ok" : "I_Info",
		};
	}

	public static Color GetColor(this NotificationType notification)
	{
		return notification switch
		{
			NotificationType.Info => FormDesign.Design.InfoColor,

			NotificationType.MissingDependency or
			NotificationType.Caution => FormDesign.Design.YellowColor,

			NotificationType.Warning or
			NotificationType.AttentionRequired => FormDesign.Design.YellowColor.MergeColor(FormDesign.Design.RedColor),

			NotificationType.Switch or
			NotificationType.Unsubscribe => FormDesign.Design.RedColor,

			_ => FormDesign.Design.GreenColor
		};
	}
}
