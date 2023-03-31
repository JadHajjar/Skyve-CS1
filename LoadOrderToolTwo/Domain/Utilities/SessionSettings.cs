using Extensions;

using System.Drawing;

namespace LoadOrderToolTwo.Domain.Utilities;
internal class SessionSettings : ISave
{
	public override string Name => nameof(SessionSettings) + ".json";

	public bool FirstTimeSetupCompleted { get; set; }
	public string? CurrentProfile { get; set; }
	public Rectangle? LastWindowsBounds { get; set; }
	public bool WindowWasMaximized { get; set; }

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
}
