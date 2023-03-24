using Extensions;

using LoadOrderToolTwo.Utilities;
using LoadOrderToolTwo.Utilities.Managers;

using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace CompatibilityReport.CatalogData
{
    [Serializable] 
    public class Mod
    {
        public ulong SteamID { get; set; }
        public string Name { get; set; }

        public DateTime Published { get; set; }
        public DateTime Updated { get; set; }

        public ulong AuthorID { get; set; }
        public string AuthorUrl { get; set; } = "";

        public Enums.Stability Stability { get; set; } = Enums.Stability.NotReviewed;
        [XmlElement("StabilityNote")]
        public ElementWithId StabilityNote { get; set; }

        public List<Enums.Status> Statuses { get; set; } = new List<Enums.Status>();
        public bool ExclusionForNoDescription { get; set; }
        [XmlElement("Note")]
        public ElementWithId Note { get; set; }

        // Game version this mod is compatible with. 'Version' is not serializable, so a converted string is used.
        [XmlElement("GameVersion")] public string GameVersionString { get; set; }
        public bool ExclusionForGameVersion { get; set; }

        public List<Enums.Dlc> RequiredDlcs { get; set; } = new List<Enums.Dlc>();
        public List<Enums.Dlc> ExclusionForRequiredDlcs { get; set; } = new List<Enums.Dlc>();

        // No mod should be in more than one of the required mods, successors, alternatives and recommendations.
        [XmlArrayItem("SteamID")] public List<ulong> RequiredMods { get; set; } = new List<ulong>();
        [XmlArrayItem("SteamID")] public List<ulong> ExclusionForRequiredMods { get; set; } = new List<ulong>();
        [XmlArrayItem("SteamID")] public List<ulong> Successors { get; set; } = new List<ulong>();
        [XmlArrayItem("SteamID")] public List<ulong> Alternatives { get; set; } = new List<ulong>();
        [XmlArrayItem("SteamID")] public List<ulong> Recommendations { get; set; } = new List<ulong>();

        public string SourceUrl { get; set; }
        public bool ExclusionForSourceUrl { get; set; }

        // Date of the last review of this mod, imported by the FileImporter, and the last automatic review for changes in mod information (WebCrawler).
        public DateTime ReviewDate { get; set; }
        public DateTime AutoReviewDate { get; set; }
        [XmlArrayItem("ChangeNote")] public List<string> ChangeNotes { get; set; } = new List<string>();

        // Properties used by the Reporter for subscribed mods.
        [CloneIgnore, XmlIgnore] public bool IsDisabled => !(ModsUtil.FindMod(SteamID)?.IsEnabled ?? false);
        [CloneIgnore, XmlIgnore] public bool IsIncluded => ModsUtil.FindMod(SteamID)?.IsIncluded ?? false;
		[CloneIgnore, XmlIgnore] public bool IsCameraScript { get; set; }
        [CloneIgnore, XmlIgnore] public string ModPath { get; set; }
        [CloneIgnore, XmlIgnore] public DateTime DownloadedTime => ModsUtil.FindMod(SteamID)?.LocalTime ?? DateTime.MinValue;
		[CloneIgnore, XmlIgnore] public Enums.ReportSeverity ReportSeverity { get; set; }

        // Used by the Updater, to see if this mod was added or updated this session.
        [CloneIgnore, XmlIgnore] public bool AddedThisSession { get; set; }
        [CloneIgnore, XmlIgnore] public bool UpdatedThisSession { get; set; }


        /// <summary>Default constructor for deserialization.</summary>
        public Mod()
        {
            // Nothing to do here
        }


        /// <summary>Constructor for mod creation.</summary>
        public Mod(ulong steamID)
        {
            SteamID = steamID;

            AddedThisSession = true;
        }



        /// <summary>Updates one or more mod properties.</summary>
        public void Update(string name = null,
                           DateTime published = default,
                           DateTime updated = default,
                           ulong authorID = 0,
                           string authorUrl = null,
                           Enums.Stability stability = default,
                           ElementWithId stabilityNote = null,
                           ElementWithId note = null,
                           string gameVersionString = null,
                           string sourceUrl = null,
                           DateTime reviewDate = default,
                           DateTime autoReviewDate = default)
        {
            Name = name ?? Name ?? "";

            // If the updated date is older than published, set it to published.
            Published = published == default ? Published : published;
            Updated = updated == default ? Updated : updated;
            Updated = Updated < Published ? Published : Updated;

            AuthorID = authorID == 0 ? AuthorID : authorID;
            AuthorUrl = authorUrl ?? AuthorUrl ?? "";

            Stability = stability == default ? Stability : stability;
            StabilityNote = stabilityNote ?? StabilityNote ?? new ElementWithId();

            Note = note ?? Note ?? new ElementWithId();
            SourceUrl = sourceUrl ?? SourceUrl ?? "";

            ReviewDate = reviewDate == default ? ReviewDate : reviewDate;
            AutoReviewDate = autoReviewDate == default ? AutoReviewDate : autoReviewDate;

            UpdatedThisSession = true;
        }


        /// <summary>Adds a required DLC.</summary>
        /// <returns>True if added, false if it was already in the list.</returns>
        public bool AddRequiredDlc(Enums.Dlc dlc)
        {
            if (RequiredDlcs.Contains(dlc))
            {
                return false;
            }

            RequiredDlcs.Add(dlc);
            return true;
        }


        /// <summary>Removes a required DLC.</summary>
        /// <returns>True if removal succeeded, false if not.</returns>
        public bool RemoveRequiredDlc(Enums.Dlc dlc)
        {
            return RequiredDlcs.Remove(dlc);
        }


        /// <summary>Adds a required mod.</summary>
        public void AddRequiredMod(ulong steamID)
        {
            if (!RequiredMods.Contains(steamID))
            {
                RequiredMods.Add(steamID);
            }
        }


        /// <summary>Removes a required mod.</summary>
        /// <returns>True if removal succeeded, false if not.</returns>
        public bool RemoveRequiredMod(ulong steamID)
        {
            return RequiredMods.Remove(steamID);
        }


        /// <summary>Adds a successor.</summary>
        /// <returns>True if added, false if it was already in the list.</returns>
        public bool AddSuccessor(ulong steamID)
        {
            if (Successors.Contains(steamID))
            {
                return false;
            }

            Successors.Add(steamID);
            return true;
        }


        /// <summary>Removes a successor.</summary>
        /// <returns>True if removal succeeded, false if not.</returns>
        public bool RemoveSuccessor(ulong steamID)
        {
            return Successors.Remove(steamID);
        }


        /// <summary>Adds an alternative.</summary>
        /// <returns>True if added, false if it was already in the list.</returns>
        public bool AddAlternative(ulong steamID)
        {
            if (Alternatives.Contains(steamID))
            {
                return false;
            }

            Alternatives.Add(steamID);
            return true;
        }


        /// <summary>Removes an alternative.</summary>
        /// <returns>True if removal succeeded, false if not.</returns>
        public bool RemoveAlternative(ulong steamID)
        {
            return Alternatives.Remove(steamID);
        }


        /// <summary>Adds a recommended mod.</summary>
        /// <returns>True if added, false if it was already in the list.</returns>
        public bool AddRecommendation(ulong steamID)
        {
            if (Recommendations.Contains(steamID))
            {
                return false;
            }

            Recommendations.Add(steamID);
            return true;
        }


        /// <summary>Removes a recommended mod.</summary>
        /// <returns>True if removal succeeded, false if not.</returns>
        public bool RemoveRecommendation(ulong steamID)
        {
            return Recommendations.Remove(steamID);
        }


        /// <summary>Updates one or more exclusions.</summary>
        public void UpdateExclusions(bool? exclusionForSourceUrl = null, bool? exclusionForGameVersion = null, bool? exclusionForNoDescription = null)
        {
            ExclusionForSourceUrl = exclusionForSourceUrl ?? ExclusionForSourceUrl;
            ExclusionForGameVersion = exclusionForGameVersion ?? ExclusionForGameVersion;
            ExclusionForNoDescription = exclusionForNoDescription ?? ExclusionForNoDescription;
        }


        /// <summary>Adds an exclusion for a required DLC.</summary>
        public void AddExclusion(Enums.Dlc requiredDlc)
        {
            if (!ExclusionForRequiredDlcs.Contains(requiredDlc))
            {
                ExclusionForRequiredDlcs.Add(requiredDlc);
            }
        }


        /// <summary>Adds an exclusion for a required mod.</summary>
        public void AddExclusion(ulong requiredMod)
        {
            if (!ExclusionForRequiredMods.Contains(requiredMod))
            {
                ExclusionForRequiredMods.Add(requiredMod);
            }
        }


        /// <summary>Removes an exclusion for a required DLC.</summary>
        /// <returns>True if removal succeeded, false otherwise.</returns>
        public bool RemoveExclusion(Enums.Dlc requiredDlc)
        {
            return ExclusionForRequiredDlcs.Remove(requiredDlc);
        }


        /// <summary>Removes an exclusion for a required mod.</summary>
        /// <returns>True if removal succeeded, false otherwise.</returns>
        public bool RemoveExclusion(ulong requiredMod)
        {
            return ExclusionForRequiredMods.Remove(requiredMod);
        }



        /// <summary>Sets the report severity for a mod.</summary>
        /// <remarks>This will only set the severity higher, not lower it.</remarks>
        public void IncreaseReportSeverity(Enums.ReportSeverity newSeverity)
        {
            ReportSeverity = (newSeverity > ReportSeverity) ? newSeverity : ReportSeverity;
        }


        /// <summary>Adds a mod change note.</summary>
        public void AddChangeNote(string changeNote)
        {
            ChangeNotes.Add(changeNote);
        }
		public Version GameVersion()
		{
			return CompatibilityManager.ConvertToVersion(GameVersionString);
		}
		public string ToString(bool hideFakeID = false, bool nameFirst = false, bool cutOff = false, bool html = false)
		{
			string disabledPrefix = IsDisabled ? "[Disabled] " : string.Empty;

            string idString = SteamID.ToString();

			return nameFirst
				? $"{disabledPrefix}{Name} {idString}"
				: $"{disabledPrefix}{idString} {Name}";
		}
	}
}
