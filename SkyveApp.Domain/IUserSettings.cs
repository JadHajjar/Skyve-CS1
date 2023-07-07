﻿using SkyveApp.Domain.Enums;

namespace SkyveApp.Domain;

public interface IUserSettings
{
	bool AdvancedIncludeEnable { get; set; }
	bool AdvancedLaunchOptions { get; set; }
	bool AlwaysOpenFiltersAndActions { get; set; }
	bool AssumeInternetConnectivity { get; set; }
	bool DisableNewAssetsByDefault { get; set; }
	bool DisableNewModsByDefault { get; set; }
	bool DisablePackageCleanup { get; set; }
	bool FilterOutPackagesWithMods { get; set; }
	bool FilterOutPackagesWithOneAsset { get; set; }
	bool FlipItemCopyFilterAction { get; set; }
	bool ForceDownloadAndDeleteAsSoonAsRequested { get; set; }
	bool HidePseudoMods { get; set; }
	bool LargeItemOnHover { get; set; }
	bool LinkModAssets { get; set; }
	bool OpenLinksInBrowser { get; set; }
	bool OverrideGameChanges { get; set; }
	PackageSorting PackageSorting { get; set; }
	bool PackageSortingDesc { get; set; }
	ProfileSorting ProfileSorting { get; set; }
	bool ResetScrollOnPackageClick { get; set; }
	bool ShowAllReferencedPackages { get; set; }
	bool ShowDatesRelatively { get; set; }
	bool ShowFolderSettings { get; set; }
	bool TreatOptionalAsRequired { get; set; }
}