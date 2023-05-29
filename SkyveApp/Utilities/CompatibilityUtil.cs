using Extensions;

using SkyveApp.Domain.Compatibility;
using SkyveApp.Domain.Interfaces;
using SkyveApp.Utilities.Managers;

using SlickControls;

using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;

namespace SkyveApp.Utilities;
internal static class CompatibilityUtil
{
	internal static void HandleStatus(CompatibilityInfo info, IndexedPackageStatus status)
	{
		var type = status.Status.Type;

		if (type is StatusType.SourceAvailable)
		{
			return;
		}

		if (type is StatusType.DependencyMod && ContentUtil.GetReferencingPackage(info.Package.SteamId, true).Any())
		{
			return;
		}

		if (type is StatusType.Deprecated && status.Status.Action is StatusAction.Switch && (status.Status.Packages?.Any() ?? false))
		{
			if ((info.Data?.Interactions.ContainsKey(InteractionType.SucceededBy) ?? false) || HandleSucceededBy(info, status.Status.Packages))
			{
				return;
			}
		}

		var packages = status.Status.Packages?.ToList() ?? new();

		if (status.Status.Action is StatusAction.Switch && status.Status.Type is not StatusType.MissingDlc and not StatusType.TestVersion)
		{
			packages = packages.ToList(GetFinalSuccessor);
		}

		if (status.Status.Action is StatusAction.SelectOne or StatusAction.Switch or StatusAction.SubscribeToPackages)
		{
			packages.RemoveAll(ShouldNotBeUsed);

			if (packages.Count == 0)
			{
				return;
			}
		}

		var reportType = type switch
		{
			StatusType.Deprecated => packages.Count == 0 ? ReportType.Stability : ReportType.Successors,
			StatusType.CausesIssues or StatusType.SavesCantLoadWithoutIt or StatusType.AutoDeprecated => ReportType.Stability,
			StatusType.DependencyMod or StatusType.TestVersion or StatusType.MusicCanBeCopyrighted => ReportType.Status,
			StatusType.SourceCodeNotAvailable or StatusType.IncompleteDescription or StatusType.Reupload => ReportType.Ambiguous,
			StatusType.MissingDlc => ReportType.DlcMissing,
			_ => ReportType.Status,
		};

		if (status.Status.Action is StatusAction.SelectOne)
		{
			packages.Insert(0, info.Package.SteamId);
		}

		var translation = LocaleCR.Get($"Status_{type}");
		var action = LocaleCR.Get($"Action_{status.Status.Action}");
		var text = packages.Count switch { 0 => translation.Zero, 1 => translation.One, _ => translation.Plural } ?? translation.One;
		var actionText = packages.Count switch { 0 => action.Zero, 1 => action.One, _ => action.Plural } ?? action.One;
		var message = string.Format($"{text}\r\n\r\n{actionText}", info.Package.CleanName(), SteamUtil.GetItem(status.Status.Packages?.FirstOrDefault() ?? 0)?.CleanName() ?? string.Empty).Trim();

		info.Add(reportType, status.Status, message, packages.ToArray());
	}

	internal static void HandleInteraction(CompatibilityInfo info, IndexedPackageInteraction interaction)
	{
		var type = interaction.Interaction.Type;

		if (type is InteractionType.Successor or InteractionType.RequirementAlternative)
		{
			return;
		}

		if (type is InteractionType.SucceededBy && interaction.Interaction.Action is StatusAction.NoAction)
		{
			return;
		}

		var packages = interaction.Interaction.Packages?.ToList() ?? new();

		if (type is InteractionType.RequiredPackages or InteractionType.OptionalPackages || interaction.Interaction.Action is StatusAction.Switch)
		{
			packages = packages.ToList(GetFinalSuccessor);
		}

		if (type is InteractionType.SameFunctionality or InteractionType.CausesIssuesWith or InteractionType.IncompatibleWith)
		{
			if (!info.Package.IsIncluded)
			{
				return;
			}

			packages.RemoveAll(x => !IsPackageEnabled(x, false, false));
		}
		else if (type is InteractionType.RequiredPackages or InteractionType.OptionalPackages)
		{
			packages.RemoveAll(x => IsPackageEnabled(x, true, true));
		}

		if (interaction.Interaction.Action is StatusAction.SelectOne or StatusAction.Switch or StatusAction.SubscribeToPackages)
		{
			packages.RemoveAll(ShouldNotBeUsed);
		}

		packages.Remove(info.Package.SteamId);

		if (packages.Count == 0)
		{
			return;
		}

		if (type is InteractionType.SucceededBy && HandleSucceededBy(info, packages))
		{
			return;
		}

		var reportType = type switch
		{
			InteractionType.SucceededBy => ReportType.Successors,
			InteractionType.Alternative => ReportType.Alternatives,
			InteractionType.SameFunctionality or InteractionType.CausesIssuesWith or InteractionType.IncompatibleWith => ReportType.Compatibility,
			InteractionType.RequiredPackages => ReportType.RequiredPackages,
			InteractionType.OptionalPackages => ReportType.OptionalPackages,
			_ => ReportType.Compatibility
		};

		var translation = LocaleCR.Get($"Interaction_{type}");
		var action = LocaleCR.Get($"Action_{interaction.Interaction.Action}");
		var text = packages.Count switch { 0 => translation.Zero, 1 => translation.One, _ => translation.Plural } ?? translation.One;
		var actionText = packages.Count switch { 0 => action.Zero, 1 => action.One, _ => action.Plural } ?? action.One;
		var message = string.Format($"{text}\r\n\r\n{actionText}", info.Package.CleanName(), SteamUtil.GetItem(packages.FirstOrDefault())?.CleanName() ?? string.Empty).Trim();

		if (interaction.Interaction.Action is StatusAction.SelectOne)
		{
			packages.Insert(0, info.Package.SteamId);
		}

		info.Add(reportType, interaction.Interaction, message, packages.ToArray());
	}

	private static bool HandleSucceededBy(CompatibilityInfo info, IEnumerable<ulong> packages)
	{
		foreach (var item in packages)
		{
			if (IsPackageEnabled(item, true, true))
			{
				HandleStatus(info, new PackageStatus(StatusType.Succeeded, StatusAction.UnsubscribeThis) { Packages = new[] { item } });

				return true;
			}
		}

		return false;
	}

	internal static ulong GetFinalSuccessor(ulong steamId)
	{
		if (!CompatibilityManager.CompatibilityData.Packages.TryGetValue(steamId, out var package))
		{
			return steamId;
		}

		if (package.Interactions.ContainsKey(InteractionType.SucceededBy))
		{
			return package.Interactions[InteractionType.SucceededBy]
					.SelectMany(x => x.Packages.Values)
					.OrderByDescending(x => x.Package.ReviewDate)
					.FirstOrDefault()?
					.Package.SteamId ?? steamId;
		}

		if (SteamUtil.GetItem(steamId)?.Package is null)
		{
			foreach (var item in package.RequirementAlternatives.Keys)
			{
				if (SteamUtil.GetItem(item)?.Package is not null)
				{
					return item;
				}
			}
		}

		return steamId;
	}

	private static bool IsPackageEnabled(ulong steamId, bool withAlternatives, bool withSuccessors)
	{
		var indexedPackage = CompatibilityManager.CompatibilityData.Packages.TryGet(steamId);

		if (indexedPackage is null)
		{
			return isEnabled(CentralManager.GetPackage(steamId));
		}

		if (withAlternatives)
		{
			foreach (var item in indexedPackage.RequirementAlternatives)
			{
				if (item.Key != steamId)
				{
					foreach (var package in FindPackage(item.Value, withSuccessors))
					{
						if (isEnabled(package))
						{
							return true;
						}
					}
				}
			}
		}

		foreach (var package in FindPackage(indexedPackage, withSuccessors))
		{
			if (isEnabled(package))
			{
				return true;
			}
		}

		foreach (var item in indexedPackage.Group)
		{
			if (item.Key != steamId)
			{
				foreach (var package in FindPackage(item.Value, withSuccessors))
				{
					if (isEnabled(package))
					{
						return true;
					}
				}
			}
		}

		return false;

		static bool isEnabled(Domain.Package? package)
		{
			if (package is null)
			{
				return false;
			}

			if (!(package.Mod?.IsEnabled ?? true))
			{
				return false;
			}

			return package.IsIncluded;
		}
	}

	private static bool ShouldNotBeUsed(ulong steamId)
	{
		var workshopItem = SteamUtil.GetItem(steamId);

		if (workshopItem is not null && (CompatibilityManager.IsBlacklisted(workshopItem) || workshopItem.RemovedFromSteam || workshopItem.Incompatible))
		{
			return true;
		}

		if (!CompatibilityManager.CompatibilityData.Packages.TryGetValue(steamId, out var package))
		{
			return false;
		}

		if (package.Package.Stability is PackageStability.Broken)
		{
			return true;
		}

		if (package.Package.Statuses?.Any(x => x.Type is StatusType.Deprecated) ?? false)
		{
			return true;
		}

		return false;
	}

	internal static IndexedPackage? GetPackageData(IPackage package)
	{
		return null;
		var steamId = package.Workshop? package.SteamId : package.Package?.Mod is null ? 0 : CompatibilityManager.CompatibilityData.PackageNames.TryGet(Path.GetFileName(package.Package.Mod.FileName));

		if (steamId > 0)
		{
			var packageData = CompatibilityManager.CompatibilityData.Packages.TryGet(steamId);

			if (packageData is null)
			{
				packageData = new IndexedPackage(CompatibilityManager.GetAutomatedReport(package));

				packageData.Load(CompatibilityManager.CompatibilityData.Packages);
			}

			return packageData;
		}

		//if (package.Package?.Mod is not null)
		//{
		//	return CompatibilityManager.CompatibilityData.Packages.Values
		//		.AllWhere(x => x.Package.FileName == Path.GetFileName(package.Package.Mod.FileName))
		//		.FirstOrAny(x => !x.Statuses.ContainsKey(StatusType.TestVersion));
		//}

		return null;
	}

	private static IEnumerable<Domain.Package> FindPackage(IndexedPackage package, bool withSuccessors)
	{
		var localPackage = CentralManager.GetPackage(package.Package.SteamId);

		if (localPackage is not null)
		{
			yield return localPackage;
		}

		localPackage = CentralManager.Mods.FirstOrDefault(x => !x.Workshop && Path.GetFileName(x.FileName) == package.Package.FileName)?.Package;

		if (localPackage is not null)
		{
			yield return localPackage;
		}

		if (!withSuccessors || !package.Interactions.ContainsKey(InteractionType.SucceededBy))
		{
			yield break;
		}

		var packages = package.Interactions[InteractionType.SucceededBy]
					.SelectMany(x => x.Packages.Values)
					.Where(x => x.Package != package.Package)
					.Select(x => FindPackage(x, true))
					.FirstOrDefault(x => x is not null);

		if (packages is not null)
		{
			foreach (var item in packages)
			{
				yield return item;
			}
		}
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
			NotificationType.Exclude => "I_X",
			NotificationType.None or _ => status ? "I_Ok" : "I_Info",
		};
	}

	public static Color GetColor(this NotificationType notification)
	{
		return notification switch
		{
			NotificationType.Info => FormDesign.Design.InfoColor,

			NotificationType.Caution => FormDesign.Design.YellowColor.MergeColor(FormDesign.Design.GreenColor, 60),

			NotificationType.MissingDependency => FormDesign.Design.YellowColor,

			NotificationType.Warning => FormDesign.Design.YellowColor.MergeColor(FormDesign.Design.RedColor, 60),
			NotificationType.AttentionRequired => FormDesign.Design.YellowColor.MergeColor(FormDesign.Design.RedColor, 30),

			NotificationType.Exclude or
			NotificationType.Unsubscribe => FormDesign.Design.RedColor,

			NotificationType.Switch => FormDesign.Design.RedColor.Tint(FormDesign.Design.RedColor.GetHue() - 10),

			_ => FormDesign.Design.GreenColor
		};
	}
}
