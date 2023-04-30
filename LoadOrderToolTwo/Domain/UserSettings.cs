using LoadOrderToolTwo.Domain.Enums;

namespace LoadOrderToolTwo.Domain;
internal class UserSettings
{
	public PackageSorting PackageSorting { get; set; }
	public ProfileSorting ProfileSorting { get; set; }
	public bool LinkModAssets { get; set; } = true;
	public bool LargeItemOnHover { get; set; }
	public bool ShowDatesRelatively { get; set; } = true;
	public bool AdvancedIncludeEnable { get; set; }
	public bool DisableNewModsByDefault { get; set; } = true;
	public bool DisableNewAssetsByDefault { get; set; }
	public bool OverrideGameChanges { get; set; }
	public bool FilterOutPackagesWithOneAsset { get; set; }
	public bool FilterOutPackagesWithMods { get; set; }
	public bool AdvancedLaunchOptions { get; set; }
	public bool HidePseudoMods { get; set; }
	public bool ShowFolderSettings { get; set; }
	public bool AlwaysOpenFiltersAndActions { get; set; }
	public bool OpenLinksInBrowser { get; set; }
	public bool ResetScrollOnPackageClick { get; set; }
	public bool PackageSortingDesc { get; set; }
	public bool FlipItemCopyFilterAction { get; set; }
    public bool DisablePackageCleanup { get; set; }
}
