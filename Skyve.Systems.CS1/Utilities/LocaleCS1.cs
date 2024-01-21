using Extensions;

using Skyve.Domain.Systems;

namespace Skyve.Systems.CS1.Utilities;
public class LocaleCS1 : LocaleHelper, ILocale
{
	private static readonly LocaleCS1 _instance = new();

	public static void Load() { _ = _instance; }

	public Translation Get(string key)
	{
		return GetGlobalText(key);
	}

	public LocaleCS1() : base($"Skyve.Systems.CS1.Properties.LocaleCS1.json") { }

	/// <summary>
	/// Show both of the included and enabled state for mods
	/// </summary>
	public static Translation AdvancedIncludeEnable => _instance.GetText("AdvancedIncludeEnable");

	/// <summary>
	/// Cities: Skylines' App Data Folder
	/// </summary>
	public static Translation AppDataPath => _instance.GetText("AppDataPath");

	/// <summary>
	/// Force Steam to download all pending packages
	/// </summary>
	public static Translation ApplyDownloads => _instance.GetText("ApplyDownloads");

	/// <summary>
	/// Delete all pending packages
	/// </summary>
	public static Translation ApplyRemoval => _instance.GetText("ApplyRemoval");

	/// <summary>
	/// Launch the game for a brief moment to apply pending subscriptions
	/// </summary>
	public static Translation ApplySubs => _instance.GetText("ApplySubs");

	/// <summary>
	/// Auto-save is disabled
	/// </summary>
	public static Translation AutoPlaysetSaveOff => _instance.GetText("AutoPlaysetSaveOff");

	/// <summary>
	/// Auto-save is enabled
	/// </summary>
	public static Translation AutoPlaysetSaveOn => _instance.GetText("AutoPlaysetSaveOn");

	/// <summary>
	/// Automatic Save
	/// </summary>
	public static Translation AutoSave => _instance.GetText("AutoSave");

	/// <summary>
	/// Automatically save changes to mods &amp; assets to your playset
	/// </summary>
	public static Translation AutoSaveTip => _instance.GetText("AutoSave_Tip");

	/// <summary>
	/// Cancel all pending downloads
	/// </summary>
	public static Translation CancelDownloads => _instance.GetText("CancelDownloads");

	/// <summary>
	/// Cancel all pending subscriptions
	/// </summary>
	public static Translation CancelSubs => _instance.GetText("CancelSubs");

	/// <summary>
	/// Changing the paths used by the app will require a restart, do you wish to proceed?
	/// </summary>
	public static Translation ChangingFoldersRequiresRestart => _instance.GetText("ChangingFoldersRequiresRestart");

	/// <summary>
	/// Check the options panel in case the wrong folders are being used.
	/// </summary>
	public static Translation CheckFolderInOptions => _instance.GetText("CheckFolderInOptions");

	/// <summary>
	/// Skyve finished cleaning up your Workshop downloads.
	/// </summary>
	public static Translation CleanupComplete => _instance.GetText("CleanupComplete");

	/// <summary>
	/// Run a quick cleanup that deletes any package you're not actually subscribed to from your computer as well as forcing the download of missing subscribed packages.
	/// </summary>
	public static Translation CleanupInfo => _instance.GetText("CleanupInfo");

	/// <summary>
	/// The cleanup process requires opening the game for a brief moment. When you see the game's launcher, click on Play to proceed.
	/// </summary>
	public static Translation CleanupRequiresGameToOpen => _instance.GetText("CleanupRequiresGameToOpen");

	/// <summary>
	/// Workshop Packages Cleanup
	/// </summary>
	public static Translation CleanupTitle => _instance.GetText("CleanupTitle");

	/// <summary>
	/// Resetting your folder settings will require you to launch the game before you can open the tool again.
	/// </summary>
	public static Translation ClearFoldersPrompt => _instance.GetText("ClearFoldersPrompt");

	/// <summary>
	/// Confirm Action
	/// </summary>
	public static Translation ClearFoldersPromptTitle => _instance.GetText("ClearFoldersPromptTitle");

	/// <summary>
	/// You need to close Cities: Skylines to run the cleanup.
	/// </summary>
	public static Translation CloseCitiesToClean => _instance.GetText("CloseCitiesToClean");

	/// <summary>
	/// Steam collection URL or ID number
	/// </summary>
	public static Translation CollectionLink => _instance.GetText("CollectionLink");

	/// <summary>
	/// Workshop Collections
	/// </summary>
	public static Translation CollectionTitle => _instance.GetText("CollectionTitle");

	/// <summary>
	/// <para>Copy this package's Workshop ID</para>
	/// <para>Plural: Copy the selected packages' Workshop IDs</para>
	/// </summary>
	public static Translation CopyWorkshopId => _instance.GetText("CopyWorkshopId");

	/// <summary>
	/// <para>Copy Workshop link</para>
	/// <para>Plural: Copy the selected packages' Workshop links</para>
	/// </summary>
	public static Translation CopyWorkshopLink => _instance.GetText("CopyWorkshopLink");

	/// <summary>
	/// Delete all displayed items
	/// </summary>
	public static Translation DeleteAll => _instance.GetText("DeleteAll");

	/// <summary>
	/// Detected Issues
	/// </summary>
	public static Translation DetectedIssues => _instance.GetText("DetectedIssues");

	/// <summary>
	/// You have FPS Booster enabled while using debug mono, would you like to disable it to get better logs?
	/// </summary>
	public static Translation DisableFpsBoosterDebug => _instance.GetText("DisableFpsBoosterDebug");

	/// <summary>
	/// Disable automatic package cleanup
	/// </summary>
	public static Translation DisablePackageCleanup => _instance.GetText("DisablePackageCleanup");

	/// <summary>
	/// Stops the tool from automatically deleting packages you are not subscribed to when opening the game.
	/// </summary>
	public static Translation DisablePackageCleanupTip => _instance.GetText("DisablePackageCleanup_Tip");

	/// <summary>
	/// Download all displayed &amp; missing items
	/// </summary>
	public static Translation DownloadAll => _instance.GetText("DownloadAll");

	/// <summary>
	/// <para>Download this package</para>
	/// <para>Plural: Download all selected Packages</para>
	/// </summary>
	public static Translation DownloadPackage => _instance.GetText("DownloadPackage");

	/// <summary>
	/// Packages you subscribe to and haven't been downloaded yet are grouped here.
	/// </summary>
	public static Translation DownloadsPendingInfo => _instance.GetText("DownloadsPendingInfo");

	/// <summary>
	/// Drop or select a playset's .json or .xml file here to import it
	/// </summary>
	public static Translation DropNewPlayset => _instance.GetText("DropNewPlayset");

	/// <summary>
	/// Exclude all items
	/// </summary>
	public static Translation ExcludeAll => _instance.GetText("ExcludeAll");

	/// <summary>
	/// Exclude all disabled items
	/// </summary>
	public static Translation ExcludeAllDisabled => _instance.GetText("ExcludeAllDisabled");

	/// <summary>
	/// Exclude filtered &amp; disabled items
	/// </summary>
	public static Translation ExcludeAllDisabledFiltered => _instance.GetText("ExcludeAllDisabledFiltered");

	/// <summary>
	/// Exclude selected &amp; disabled items
	/// </summary>
	public static Translation ExcludeAllDisabledSelected => _instance.GetText("ExcludeAllDisabledSelected");

	/// <summary>
	/// Exclude filtered items
	/// </summary>
	public static Translation ExcludeAllFiltered => _instance.GetText("ExcludeAllFiltered");

	/// <summary>
	/// <para>Exclude all items in this package</para>
	/// <para>Plural: Exclude all of the selected packages</para>
	/// </summary>
	public static Translation ExcludeAllItemsInThisPackage => _instance.GetText("ExcludeAllItemsInThisPackage");

	/// <summary>
	/// Exclude selected items
	/// </summary>
	public static Translation ExcludeAllSelected => _instance.GetText("ExcludeAllSelected");

	/// <summary>
	/// Exclude this item
	/// </summary>
	public static Translation ExcludeItem => _instance.GetText("ExcludeItem");

	/// <summary>
	/// <para>Exclude this item in all your playsets</para>
	/// <para>Plural: Exclude the selected items in all your playsets</para>
	/// </summary>
	public static Translation ExcludeThisItemInAllPlaysets => _instance.GetText("ExcludeThisItemInAllPlaysets");

	/// <summary>
	/// Failed to convert this legacy playset.
	/// </summary>
	public static Translation FailedToImportLegacyPlayset => _instance.GetText("FailedToImportLegacyPlayset");

	/// <summary>
	/// Please enable the Skyve Mod inside Cities: Skylines before using the Skyve App.  Or click ignore to manually set up your folder settings.
	/// </summary>
	public static Translation FirstSetupInfo => _instance.GetText("FirstSetupInfo");

	/// <summary>
	/// Fix All Issues
	/// </summary>
	public static Translation FixAllIssues => _instance.GetText("FixAllIssues");

	/// <summary>
	/// Start the download or removal of packages immediately after subscribing/unsubscribing
	/// </summary>
	public static Translation ForceDownloadAndDeleteAsSoonAsRequested => _instance.GetText("ForceDownloadAndDeleteAsSoonAsRequested");

	/// <summary>
	/// Forces the download or removal of packages instead of waiting till they are actually subscribed or unsubscribed.
	/// </summary>
	public static Translation ForceDownloadAndDeleteAsSoonAsRequestedTip => _instance.GetText("ForceDownloadAndDeleteAsSoonAsRequested_Tip");

	/// <summary>
	/// Cities: Skylines' Steam Folder
	/// </summary>
	public static Translation GamePath => _instance.GetText("GamePath");

	/// <summary>
	/// Hide pseudo-mods like theme mixes
	/// </summary>
	public static Translation HidePseudoMods => _instance.GetText("HidePseudoMods");

	/// <summary>
	/// Include all items
	/// </summary>
	public static Translation IncludeAll => _instance.GetText("IncludeAll");

	/// <summary>
	/// Include filtered items
	/// </summary>
	public static Translation IncludeAllFiltered => _instance.GetText("IncludeAllFiltered");

	/// <summary>
	/// <para>Include all items in this package</para>
	/// <para>Plural: Include all of the selected packages</para>
	/// </summary>
	public static Translation IncludeAllItemsInThisPackage => _instance.GetText("IncludeAllItemsInThisPackage");

	/// <summary>
	/// Include selected items
	/// </summary>
	public static Translation IncludeAllSelected => _instance.GetText("IncludeAllSelected");

	/// <summary>
	/// Include this item
	/// </summary>
	public static Translation IncludeItem => _instance.GetText("IncludeItem");

	/// <summary>
	/// <para>Include this item in all your playsets</para>
	/// <para>Plural: Include the selected items in all your playsets</para>
	/// </summary>
	public static Translation IncludeThisItemInAllPlaysets => _instance.GetText("IncludeThisItemInAllPlaysets");

	/// <summary>
	/// Incorrect Folder Settings
	/// </summary>
	public static Translation IncorrectFolderSettings => _instance.GetText("IncorrectFolderSettings");

	/// <summary>
	/// Your folder settings are not set up correctly, click here to change or reset them in the Options page.
	/// </summary>
	public static Translation IncorrectFolderSettingsInfo => _instance.GetText("IncorrectFolderSettingsInfo");

	/// <summary>
	/// Launch through Cities.exe
	/// </summary>
	public static Translation LaunchThroughCities => _instance.GetText("LaunchThroughCities");

	/// <summary>
	/// Load Collection
	/// </summary>
	public static Translation LoadCollection => _instance.GetText("LoadCollection");

	/// <summary>
	/// Load the collection’s items list and manage which items you want to subscribe to, include, or exclude from this collection.
	/// </summary>
	public static Translation LoadCollectionTip => _instance.GetText("LoadCollectionTip");

	/// <summary>
	/// Load all enabled assets
	/// </summary>
	public static Translation LoadEnabled => _instance.GetText("LoadEnabled");

	/// <summary>
	/// Loads enabled assets while loading a game. This should be left enabled unless you know what you're doing
	/// </summary>
	public static Translation LoadEnabledTip => _instance.GetText("LoadEnabled_Tip");

	/// <summary>
	/// Loading Screen Mod
	/// </summary>
	public static Translation LoadingScreenMod => _instance.GetText("LoadingScreenMod");

	/// <summary>
	/// Load Asset (Editor)
	/// </summary>
	public static Translation LoadLoadAsset => _instance.GetText("LoadLoadAsset");

	/// <summary>
	/// New Asset (Editor)
	/// </summary>
	public static Translation LoadNewAsset => _instance.GetText("LoadNewAsset");

	/// <summary>
	/// Load used assets
	/// </summary>
	public static Translation LoadUsed => _instance.GetText("LoadUsed");

	/// <summary>
	/// Load assets that are used in your map, even if they are disabled in-game. This is not related to the asset settings inside Skyve
	/// </summary>
	public static Translation LoadUsedTip => _instance.GetText("LoadUsed_Tip");

	/// <summary>
	/// Skyve will temporarily close during the update process, but it will automatically resume once the update is complete.
	/// </summary>
	public static Translation LOTWillRestart => _instance.GetText("LOTWillRestart");

	/// <summary>
	/// Loading Screen Mod Report
	/// </summary>
	public static Translation LsmImport => _instance.GetText("LsmImport");

	/// <summary>
	/// Make sure to select the 'Report' file
	/// </summary>
	public static Translation LsmImportInfo => _instance.GetText("LsmImportInfo");

	/// <summary>
	/// View and subscribe to missing assets
	/// </summary>
	public static Translation LsmImportMissingInfo => _instance.GetText("LsmImportMissingInfo");

	/// <summary>
	/// View and remove unused assets
	/// </summary>
	public static Translation LsmImportUnusedInfo => _instance.GetText("LsmImportUnusedInfo");

	/// <summary>
	/// Drag &amp; drop a LSM report file here or click to select one to subscribe to missing assets listed in the report
	/// </summary>
	public static Translation LsmMissingTip => _instance.GetText("LsmMissingTip");

	/// <summary>
	/// Drag &amp; drop a LSM report file here or click to select one to manage assets you haven't used in your save-game that are listed in the report
	/// </summary>
	public static Translation LsmUnusedTip => _instance.GetText("LsmUnusedTip");

	/// <summary>
	/// Some items are still missing from this playset. Saving the playset in the future will remove those items from it.
	/// </summary>
	public static Translation MissingItemsRemain => _instance.GetText("MissingItemsRemain");

	/// <summary>
	/// LSM Missing Assets
	/// </summary>
	public static Translation MissingLSMReport => _instance.GetText("MissingLSMReport");

	/// <summary>
	/// Missing packages from '{0}'
	/// </summary>
	public static Translation MissingPackagesPlayset => _instance.GetText("MissingPackagesPlayset");

	/// <summary>
	/// You have multiple versions of Skyve installed or subscribed to, which can cause conflicts. To avoid issues, please remove or unsubscribe from all other versions except for the one you're using
	/// </summary>
	public static Translation MultipleLOM => _instance.GetText("MultipleLOM");

	/// <summary>
	/// Disable All Assets
	/// </summary>
	public static Translation NoAssets => _instance.GetText("NoAssets");

	/// <summary>
	/// Disable All Mods
	/// </summary>
	public static Translation NoMods => _instance.GetText("NoMods");

	/// <summary>
	/// Disable workshop packages
	/// </summary>
	public static Translation NoWorkshop => _instance.GetText("NoWorkshop");

	/// <summary>
	/// Open C:S's AppData folder
	/// </summary>
	public static Translation OpenCitiesAppData => _instance.GetText("OpenCitiesAppData");

	/// <summary>
	/// Open Steam links in your browser
	/// </summary>
	public static Translation OpenLinksInBrowser => _instance.GetText("OpenLinksInBrowser");

	/// <summary>
	/// Change Steam links to open in your default browser rather than within the Steam application.
	/// </summary>
	public static Translation OpenLinksInBrowserTip => _instance.GetText("OpenLinksInBrowser_Tip");

	/// <summary>
	/// Override changes made in-game to the Enabled status of mods
	/// </summary>
	public static Translation OverrideGameChanges => _instance.GetText("OverrideGameChanges");

	/// <summary>
	/// The local size of '{0}' ({1}) is different than Steam's {2}
	/// </summary>
	public static Translation PackageIsIncomplete => _instance.GetText("PackageIsIncomplete");

	/// <summary>
	/// '{0}' was removed from Steam
	/// </summary>
	public static Translation PackageIsRemoved => _instance.GetText("PackageIsRemoved");

	/// <summary>
	/// The information from Steam hasn't loaded for '{0}' yet
	/// </summary>
	public static Translation PackageIsUnknown => _instance.GetText("PackageIsUnknown");

	/// <summary>
	/// Paste the URL or ID of the collection you'd like to view
	/// </summary>
	public static Translation PasteCollection => _instance.GetText("PasteCollection");

	/// <summary>
	/// <para>{0} item pending removal</para>
	/// <para>Plural: {0} items pending removal</para>
	/// </summary>
	public static Translation PendingDeletions => _instance.GetText("PendingDeletions");

	/// <summary>
	/// <para>{0} item pending download</para>
	/// <para>Plural: {0} items pending download</para>
	/// </summary>
	public static Translation PendingDownloads => _instance.GetText("PendingDownloads");

	/// <summary>
	/// <para>{0} item pending subscription</para>
	/// <para>Plural: {0} items pending subscription</para>
	/// </summary>
	public static Translation PendingSubscribeTo => _instance.GetText("PendingSubscribeTo");

	/// <summary>
	/// <para>{0} item pending unsubscription</para>
	/// <para>Plural: {0} items pending unsubscription</para>
	/// </summary>
	public static Translation PendingUnsubscribeFrom => _instance.GetText("PendingUnsubscribeFrom");

	/// <summary>
	/// Some characters were removed from your playset name because they can not be used in a windows file name
	/// </summary>
	public static Translation PlaysetNameChangedIllegalChars => _instance.GetText("PlaysetNameChangedIllegalChars");

	/// <summary>
	/// You already have a playset with the same name. Please change the name of either one of the files to continue.
	/// </summary>
	public static Translation PlaysetNameUsed => _instance.GetText("PlaysetNameUsed");

	/// <summary>
	/// Changes in this screen are saved automatically, the "Automatic Save" option and the save button at the top affect your selected mods, assets &amp; DLCs only
	/// </summary>
	public static Translation PlaysetSaveInfo => _instance.GetText("PlaysetSaveInfo");

	/// <summary>
	/// {0} votes {1}
	/// </summary>
	public static Translation RatingCount => _instance.GetText("RatingCount");

	/// <summary>
	/// Re-Download all displayed &amp; downloaded items
	/// </summary>
	public static Translation ReDownloadAll => _instance.GetText("ReDownloadAll");

	/// <summary>
	/// Skyve finished re-downloading the packages with issues.
	/// </summary>
	public static Translation RedownloadComplete => _instance.GetText("RedownloadComplete");

	/// <summary>
	/// Clear Steam cache
	/// </summary>
	public static Translation ResetSteamCache => _instance.GetText("ResetSteamCache");

	/// <summary>
	/// Run Cleanup
	/// </summary>
	public static Translation RunCleanup => _instance.GetText("RunCleanup");

	/// <summary>
	/// Select a save-game file to be automatically loaded when you open the game
	/// </summary>
	public static Translation SaveFileInfo => _instance.GetText("SaveFileInfo");

	/// <summary>
	/// Save the current package settings to this playset's file
	/// </summary>
	public static Translation SavePlaysetChanges => _instance.GetText("SavePlaysetChanges");

	/// <summary>
	/// Set Up is Incomplete
	/// </summary>
	public static Translation SetupIncomplete => _instance.GetText("SetupIncomplete");

	/// <summary>
	/// Show folder settings
	/// </summary>
	public static Translation ShowFolderSettings => _instance.GetText("ShowFolderSettings");

	/// <summary>
	/// Select a Skip-file to be used for Loading Screen Mod
	/// </summary>
	public static Translation SkipFileInfo => _instance.GetText("SkipFileInfo");

	/// <summary>
	/// Launch Cities: Skylines
	/// </summary>
	public static Translation StartCities => _instance.GetText("StartCities");

	/// <summary>
	/// Steam's Installation Folder
	/// </summary>
	public static Translation SteamPath => _instance.GetText("SteamPath");

	/// <summary>
	/// Stop Cities: Skylines
	/// </summary>
	public static Translation StopCities => _instance.GetText("StopCities");

	/// <summary>
	/// Subscribe
	/// </summary>
	public static Translation Subscribe => _instance.GetText("Subscribe");

	/// <summary>
	/// Subscribe to all displayed items
	/// </summary>
	public static Translation SubscribeAll => _instance.GetText("SubscribeAll");

	/// <summary>
	/// <para>Subscribe from this Package</para>
	/// <para>Plural: Subscribe from the selected Packages</para>
	/// </summary>
	public static Translation SubscribeToItem => _instance.GetText("SubscribeToItem");

	/// <summary>
	/// Subscribing or unsubscribing from packages requires opening the game for a brief moment.  When you see the game's launcher, click on Play to proceed.
	/// </summary>
	public static Translation SubscribingRequiresGameToOpen => _instance.GetText("SubscribingRequiresGameToOpen");

	/// <summary>
	/// Subscription Process
	/// </summary>
	public static Translation SubscribingRequiresGameToOpenTitle => _instance.GetText("SubscribingRequiresGameToOpenTitle");

	/// <summary>
	/// Packages you subscribe to are grouped here. Once you open the game, those items will be subscribed to on Steam.
	/// </summary>
	public static Translation SubsPendingInfo => _instance.GetText("SubsPendingInfo");

	/// <summary>
	/// Temporary Playset
	/// </summary>
	public static Translation TemporaryPlayset => _instance.GetText("TemporaryPlayset");

	/// <summary>
	/// Temporary playset's settings will reset when you re-open the app
	/// </summary>
	public static Translation TemporaryPlaysetCanNotBeEdited => _instance.GetText("TemporaryPlaysetCanNotBeEdited");

	/// <summary>
	/// Switch to a temporary playset
	/// </summary>
	public static Translation TempPlayset => _instance.GetText("TempPlayset");

	/// <summary>
	/// Switch to a temporary playset with your current settings
	/// </summary>
	public static Translation TempPlaysetTip => _instance.GetText("TempPlayset_Tip");

	/// <summary>
	/// Use Unity Profiler
	/// </summary>
	public static Translation UnityProfilerMode => _instance.GetText("UnityProfilerMode");

	/// <summary>
	/// Unsubscribe from all displayed items
	/// </summary>
	public static Translation UnsubscribeAll => _instance.GetText("UnsubscribeAll");

	/// <summary>
	/// <para>Unsubscribe to this package</para>
	/// <para>Plural: Unsubscribe to the selected packages</para>
	/// </summary>
	public static Translation UnsubscribePackage => _instance.GetText("UnsubscribePackage");

	/// <summary>
	/// LSM Unused Assets
	/// </summary>
	public static Translation UnusedLSMReport => _instance.GetText("UnusedLSMReport");

	/// <summary>
	/// Skyve is currently being updated. Please wait...
	/// </summary>
	public static Translation UpdatingLot => _instance.GetText("UpdatingLot");

	/// <summary>
	/// Use Debug Mono
	/// </summary>
	public static Translation UseDebugMono => _instance.GetText("UseDebugMono");

	/// <summary>
	/// Use a skip-file to exclude vanilla assets
	/// </summary>
	public static Translation UseSkipFile => _instance.GetText("UseSkipFile");

	/// <summary>
	/// Use this to exclude vanilla assets from your game
	/// </summary>
	public static Translation UseSkipFileTip => _instance.GetText("UseSkipFile_Tip");

	/// <summary>
	/// View this package on Steam
	/// </summary>
	public static Translation ViewOnWorkshop => _instance.GetText("ViewOnWorkshop");

	/// <summary>
	/// Real "Cities: Skylines' App Data Folder" on your Linux/Mac
	/// </summary>
	public static Translation VirtualAppDataPath => _instance.GetText("VirtualAppDataPath");

	/// <summary>
	/// Real "Cities: Skylines' Steam Folder" on your Linux/Mac
	/// </summary>
	public static Translation VirtualGamePath => _instance.GetText("VirtualGamePath");

	/// <summary>
	/// The voting icon displayed in the list is split into 3 layers to better represent both small or niche packages, and large &amp; popular packages.
	/// </summary>
	public static Translation VotingInfo1 => _instance.GetText("VotingInfo1");

	/// <summary>
	/// The first layer is the vote count, which represents the scale of votes based on how many subscribers the package has.
	/// </summary>
	public static Translation VotingInfo2 => _instance.GetText("VotingInfo2");

	/// <summary>
	/// This uses a special formula to try and be as fair as possible to all creators.
	/// </summary>
	public static Translation VotingInfo3 => _instance.GetText("VotingInfo3");

	/// <summary>
	/// Negative votes count for {0} compared to positive votes.
	/// </summary>
	public static Translation VotingInfo4 => _instance.GetText("VotingInfo4");

	/// <summary>
	/// The second layer is the subscriber count, represented by the golden circle around the icon which scales up to {0} subscribers.
	/// </summary>
	public static Translation VotingInfo5 => _instance.GetText("VotingInfo5");

	/// <summary>
	/// The third and final layer is when a package has a high voting score and over {0} subscribers, represented by a golden icon.
	/// </summary>
	public static Translation VotingInfo6 => _instance.GetText("VotingInfo6");

	/// <summary>
	/// How the voting system works
	/// </summary>
	public static Translation VotingTitle => _instance.GetText("VotingTitle");

	/// <summary>
	/// Import XML Configuration
	/// </summary>
	public static Translation XMLImport => _instance.GetText("XMLImport");

	/// <summary>
	/// You can use files from BOB, Tree Brush, Theme Mix, etc..
	/// </summary>
	public static Translation XMLImportInfo => _instance.GetText("XMLImportInfo");

	/// <summary>
	/// View and subscribe to packages in any XML configuration
	/// </summary>
	public static Translation XMLImportMissingInfo => _instance.GetText("XMLImportMissingInfo");

	/// <summary>
	/// Select or drag &amp; drop an XML config file to manage the packages listed inside of it.
	/// </summary>
	public static Translation XMLTip => _instance.GetText("XMLTip");
}
