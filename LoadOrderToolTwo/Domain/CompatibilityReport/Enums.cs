namespace CompatibilityReport.CatalogData;

public static class Enums
{
	/// <summary>The stability of a mod.</summary>
	/// <remarks>Order matters because it is used by the Catalog and Reporter.</remarks>
	public enum Stability
	{
		Undefined = 0,                      // Unused.
		NotReviewed,                        // Mod is not reviewed yet, stability is unknown. Assigned by default.
		Stable,                             // This mod will function.
		NotEnoughInformation,               // Stability unknown, because we don't have enough information yet to determine it.
		UsersReportIssues,                  // Stability uncertain, but various users report issues, while others may say it still works fine for them.
		MinorIssues,                        // Will function but may give minor issues.
		MajorIssues,                        // Will function, at least partially, but with some serious issues.
		Broken,                             // Broken, as in doesn't really function.
		GameBreaking,                       // Broken and also crashes or otherwise disrupts the game.
		RequiresIncompatibleMod,            // The mod requires a mod that is incompatible according to the Steam Workshop, and is thus incompatible itself.
		IncompatibleAccordingToWorkshop     // The status the Steam Workshop uses for seriously broken mods. These are incompatible with the game itself.
	}


	/// <summary>Statuses of a mod. A mod can have none, one or multiple. Not all can be combined, like unlisted and removed or multiple 'music' statuses.</summary>
	public enum Status
	{
		Undefined = 0,                      // Unused.
		UnlistedInWorkshop,                 // Available in the Steam Workshop, but is not shown in searches or collections. Can only be found with a direct link.
		RemovedFromWorkshop,                // This was once available in the Steam Workshop, but no more. Better not to use it anymore.
		NoDescription,                      // No (real) description in the Steam Workshop for this mod, which indicates a sparsely supported mod.
		NoCommentSection,                   // This mods Steam Workshop page has the comment section disabled, making it hard to see if people are experiencing issues.
		Obsolete,                           // No longer needed, because whatever it did is now done by the game itself or by the mod it was a patch/addon for.
		Deprecated,                         // No longer supported and should not be used anymore.
		Abandoned,                          // No longer updated and the author doesn't respond to questions/issues in the comments.
		Reupload,                           // This is reupload from another persons mod and should not be used. Use the original, which should be added as alternative.
		SavesCantLoadWithout,               // Needed to successfully load a savegame where it was used. You may lose access to the savegame if the mod breaks.
		BreaksEditors,                      // Gives serious issues in the map and/or asset editor, or prevents them from loading.
		TestVersion,                        // This is a test/beta/alpha/experimental version, and a stable version exists as well.
		DependencyMod,                      // This is a dependency mod (needed for other mods to function) and adds no functionality on its own.
		ModForModders,                      // Only needed for modders, to help in creating mods or assets. No use for regular players.
		SourceBundled,                      // The source files are bundled with the mod and can be found in the mods folder.
		SourceNotUpdated,                   // Source files are not updated, making it hard for other modders to support compatibility or take over when abandoned.
		SourceObfuscated,                   // The author has deliberately hidden the mod code from other modders, which is somewhat suspicious.
		MusicCopyrightFree,                 // This mod uses music that is all copyright-free. It's safe to use for videos and streaming.
		MusicCopyrighted,                   // This mod uses music with copyright and should not be used in videos and streaming.
		MusicCopyrightUnknown,              // This mod uses music, but it's unclear whether that music has copyright on it or not. Not safe for videos or streaming.
		WorksWhenDisabled,                  // This mod will still work when disabled. This suppresses any 'enable this' warnings.
		ModNamesDiffer,                     // This mods in-game name differs so much from the Workshop name that it can be confusing.
		SourceNotPublic,                    // No source code published. Without source code it is harder to ensure a mod not include malicious code.
		MusicCopyright                      // As the included music might be copyright protected, it is better not to use Music Packs for streaming to avoid legal issues.
	}




	/// <summary>Compatibility statuses between two mods. In most cases only one compatibility can exist between two mods.</summary>
	/// <remarks>Don't create 'mirrored' compatibilities (between A and B and between B and A), the mods handles that. 
	///          RequiresSpecificSettings can exist next to MinorIssues, MajorIssues, CompatibleAccordingToAuthor or SameFunctionalityCompatible.
	///          SameModDifferentReleaseType can be created when SameFunctionality already exists, but the latter (including note) will then be removed.</remarks>
	public enum CompatibilityStatus
	{
		Undefined = 0,                      // Unused.
		SameModDifferentReleaseType,        // These mods are different release types ('stable' vs. 'beta', etc.) of the same mod.
		SameFunctionality,                  // These mods are incompatible because they change the same functionality (at least partially).
		SameFunctionalityCompatible,        // These mods are compatible, but have the same functionality (could be partially) and shouldn't both be necessary. 
		IncompatibleAccordingToAuthor,      // These mods are incompatible according to the author of one of the mods
		IncompatibleAccordingToUsers,       // These mods are incompatible according to users of one of the mods. Should only be used on 'clear cases', not on a whim.
		CompatibleAccordingToAuthor,        // These mods are fully compatible according to the author of one of the mods.
		MajorIssues,                        // These mods have some serious issues when used together. A compatibility note should clarify this and is mandatory.
		MinorIssues,                        // These mods have minor issues when used together. A compatibility note should clarify this and is mandatory.
		RequiresSpecificSettings            // These mods require specific settings when used together. A compatibility note should clarify this and is mandatory.
	}


	/// <summary>DLCs that might be required for a mod.</summary>
	/// <remarks>The numbers are the AppIDs of the DLC. The enum names are used as DLC names in the report, 
	///          with double underscores replaced by colon+space, and single underscores replaced by a space.</remarks>
	public enum Dlc : uint
	{
		Unknown = 0,                        // Unused
		Deluxe_Edition = 346791,
		After_Dark = 369150,
		Snowfall = 420610,
		Match_Day = 456200,
		Content_Creator_Pack__Art_Deco = 515190,
		Natural_Disasters = 515191,
		Stadiums_Europe = 536610,
		Content_Creator_Pack__High_Tech_Buildings = 547500,
		Relaxation_Station = 547501,
		Mass_Transit = 547502,
		Pearls_From_the_East = 563850,
		Green_Cities = 614580,
		Concerts = 614581,
		Rock_City_Radio = 614582,
		Content_Creator_Pack__European_Suburbia = 715190,
		Park_Life = 715191,
		Carols_Candles_and_Candy = 715192,
		All_That_Jazz = 715193,
		Industries = 715194,
		Country_Road_Radio = 815380,
		Synthetic_Dawn_Radio = 944070,
		Campus = 944071,
		Content_Creator_Pack__University_City = 1059820,
		Deep_Focus_Radio = 1065490,
		Campus_Radio = 1065491,
		Sunset_Harbor = 1146930,
		Content_Creator_Pack__Modern_City_Center = 1148020,
		Downtown_Radio = 1148021,
		Content_Creator_Pack__Modern_Japan = 1148022,
		Coast_to_Coast_Radio = 1196100,
		Rail_Hawk_Radio = 1531472,
		Sunny_Breeze_Radio = 1531473,
		Content_Creator_Pack__Train_Station = 1531470,
		Content_Creator_Pack__Bridges_and_Piers = 1531471,
		Airports = 1726380,
		Content_Creator_Pack__Vehicles_of_the_World = 1726381,
		Content_Creator_Pack__Map_Pack = 1726382,
		On_Air_Radio = 1726383,
		Calm_The_Mind_Radio = 1726384,
		Content_Creator_Pack__MidCentury_Modern = 1992290,
		Content_Creator_Pack__Seaside_Resorts = 1992291,
		Shoreline_Radio = 1992292,
		Paradise_Radio = 1992293,
		Plazas_and_Promenades = 2008400,
		Content_Creator_Pack__Heart_of_Korea = 2144480,
		Content_Creator_Pack__Skyscrapers = 2144481,
		Kpop_Station = 2144482,
		Eighties_Downtown_Beat = 2144483,
		Financial_Districts = 2148901,
		Content_Creator_Pack__Map_Pack_2 = 2148903,
		African_Vibes = 2148904
	}


	/// <summary>The severity level for the report.</summary>
	/// <remarks>Order matters because it is used by the Reporter.</remarks>
	public enum ReportSeverity
	{
		NothingToReport = 0,
		Remarks,
		MinorIssues,
		MajorIssues,
		Unsubscribe
	}
	public enum ReportSeverityFilter
	{
		Any,
		AnyIssue,
		NothingToReport,
		Remarks,
		MinorIssues,
		MajorIssues,
		Unsubscribe
	}

	public enum ReportType
	{
		Stability,
		Successors,
		RequiredMods,
		DlcMissing,
		Compatibility,
		UnneededDependency,
		WorksWhenDisabled,
		Alternatives,
		Status,
		Recommendations,
		Note,
	}
}
