using CompatibilityReport.CatalogData;

using Extensions;

using LoadOrderToolTwo.ColossalOrder;

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Xml.Serialization;

using static CompatibilityReport.CatalogData.Enums;

namespace LoadOrderToolTwo.Utilities.Managers;
#nullable disable
#pragma warning disable CS0649 // Never Used
#pragma warning disable IDE1006 // Naming Styles
internal static class CompatibilityManager
{
	internal static Catalog? Catalog { get; private set; }
	internal static bool CatalogAvailable { get; private set; }

	internal static void LoadCompatibilityReport(Domain.Package compatibilityReport)
	{
		try
		{
			var file = Directory.GetFiles(compatibilityReport.Folder, "*.gz").FirstOrDefault();

			if (file == null)
			{
				return;
			}

			Catalog = ReadGzFile(file);

			if (Catalog != null)
			{
				Catalog.CreateIndexes();

				CatalogAvailable = true;
			}
		}
		catch { }
	}

	private static Catalog? ReadGzFile(string filePath)
	{
		using var fileStream = new FileStream(filePath, FileMode.Open);
		using var gzipStream = new GZipStream(fileStream, CompressionMode.Decompress);
		using var reader = new StreamReader(gzipStream);

		var ser = new XmlSerializer(typeof(Catalog));

		return ser.Deserialize(reader) as Catalog;
	}

	internal static ReportInfo? GetCompatibilityReport(Domain.Package package)
	{
		if (Catalog is null)
		{
			return null;
		}

		var reportInfo = new ReportInfo(package);

		if (!package.Workshop)
		{
			return reportInfo;
		}

		var subscribedMod = Catalog.GetMod(package.SteamId);

		if (subscribedMod is null && package.Mod is null)
		{
			return null;
		}

		if (subscribedMod is null)
		{
			reportInfo.Messages.Add(new ReportMessage(ReportType.Stability
				, ReportSeverity.Remarks
				, Locale.CR_NotInCatalogMod));

			return reportInfo;
		}

		var subscriptionAuthor = Catalog.GetAuthor(subscribedMod.AuthorID, subscribedMod.AuthorUrl);

		if (package.Workshop)
		{
			reportInfo.Messages.AddIfNotNull(Stability(subscribedMod));
			reportInfo.Messages.AddIfNotNull(RequiredDlc(subscribedMod));
			reportInfo.Messages.AddIfNotNull(UnneededDependencyMod(subscribedMod));
			reportInfo.Messages.AddIfNotNull(Disabled(subscribedMod));
			reportInfo.Messages.AddIfNotNull(Successors(subscribedMod));
			reportInfo.Messages.AddIfNotNull(Alternatives(subscribedMod));
			reportInfo.Messages.AddIfNotNull(RequiredMods(subscribedMod));
			reportInfo.Messages.AddIfNotNull(ModNote(subscribedMod));

			reportInfo.Messages.AddRange(Compatibilities(subscribedMod));
			reportInfo.Messages.AddRange(Statuses(subscribedMod, package, authorRetired: subscriptionAuthor != null && subscriptionAuthor.Retired));

			if (reportInfo.Severity < ReportSeverity.Unsubscribe)
			{
				reportInfo.Messages.AddIfNotNull(Recommendations(subscribedMod));
				reportInfo.Messages.AddRange(ExtraStatuses(subscribedMod));
			}
		}

		return reportInfo;
	}

	internal static ReportInfo? GetCompatibilityReport(ulong packageId)
	{
		if (Catalog is null || packageId is 0)
		{
			return null;
		}

		var subscribedMod = Catalog.GetMod(packageId);

		if (subscribedMod is null)
		{
			return null;
		}

		var reportInfo = new ReportInfo(null);
		var subscriptionAuthor = Catalog.GetAuthor(subscribedMod.AuthorID, subscribedMod.AuthorUrl);

		reportInfo.Messages.AddIfNotNull(Stability(subscribedMod));
		reportInfo.Messages.AddIfNotNull(RequiredDlc(subscribedMod));
		reportInfo.Messages.AddIfNotNull(UnneededDependencyMod(subscribedMod));
		reportInfo.Messages.AddIfNotNull(Disabled(subscribedMod));
		reportInfo.Messages.AddIfNotNull(Successors(subscribedMod));
		reportInfo.Messages.AddIfNotNull(Alternatives(subscribedMod));
		reportInfo.Messages.AddIfNotNull(ModNote(subscribedMod));

		reportInfo.Messages.AddRange(Compatibilities(subscribedMod));
		reportInfo.Messages.AddRange(Statuses(subscribedMod, null, authorRetired: subscriptionAuthor != null && subscriptionAuthor.Retired));

		if (reportInfo.Severity < ReportSeverity.Unsubscribe)
		{
			reportInfo.Messages.AddIfNotNull(Recommendations(subscribedMod));
			reportInfo.Messages.AddRange(ExtraStatuses(subscribedMod));
		}

		return reportInfo;
	}

	internal static bool? IsForAssetEditor(Domain.Package package)
	{
		if (Catalog is null || !package.Workshop)
		{
			return null;
		}

		return Catalog.GetMod(package.SteamId)?.Statuses.Any(x => x == Status.ModForModders);
	}

	internal static bool? IsForNormalGame(Domain.Package package)
	{
		if (Catalog is null || !package.Workshop)
		{
			return null;
		}

		return Catalog.GetMod(package.SteamId)?.Statuses.Any(x => x == Status.BreaksEditors);
	}

	private static ReportMessage Stability(Mod subscribedMod)
	{
		return subscribedMod.Stability switch
		{
			Enums.Stability.IncompatibleAccordingToWorkshop => new ReportMessage(ReportType.Stability
								, ReportSeverity.Unsubscribe
								, Locale.CR_IncompatibleWithGameVersion
								, subscribedMod.StabilityNote.Value),

			Enums.Stability.RequiresIncompatibleMod => new ReportMessage(ReportType.Stability
								, ReportSeverity.Unsubscribe
								, Locale.CR_RequiresIncompatibleMod
								, subscribedMod.StabilityNote.Value),

			Enums.Stability.GameBreaking => new ReportMessage(ReportType.Stability
								, ReportSeverity.Unsubscribe
								, Locale.CR_BreaksGame
								, subscribedMod.StabilityNote.Value),

			Enums.Stability.Broken => new ReportMessage(ReportType.Stability
								, ReportSeverity.Unsubscribe
								, Locale.CR_BrokenMod
								, subscribedMod.StabilityNote.Value),

			Enums.Stability.MajorIssues => new ReportMessage(ReportType.Stability
								, ReportSeverity.MajorIssues
								, string.IsNullOrEmpty(subscribedMod.StabilityNote.Value) ? Locale.CR_MajorIssuesNoNote : Locale.CR_MajorIssuesWithNote
								, subscribedMod.StabilityNote.Value),

			Enums.Stability.MinorIssues => new ReportMessage(ReportType.Stability
								, ReportSeverity.MinorIssues
								, string.IsNullOrEmpty(subscribedMod.StabilityNote.Value) ? Locale.CR_MinorIssuesNoNote : Locale.CR_MinorIssuesWithNote
								, subscribedMod.StabilityNote.Value),

			Enums.Stability.UsersReportIssues => new ReportMessage(ReportType.Stability
								, ReportSeverity.MinorIssues
								, string.IsNullOrEmpty(subscribedMod.StabilityNote.Value) ? Locale.CR_UserReportsNoNote : Locale.CR_UserReportsWithNote
								, subscribedMod.StabilityNote.Value),

			Enums.Stability.NotEnoughInformation => new ReportMessage(ReportType.Stability
								, ReportSeverity.Remarks
								, subscribedMod.GameVersion() == CurrentGameVersion() ? Locale.CR_NotEnoughInformationUpdated : Locale.CR_NotEnoughInformationOutdated
								, subscribedMod.StabilityNote.Value),

			Enums.Stability.Stable => new ReportMessage(ReportType.Stability
								, string.IsNullOrEmpty(subscribedMod.StabilityNote.Value) ? ReportSeverity.NothingToReport : ReportSeverity.Remarks
								, Locale.CR_Stable
								, subscribedMod.StabilityNote.Value),

			Enums.Stability.NotReviewed or Enums.Stability.Undefined => new ReportMessage(ReportType.Stability
								, ReportSeverity.Remarks
								, subscribedMod.GameVersion() == CurrentGameVersion() ? Locale.CR_NotReviewedUpdated : Locale.CR_NotReviewedOutdated
								, subscribedMod.StabilityNote.Value),
			_ => null,
		};
	}

	private static Version CurrentGameVersion()
	{
		return new Version(1, 16, 0, 3);
	}

	private static Version CurrentMajorGameVersion()
	{
		return new Version(1, 16);
	}

	private static IEnumerable<ReportMessage> Statuses(Mod subscribedMod, Domain.Package package, bool authorRetired)
	{
		if (subscribedMod.Statuses.Contains(Status.Obsolete))
		{
			yield return new ReportMessage(ReportType.Status
				, ReportSeverity.Unsubscribe
				, Locale.CR_Obsolete);
		}

		if (subscribedMod.Statuses.Contains(Status.RemovedFromWorkshop) || package?.Status == Domain.DownloadStatus.Removed)
		{
			yield return new ReportMessage(ReportType.Status
				, ReportSeverity.MajorIssues
				, Locale.CR_RemovedFromWorkshop);
		}

		if (subscribedMod.Statuses.Contains(Status.Deprecated))
		{
			yield return new ReportMessage(ReportType.Status
				, ReportSeverity.MajorIssues
				, Locale.CR_Deprecated);
		}

		if (subscribedMod.Statuses.Contains(Status.Abandoned))
		{
			yield return new ReportMessage(ReportType.Status
				, ReportSeverity.NothingToReport
				, authorRetired ? Locale.CR_AbandonedRetired : Locale.CR_Abandoned);
		}
		else if (authorRetired)
		{
			yield return new ReportMessage(ReportType.Status
				, ReportSeverity.NothingToReport
				, Locale.CR_Retired);
		}

		if (subscribedMod.Statuses.Contains(Status.SavesCantLoadWithout))
		{
			yield return new ReportMessage(ReportType.Status
				, ReportSeverity.Remarks
				, Locale.CR_SavesCantLoadWithout);
		}

		//added to inform about non public source code
		if (subscribedMod.Statuses.Contains(Status.SourceNotPublic))
		{
			yield return new ReportMessage(ReportType.Status
				, ReportSeverity.NothingToReport
				, Locale.CR_SourceNotPublic);
		}

		// added to inform about mods that have no comment section active, no workshop description and no source code available
		if (subscribedMod.Statuses.Contains(Status.NoDescription))
		{
			yield return new ReportMessage(ReportType.Status
				, ReportSeverity.NothingToReport
				, Locale.CR_NoDescription);
		}

		if (subscribedMod.Statuses.Contains(Status.NoCommentSection))
		{
			yield return new ReportMessage(ReportType.Status
				, ReportSeverity.NothingToReport
				, Locale.CR_NoCommentSection);
		}

		var abandoned = subscribedMod.Statuses.Contains(Status.Obsolete) || subscribedMod.Statuses.Contains(Status.Deprecated) ||
			subscribedMod.Statuses.Contains(Status.RemovedFromWorkshop) || subscribedMod.Statuses.Contains(Status.Abandoned) ||
			(subscribedMod.Stability == Enums.Stability.IncompatibleAccordingToWorkshop) || authorRetired;

		if (abandoned && string.IsNullOrEmpty(subscribedMod.SourceUrl) && subscribedMod.Statuses.Contains(Status.SourceBundled))
		{
			yield return new ReportMessage(ReportType.Status
				, ReportSeverity.NothingToReport
				, Locale.CR_SourceBundled);
		}
		else if (abandoned && subscribedMod.Statuses.Contains(Status.SourceNotPublic))
		{
			yield return new ReportMessage(ReportType.Status
				, ReportSeverity.NothingToReport
				, Locale.CR_SourceNotPublicAbandoned);
		}
		else if (abandoned && subscribedMod.Statuses.Contains(Status.SourceObfuscated))
		{
			yield return new ReportMessage(ReportType.Status
				, ReportSeverity.NothingToReport
				, Locale.CR_SourceObfuscated);
		}
	}

	private static IEnumerable<ReportMessage> ExtraStatuses(Mod subscribedMod)
	{
		// Several statuses only listed if there are no breaking issues.
		if (subscribedMod.Statuses.Contains(Status.Reupload))
		{
			yield return new ReportMessage(ReportType.Status
				, ReportSeverity.Unsubscribe
				, Locale.CR_Reupload);
		}

		if (subscribedMod.Statuses.Contains(Status.BreaksEditors))
		{
			yield return new ReportMessage(ReportType.Status
				, ReportSeverity.Remarks
				, Locale.CR_BreaksEditors);
		}

		if (subscribedMod.Statuses.Contains(Status.ModForModders))
		{
			yield return new ReportMessage(ReportType.Status
				, ReportSeverity.Remarks
				, Locale.CR_ModForModders);
		}

		if (subscribedMod.Statuses.Contains(Status.TestVersion))
		{
			yield return new ReportMessage(ReportType.Status
				, ReportSeverity.NothingToReport
				, subscribedMod.Stability == Enums.Stability.Stable ? Locale.CR_TestVersionStable : Locale.CR_TestVersion);
		}

		// Changed to have only one Music Pack Copyright status
		if (subscribedMod.Statuses.Contains(Status.MusicCopyright))
		{
			yield return new ReportMessage(ReportType.Status
				, ReportSeverity.NothingToReport
				, Locale.CR_MusicCopyright);
		}
	}

	private static ReportMessage UnneededDependencyMod(Mod subscribedMod)
	{
		if (!subscribedMod.Statuses.Contains(Status.DependencyMod))
		{
			return null;
		}

		// Check if any of the mods that need this is actually subscribed, enabled or not. If this is in a group, check all group members. Exit if any is needed.
		if (Catalog.IsGroupMember(subscribedMod.SteamID))
		{
			foreach (var groupMemberID in Catalog.GetThisModsGroup(subscribedMod.SteamID).GroupMembers)
			{
				if (IsModNeeded(groupMemberID))
				{
					// Group member is needed. No need to check other group members.
					return null;
				}
			}
		}
		else if (IsModNeeded(subscribedMod.SteamID))
		{
			return null;
		}

		return new ReportMessage(ReportType.UnneededDependency
			, ReportSeverity.Unsubscribe
			, Locale.CR_UnneededDependency);
	}


	private static bool IsModNeeded(ulong SteamID)
	{
		// Check if any of the mods that need this is actually subscribed, enabled or not.
		var ModsRequiringThis = Catalog.Mods.FindAll(x => x.RequiredMods.Contains(SteamID));

		foreach (var mod in ModsRequiringThis)
		{
			if (Catalog.GetSubscription(mod.SteamID) != null)
			{
				// Found a subscribed mod that needs this.
				return true;
			}
		}

		return false;
	}

	private static ReportMessage Disabled(Mod subscribedMod)
	{
		if (!subscribedMod.IsIncluded)
		{
			return null;
		}

		if (!subscribedMod.IsDisabled || !subscribedMod.Statuses.Contains(Status.WorksWhenDisabled))
		{
			return null;
		}

		return new ReportMessage(ReportType.WorksWhenDisabled
			, ReportSeverity.Unsubscribe
			, Locale.CR_WorksWhenDisabled);
	}


	private static ReportMessage ModNote(Mod subscribedMod)
	{
		if (subscribedMod.Note == null || string.IsNullOrEmpty(subscribedMod.Note.Value))
		{
			return null;
		}

		return new ReportMessage(ReportType.Note, ReportSeverity.Remarks, Locale.CR_Note, subscribedMod.Note.Value);
	}


	private static ReportMessage RequiredDlc(Mod subscribedMod)
	{
		var dlcs = new List<DLC>();

		foreach (var dlc in subscribedMod.RequiredDlcs)
		{
			if (!SteamUtil.IsDlcInstalledLocally((uint)dlc))
			{
				dlcs.Add((DLC)(uint)dlc);
			}
		}

		if (dlcs.Count == 0)
		{
			return null;
		}

		return new ReportMessage(ReportType.DlcMissing
			, ReportSeverity.Unsubscribe
			, Locale.CR_MissingDLC
			, dlcs.Select(x => "• " + x.GetDLCInfo()?.Text).ListStrings("\r\n"));
	}

	private static ReportMessage RequiredMods(Mod subscribedMod)
	{
		var mods = subscribedMod.RequiredMods.FindAll(x => !Catalog.IsValidID(x) || ModAndGroupItem(x) != null);

		if (!mods.Any())
		{
			return null;
		}

		var RequiredModsAvailable = mods.Any(x => Catalog.IsValidID(x) && Catalog.GetSubscription(x) != null);

		return new ReportMessage(ReportType.RequiredMods
			, ReportSeverity.MajorIssues
			, Locale.CR_RequiredModsMissing
			, packages: mods.ToArray());
	}

	private static ReportMessage Recommendations(Mod subscribedMod)
	{
		var missingRecommendations = subscribedMod.Recommendations.FindAll(x => !Catalog.IsValidID(x) || Catalog.GetSubscription(x) == null);

		if (missingRecommendations.Count == 0)
		{
			return null;
		}

		return new ReportMessage(ReportType.Recommendations
			, ReportSeverity.Remarks
			, Locale.CR_Recommendations
			, packages: missingRecommendations.ToArray());
	}

	private static Message ModAndGroupItem(ulong steamID)
	{
		var catalogMod = Catalog.GetSubscription(steamID);

		if (catalogMod != null && (!catalogMod.IsDisabled || catalogMod.Statuses.Contains(Status.WorksWhenDisabled)))
		{
			// Mod is subscribed and enabled (or works when disabled). Don't report.
			return null;
		}
		catalogMod = Catalog.GetMod(steamID);

		if (catalogMod.IsDisabled)
		{
			// Mod is subscribed and disabled. Report as "missing", without Workshop page.
			return new Message() { message = Catalog.GetMod(steamID).ToString(hideFakeID: true, nameFirst: true, html: true) };
		}

		if (!Catalog.IsGroupMember(steamID))
		{
			// Mod is not subscribed and not in a group. Report as missing with Workshop page.
			return new Message() { message = catalogMod.ToString() };
		}

		// Mod is not subscribed but in a group. Check if another group member is subscribed.
		foreach (var groupMemberID in Catalog.GetThisModsGroup(steamID).GroupMembers)
		{
			var groupMember = Catalog.GetSubscription(groupMemberID);

			if (groupMember != null)
			{
				// Group member is subscribed. No need to check other group members, but report as "missing" if disabled (unless it works when disabled).
				if (!groupMember.IsDisabled || groupMember.Statuses.Contains(Status.WorksWhenDisabled))
				{
					return null;
				}
				return new Message() { message = groupMember.ToString(hideFakeID: true, nameFirst: true, html: true) };
			}
		}

		// No group member is subscribed. Report original mod as missing.
		return new Message() { message = catalogMod.ToString() };
	}

	private static ReportMessage Successors(Mod subscribedMod)
	{
		if (!subscribedMod.Successors.Any())
		{
			return null;
		}

		var successorsAvailable = subscribedMod.Successors.Any(x => Catalog.IsValidID(x) && Catalog.GetSubscription(x) != null);
		return new ReportMessage(ReportType.Successors
			, successorsAvailable ? ReportSeverity.Unsubscribe : ReportSeverity.MinorIssues
			, subscribedMod.Successors.Count == 1 ? Locale.CR_SuccessorsAvailable : Locale.CR_SuccessorsAvailableMultiple
			, packages: subscribedMod.Successors.ToArray());
	}

	private static ReportMessage Alternatives(Mod subscribedMod)
	{
		if (!subscribedMod.Alternatives.Any())
		{
			return null;
		}

		var alternativesAvailable = subscribedMod.Alternatives.Any(x => Catalog.IsValidID(x) && Catalog.GetSubscription(x) != null);

		return new ReportMessage(ReportType.Alternatives
			, alternativesAvailable ? ReportSeverity.Remarks : ReportSeverity.NothingToReport
			, subscribedMod.Alternatives.Count == 1 ? Locale.CR_AlternativesAvailable : Locale.CR_AlternativesAvailableMultiple
			, packages: subscribedMod.Alternatives.ToArray());
	}

	private static IEnumerable<ReportCompatibilityMessage> Compatibilities(Mod subscribedMod)
	{
		foreach (var compatibility in Catalog.GetSubscriptionCompatibilities(subscribedMod.SteamID))
		{
			var otherModID = (subscribedMod.SteamID == compatibility.FirstModID) ? compatibility.SecondModID : compatibility.FirstModID;

			var otherMod = Catalog.GetMod(otherModID);
			if (subscribedMod.Successors.Contains(otherModID) || otherMod == null || otherMod.Successors.Contains(subscribedMod.SteamID))
			{
				// Don't mention the incompatibility if either mod is the others successor. The succeeded mod will already be mentioned in 'Unsubscribe' severity.
				continue;
			}

			switch (compatibility.Status)
			{
				case CompatibilityStatus.SameModDifferentReleaseType:
					yield return new ReportCompatibilityMessage(ReportType.Compatibility
						, ReportSeverity.Unsubscribe
						, compatibility.Status
						, Locale.CR_SameModDifferentReleaseType
						, otherModID);
					break;

				case CompatibilityStatus.SameFunctionality:
					yield return new ReportCompatibilityMessage(ReportType.Compatibility
						, ReportSeverity.Unsubscribe
						, compatibility.Status
						, Locale.CR_SameFunctionality
						, otherModID);
					break;

				case CompatibilityStatus.IncompatibleAccordingToAuthor:
					yield return new ReportCompatibilityMessage(ReportType.Compatibility
						, ReportSeverity.Unsubscribe
						, compatibility.Status
						, Locale.CR_IncompatibleAccordingToAuthor
						, otherModID);
					break;

				case CompatibilityStatus.IncompatibleAccordingToUsers:
					yield return new ReportCompatibilityMessage(ReportType.Compatibility
						, ReportSeverity.MajorIssues
						, compatibility.Status
						, Locale.CR_IncompatibleAccordingToUsers
						, otherModID);
					break;

				case CompatibilityStatus.MajorIssues:
					yield return new ReportCompatibilityMessage(ReportType.Compatibility
						, ReportSeverity.MajorIssues
						, compatibility.Status
						, Locale.CR_MajorIssuesWith
						, otherModID);
					break;

				case CompatibilityStatus.MinorIssues:
					yield return new ReportCompatibilityMessage(ReportType.Compatibility
						, ReportSeverity.MinorIssues
						, compatibility.Status
						, Locale.CR_MinorIssuesWith
						, otherModID);
					break;

				case CompatibilityStatus.RequiresSpecificSettings:
					yield return new ReportCompatibilityMessage(ReportType.Compatibility
						, ReportSeverity.Remarks
						, compatibility.Status
						, Locale.CR_RequiresSpecificSettings
						, otherModID);
					break;

				case CompatibilityStatus.SameFunctionalityCompatible:
					yield return new ReportCompatibilityMessage(ReportType.Compatibility
						, ReportSeverity.Remarks
						, compatibility.Status
						, Locale.CR_SameFunctionalityCompatible
						, otherModID);
					break;

				case CompatibilityStatus.CompatibleAccordingToAuthor:
					yield return new ReportCompatibilityMessage(ReportType.Compatibility
						, ReportSeverity.Remarks
						, compatibility.Status
						, Locale.CR_CompatibleAccordingToAuthor
						, otherModID);
					break;

				default:
					break;
			}
		}
	}

	public static Version ConvertToVersion(string versionString)
	{
		try
		{
			var elements = versionString.Split(new char[] { '.', '-', 'f' }, StringSplitOptions.RemoveEmptyEntries);

			if (elements.Length >= 4)
			{
				return new Version(Convert.ToInt32(elements[0]), Convert.ToInt32(elements[1]), Convert.ToInt32(elements[2]), Convert.ToInt32(elements[3]));
			}
		}
		catch { }

		return new();
	}

	public static Bitmap GetSeverityIcon(this Enums.ReportSeverity severity, bool status)
	{
		return severity switch
		{
			ReportSeverity.Remarks => ImageManager.GetIcon(nameof(Properties.Resources.I_Remarks)),
			ReportSeverity.MinorIssues => ImageManager.GetIcon(nameof(Properties.Resources.I_MinorIssues)),
			ReportSeverity.MajorIssues => ImageManager.GetIcon(nameof(Properties.Resources.I_MajorIssues)),
			ReportSeverity.Unsubscribe => ImageManager.GetIcon(nameof(Properties.Resources.I_Broken)),
			ReportSeverity.NothingToReport or _ => ImageManager.GetIcon(status ? nameof(Properties.Resources.I_Ok) : nameof(Properties.Resources.I_Info)),
		};
	}

	public static Color GetSeverityColor(this Enums.ReportSeverity severity)
	{
		return severity switch
		{
			ReportSeverity.MinorIssues => FormDesign.Design.YellowColor,
			ReportSeverity.MajorIssues => FormDesign.Design.YellowColor.MergeColor(FormDesign.Design.RedColor),
			ReportSeverity.Unsubscribe => FormDesign.Design.RedColor,
			ReportSeverity.Remarks or (ReportSeverity)(-1) => FormDesign.Design.ForeColor,
			_ => FormDesign.Design.GreenColor
		};
	}

	internal static object GetCompatibilityReport(string id)
	{
		throw new NotImplementedException();
	}

	internal class ModInfo
	{
		public bool isLocal;
		public string authorName;
		public string modName;
		public string idString;
		public bool isDisabled;
		public bool isCameraScript;
		public Enums.ReportSeverity reportSeverity;
		public Message instability;
		public MessageList requiredDlc;
		public Message unneededDependencyMod;
		public Message disabled;
		public MessageList successors;
		public Message stability;
		public List<CompatibilityList> compatibilities;
		public MessageList requiredMods;
		public MessageList statuses;
		public string note;
		public string noteLocaleId;
		public MessageList alternatives;
		public MessageList recommendations;
		public bool anyIssues;
		public string steamUrl;
	}

	private class InstalledModInfo
	{
		public string disabled;
		public bool isSteam;

		public string subscriptionName;
		public string type;
		public string typeLocaleID;
		public string status;
		public string statusLocaleID;
		public string url;

		public InstalledModInfo(string subscriptionName, string disabled, string type, string typeLocaleID, string status, string statusLocaleID, bool isSteam, string url)
		{
			this.subscriptionName = subscriptionName;
			this.disabled = disabled;
			this.type = type;
			this.typeLocaleID = typeLocaleID;
			this.status = status;
			this.statusLocaleID = statusLocaleID;
			this.isSteam = isSteam;
			this.url = url;
		}
	}

	internal class MessageList
	{
		public string title;
		public string titleLocaleId;
		public List<Message> messages;
		public Dictionary<int, List<Message>> messageDictionary;
	}

	internal class CompatibilityList
	{
		public string message;
		public string messageLocaleId;
		public string messageLocalized;
		public List<Details> details;
	}

	internal class Message
	{
		public string message;
		public string messageLocaleId;
		public string messageLocalized;
		public string localeIdVariables;
		public string details;
		public string detailsLocaleId;
		public string detailsLocalized;
		public string detailsValue;
	}

	internal class Details
	{
		public string details;
		public string detailsLocaleId;
		public string detailsLocalized;
		public string detailsValue;
	}

	internal class ReportMessage
	{
		public Enums.ReportSeverity Severity { get; }
		public Enums.ReportType Type { get; }
		public string Message { get; }
		public string Note { get; }
		public ulong[] LinkedPackages { get; }

		public ReportMessage(Enums.ReportType type, Enums.ReportSeverity severity, string message, string note = "", params ulong[] packages)
		{
			Type = type;
			Severity = severity;
			Message = message;
			Note = note;
			LinkedPackages = packages;
		}
	}

	internal class ReportCompatibilityMessage : ReportMessage
	{
		public Enums.CompatibilityStatus Compatibility { get; }

		public ReportCompatibilityMessage(Enums.ReportType type, Enums.ReportSeverity severity, Enums.CompatibilityStatus compatibility, string message, params ulong[] packages) : base(type, severity, message, "", packages)
		{
			Compatibility = compatibility;
		}
	}

	internal class ReportInfo
	{
		public Domain.Package Package { get; }
		public List<ReportMessage> Messages { get; }
		public Enums.ReportSeverity Severity => Messages.Count == 0 ? ReportSeverity.NothingToReport : Messages.Max(x => x.Severity);

		public ReportInfo(Domain.Package package)
		{
			Package = package;
			Messages = new();
		}
	}
}
#pragma warning restore IDE1006 // Naming Styles
#pragma warning restore CS0649 // Never Used
#nullable enable