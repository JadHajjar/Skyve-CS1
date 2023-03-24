using Extensions;

using LoadOrderToolTwo.Utilities.Managers;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;

namespace CompatibilityReport.CatalogData;

[Serializable]
[XmlRoot("CompatibilityReportCatalog")]
public class Catalog
{
	// Catalog structure version will change on structural changes that make the xml incompatible. Version will not reset on a new StructureVersion.
	public int Version { get; set; }
	public DateTime Updated { get; set; }

	// A note about the catalog, displayed in the report, and the header and footer text for the report.
	[XmlElement("Note")]
	public TextElement? Note { get; set; }
	public string? ReportHeaderText { get; set; }
	public string? ReportFooterText { get; set; }

	// The actual mod data in four lists.
	public List<Mod> Mods { get; set; } = new List<Mod>();
	public List<Group> Groups { get; set; } = new List<Group>();
	public List<Compatibility> Compatibilities { get; set; } = new List<Compatibility>();
	public List<Author> Authors { get; set; } = new List<Author>();

	// Map Themes are technically mods, but we don't want to include them in the catalog or the report. This list is used to recognize them.
	[XmlArrayItem("SteamID")] public List<ulong> MapThemes { get; set; } = new List<ulong>();

	// Assets that show up as required items. This is used to distinguish between a required asset and an unknown required mod.
	[XmlArrayItem("SteamID")] public List<ulong> RequiredAssets { get; set; } = new List<ulong>();

	// Steam IDs that give warnings we don't want to see, either about an unnamed mod or about a duplicate author name (add both authors).
	[XmlArrayItem("SteamID")] public List<ulong> SuppressedWarnings { get; set; } = new List<ulong>();

	// Temporary list of newly found unknown required Steam IDs which might be assets, to be evaluated for adding to the RequiredAssets list.
	private readonly List<ulong> potentialAssets = new List<ulong>();

	// Dictionaries for faster lookup.
	private readonly Dictionary<ulong, Mod> modIndex = new Dictionary<ulong, Mod>();
	private readonly Dictionary<ulong, Group> groupIndex = new Dictionary<ulong, Group>();
	private readonly Dictionary<ulong, Author> authorIDIndex = new Dictionary<ulong, Author>();
	private readonly Dictionary<string, Author> authorUrlIndex = new Dictionary<string, Author>();
	private readonly List<ulong> subscriptionIDIndex = new List<ulong>();
	private readonly Dictionary<string, List<ulong>> subscriptionNameIndex = new Dictionary<string, List<ulong>>();
	private readonly Dictionary<ulong, List<Compatibility>> subscriptionCompatibilityIndex = new Dictionary<ulong, List<Compatibility>>();

	public static string SettingsUIText { get; private set; } = "unknown, catalog not loaded yet.";

	public void CreateIndexes()
	{
		foreach (var catalogMod in Mods)
		{
			if (!modIndex.ContainsKey(catalogMod.SteamID))
			{
				modIndex.Add(catalogMod.SteamID, catalogMod);
			}
		}

		foreach (var catalogGroup in Groups)
		{
			if (!groupIndex.ContainsKey(catalogGroup.GroupID))
			{
				groupIndex.Add(catalogGroup.GroupID, catalogGroup);
			}
		}

		foreach (var catalogAuthor in Authors)
		{
			if (catalogAuthor.SteamID != 0 && !authorIDIndex.ContainsKey(catalogAuthor.SteamID))
			{
				authorIDIndex.Add(catalogAuthor.SteamID, catalogAuthor);
			}

			if (!string.IsNullOrEmpty(catalogAuthor.CustomUrl) && !authorUrlIndex.ContainsKey(catalogAuthor.CustomUrl))
			{
				authorUrlIndex.Add(catalogAuthor.CustomUrl, catalogAuthor);
			}
		}

		foreach (var item in CentralManager.Mods)
		{
			subscriptionCompatibilityIndex[item.SteamId]=new List<Compatibility>();
		}

		foreach (Compatibility catalogCompatibility in Compatibilities)
		{
			if (subscriptionIDIndex.Contains(catalogCompatibility.FirstModID) && subscriptionIDIndex.Contains(catalogCompatibility.SecondModID))
			{
				subscriptionCompatibilityIndex[catalogCompatibility.FirstModID].Add(catalogCompatibility);
				subscriptionCompatibilityIndex[catalogCompatibility.SecondModID].Add(catalogCompatibility);
			}
		}
	}

	public Mod? GetMod(ulong steamId)
	{
		return modIndex.TryGet(steamId)?.Clone();
	}

	public Author? GetAuthor(ulong authorID, string authorUrl)
	{
        return authorIDIndex.ContainsKey(authorID) ? authorIDIndex[authorID] : authorUrlIndex.ContainsKey(authorUrl ?? "") ? authorUrlIndex[authorUrl ?? ""] : null;
	}
	public bool IsGroupMember(ulong steamID)
	{
		return GetThisModsGroup(steamID) != default;
	}

	public bool IsValidID(ulong steamID, bool allowBuiltin = true, bool shouldExist = true)
	{
		bool valid = (steamID > 999999);

		bool exists = modIndex.ContainsKey(steamID);

		return valid && (shouldExist ? exists : !exists);
	}

	public Group GetThisModsGroup(ulong steamID)
	{
		return Groups.FirstOrDefault(x => x.GroupMembers.Contains(steamID));
	}

	public Mod? GetSubscription(ulong steamID)
	{
		return CentralManager.Packages.Any(x => x.SteamId == steamID && (x.Mod?.IsIncluded ?? true) && (x.Mod?.IsEnabled ?? true)) && modIndex.ContainsKey(steamID) ? modIndex[steamID] : null;
	}

    public List<Compatibility> GetSubscriptionCompatibilities(ulong steamID)
	{
		return GetSubscription(steamID) == null || !subscriptionCompatibilityIndex.ContainsKey(steamID) ? new List<Compatibility>() : subscriptionCompatibilityIndex[steamID];
	}
}
