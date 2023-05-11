using Extensions;

using LoadOrderToolTwo.Domain.Compatibility;
using LoadOrderToolTwo.Domain.Interfaces;

using Newtonsoft.Json;

using SlickControls;

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;

namespace LoadOrderToolTwo.Utilities.Managers;
public static class CompatibilityManager
{
	private const string DATA_CACHE_FILE = "CompatibilityDataCache.json";
	private static IndexedCompatibilityData compatibilityData;
	private static readonly Dictionary<IPackage, CompatibilityInfo> _cache = new();

	public static IndexedCompatibilityData CompatibilityData  => compatibilityData;

	static CompatibilityManager()
	{
		compatibilityData = new(null);

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

			compatibilityData = new IndexedCompatibilityData(data);
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
				compatibilityData = new IndexedCompatibilityData(data);

				ISave.Save(data, DATA_CACHE_FILE);

				CacheReport();

				return;
			}
		}
		catch (Exception ex)
		{
			Log.Exception(ex, "Failed to get compatibility data");
		}

		compatibilityData ??= new IndexedCompatibilityData(new());
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

		foreach (var item in packageData.Statuses)
		{
			info.Add(ReportType.Status, item.Value.Status, item.Value.Status.Note ?? string.Empty);
		}

		foreach (var item in packageData.Interactions)
		{
			HandleInteraction(info, item.Value.Interaction);
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

	private static void HandleInteraction(CompatibilityInfo info, PackageInteraction interaction)
	{
		var requiresPackage = interaction.Type is not InteractionType.Successor and not InteractionType.Alternative; // the interaction requires both mods present to be eligible

		var packages = interaction.Packages.AllWhere(x => GetPackage(x) is null == requiresPackage);

		if (!packages.Any())
			return;

		var reportType = interaction.Type switch
		{
			InteractionType.Successor => ReportType.Successors,
			_ => ReportType.Note
		};

		var text = interaction.Type switch
		{
			InteractionType.Successor => LocaleCR.CR_SuccessorsAvailable.ToString(),
			_ => string.Empty
		};

		info.Add(reportType, interaction, text);
	}

	private static IPackage? GetPackage(ulong steamId)
	{
		var package = CompatibilityData.Packages.TryGet(steamId);

		if (package is not null)
			return FindPackage(package.Package);

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
			info.Interactions.Add(new PackageInteraction { Type = InteractionType.Required, Action = InteractionAction.SubscribeToPackages, Packages = package.RequiredPackages });
		}

		package.ToString().RemoveVersionText(out var titleTags);

		foreach (var tag in titleTags)
		{
			if (tag.ToLower() is "obsolete" or "deprecated" or "abandoned")
			{
				info.Stability = PackageStability.Broken;
			}
			else if (tag.ToLower() is "alpha" or "experimental" or "beta" or "test" or "testing")
			{
				info.Statuses.Add(new PackageStatus { Type = StatusType.TestVersion });
			}
		}

		const ulong MUSIC_MOD_ID = 2474585115;

		if ((package.RequiredPackages?.Contains(MUSIC_MOD_ID) ??false))
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

		if (package.IsMod && !info.Links.Any(x => x.Type is LinkType.Github))
		{
			info.Statuses.Add(new PackageStatus { Type = StatusType.SourceCodeNotAvailable });
		}

		if (package.IsMod && (package.SteamDescription is null || package.SteamDescription.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries).Length <= 3))
		{
			info.Statuses.Add(new PackageStatus { Type = StatusType.IncompleteDescription });
		}

		return info;
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

	public static Color GetColor(this NotificationType notification)
	{
		return notification switch
		{
			NotificationType.Info => FormDesign.Design.InfoColor,
			NotificationType.MissingDependency => FormDesign.Design.YellowColor,
			NotificationType.Caution => FormDesign.Design.YellowColor,
			NotificationType.Warning => FormDesign.Design.YellowColor.MergeColor(FormDesign.Design.RedColor),
			NotificationType.AttentionRequired => FormDesign.Design.YellowColor.MergeColor(FormDesign.Design.RedColor),
			NotificationType.Switch => FormDesign.Design.RedColor,
			NotificationType.Unsubscribe => FormDesign.Design.RedColor,
			_ => FormDesign.Design.GreenColor
		};
	}
}
