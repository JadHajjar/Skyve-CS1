﻿using Extensions;

using SkyveApp.Domain.Systems;

namespace SkyveApp.Systems.CS1.Utilities;
public class Locale : LocaleHelper, ILocale
{
	private static readonly Locale _instance = new();

	public static void Load() { _ = _instance; }

	public Translation Get(string key)
	{
		return GetGlobalText(key);
	}

	public Locale() : base($"SkyveApp.Systems.CS1.Properties.Locale.json") { }

	public static Translation Dashboard => _instance.GetText(nameof(Dashboard));
	public static Translation StartCities => _instance.GetText(nameof(StartCities));
	public static Translation StopCities => _instance.GetText(nameof(StopCities));
	public static Translation PlaysetBubble => _instance.GetText(nameof(PlaysetBubble));
	public static Translation AssetsBubble => _instance.GetText(nameof(AssetsBubble));
	public static Translation ModsBubble => _instance.GetText(nameof(ModsBubble));
	public static Translation Local => _instance.GetText(nameof(Local));
	public static Translation Workshop => _instance.GetText(nameof(Workshop));
	public static Translation Enabled => _instance.GetText(nameof(Enabled));
	public static Translation Disabled => _instance.GetText(nameof(Disabled));
	public static Translation Included => _instance.GetText(nameof(Included));
	public static Translation Excluded => _instance.GetText(nameof(Excluded));
	public static Translation Loading => _instance.GetText(nameof(Loading));
	public static Translation Server => _instance.GetText(nameof(Server));
	public static Translation Vanilla => _instance.GetText(nameof(Vanilla));
	public static Translation UpToDate => _instance.GetText(nameof(UpToDate));
	public static Translation StatusUnknown => _instance.GetText(nameof(StatusUnknown));
	public static Translation OutOfDate => _instance.GetText(nameof(OutOfDate));
	public static Translation PartiallyDownloaded => _instance.GetText(nameof(PartiallyDownloaded));
	public static Translation CompatibilityReport => _instance.GetText(nameof(CompatibilityReport));
	public static Translation AutoPlaysetSaveOn => _instance.GetText(nameof(AutoPlaysetSaveOn));
	public static Translation AutoPlaysetSaveOff => _instance.GetText(nameof(AutoPlaysetSaveOff));
	public static Translation TemporaryPlaysetCanNotBeEdited => _instance.GetText(nameof(TemporaryPlaysetCanNotBeEdited));
	public static Translation LaunchSettings => _instance.GetText(nameof(LaunchSettings));
	public static Translation IncludesItemsYouDoNotHave => _instance.GetText(nameof(IncludesItemsYouDoNotHave));
	public static Translation ContentAndInfo => _instance.GetText(nameof(ContentAndInfo));
	public static Translation OtherPlaysets => _instance.GetText(nameof(OtherPlaysets));
	public static Translation Filters => _instance.GetText(nameof(Filters));
	public static Translation TotalSize => _instance.GetText(nameof(TotalSize));
	public static Translation AssetStatus => _instance.GetText(nameof(AssetStatus));
	public static Translation ModStatus => _instance.GetText(nameof(ModStatus));
	public static Translation CompatibilityStatus => _instance.GetText(nameof(CompatibilityStatus));
	public static Translation AnyStatus => _instance.GetText(nameof(AnyStatus));
	public static Translation AnyCompatibilityStatus => _instance.GetText(nameof(AnyCompatibilityStatus));
	public static Translation Subscribe => _instance.GetText(nameof(Subscribe));
	public static Translation Switch => _instance.GetText(nameof(Switch));
	public static Translation Enable => _instance.GetText(nameof(Enable));
	public static Translation ModOwned => _instance.GetText(nameof(ModOwned));
	public static Translation Settings => _instance.GetText(nameof(Settings));
	public static Translation NoLocalPackagesFound => _instance.GetText(nameof(NoLocalPackagesFound));
	public static Translation NoPackagesMatchFilters => _instance.GetText(nameof(NoPackagesMatchFilters));
	public static Translation Actions => _instance.GetText(nameof(Actions));
	public static Translation Utilities => _instance.GetText(nameof(Utilities));
	public static Translation CollectionTitle => _instance.GetText(nameof(CollectionTitle));
	public static Translation Options => _instance.GetText(nameof(Options));
	public static Translation CrNotAvailable => _instance.GetText(nameof(CrNotAvailable));
	public static Translation ModsWithMinorIssues => _instance.GetText(nameof(ModsWithMinorIssues));
	public static Translation ModsWithMajorIssues => _instance.GetText(nameof(ModsWithMajorIssues));
	public static Translation ModsShouldUnsub => _instance.GetText(nameof(ModsShouldUnsub));
	public static Translation ModsNoIssues => _instance.GetText(nameof(ModsNoIssues));
	public static Translation Preferences => _instance.GetText(nameof(Preferences));
	public static Translation StartScratch => _instance.GetText(nameof(StartScratch));
	public static Translation ContinueFromCurrent => _instance.GetText(nameof(ContinueFromCurrent));
	public static Translation PlaysetUsage => _instance.GetText(nameof(PlaysetUsage));
	public static Translation CheckFolderInOptions => _instance.GetText(nameof(CheckFolderInOptions));
	public static Translation SomePackagesWillBeDisabled => _instance.GetText(nameof(SomePackagesWillBeDisabled));
	public static Translation AffectedPackagesAre => _instance.GetText(nameof(AffectedPackagesAre));
	public static Translation ConfirmDeletePlayset => _instance.GetText(nameof(ConfirmDeletePlayset));
	public static Translation PlaysetReplace => _instance.GetText(nameof(PlaysetReplace));
	public static Translation PlaysetExclude => _instance.GetText(nameof(PlaysetExclude));
	public static Translation PlaysetMerge => _instance.GetText(nameof(PlaysetMerge));
	public static Translation PlaysetDelete => _instance.GetText(nameof(PlaysetDelete));
	public static Translation ShouldNotBeSubscribed => _instance.GetText(nameof(ShouldNotBeSubscribed));
	public static Translation LoadingScreenMod => _instance.GetText(nameof(LoadingScreenMod));
	public static Translation ExcludeInclude => _instance.GetText(nameof(ExcludeInclude));
	public static Translation EnableDisable => _instance.GetText(nameof(EnableDisable));
	public static Translation OpenPackagePage => _instance.GetText(nameof(OpenPackagePage));
	public static Translation OpenLocalFolder => _instance.GetText(nameof(OpenLocalFolder));
	public static Translation ViewOnSteam => _instance.GetText(nameof(ViewOnSteam));
	public static Translation ReDownloadPackage => _instance.GetText(nameof(ReDownloadPackage));
	public static Translation DownloadPackage => _instance.GetText(nameof(DownloadPackage));
	public static Translation CopySteamId => _instance.GetText(nameof(CopySteamId));
	public static Translation OpenAuthorPage => _instance.GetText(nameof(OpenAuthorPage));
	public static Translation Sorting => _instance.GetText(nameof(Sorting));
	public static Translation FolderSettings => _instance.GetText(nameof(FolderSettings));
	public static Translation ChangingFoldersRequiresRestart => _instance.GetText(nameof(ChangingFoldersRequiresRestart));
	public static Translation CreatePlaysetHere => _instance.GetText(nameof(CreatePlaysetHere));
	public static Translation TemporaryPlayset => _instance.GetText(nameof(TemporaryPlayset));
	public static Translation CouldNotCreatePlayset => _instance.GetText(nameof(CouldNotCreatePlayset));
	public static Translation PlaysetNameChangedIllegalChars => _instance.GetText(nameof(PlaysetNameChangedIllegalChars));
	public static Translation PlaysetSaveInfo => _instance.GetText(nameof(PlaysetSaveInfo));
	public static Translation From => _instance.GetText(nameof(From));
	public static Translation To => _instance.GetText(nameof(To));
	public static Translation DateSubscribed => _instance.GetText(nameof(DateSubscribed));
	public static Translation DateUpdated => _instance.GetText(nameof(DateUpdated));
	public static Translation PackageStatus => _instance.GetText(nameof(PackageStatus));
	public static Translation MultiplePackagesIncluded => _instance.GetText(nameof(MultiplePackagesIncluded));
	public static Translation CopyWorkshopLink => _instance.GetText(nameof(CopyWorkshopLink));
	public static Translation DeletePackage => _instance.GetText(nameof(DeletePackage));
	public static Translation UnsubscribePackage => _instance.GetText(nameof(UnsubscribePackage));
	public static Translation CopyWorkshopId => _instance.GetText(nameof(CopyWorkshopId));
	public static Translation CopyAuthorLink => _instance.GetText(nameof(CopyAuthorLink));
	public static Translation CopyAuthorId => _instance.GetText(nameof(CopyAuthorId));
	public static Translation Copy => _instance.GetText(nameof(Copy));
	public static Translation YourPlaysets => _instance.GetText(nameof(YourPlaysets));
	public static Translation UnFavoriteThisPlayset => _instance.GetText(nameof(UnFavoriteThisPlayset));
	public static Translation FavoriteThisPlayset => _instance.GetText(nameof(FavoriteThisPlayset));
	public static Translation PlaysetCreationFailed => _instance.GetText(nameof(PlaysetCreationFailed));
	public static Translation Tags => _instance.GetText(nameof(Tags));
	public static Translation DLCs => _instance.GetText(nameof(DLCs));
	public static Translation NoDlcsNoInternet => _instance.GetText(nameof(NoDlcsNoInternet));
	public static Translation NoDlcsOpenGame => _instance.GetText(nameof(NoDlcsOpenGame));
	public static Translation DlcUpdateNotice => _instance.GetText(nameof(DlcUpdateNotice));
	public static Translation DlcCount => _instance.GetText(nameof(DlcCount));
	public static Translation IncludeAllItemsInThisPackage => _instance.GetText(nameof(IncludeAllItemsInThisPackage));
	public static Translation ExcludeAllItemsInThisPackage => _instance.GetText(nameof(ExcludeAllItemsInThisPackage));
	public static Translation Unfiltered => _instance.GetText(nameof(Unfiltered));
	public static Translation AnyTags => _instance.GetText(nameof(AnyTags));
	public static Translation MovePackageToLocalFolder => _instance.GetText(nameof(MovePackageToLocalFolder));
	public static Translation TotalItems => _instance.GetText(nameof(TotalItems));
	public static Translation MissingItemsRemain => _instance.GetText(nameof(MissingItemsRemain));
	public static Translation ModIsPrivate => _instance.GetText(nameof(ModIsPrivate));
	public static Translation SelectedFileInvalid => _instance.GetText(nameof(SelectedFileInvalid));
	public static Translation NoItemsToBeDisplayed => _instance.GetText(nameof(NoItemsToBeDisplayed));
	public static Translation FirstSetupInfo => _instance.GetText(nameof(FirstSetupInfo));
	public static Translation SetupIncomplete => _instance.GetText(nameof(SetupIncomplete));
	public static Translation CopyAuthorSteamId => _instance.GetText(nameof(CopyAuthorSteamId));
	public static Translation LoadPlayset => _instance.GetText(nameof(LoadPlayset));
	public static Translation CloseCitiesToSub => _instance.GetText(nameof(CloseCitiesToSub));
	public static Translation ShowingFilteredItems => _instance.GetText(nameof(ShowingFilteredItems));
	public static Translation ShowingCount => _instance.GetText(nameof(ShowingCount));
	public static Translation ShowingCountWarning => _instance.GetText(nameof(ShowingCountWarning));
	public static Translation ShowingSelectedCount => _instance.GetText(nameof(ShowingSelectedCount));
	public static Translation ShowingSelectedCountWarning => _instance.GetText(nameof(ShowingSelectedCountWarning));
	public static Translation ItemsHidden => _instance.GetText(nameof(ItemsHidden));
	public static Translation ClearFoldersPromptTitle => _instance.GetText(nameof(ClearFoldersPromptTitle));
	public static Translation ClearFoldersPrompt => _instance.GetText(nameof(ClearFoldersPrompt));
	public static Translation LaunchTooltip => _instance.GetText(nameof(LaunchTooltip));
	public static Translation AreYouSure => _instance.GetText(nameof(AreYouSure));
	public static Translation AssetOutOfDate => _instance.GetText(nameof(AssetOutOfDate));
	public static Translation AssetOutOfDatePlural => _instance.GetText(nameof(AssetOutOfDatePlural));
	public static Translation CopyVersionNumber => _instance.GetText(nameof(CopyVersionNumber));
	public static Translation SubscribeToItem => _instance.GetText(nameof(SubscribeToItem));
	public static Translation MultipleLOM => _instance.GetText(nameof(MultipleLOM));
	public static Translation FilterByThisReportStatus => _instance.GetText(nameof(FilterByThisReportStatus));
	public static Translation FilterByThisPackageStatus => _instance.GetText(nameof(FilterByThisPackageStatus));
	public static Translation FilterSinceThisDate => _instance.GetText(nameof(FilterSinceThisDate));
	public static Translation FilterByThisTag => _instance.GetText(nameof(FilterByThisTag));
	public static Translation ItemsShouldNotBeSubscribedInfo => _instance.GetText(nameof(ItemsShouldNotBeSubscribedInfo));
	public static Translation WouldYouLikeToSkipThose => _instance.GetText(nameof(WouldYouLikeToSkipThose));
	public static Translation LOTWillRestart => _instance.GetText(nameof(LOTWillRestart));
	public static Translation UpdatingLot => _instance.GetText(nameof(UpdatingLot));
	public static Translation SubscribingRequiresGameToOpenTitle => _instance.GetText(nameof(SubscribingRequiresGameToOpenTitle));
	public static Translation SubscribingRequiresGameToOpen => _instance.GetText(nameof(SubscribingRequiresGameToOpen));
	public static Translation HelpLogs => _instance.GetText(nameof(HelpLogs));
	public static Translation DisableFpsBoosterDebug => _instance.GetText(nameof(DisableFpsBoosterDebug));
	public static Translation DefaultLogViewInfo => _instance.GetText(nameof(DefaultLogViewInfo));
	public static Translation ModIncludedTotal => _instance.GetText(nameof(ModIncludedTotal));
	public static Translation ModIncludedEnabledTotal => _instance.GetText(nameof(ModIncludedEnabledTotal));
	public static Translation ModIncludedAndEnabledTotal => _instance.GetText(nameof(ModIncludedAndEnabledTotal));
	public static Translation PackageIncludedTotal => _instance.GetText(nameof(PackageIncludedTotal));
	public static Translation PackageIncludedEnabledTotal => _instance.GetText(nameof(PackageIncludedEnabledTotal));
	public static Translation PackageIncludedAndEnabledTotal => _instance.GetText(nameof(PackageIncludedAndEnabledTotal));
	public static Translation AssetIncludedTotal => _instance.GetText(nameof(AssetIncludedTotal));
	public static Translation ClickAuthorFilter => _instance.GetText(nameof(ClickAuthorFilter));
	public static Translation ActionUnreversible => _instance.GetText(nameof(ActionUnreversible));
	public static Translation FavoriteTotal => _instance.GetText(nameof(FavoriteTotal));
	public static Translation FavoritePlaysetTotal => _instance.GetText(nameof(FavoritePlaysetTotal));
	public static Translation PlaysetFilter => _instance.GetText(nameof(PlaysetFilter));
	public static Translation CheckDocumentsFolder => _instance.GetText(nameof(CheckDocumentsFolder));
	public static Translation FailedToSaveLanguage => _instance.GetText(nameof(FailedToSaveLanguage));
	public static Translation FailedToOpenTC => _instance.GetText(nameof(FailedToOpenTC));
	public static Translation FailedToDeleteItem => _instance.GetText(nameof(FailedToDeleteItem));
	public static Translation DeleteAsset => _instance.GetText(nameof(DeleteAsset));
	public static Translation IncludeThisItemInAllPlaysets => _instance.GetText(nameof(IncludeThisItemInAllPlaysets));
	public static Translation ExcludeThisItemInAllPlaysets => _instance.GetText(nameof(ExcludeThisItemInAllPlaysets));
	public static Translation CopyPackageName => _instance.GetText(nameof(CopyPackageName));
	public static Translation CopyAuthorName => _instance.GetText(nameof(CopyAuthorName));
	public static Translation CopyFolderName => _instance.GetText(nameof(CopyFolderName));
	public static Translation PlaysetNameUsed => _instance.GetText(nameof(PlaysetNameUsed));
	public static Translation FailedToImportLegacyPlayset => _instance.GetText(nameof(FailedToImportLegacyPlayset));
	public static Translation CurrentPlayset => _instance.GetText(nameof(CurrentPlayset));
	public static Translation PlaysetStillLoading => _instance.GetText(nameof(PlaysetStillLoading));
	public static Translation ApplyPlaysetNameBeforeExit => _instance.GetText(nameof(ApplyPlaysetNameBeforeExit));
	public static Translation SteamNotOpenTo => _instance.GetText(nameof(SteamNotOpenTo));
	public static Translation OpenSteamToContinue => _instance.GetText(nameof(OpenSteamToContinue));
	public static Translation IncludeExcludeOtherPlayset => _instance.GetText(nameof(IncludeExcludeOtherPlayset));
	public static Translation ChangePlaysetColor => _instance.GetText(nameof(ChangePlaysetColor));
	public static Translation OpenPlaysetFolder => _instance.GetText(nameof(OpenPlaysetFolder));
	public static Translation CreateShortcutPlayset => _instance.GetText(nameof(CreateShortcutPlayset));
	public static Translation AskToLaunchGameForShortcut => _instance.GetText(nameof(AskToLaunchGameForShortcut));
	public static Translation NoPlaysetsFound => _instance.GetText(nameof(NoPlaysetsFound));
	public static Translation NoPlaysetsMatchFilters => _instance.GetText(nameof(NoPlaysetsMatchFilters));
	public static Translation Author => _instance.GetText(nameof(Author));
	public static Translation AnyAuthor => _instance.GetText(nameof(AnyAuthor));
	public static Translation ItemsCount => _instance.GetText(nameof(ItemsCount));
	public static Translation AuthorsSelected => _instance.GetText(nameof(AuthorsSelected));
	public static Translation AnyIssue => _instance.GetText(nameof(AnyIssue));
	public static Translation IncludeAll => _instance.GetText(nameof(IncludeAll));
	public static Translation ExcludeAll => _instance.GetText(nameof(ExcludeAll));
	public static Translation EnableAll => _instance.GetText(nameof(EnableAll));
	public static Translation DisableAll => _instance.GetText(nameof(DisableAll));
	public static Translation UnsubscribeAll => _instance.GetText(nameof(UnsubscribeAll));
	public static Translation CopyAllIds => _instance.GetText(nameof(CopyAllIds));
	public static Translation DeleteAll => _instance.GetText(nameof(DeleteAll));
	public static Translation AltClickTo => _instance.GetText(nameof(AltClickTo));
	public static Translation FilterByThisAuthor => _instance.GetText(nameof(FilterByThisAuthor));
	public static Translation AddToSearch => _instance.GetText(nameof(AddToSearch));
	public static Translation CopyToClipboard => _instance.GetText(nameof(CopyToClipboard));
	public static Translation ViewPackageCR => _instance.GetText(nameof(ViewPackageCR));
	public static Translation FilterByThisEnabledStatus => _instance.GetText(nameof(FilterByThisEnabledStatus));
	public static Translation FilterByThisIncludedStatus => _instance.GetText(nameof(FilterByThisIncludedStatus));
	public static Translation CleanupInfo => _instance.GetText(nameof(CleanupInfo));
	public static Translation CloseCitiesToClean => _instance.GetText(nameof(CloseCitiesToClean));
	public static Translation CleanupRequiresGameToOpen => _instance.GetText(nameof(CleanupRequiresGameToOpen));
	public static Translation SubscribersCount => _instance.GetText(nameof(SubscribersCount));
	public static Translation RatingCount => _instance.GetText(nameof(RatingCount));
	public static Translation SubscribeAll => _instance.GetText(nameof(SubscribeAll));
	public static Translation MissingLSMReport => _instance.GetText(nameof(MissingLSMReport));
	public static Translation UnusedLSMReport => _instance.GetText(nameof(UnusedLSMReport));
	public static Translation MissingPackagesPlayset => _instance.GetText(nameof(MissingPackagesPlayset));
	public static Translation SearchWorkshop => _instance.GetText(nameof(SearchWorkshop));
	public static Translation SearchWorkshopBrowser => _instance.GetText(nameof(SearchWorkshopBrowser));
	public static Translation VotingTitle => _instance.GetText(nameof(VotingTitle));
	public static Translation VotingInfo1 => _instance.GetText(nameof(VotingInfo1));
	public static Translation VotingInfo2 => _instance.GetText(nameof(VotingInfo2));
	public static Translation VotingInfo3 => _instance.GetText(nameof(VotingInfo3));
	public static Translation VotingInfo4 => _instance.GetText(nameof(VotingInfo4));
	public static Translation VotingInfo5 => _instance.GetText(nameof(VotingInfo5));
	public static Translation VotingInfo6 => _instance.GetText(nameof(VotingInfo6));
	public static Translation UnknownPackage => _instance.GetText(nameof(UnknownPackage));
	public static Translation AssetsWithMinorIssues => _instance.GetText(nameof(AssetsWithMinorIssues));
	public static Translation AssetsWithMajorIssues => _instance.GetText(nameof(AssetsWithMajorIssues));
	public static Translation AssetsShouldUnsub => _instance.GetText(nameof(AssetsShouldUnsub));
	public static Translation SelectThisPackage => _instance.GetText(nameof(SelectThisPackage));
	public static Translation ControlToSelectMultiplePackages => _instance.GetText(nameof(ControlToSelectMultiplePackages));
	public static Translation Include => _instance.GetText(nameof(Include));
	public static Translation Snooze => _instance.GetText(nameof(Snooze));
	public static Translation UnSnooze => _instance.GetText(nameof(UnSnooze));
	public static Translation Unsubscribe => _instance.GetText(nameof(Unsubscribe));
	public static Translation Exclude => _instance.GetText(nameof(Exclude));
	public static Translation Mod => _instance.GetText(nameof(Mod));
	public static Translation Asset => _instance.GetText(nameof(Asset));
	public static Translation Package => _instance.GetText(nameof(Package));
	public static Translation Playset => _instance.GetText(nameof(Playset));
	public static Translation IncludedCount => _instance.GetText(nameof(IncludedCount));
	public static Translation EnabledCount => _instance.GetText(nameof(EnabledCount));
	public static Translation IncludedEnabledCount => _instance.GetText(nameof(IncludedEnabledCount));
	public static Translation LoadedCount => _instance.GetText(nameof(LoadedCount));
	public static Translation EditCompatibility => _instance.GetText(nameof(EditCompatibility));
	public static Translation AnyUsage => _instance.GetText(nameof(AnyUsage));
	public static Translation ReviewRequestSent => _instance.GetText(nameof(ReviewRequestSent));
	public static Translation ReviewRequestFailed => _instance.GetText(nameof(ReviewRequestFailed));
	public static Translation EditTags => _instance.GetText(nameof(EditTags));
	public static Translation AddCustomTag => _instance.GetText(nameof(AddCustomTag));
	public static Translation OutOfDateCount => _instance.GetText(nameof(OutOfDateCount));
	public static Translation IncompleteCount => _instance.GetText(nameof(IncompleteCount));
	public static Translation PackageIsIncomplete => _instance.GetText(nameof(PackageIsIncomplete));
	public static Translation PackageIsMaybeOutOfDate => _instance.GetText(nameof(PackageIsMaybeOutOfDate));
	public static Translation PackageIsOutOfDate => _instance.GetText(nameof(PackageIsOutOfDate));
	public static Translation PackageIsNotDownloaded => _instance.GetText(nameof(PackageIsNotDownloaded));
	public static Translation PackageIsUnknown => _instance.GetText(nameof(PackageIsUnknown));
	public static Translation PackageIsRemoved => _instance.GetText(nameof(PackageIsRemoved));
	public static Translation RemovedFromSteam => _instance.GetText(nameof(RemovedFromSteam));
	public static Translation Missing => _instance.GetText(nameof(Missing));
	public static Translation LoggedInAs => _instance.GetText(nameof(LoggedInAs));
	public static Translation SendReview => _instance.GetText(nameof(SendReview));
	public static Translation UnknownUser => _instance.GetText(nameof(UnknownUser));
	public static Translation SelectPackage => _instance.GetText(nameof(SelectPackage));
	public static Translation AddMeaningfulDescription => _instance.GetText(nameof(AddMeaningfulDescription));
	public static Translation CleanupComplete => _instance.GetText(nameof(CleanupComplete));
	public static Translation RedownloadComplete => _instance.GetText(nameof(RedownloadComplete));
	public static Translation PendingSubscribeTo => _instance.GetText(nameof(PendingSubscribeTo));
	public static Translation PendingUnsubscribeFrom => _instance.GetText(nameof(PendingUnsubscribeFrom));
	public static Translation ThisSubscribesTo => _instance.GetText(nameof(ThisSubscribesTo));
	public static Translation ThisUnsubscribesFrom => _instance.GetText(nameof(ThisUnsubscribesFrom));
	public static Translation ViewThisPlaysetsPackages => _instance.GetText(nameof(ViewThisPlaysetsPackages));
	public static Translation DownloadAll => _instance.GetText(nameof(DownloadAll));
	public static Translation ReDownloadAll => _instance.GetText(nameof(ReDownloadAll));
	public static Translation PendingDownloads => _instance.GetText(nameof(PendingDownloads));
	public static Translation PendingDeletions => _instance.GetText(nameof(PendingDeletions));
	public static Translation YouHavePackagesUser => _instance.GetText(nameof(YouHavePackagesUser));
	public static Translation SharePlayset => _instance.GetText(nameof(SharePlayset));
	public static Translation FailedToFetchLogs => _instance.GetText(nameof(FailedToFetchLogs));
	public static Translation AllUsages => _instance.GetText(nameof(AllUsages));
	public static Translation Invalid => _instance.GetText(nameof(Invalid));
	public static Translation UpdatePlayset => _instance.GetText(nameof(UpdatePlayset));
	public static Translation FailedToRetrievePlaysets => _instance.GetText(nameof(FailedToRetrievePlaysets));
	public static Translation DownloadPlayset => _instance.GetText(nameof(DownloadPlayset));
	public static Translation DiscoverPlaysets => _instance.GetText(nameof(DiscoverPlaysets));
	public static Translation FailedToDownloadPlayset => _instance.GetText(nameof(FailedToDownloadPlayset));
	public static Translation UpdatePlaysetTip => _instance.GetText(nameof(UpdatePlaysetTip));
	public static Translation DownloadPlaysetTip => _instance.GetText(nameof(DownloadPlaysetTip));
	public static Translation EditPlaysetThumbnail => _instance.GetText(nameof(EditPlaysetThumbnail));
	public static Translation MakePrivate => _instance.GetText(nameof(MakePrivate));
	public static Translation MakePublic => _instance.GetText(nameof(MakePublic));
	public static Translation CopyPlaysetLink => _instance.GetText(nameof(CopyPlaysetLink));
	public static Translation FailedToUploadPlayset => _instance.GetText(nameof(FailedToUploadPlayset));
	public static Translation FailedToDeletePlayset => _instance.GetText(nameof(FailedToDeletePlayset));
	public static Translation FailedToUpdatePlayset => _instance.GetText(nameof(FailedToUpdatePlayset));
	public static Translation ImportFromLink => _instance.GetText(nameof(ImportFromLink));
	public static Translation PastePlaysetId => _instance.GetText(nameof(PastePlaysetId));
	public static Translation SelectAll => _instance.GetText(nameof(SelectAll));
	public static Translation DeselectAll => _instance.GetText(nameof(DeselectAll));
	public static Translation TagsTitle => _instance.GetText(nameof(TagsTitle));
	public static Translation EditingMultipleTags => _instance.GetText(nameof(EditingMultipleTags));
	public static Translation WorkshopAndGameTags => _instance.GetText(nameof(WorkshopAndGameTags));
	public static Translation CustomTags => _instance.GetText(nameof(CustomTags));
	public static Translation EditTagsOfPackage => _instance.GetText(nameof(EditTagsOfPackage));
	public static Translation TroubleshootInfo => _instance.GetText(nameof(TroubleshootInfo));
	public static Translation TroubleshootSelection => _instance.GetText(nameof(TroubleshootSelection));
}